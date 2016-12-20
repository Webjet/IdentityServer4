#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx


function Publish-DODOAzureAvailabilitySet
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	
    Write-Host "Executing Publish-DODOAzureAvailabilitySet"

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
        $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureAvailabilitySet" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureAvailabilitySet" }
    }

    if($deploymentJson -eq $NULL)
    {
	    throw "AzureAvailabilitySet container not found in json" + $ContainerName
    }
    #endregion
	
    foreach($deployment in $deploymentJson)
    {
        $name = $deployment.Attributes.Properties.Name
        $subscriptionID = $deployment.Attributes.Properties.SubscriptionID
        $tenantID = $deployment.Attributes.Properties.TenantID
        $resourceGroupName = $deployment.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $deployment.Attributes.ResourceGroup.Location
        $rmProperties = $deployment.Attributes.Properties.ResourceManagerProperties

        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
       
        Write-Host "Checking AzureAvailabilitySet $name ..."
         
        $availset = Find-AzureRmResource -ResourceGroupName $resourceGroupName -ResourceNameContains $name -ResourceType Microsoft.Compute/availabilitySets -ApiVersion 2015-11-01
        
        if ($availset -eq $null -or $availset -eq "")
        {
	        Write-Host "AzureAvailabilitySet $name does not exist, creating..."
            New-AzureRmResource -Name $name -Location $resourceGroupLocation -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Compute/availabilitySets -ApiVersion 2016-03-30 -Force     
	        Write-Host "AzureAvailabilitySet created!"
        }
        else
        {
            Write-Host "AzureAvailabilitySet $name exists"
            #Set-AzureRmResource -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Compute/availabilitySets -ResourceName $name -ApiVersion 2015-06-15 -Force
        }
       
    }

    Write-Host "Done executing  Publish-DODOAzureAvailabilitySet"
}

Export-ModuleMember -Function 'Publish-DODOAzureAvailabilitySet'


