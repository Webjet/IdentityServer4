#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureKeyVault
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureKeyVault"
	 
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureKeyVault" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureKeyVault" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureKeyVault container not found in json" + $ContainerName
    }
	
    foreach($container in $json)
    {
        $name = $container.Attributes.Properties.Name
        $subscriptionID = $container.Attributes.Properties.SubscriptionID
        $tenantID = $container.Attributes.Properties.TenantID
        $resourceGroupName = $container.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $container.Attributes.ResourceGroup.Location
		$sku = $container.Attributes.Properties.Sku
        $policies = $container.Attributes.AccessPolicies
        
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID

        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation

        Write-Host "Checking Azure Key Vault $name ..."
        $keyVault = Find-AzureRmResource -ResourceNameContains $name -ResourceGroupName $resourceGroupName -ResourceType Microsoft.KeyVault/vaults -ApiVersion 2015-11-01
        
        if ($keyVault -eq $null -or $keyVault -eq "")
        {
	        Write-Host "Azure Key Vault $name does not exist, creating..."
            
	        New-AzureRmKeyVault -VaultName $name -Sku $sku -Location $resourceGroupLocation -ResourceGroupName $resourceGroupName
	        Write-Host "Key Vault created!"
        }
        else
        {
            Write-Host "Key Vault $name exists!"
        }
        
        if($policies -ne "" -and $policies -ne $NULL)
        {
            foreach($accessPolicy in $policies)
            {
                if($accessPolicy.ObjectID -ne "" -and $accessPolicy.ObjectID -ne $NULL)
                {
                    Write-Host "Setting up access policy on object ID $($accessPolicy.ObjectID)..."
                    Set-AzureRmKeyVaultAccessPolicy -VaultName $name -ResourceGroupName $resourceGroupName -ObjectId $accessPolicy.ObjectID -ApplicationId $accessPolicy.ApplicationID -PermissionsToKeys $accessPolicy.PermissionToKeys -PermissionsToSecrets $accessPolicy.PermissionToSecrets
                    Write-Host "access policy set!" 
                }
                else {
                     Write-Host "Setting up access policy on service principle $($accessPolicy.ServicePrincipleName)..."
                     Set-AzureRmKeyVaultAccessPolicy -VaultName $name -ResourceGroupName $resourceGroupName -ServicePrincipalName $accessPolicy.ServicePrincipleName -PermissionsToKeys $accessPolicy.PermissionToKeys -PermissionsToSecrets $accessPolicy.PermissionToSecrets
                     Write-Host "access policy set!"
                }
            }
        }
    }

	Write-Host "Done executing  Publish-DODOAzureKeyVault"
}


Export-ModuleMember -Function 'Publish-DODOAzureKeyVault'

