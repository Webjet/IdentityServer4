#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx
#https://azure.microsoft.com/en-us/documentation/articles/virtual-machines-ps-create-preconfigure-windows-resource-manager-vms/

function Publish-DODOAzureVMScaleSet
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureVMScaleSet"
	
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVmScaleSet" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVmScaleSet" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureVmScaleSet container not found in json" + $ContainerName
    }
	#endregion

    foreach($container in $json)
    {
        $name = $container.Attributes.Properties.Name
        $subscriptionID = $container.Attributes.Properties.SubscriptionID
        $tenantID = $container.Attributes.Properties.TenantID
        
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
        
        $size = $container.Attributes.Properties.Size
        $upgradePolicyMode = $container.Attributes.Properties.UpgradePolicyMode 
        $skuCapacity = $container.Attributes.Properties.SkuCapacity

        #resource group info
        $resourceGroupName = $container.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $container.Attributes.ResourceGroup.Location

        #storage account info
        $storageAccName = $container.Attributes.Properties.StorageAccountName
        $storageProfile = $container.Attributes.Properties.StorageProfileName
        if($storageAccName -eq "" -or $storageAccName -eq $NULL){
            throw "Azure StorageAccount needs to be supplied for AzureVM type!"
        }
        else {
            $storage = Find-AzureRmResource -ResourceNameContains $storageAccName -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Storage/storageAccounts -ApiVersion 2015-11-01	

            if ($storage -eq "" -or $storage -eq $null)
            {
                 throw "Azure StorageAccount specified cannot be found, please ensure the storage account exists!"
            }
            else {
                Write-Host "VM Storage account verified!"
            }
        }
        
        #image info
        $publisherName = $container.Attributes.Properties.Publisher
        $offerName = $container.Attributes.Properties.Offer
        $SKU = $container.Attributes.Properties.SKU

        #network info
        $nics = $container.Attributes.NetworkInterfaces
        $subnetName = $container.Attributes.VirtualNetwork.SubnetName
        $vnetName = $container.Attributes.VirtualNetwork.Name
        $vnetResourceGroup = $container.Attributes.VirtualNetwork.ResourceGroupName
        $loadbalancerBackendAddresspoolID = $container.Attributes.Properties.LoadbalancerBackendAddresspoolID
        $loadbalancerInboundNATPoolID = $container.Attributes.Properties.LoadbalancerInboundNATPoolID
        $publicIpAddress = $container.Attributes.Properties.PublicIPAddress


        if($loadbalancerBackendAddresspoolID -eq "")
        {
            $loadbalancerBackendAddresspoolID = $NULL
        } 

        if($loadbalancerInboundNATPoolID -eq "")
        {
            $loadbalancerInboundNATPoolID = $NULL
        }     

        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
        
        Write-Host "Checking public ip address..."
        if($publicIpAddress -ne "" -and $publicIpAddress -ne $NULL)
        {
            Internal-CreatePublicIpAddress -Name $publicIpAddress.Name -Location $resourceGroupLocation -ResourceGroupName $resourceGroupName -ResourceManagerProperties $publicIpAddress.ResourceManagerProperties
        }

        $vmssConfig = New-AzureRmVmssConfig -Location $resourceGroupLocation -SkuCapacity $skuCapacity -SkuName $size -UpgradePolicyMode $upgradePolicyMode
       
        Write-Host "Checking VMSS $name ..."
        $vmss = Find-AzureRmResource -ResourceType Microsoft.Compute/virtualMachineScaleSets -ResourceNameContains $name  -ApiVersion 2015-11-01 -ResourceGroupNameContains $resourceGroupName

        if ($vmss -eq $null -or $vmss -eq "")
        {
	        Write-Host "VMSS not found..."
            
            Write-Host "Configuring VMSS credentials..."
            #credentials
            $credentialType = $container.Attributes.Properties.Credential.Type
            $username = ""
            $password = ""
            
            if($credentialType -eq "Standard")
            {
                $username = $container.Attributes.Properties.Credential.Username
	            $password = ConvertTo-SecureString -String $($container.Attributes.Properties.Credential.Password) -AsPlainText -Force
            }
        
            if($credentialType -eq "PSCredentialPath")
            {
                $username = Get-Content $container.Attributes.Properties.Credential.Username
	            $password =  Get-Content $container.Attributes.Properties.Credential.Password  | ConvertTo-SecureString
            }

            if($username -eq "" -or $password -eq "")
            {
                throw "VMSS credential either invalid or not supplied!"
            }

            Write-Host "Checking VNET subnet..."
            $subnets = Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $vnetResourceGroup | Select Subnets
            foreach($subnet in $subnets.Subnets)
            {
                if($subnet.Name -eq $subnetName)
                {
                    Write-Host "Add VMSS to subnet $($subnet.Name) id $($subnet.Id)..."

                    $ipConfig = New-AzureRmVmssIpConfig -Name $publicIpAddress.Name -LoadBalancerBackendAddressPoolsId $loadbalancerBackendAddresspoolID -LoadBalancerInboundNatPoolsId $loadbalancerInboundNATPoolID -SubnetId $subnet.id;

                    Write-Host "Adding network interface configuration..."
                    Add-AzureRmVmssNetworkInterfaceConfiguration -VirtualMachineScaleSet $vmssConfig -Name $name -Primary $true -IPConfiguration $ipConfig   
                }
            }

            Write-Host "Setting VMSS OS profile..."
            $computerName = $container.Attributes.Properties.ComputerName
            Set-AzureRmVmssOsProfile -VirtualMachineScaleSet $vmssConfig -ComputerNamePrefix $computerName -AdminUsername $username -AdminPassword $password
           
            Write-Host "Setting storage profile..."
            $storageAcc = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccName
            $osDiskUri = $storageAcc.PrimaryEndpoints.Blob.ToString() + "vhds"

            Set-AzureRmVmssStorageProfile -VirtualMachineScaleSet $vmssConfig -ImageReferencePublisher $publisherName -ImageReferenceOffer $offerName -ImageReferenceSku $SKU -ImageReferenceVersion "latest" -Name $storageProfile -VhdContainer $osDiskUri -OsDiskCreateOption "FromImage" -OsDiskCaching "None"  
          
            Write-Host "Creating VMSS..."
	        New-AzureRmVmss -ResourceGroupName $resourceGroupName -Name $name -VirtualMachineScaleSet $vmssConfig
	        Write-Host "VMSS created!"
			
        }
        else
        {
            Write-Host "VMSS already exists, updating..."

            Write-Host "Checking VNET subnet..."
            $subnets = Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $vnetResourceGroup | Select Subnets
            foreach($subnet in $subnets.Subnets)
            {
                if($subnet.Name -eq $subnetName)
                {
                    Write-Host "Creating VMSS IP config..."
                    $ipConfig = New-AzureRmVmssIpConfig -Name $publicIpAddress.Name -LoadBalancerBackendAddressPoolsId $loadbalancerBackendAddresspoolID -LoadBalancerInboundNatPoolsId $loadbalancerInboundNATPoolID  -SubnetId $subnet.id;

                    Write-Host "Adding network interface configuration..."
                    Add-AzureRmVmssNetworkInterfaceConfiguration -VirtualMachineScaleSet $vmssConfig -Name $name -Primary $true -IPConfiguration $ipConfig   
                }
            }

            Write-Host "Updating VMSS..."
            Update-AzureRmVmss -ResourceGroupName $resourceGroupName -Name $name -VirtualMachineScaleSet $vmssConfig
            Write-Host "VMSS updated!"
        }
    }

	Write-Host "Done executing  Publish-DODOAzureVMScaleSet"
}

function Internal-CreatePublicIpAddress
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1)] [string]$Name,
		[Parameter(Position=1,Mandatory=1)] [string]$Location,
        [Parameter(Position=2,Mandatory=1)] [string]$ResourceGroupName,
        [Parameter(Position=3,Mandatory=1)] [PSCustomObject]$ResourceManagerProperties
     )

    Write-Host "Checking public ip address $Name ..."

    # GET publicIPAddresses
    $pubIp = Find-AzureRmResource -ResourceNameContains $Name -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Network/publicIPAddresses -ApiVersion 2015-11-01

    if($pubIp -eq "" -or $pubIp -eq $NULL)
    {
        Write-Host "AzurePublicIpAddress $Name does not exist, creating..."
        $ResourceManagerProperties
        New-AzureRmPublicIpAddress -Name $Name -ResourceGroupName $ResourceGroupName -Location $Location -AllocationMethod $ResourceManagerProperties.publicIPAllocationMethod
        Write-Host "AzurePublicIpAddress $Name created"
    }
    else
    {
        Write-Host "AzurePublicIpAddress $Name exists, updating..."
        $ResourceManagerProperties
        
        $publicIP = Get-AzureRmPublicIpAddress -Name $Name -ResourceGroupName $ResourceGroupName
        $publicIP.PublicIpAllocationMethod = $ResourceManagerProperties.publicIPAllocationMethod
        
        $publicIP | Set-AzureRmPublicIpAddress
        Write-Host "AzurePublicIpAddress $Name updated!"
    }
}

Export-ModuleMember -Function 'Publish-DODOAzureVMScaleSet'

