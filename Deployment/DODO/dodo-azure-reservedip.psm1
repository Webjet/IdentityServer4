#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx


function Publish-DODOAzureReservedIp
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureReservedIp"
	
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
        $ipJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureClassicReservedIP" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $ipJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureClassicReservedIP" }
    }

    if($ipJson -eq $NULL)
    {
	    throw "AzureClassicReservedIP container not found in json" + $ContainerName
    }
    #endregion
	
    foreach($ipContainers in $ipJson)
    {
        $ipName = $ipContainers.Attributes.Properties.Name
        $subscriptionID = $ipContainers.Attributes.Properties.SubscriptionID
        $tenantID = $ipContainers.Attributes.Properties.TenantID
        $resourceGroupName = $ipContainers.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $ipContainers.Attributes.ResourceGroup.Location
        $rmProperties = $ipContainers.Attributes.Properties.ResourceManagerProperties

		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
       
        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation

        Write-Host "Checking reserved ip addresss $ipName ..."
         
        $ip = Find-AzureRmResource -ResourceGroupName $resourceGroupName -ResourceNameContains $ipName -ResourceType microsoft.classicnetwork/reservedIps -ApiVersion 2015-11-01
        
        if ($ip -eq $null -or $ip -eq "")
        {
	        Write-Host "Classic Azure reserved ip $ipName does not exist, creating..."
            New-AzureRmResource -Name $ipName -Location $resourceGroupLocation -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType microsoft.classicnetwork/reservedIps -ApiVersion 2015-06-01 -Force     
	        Write-Host "Classic Azure reserved ip address created!"
        }
        else
        {
            Write-Host "Classic Azure reserved $ipName exists...updating"
            Set-AzureRmResource -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType microsoft.classicnetwork/reservedIps -ResourceName $ipName -ApiVersion 2015-06-01 -Force
            Write-Host "Classic Azure reserved updated!"
        }
    }

	Write-Host "Done executing  Publish-DODOAzureReservedIp"
}

function Publish-DODOAzurePublicIp
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		[Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
		[Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
     )

	Write-Host "Executing Publish-DODOAzurePublicIp"
	 
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzurePublicIp" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzurePublicIp" }
    }

    if($json -eq $NULL)
	{
		throw "AzurePublicIp container not found in json" + $ContainerName
	}
	
	foreach($container in $json)
	{
		$subscriptionID = $container.Attributes.Properties.SubscriptionID
		$tenantID = $container.Attributes.Properties.TenantID
		$resourceGroupName = $container.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $container.Attributes.ResourceGroup.Location
		$name = $container.Attributes.Properties.Name
		$resourceManagerProperties = $container.Attributes.ResourceManagerProperties
		
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
		Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 
		
		Write-Host "Checking public ip address $name ..."

        # GET publicIPAddresses
        $pubIp = Find-AzureRmResource -ResourceNameContains $name -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Network/publicIPAddresses -ApiVersion 2015-11-01

        if($pubIp -eq "" -or $pubIp -eq $NULL)
        {
            Write-Host "AzurePublicIpAddress $name does not exist, creating..."
            $resourceManagerProperties
            New-AzureRmPublicIpAddress -Name $name -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation -AllocationMethod $resourceManagerProperties.publicIPAllocationMethod -DomainNameLabel $resourceManagerProperties.dnsSettings.domainNameLabel 
            Write-Host "AzurePublicIpAddress $Name created"
        }
        else
        {
            Write-Host "AzurePublicIpAddress $Name exists, updating..."
            $resourceManagerProperties
           <# 
            $publicIP = Get-AzureRmPublicIpAddress -Name $name -ResourceGroupName $resourceGroupName
            $publicIP.PublicIpAllocationMethod = $resourceManagerProperties.publicIPAllocationMethod
            $publicIP.DnsSettings.DomainNameLabel = $resourceManagerProperties.dnsSettings.domainNameLabel
            $publicIP | Set-AzureRmPublicIpAddress
            #>
            Set-AzureRmResource -PropertyObject $resourceManagerProperties -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Network/publicIPAddresses -ResourceName $name -ApiVersion 2016-03-30 -Force
            Write-Host "AzurePublicIpAddress $name updated!"
        }
	}

	 Write-Host "Done executing Publish-DODOAzurePublicIp"
}

Export-ModuleMember -Function 'Publish-DODOAzureReservedIp'
Export-ModuleMember -Function 'Publish-DODOAzurePublicIp'


