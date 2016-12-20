#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx


function Publish-DODOAzureSearch
{
    [CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		 [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )

    Write-Host "Executing Publish-DODOAzureSearch"

    #region Read Json
    switch ($PsCmdlet.ParameterSetName) 
    { 
        "File"  
        { 
            $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; 
            $ParametersJSONObject = Get-Content -Raw -Path $ParametersJSONPath | ConvertFrom-Json;
            break; 
        } 
    }
      
    $ConfigurationJSONObject = Set-InternalDODOVariables -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
    
    if($ContainerName -ne $NULL -and $ContainerName -ne "")
    {
        $azureSearchJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureSearch" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $azureSearchJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureSearch" }
    }

    if($azureSearchJson -eq $NULL)
	{
		throw "AzureSearch container not found in json" + $ContainerName
	}
    #endregion

    foreach($azureSearchContainer in $azureSearchJson)
	{
        $subscriptionName = $azureSearchContainer.Attributes.Properties.Subscription
		$subscriptionId = $azureSearchContainer.Attributes.Properties.SubscriptionID
		$tenantId = $azureSearchContainer.Attributes.Properties.TenantID
        $azureSearchName = $azureSearchContainer.Attributes.Properties.Name
        $azureSearchLocation = $azureSearchContainer.Attributes.Properties.Location
		$resourceGroupName = $azureSearchContainer.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $azureSearchContainer.Attributes.ResourceGroup.Location
        $partitionCount = $azureSearchContainer.Attributes.Properties.partitionCount
        $replicaCount = $azureSearchContainer.Attributes.Properties.replicaCount
        $sku = $azureSearchContainer.Attributes.Properties.Sku


        if($azureSearchLocation -eq "" -or $azureSearchLocation -eq $null)
        {
            $azureSearchLocation = $resourceGroupLocation
        }

        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionId -TenantId $tenantId
       
        Internal-CreateAzureSearch -Name $azureSearchName -ResourceGroupName $resourceGroupName -ResourceGroupLocation $resourceGroupLocation -ReplicaCount $replicaCount -PartitionCount $partitionCount -Sku $sku

    }
 
}

function Internal-CreateAzureSearch
{
    param(
        [Parameter(Mandatory=$true)]
        [String]$Name,

        [Parameter(Mandatory=$true)]
        [String]$ResourceGroupName,

        [Parameter(Mandatory=$true)]
        [String]$ResourceGroupLocation,

        [Parameter(Mandatory=$true)]
        [String]$ReplicaCount,

        [Parameter(Mandatory=$true)]
        [String]$PartitionCount,

        [Parameter(Mandatory=$true)]
        [String]$Sku
    )

        $PropertiesObject = @{
            
            "replicaCount" = $ReplicaCount;
            "partitionCount" = $PartitionCount;
        }            

        $Skuhash = @{
            "name" = $Sku;
        }    

        $searchService = Find-AzureRmResource -ResourceNameContains $Name  -ResourceGroupNameContains $ResourceGroupName -ResourceType Microsoft.Search/searchServices -ApiVersion 2015-11-01
        
        if ($searchService -eq $NULL -or $searchService -eq "")
        {
	        Write-Host "Azure Search $Name does not exist, creating..."
            Create-DODOAzureResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation 
			
			Write-Host "Creating Search Service..."
            New-AzureRmResource -PropertyObject $PropertiesObject -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Search/searchServices -ResourceName $Name -Location $ResourceGroupLocation -sku $Skuhash  -ApiVersion 2015-08-19 -Force

        }
        else
        {
            Write-Host "AzureSearch $Name exists!"
        }

    
}

Export-ModuleMember -Function 'Publish-DODOAzureSearch'




