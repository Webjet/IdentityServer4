
 function Publish-DODOAzureWebjob
 {
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		[Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
		[Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	 
	Write-Host "Executing Publish-DODOAzureWebjob" 
	
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
        $webjobJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebjob" -and $_.Name -eq $ContainerName  }
    }
    else
    {
        $webjobJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebjob" }
    }

    if($webjobJson -eq $NULL)
	{
		throw "AzureWebjob container not found in json" + $ContainerName
	}
	
	foreach($webjobContainer in $webjobJson)
	{
		$subscriptionName = $webjobContainer.Attributes.Properties.Subscription
		$subscriptionId = $webjobContainer.Attributes.Properties.SubscriptionID
		$tenantID = $webjobContainer.Attributes.Properties.TenantID
		$azureWebAppName = $webjobContainer.Attributes.Properties.AzureWebAppName
		$azureWebsiteJobName = $webjobContainer.Attributes.Properties.Name
		$slot = $webjobContainer.Attributes.Properties.Slot
		$jobType = $webjobContainer.Attributes.Properties.JobType
		$jobFile = $webjobContainer.Attributes.Properties.Package
		
		#Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionId -TenantId $tenantID
		
		Write-Host "Checking Azure Webjob : $azureWebsiteJobName site: $azureWebAppName"

		Get-AzureWebsiteJob -JobName $azureWebsiteJobName -Name $azureWebAppName -Slot $slot -JobType $jobType -ErrorVariable a -ErrorAction SilentlyContinue
		if ($a[0] -ne $null){
			if($a[0] -Match "Not Found"){

			  Write-Host "Webjob does not exist, creating..."
			  New-AzureWebsiteJob -JobFile $jobFile -JobName $azureWebsiteJobName -JobType $jobType -Name $azureWebAppName -Slot $slot
			  
			  Write-Host "Webjob created, starting..."
			  Stop-AzureWebsiteJob -JobName $azureWebsiteJobName -Name $azureWebAppName -Slot $slot
			  Start-AzureWebsiteJob -JobName $azureWebsiteJobName -JobType $jobType -Name $azureWebAppName -Slot $slot

			  Write-Host "Webjob started!"
			}else{
				throw "Unable to deploy $azureWebsiteJobName on app $azureWebAppName -" + $a
			}
		}else{
			Write-Host "Webjob exists, stopping..."
			Stop-AzureWebsiteJob -JobName $azureWebsiteJobName -Name $azureWebAppName -Slot $slot

			Write-Host "Webjob stopped, removing..."
			Remove-AzureWebsiteJob -JobName $azureWebsiteJobName -JobType $jobType -Name $azureWebAppName -Slot $slot -Force

			Write-Host "Webjob removed, creating..."
			New-AzureWebsiteJob -JobFile $jobFile -JobName $azureWebsiteJobName -JobType $jobType -Name $azureWebAppName -Slot $slot

			Write-Host "Webjob created, restarting..."
			Stop-AzureWebsiteJob -JobName $azureWebsiteJobName -Name $azureWebAppName -Slot $slot
			Start-AzureWebsiteJob -JobName $azureWebsiteJobName -JobType $jobType -Name $azureWebAppName -Slot $slot

			Write-Host "Webjob started!"
		}
	}	
	
	Write-Host "Done executing Publish-DODOAzureWebjob" 
 }

Export-ModuleMember -Function 'Publish-DODOAzureWebjob'