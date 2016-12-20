#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureTrafficManager
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		[Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
		[Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
     )

	Write-Host "Executing Publish-DODOAzureTrafficManager"
	 
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureTrafficManager" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureTrafficManager" }
    }

    if($json -eq $NULL)
	{
		throw "AzureTrafficManager container not found in json" + $ContainerName
	}
	
	foreach($container in $json)
	{
		$subscriptionID = $container.Attributes.Properties.SubscriptionID
		$tenantID = $container.Attributes.Properties.TenantID
		$resourceGroupName = $container.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $container.Attributes.ResourceGroup.Location
		$name = $container.Attributes.Properties.Name
		
		$monitorPath = $container.Attributes.Properties.MonitorPath
		$monitorPort = $container.Attributes.Properties.MonitorPort
		$monitorProtocol = $container.Attributes.Properties.MonitorProtocol
		$relativeDnsName = $container.Attributes.Properties.RelativeDnsName
		$trafficRoutingMethod = $container.Attributes.Properties.TrafficRoutingMethod
		$profileStatus = $container.Attributes.Properties.ProfileStatus
        $ttl = $container.Attributes.Properties.Ttl
		$endpoints = $container.Attributes.EndPoints

		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
		Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 
		
		Write-Host "Registering network provider..."
		Register-AzureRmResourceProvider -ProviderNamespace "Microsoft.Network" -Force

		Write-Host "Checking traffic manager..."
		 
		$tm = Get-AzureRmTrafficManagerProfile -Name $name -ResourceGroupName $resourceGroupName -ErrorVariable e -ErrorAction SilentlyContinue
		if ($e[0] -ne $null)
		{
			if($e[0] -Match "not found" -or $e[0] -Match "ResourceNotFound")
			{
				Write-Host "Creating traffic manager profile $($name)..."
				
				New-AzureRmTrafficManagerProfile -Name $name -ResourceGroupName $resourceGroupName -MonitorPath $monitorPath -MonitorPort $monitorPort -MonitorProtocol $monitorProtocol -RelativeDnsName $relativeDnsName -TrafficRoutingMethod $trafficRoutingMethod -Ttl $ttl -ProfileStatus $profileStatus
				 
				Write-Host "Azure traffic manager profile created successfully!"

				#get the refreshed traffic manager profile!
				$tm = Get-AzureRmTrafficManagerProfile -Name $name -ResourceGroupName $resourceGroupName
			}
			else
			{
				throw "Unable to query traffic manager -" + $e
			}
		}
		else 
		{
			Write-Host "Traffic manager profile exists, updating..."

			$tm.MonitorPath = $monitorPath
			$tm.MonitorPort = $monitorPort
			$tm.MonitorProtocol = $monitorProtocol
			$tm.RelativeDnsName = $relativeDnsName
			$tm.TrafficRoutingMethod = $trafficRoutingMethod
			$tm.ProfileStatus = $profileStatus
			$tm.Ttl = $ttl

			Set-AzureRmTrafficManagerProfile -TrafficManagerProfile $tm

			Write-Host "Traffic manager profile updated!"
		}

		if($endpoints -ne "" -and $endpoints -ne $null)
		{
			foreach($endpoint in $endpoints)
			{
				Write-Host "Checking endpoint, $($endpoint.Name) resource type: $($endpoint.ResourceType)..."

				$id = ""
				switch($endpoint.ResourceType)
				{
					"AzureRmPublicIpAddress"
					{
						$id = (Get-AzureRmPublicIpAddress -Name $endpoint.Name -ResourceGroupName $endpoint.ResourceGroupName).Id
					}

					"AzureRmResource"
					{
						$id = (Get-AzureRmResource -ResourceName $endpoint.Name -ResourceGroupName $endpoint.ResourceGroupName).ResourceId
					}
				}

				if($id -eq $null -or $id -eq "")
				{
					throw "Cannot resolve ID for traffic manager endpoint. Please check your deployment template and ensure resource exists"
				}

				$found = $false
				foreach($trafficManagerEndpoint in $tm.Endpoints)
				{
					if($trafficManagerEndpoint.Name -eq $endpoint.Name)
					{
						$found = $true
						Write-Host "Endpoint $($endpoint.Name) exists"
						 
						$trafficManagerEndpoint.EndpointStatus = $endpoint.Status
						$trafficManagerEndpoint.Priority = $endpoint.Priority
						$trafficManagerEndpoint.Weight = $endpoint.Weight
						
						Set-AzureRmTrafficManagerProfile -TrafficManagerProfile $tm
						Write-Host "Endpoint udpated!"
						
						break
					}
				}
				
				if(!$found)
				{
					Write-Host "Endpoint $($endpoint.Name) not found, adding..."
					Add-AzureRmTrafficManagerEndpointConfig	-EndpointName $endpoint.Name `
															-EndpointStatus $endpoint.Status `
															-TrafficManagerProfile $tm `
															-Type $endpoint.Type `
															-TargetResourceId $id `
															-Priority $endpoint.Priority `
															-Weight $endpoint.Weight
					Set-AzureRmTrafficManagerProfile -TrafficManagerProfile $tm
					
					Write-Host "Endpoint added!"
				}
			}
		}
	}

	 Write-Host "Done executing Publish-DODOAzureTrafficManager"
}

Export-ModuleMember -Function 'Publish-DODOAzureTrafficManager'