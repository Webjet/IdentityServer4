#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureRedis
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		[Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
		[Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
     )

	Write-Host "Executing Publish-DODOAzureRedis"
	 
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
        $redisJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureRedisCache" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $redisJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureRedisCache" }
    }

    if($redisJson -eq $NULL)
	{
		throw "AzureRedisCache container not found in json" + $ContainerName
	}
	
	foreach($redisContainers in $redisJson)
	{
		$subscriptionId = $redisContainers.Attributes.Properties.SubscriptionID
		$resourceGroupName = $redisContainers.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $redisContainers.Attributes.ResourceGroup.Location
		$tenantId = $redisContainers.Attributes.Properties.TenantID
		$redisSize = $redisContainers.Attributes.Properties.Size
		$redisSKU = $redisContainers.Attributes.Properties.SKU
		$redisName = $redisContainers.Attributes.Properties.Name
		
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionId -TenantId $tenantId
		
		Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 
		
		Write-Host "Registering cache provider..."
		Register-AzureRmResourceProvider -ProviderNamespace "Microsoft.Cache" -Force

		Write-Host "Checking redis cache..."
		$redisCache = Get-AzureRmRedisCache -ResourceGroupName $resourceGroupName -Name $redisName -ErrorVariable e -ErrorAction SilentlyContinue
		if ($e[0] -ne $null)
		{
			if($e[0] -Match "was not found")
			{
				Write-Host "Creating a new Redis Cache named $($redisName)..."
				New-AzureRmRedisCache -ResourceGroupName $resourceGroupName -Name $redisName -Location $resourceGroupLocation -Sku $redisSKU -Size $redisSize 
				Write-Host "Azure redis created successfully!"
			}
			else
			{
				throw "Unable to create resourcegroup -" + $e
			}
		}
	}

	 Write-Host "Done executing Publish-DODOAzureRedis"
}

Export-ModuleMember -Function 'Publish-DODOAzureRedis'