#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx



function Publish-DODOAzureApiManagement
{
    [CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		 [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )

    Write-Host "Executing Publish-DODOAzureApiManagement"

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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureApiManagement" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureApiManagement" }
    }

    if($json -eq $NULL)
	{
		throw "AzureApiManagement container not found in json" + $ContainerName
	}
    #endregion

    foreach($container in $json)
	{
        $name = $container.Attributes.Properties.Name
        $subscriptionName = $container.Attributes.Properties.Subscription
		$subscriptionId = $container.Attributes.Properties.SubscriptionID
		$tenantId = $container.Attributes.Properties.TenantID
        $adminEmail = $container.Attributes.Properties.AdminEmail
        $organization = $container.Attributes.Properties.Organization
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionId -TenantId $tenantId
		
        $location = $container.Attributes.Properties.Location
		$resourceGroupName = $container.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $container.Attributes.ResourceGroup.Location

		$appServicePlan = $container.Attributes.AppServicePlan.Name
		$appServiceTier = $container.Attributes.AppServicePlan.Tier
        $appServicePlanLocation = $container.Attributes.AppServicePlan.Location

        if($location -eq "" -or $location -eq $null)
        {
            $location = $resourceGroupLocation
        }

        #$app = Find-AzureRmResource -ResourceNameContains $name  -ResourceGroupNameContains $resourceGroupName -ResourceType Microsoft.Web/sites -ApiVersion 2015-11-01
        
        if ($app -eq $NULL -or $app -eq "")
        {
	        Write-Host "Manamgenent API $name does not exist, creating..."
            Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 
			
			Write-Host "Creating Azure Management API..."
			New-AzureRmApiManagement -AdminEmail $adminEmail -Location $location -Name $name -ResourceGroupName $resourceGroupName -Organization $organization
			
            Write-Host "Azure API Mangement created!"
           
        }
        else
        {
            Write-Host "Azure API Mangement: $name exists!"
        }
    }

}

Export-ModuleMember -Function 'Publish-DODOAzureApiManagement'


