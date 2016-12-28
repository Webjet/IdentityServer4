#Approved Verbs for Windows PowerShell Commands
#http://www.powershellmagazine.com/2014/08/05/understanding-azure-vm-dsc-extension/

function Publish-DODOAzureVMDSC
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureVMDSC"
	
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVmDSC" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVmDSC" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureVmDSC container not found in json" + $ContainerName
    }
	#endregion

    foreach($jsonContainer in $json)
    {
        
        $subscriptionID = $jsonContainer.Attributes.SubscriptionID
        $tenantID = $jsonContainer.Attributes.TenantID
        
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
        
        $containerName = $jsonContainer.Attributes.StorageContainerName
        $configurationPath = $jsonContainer.Attributes.ConfigurationPath
       
        if($($configurationPath).Contains("`$PSScriptRoot"))
        {
            $Invocation = (Get-Variable MyInvocation -Scope 2).Value;
            $InvocationPath =  Split-Path $Invocation.MyCommand.Path
            $configurationPath = $($configurationPath).Replace("`$PSScriptRoot", $InvocationPath )
        }

        #storage account info
        $storageAccountContainer = $jsonContainer.Attributes.DODOAzureStorageAccount;
        if($storageAccountContainer -eq "" -or $storageAccountContainer -eq $NULL){
            throw "DODO AzureStorageAccount needs to be supplied for AzureVM type!"
        }
        
        $storageJson = $ConfigurationJSONObject.Containers | where { $_.Name -eq $storageAccountContainer }
        $storageAccountName = $storageJson.Attributes.Properties.Name
        $storageAccountResourceGroupName = $storageJson.Attributes.ResourceGroup.Name
        $StorageAccountType = $storageJson.Attributes.Properties.Type
        
        Write-Host "Setting Target Storage Acc : $storageAccountName Type: $StorageAccountType on resource group : $storageAccountResourceGroupName "

        Set-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $storageAccountResourceGroupName -Type $StorageAccountType
        
        Write-Host "Publishing DSC Configuration $configurationPath..."

        Set-AzureRmContext -SubscriptionId $subscriptionID -TenantId $tenantID
        Publish-AzureRmVMDscConfiguration -ConfigurationPath $configurationPath -ResourceGroupName $storageAccountResourceGroupName -StorageAccountName $storageAccountName -ContainerName $ContainerName -Force
    
    }

	Write-Host "Done executing  Publish-DODOAzureVMDSC"
}


function Apply-DODOAzureVMExtension
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Apply-DODOAzureVMExtension"
	
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVM" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVM" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureVM container not found in json" + $ContainerName
    }
	#endregion

    foreach($vmContainers in $json)
    {
        $vmName = $vmContainers.Attributes.Properties.Name
        $subscriptionID = $vmContainers.Attributes.Properties.SubscriptionID
        $tenantID = $vmContainers.Attributes.Properties.TenantID
        $dscExtensions = $vmContainers.Attributes.DSCExtensions

        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
        #resource group info
        $resourceGroupName = $vmContainers.Attributes.ResourceGroup.Name

        #storage account info
        $storageAccountContainer = $vmContainers.Attributes.Properties.DODOAzureStorageAccount;
        if($storageAccountContainer -eq "" -or $storageAccountContainer -eq $NULL){
            throw "DODO AzureStorageAccount needs to be supplied for AzureVM type!"
        }

          if($dscExtensions -ne "" -and $dscExtensions -ne $null)
        {
            foreach($dscExtension in $dscExtensions)
            {
                Write-Host "Setting VM DSC Extension..."
                $dsc_BlobName = $dscExtension.BlobName
                $dsc_StorageAccName = $dscExtension.StorageAccountName
                $dsc_ResourceGroupName = $dscExtension.ResourceGroupName
                $dsc_Version = $dscExtension.Version
                $dsc_ContainerName = $dscExtension.StorageContainerName
                $dsc_ConfigurationName = $dscExtension.ConfigurationName

                Set-AzureRmVMDscExtension -ConfigurationName $dsc_ConfigurationName -ArchiveBlobName $dsc_BlobName -ArchiveContainerName $dsc_ContainerName -ArchiveStorageAccountName $dsc_StorageAccName -ResourceGroupName $dsc_ResourceGroupName -Version $dsc_Version -VMName $vmName
                Write-Host "VM DSC extension set!"
            }
        }
       
    }
}

Export-ModuleMember -Function 'Publish-DODOAzureVMDSC'
Export-ModuleMember -Function 'Apply-DODOAzureVMExtension'

