
function Publish-DODOIISARR
{
   [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOIISARR"
	
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
        $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "IIS-ARR" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "IIS-ARR" }
    }

    if($deploymentJson -eq $NULL)
    {
	    throw "IIS-ARR container not found in json" + $ContainerName
    }
	#endregion

    foreach($container in $deploymentJson)
    {
        $ErrorActionPreference = "Stop"
        
        $features = $container.Attributes.Properties.WindowsFeatures
        $downloadFolder = $container.Attributes.Properties.DownloadFolder
        $webpiUrl = $container.Attributes.Properties.WebPlatformInstaller.HTTPUri
        $serverFarms =  $container.Attributes.Properties.ServerFarms

        Write-Host "Checking Windows Feature dependancies..."
        
        if($features -ne "" -and $features -ne $null)
        {
            Import-Module ServerManager 
            foreach($feature in $features)
            {
                $windowsFeature = (Get-WindowsFeature | where { $_.Name -eq $feature.Name })
    
                if($windowsFeature.Installed)
                {
                   Write-Host "Feature $($feature.Name) is installed"
                }
                else
                {
        
                     Write-Host "Feature $($feature.Name) is not installed, installing"
                     Install-WindowsFeature -Name $feature.Name -IncludeAllSubFeature
                     Write-Host  "Feature $($feature.Name) has been installed"
                }
            }
        }
        
        Write-Host "Checking Web Platform Installer..."

        $webPIlocation = $env:programfiles + "\microsoft\web platform installer\"
        if(!(Test-Path -Path $webPIlocation ))
        {
            Write-Host "Web platform installer does not exist, downloading..."

            Write-Host "Checking DownloadFolder..."
            if($downloadFolder -eq "" -or $downloadFolder -eq $null)
            {
                $downloadFolder = "C:\temp" #default!
            }

            if(!(Test-Path -Path $downloadFolder))
            {
                md $downloadFolder
            }

            Write-Host "Downloading web platform installer from given URL..."
            $downloadFile = "WebPI_x64.msi"
		
            Invoke-WebRequest -uri $webpiUrl -outfile "$downloadFolder\$downloadFile"
		
            #test the download was successful
            if(!(Test-Path "$downloadFolder\$downloadFile") ){
	            throw "Please check that the $downloadFile file had downloaded successfully"
            }

            Write-Host "Download web platform installer complete."

            Write-Host "Installing web platform installer..."
	        $process = Start-Process -FilePath msiexec -ArgumentList /package, "$downloadFolder\$downloadFile", /quiet -Wait -PassThru
        
            if ($process.ExitCode -ne 0){
		        Write-Host "Installer returned exit code"$($process.ExitCode)
		        throw "Error occurred. Installation aborted"
	        }
            else{
                Write-Host "Web platform installer has been installed"
            }
        }

        Write-Host "Installing ARR..."
        
        Internal-StartProcess -Path "$webPIlocation\webpicmd" -Arguments "/install /accepteula /Products:ARRv3_0"
        
        Internal-ConfigureARR -ARRContainer $container
       
    }
	
	Write-Host "Done executing Publish-DODOIISARR"
}

function Internal-StartProcess
{
    [CmdletBinding()]
    param(        
    [Parameter(Position=0,Mandatory=1)] [string]$Path,
    [Parameter(Position=0,Mandatory=0)] [string]$Arguments,
    [Parameter(Position=0,Mandatory=0)] [string]$Verb
    )

    $psi = New-object System.Diagnostics.ProcessStartInfo 
    $psi.CreateNoWindow = $true 
    $psi.UseShellExecute = $false 
    $psi.RedirectStandardOutput = $true 
    $psi.RedirectStandardError = $true 
    $psi.FileName = $Path
    $psi.Arguments = $Arguments
    $process = New-Object System.Diagnostics.Process 
    $process.StartInfo = $psi 
    $process.Start() | Out-Null
    $output = $process.StandardOutput.ReadToEnd() 
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()
    Write-Host $output
    Write-Host $stderr
    $stderr
    if($process.ExitCode -eq 0){
		Write-Host "Internal-StartProcess Process completed successfully!"
    }
	else{
		throw "non zero exit code detected, see logs above..."
	}
}

function Internal-ConfigureARR
{
     [CmdletBinding()]
     param(        
            [Parameter(Position=0,Mandatory=1)] [PSCustomObject]$ARRContainer
        )

    Write-Host "Configuring ARR..."

    $appHostConfigXPath = 'MACHINE/WEBROOT/APPHOST'

    Write-Host "Seting up process model defaults..."
    $appPoolsXPath = 'system.applicationHost/applicationPools'
    $appHostAppPoolDefaultsXPath = "$appPoolsXPath/applicationPoolDefaults"
    Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$appHostAppPoolDefaultsXPath" -name "startMode" -value "AlwaysRunning"
    Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$appHostAppPoolDefaultsXPath/processModel" -name "idleTimeout" -value "00:00:00"
    Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$appHostAppPoolDefaultsXPath/recycling/periodicRestart" -name "time" -value "00:00:00"

    Write-Host "Setup process model config for default app pool..."
    Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$appPoolsXPath/add[@name='DefaultAppPool']" -name "startMode" -value "AlwaysRunning"
    Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$appPoolsXPath/add[@name='DefaultAppPool']/processModel" -name "idleTimeout" -value "00:00:00"
    Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$appPoolsXPath/add[@name='DefaultAppPool']/recycling/periodicRestart" -name "time" -value "00:00:00"


    Write-Host "Checking Server Farms..."
    $serverFarms =  $ARRContainer.Attributes.Properties.ServerFarms

    if($serverFarms -ne "" -and $serverFarms -ne $null)
    {
        foreach($serverFarm in $serverFarms)
        {
            #Set up server farm and load balance in ARR
            $arrWebFarmConfigXPath = "webFarms/webFarm[@name='$($serverFarm.Name)']/applicationRequestRouting"

            #Get-WebConfigurationProperty -PSPath $appHostConfigXPath -Filter "webFarms"
            Write-Host "Checking web farm config..."

            $webFarm = Get-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "webFarms/webFarm[@name='$($serverFarm.Name)']" -name "."
            Write-Host "Web farm : $webFarm"
            if($webFarm -eq "" -or $webFarm -eq $null)
            {
                Write-Host "Adding web farm $($serverFarm.Name)..."
                Add-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "webFarms" -name "." -value @{name=$($serverFarm.Name)}
            }
            
            Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/protocol" -name "xForwardedByHeaderName" -value "X-Forwarded-By"
            Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/protocol" -name "xForwardedForHeaderName" -value "X-Forwarded-For"
            Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/protocol/cache" -name "enabled" -value "False"
            Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/affinity" -name "useCookie" -value "True"
            Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/loadBalancing" -name "algorithm" -value "WeightedTotalTraffic"

            if($serverFarm.EnableRoutingRules -ne "" -and $serverFarm.EnableRoutingRules -ne $null)
            {
                Write-Host "Setting routing rules..."
            }

            if($serverFarm.EnableSSLOffLoad -ne "" -and $serverFarm.EnableSSLOffLoad -ne $null)
            {
                Write-Host "Setting SSL offload..."
            }

            if($serverFarm.HealthTest -ne "" -and $serverFarm.HealthTest -ne $null)
            {
                Write-Host "Configuring Health Check..."
                Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/healthCheck" -name "url" -value $($serverFarm.HealthTest.Url)
                Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/healthCheck" -name "interval" -value $($serverFarm.HealthTest.Interval)
                Set-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$arrWebFarmConfigXPath/healthCheck" -name "responseMatch" -value $($serverFarm.HealthTest.ResponseMatch)
            }

           

            foreach ($server in $serverFarm.Servers) 
            {
                Write-Host "Checking server $($server.Name) in web farm $($serverFarm.Name)"
                $serverValue = Get-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "/webFarms/webFarm[@name='$($serverFarm.Name)']/server[@address='$($server.Address)']" -Name "."
                
                if($serverValue -eq "" -or $serverValue -eq $null)
                {
                    Add-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "webFarms/webFarm[@name='$($serverFarm.Name)']" -name "." -value @{address=$server.Address}
                }
                

            }
          
        }
    }

    Write-Host "Checking Routing Rules..."

    $routingRules = $ARRContainer.Attributes.Properties.RoutingRules

    if($routingRules -ne "" -and $routingRules -ne $null)
    {
        foreach($routingRule in $routingRules)
        {
            Internal-AddRoutingRule -RoutingRule $routingRule
        }
    }

    Write-Host "ARR configuration complete"
}

function Internal-AddRoutingRule
{
    [CmdletBinding()]
    param(        
    [Parameter(Position=0,Mandatory=1)] [PSCustomObject]$RoutingRule
    )

    Write-Host "Checking Rule.."
    $appHostConfigXPath = 'MACHINE/WEBROOT/APPHOST'
    $globalRulesXPath = "system.webServer/rewrite/globalRules"

    $ruleName = $RoutingRule.Name  

    $ruleValue = Get-WebConfigurationProperty -pspath $appHostConfigXPath  -filter "$globalRulesXPath/rule[@name='$ruleName']" -Name "."
    
    if($ruleValue -eq "" -or $ruleValue -eq $null)
    {
        Write-Host "Rule doesnt exist, adding ..."
        Add-WebConfigurationProperty -pspath $appHostConfigXPath -filter $globalRulesXPath -name "." -value @{name=$ruleName;patternSyntax="$($RoutingRule.PatternSyntax)";stopProcessing="$($RoutingRule.StopProcessing)";enabled="$($RoutingRule.Enabled)"}
    }
    else
    {
        Write-Host "Rule exists, updating ..."
        Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$ruleName']" -name "." -value @{name=$ruleName;patternSyntax="$($RoutingRule.PatternSyntax)";stopProcessing="$($RoutingRule.StopProcessing)";enabled="$($RoutingRule.Enabled)"}
    }
    
    Write-Host "Setting match.."
    $match = $RoutingRule.Match
    Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$ruleName']/match" -name "$($match.Name)" -value "$($match.Value)"

    Write-Host "Checking conditions.."
    $conditions = $RoutingRule.Conditions
    foreach($condition in $conditions)
    {
        Write-Host "Checking condition input $($condition.Input).."

        $conditionsXpath = "$globalRulesXPath/rule[@name='$ruleName']/conditions/add[@input='$($condition.Input)']"
        $conditionsXpath = $conditionsXpath.Replace("{", "{{")
        $conditionsXpath = $conditionsXpath.Replace("}", "}}")

        $conditionsValue = Get-WebConfigurationProperty -pspath $appHostConfigXPath  -filter $conditionsXpath -Name "."

        if($conditionsValue -eq "" -or $conditionsValue  -eq $null)
        {
            Write-Host "Condition doesnt exist, adding ..."
            Add-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$ruleName']/conditions" -name "." -value @{input="$($condition.Input)";pattern="$($condition.Pattern)"}
        }
        else
        {
            Write-Host "Condition exists, updating ..."
            Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter $conditionsXpath -name "." -value @{input="$($condition.Input)";pattern="$($condition.Pattern)"}
        }
    }

    $conditionAttributes = $RoutingRule.ConditionAttributes
    if($conditionAttributes.LogicalGrouping -ne "" -and $conditionAttributes.LogicalGrouping -ne $null)
    {
        Write-Host "Setting LogicalGrouping condition attribute..."
        Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$ruleName']/conditions" -name "logicalGrouping" -value "$($conditionAttributes.LogicalGrouping)"
    }

    if($conditionAttributes.LogicalGrouping -ne "" -and $conditionAttributes.LogicalGrouping -ne $null)
    {
        Write-Host "Setting TrackAllCaptures condition attribute..."
        Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$ruleName']/conditions" -name "trackAllCaptures" -value "$($conditionAttributes.TrackAllCaptures)"
    }

    Write-Host "Setting actions.."
    $actions = $RoutingRule.Actions
    foreach($action in $actions)
    {
        Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$ruleName']/action" -name "type" -value "$($action.Type)"
        Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$ruleName']/action" -name "url" -value "$($action.Url)"
    }
   
    <#
    Add-WebConfigurationProperty -pspath $appHostConfigXPath -filter $globalRulesXPath -name "." -value @{name=$httpRedirectRuleName;patternSyntax='Wildcard';stopProcessing='True'}
    Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpRedirectRuleName']/match" -name "url" -value "*"
    Add-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpRedirectRuleName']/conditions" -name "." -value @{input='{HTTPS}';pattern='off'}
    Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpRedirectRuleName']/action" -name "type" -value "Redirect"
    Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpRedirectRuleName']/action" -name "url" -value "https://{HTTP_HOST}/{R:0}"

    Add-WebConfigurationProperty -pspath $appHostConfigXPath -filter $globalRulesXPath -name "." -value @{name=$httpsRuleName;patternSyntax='Wildcard';stopProcessing='True'}
    Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpsRuleName']/match" -name "url" -value "*"
    Add-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpsRuleName']/conditions" -name "." -value @{input='{HTTPS}';pattern='on'}
    Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpsRuleName']/action" -name "type" -value "Rewrite"
    Set-WebConfigurationProperty -pspath $appHostConfigXPath -filter "$globalRulesXPath/rule[@name='$httpsRuleName']/action" -name "url" -value "https://$($serverFarm.Name)/{R:0}"
    #>

    
}

Export-ModuleMember -Function 'Publish-DODOIISARR'
