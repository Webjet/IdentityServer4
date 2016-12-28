
function Publish-DODOIISWebApplication
{
   [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOIISWebApplication"
	
    #region Read JSON 
   switch ($PsCmdlet.ParameterSetName) 
    { 
        "File"  
        { 
            $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; 
            if($ParametersJSONObject -ne $NULL -and $ParametersJSONObject -ne "")
            {
                $ParametersJSONObject = Get-Content -Raw -Path $ParametersJSONPath | ConvertFrom-Json;
            } 
            break 
        } 
    }  
    $ConfigurationJSONObject = Set-InternalDODOVariables -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
    
    if($ContainerName -ne $NULL -and $ContainerName -ne "")
    {
        $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "IISWebApplication" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "IISWebApplication" }
    }

    if($deploymentJson -eq $NULL)
    {
	    throw "IISWebApplication container not found in json" + $ContainerName
    }
	#endregion

    foreach($container in $deploymentJson)
    {
        $iisApplicationName = $container.Attributes.Properties.Name
        $iisSiteName = $container.Attributes.Properties.IISSite
        $appPoolName = $container.Attributes.Properties.ApplicationPool.Name
        $iisApplicationPath = $container.Attributes.Properties.PhysicalPath
        $iisAppPoolDotNetVersion = $container.Attributes.Properties.ApplicationPool.DotNetVersion
        $features = $container.Attributes.Properties.WindowsFeatures
        $bindings = $container.Attributes.Bindings
        $sitePath  = ""


        if($iisApplicationName -ne "" -and $iisApplicationName -ne $NULL)
        {
            $sitePath = "IIS:\Sites\$($iisSiteName)\$($iisApplicationName)"
            $siteAppName = $iisApplicationName
        }
        else 
        {
            $sitePath = "IIS:\Sites\$($iisSiteName)"
            $siteAppName = $iisSiteName
        }

        Write-Host "Checking Windows Feature dependancies..."
        
        if($features -ne "" -and $features -ne $null)
        {
            foreach($feature in $features)
            {
                $windowsFeature = (Get-WindowsFeature | where { $_.Name -eq $feature.Name })
    
                if($windowsFeature.Installed)
                {
                   Write-Host "Feature $($feature.Name) is installed"

                }else
                {
                     Write-Host "Feature $($feature.Name) is not installed, installing"
                     Install-WindowsFeature -Name $feature.Name -IncludeAllSubFeature
                     Write-Host  "Feature $($feature.Name) has been installed"
                }
            }
        }
        #note: this should be after feature check as it requires IIS role
        Import-Module WebAdministration
        
        Write-Host "Checking application pool $appPoolName ... "

        $appPool =  Get-Item "IIS:\AppPools\$appPoolName"
        
        if($appPool -eq "" -or $appPool -eq $null)
        {
            Write-Host "$appPoolName Application pool does not exist, creating..."
            New-WebAppPool -Name $appPoolName -Force
            Write-Host "$appPoolName Application pool created, configuring..."

            $appPool =  Get-Item "IIS:\AppPools\$appPoolName"
            $appPool | Set-ItemProperty -Name "managedRuntimeVersion" -Value $iisAppPoolDotNetVersion

            Write-Host "$appPoolName Application pool configured, starting..."
            Start-WebAppPool -Name $appPoolName

            $appPoolState = (Get-WebAppPoolState -Name $appPoolName).Value

            Write-Host "Application pool : $appPoolState "
        }

        #Creating applications under a site
        if($iisApplicationName -ne "" -and $iisApplicationName -ne $NULL)
        {
            Write-Host "Checking web application $iisApplicationName on site $iisSiteName ... "
            $webApp = Get-WebApplication -Site $iisSiteName -Name $iisApplicationName

            if($webApp -eq "" -or $webApp -eq $null)
            {
                Write-Host "$iisApplicationName Web application does not exist, creating..."

                if(!(Test-Path -Path $iisApplicationPath))
                {
                    md $iisApplicationPath
                }

                New-WebApplication -Name $iisApplicationName -ApplicationPool $appPoolName -PhysicalPath $iisApplicationPath -Site $iisSiteName -Force
                Write-Host "$iisApplicationName Web application created"
            }

            #Setting preload
            $enablePreLoad = $container.Attributes.Properties.EnablePreLoad
            
            if($enablePreLoad -ne "" -and $enablePreLoad -ne $null)
            {
                Write-Host "Configuring preload..."
                Set-ItemProperty -Path $sitePath -name "preloadEnabled" -value $enablePreLoad  
            }
        }
        
        $startMode = $container.Attributes.Properties.ApplicationPool.StartMode
        
        if($startMode -ne "" -and $startMode -ne $null)
        {
            Write-Host "Configuring start mode..."
            $appPool =  Get-Item "IIS:\AppPools\$appPoolName"
            $appPool | Set-ItemProperty -Name "startMode" -Value $startMode   
        }

        $idleTimeout = $container.Attributes.Properties.ApplicationPool.IdleTimeOutMinutes
        
        if($idleTimeout -ne "" -and $idleTimeout -ne $null)
        {
            Write-Host "Configuring idle timeout..."
            $appPool =  Get-Item "IIS:\AppPools\$appPoolName"
            $appPool | Set-ItemProperty -Name "processModel.idleTimeout" -Value $idleTimeout   
        }

        #Bindings
        if($bindings -ne "" -and $bindings -ne $null)
        {
            $ipAddress = (Test-Connection -ComputerName (hostname) -Count 1  | Select IPV4Address).IPV4Address
           
            foreach($binding in $bindings)
            {
                if($binding.IpAddressValue -eq "IPV4Address")
                {
                   $binding.IpAddressValue = $ipAddress
                }

                Write-Host "Checking binding... "
                
                $siteBinding = Get-WebBinding -Name $siteAppName -Protocol $binding.Protocol -IPAddress $binding.IpAddressValue -Port $binding.Port -HostHeader $binding.HostHeader 

                <# SSL Flags
                0  No SNI
                1  SNI Enabled
                2  Non SNI binding which uses Central Certificate Store.
                3  SNI binding which uses Central Certificate store
                #>

                if($siteBinding -ne $NULL -and $siteBinding -ne "")
                {
                    Write-Host "Binding exists"

                }
                else {
                    Write-Host "Binding not found, creating..."
                    New-WebBinding -Name $siteAppName -Protocol $binding.Protocol -Port $binding.Port -IPAddress $binding.IpAddressValue -HostHeader $binding.HostHeader -SslFlags $binding.SSFlags

                }
            }
        }
    }
	
	Write-Host "Done executing Publish-DODOIISWebApplication"
}

Export-ModuleMember -Function 'Publish-DODOIISWebApplication'
