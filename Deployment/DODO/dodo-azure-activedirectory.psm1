#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureActiveDirectory
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureActiveDirectory"
	 
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureActiveDirectory" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureActiveDirectory" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureActiveDirectory container not found in json" + $ContainerName
    }
	
    foreach($container in $json)
    {
        $name = $container.Attributes.Properties.Name
        $subscriptionID = $container.Attributes.Properties.SubscriptionID
        $tenantID = $container.Attributes.Properties.TenantID
        $resourceGroupName = $container.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $container.Attributes.ResourceGroup.Location
		 
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID

        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation

        Write-Host "Checking Azure Active Directory $name ..."
        <#$keyVault = Find-AzureRmResource -ResourceNameContains $name -ResourceGroupName $resourceGroupName -ResourceType Microsoft.KeyVault/vaults -ApiVersion 2015-11-01
        
        if ($keyVault -eq $null -or $keyVault -eq "")
        {
	        Write-Host "Azure Active Directory $name does not exist, creating..."
            
	        New-AzureRmKeyVault -VaultName $name -Sku $sku -Location $resourceGroupLocation -ResourceGroupName $resourceGroupName
	        Write-Host "Active Directory created!"
        }
        else
        {
            Write-Host "Active Directory $name exists!"
        }
        #>
    }

	Write-Host "Done executing  Publish-DODOAzureActiveDirectory"
}

Export-ModuleMember -Function 'Publish-DODOAzureActiveDirectory'

