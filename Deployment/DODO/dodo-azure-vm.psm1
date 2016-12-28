#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx
#https://azure.microsoft.com/en-us/documentation/articles/virtual-machines-ps-create-preconfigure-windows-resource-manager-vms/

function Publish-DODOAzureVM
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureVM"
	
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
        $vmJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVM" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $vmJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVM" }
    }

    if($vmJson -eq $NULL)
    {
	    throw "AzureVM container not found in json" + $ContainerName
    }
	#endregion

    foreach($vmContainers in $vmJson)
    {
        $vmName = $vmContainers.Attributes.Properties.Name
        $subscriptionID = $vmContainers.Attributes.Properties.SubscriptionID
        $tenantID = $vmContainers.Attributes.Properties.TenantID
        
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
        
        $size = $vmContainers.Attributes.Properties.Size
        #resource group info
        $resourceGroupName = $vmContainers.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $vmContainers.Attributes.ResourceGroup.Location

        #storage account info
        $storageAccName = $vmContainers.Attributes.Properties.StorageAccountName
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
        
        #availability set
        $availabilitySetName = $vmContainers.Attributes.Properties.AvailabilitySetName

        #image info
        $publisherName = $vmContainers.Attributes.Properties.Publisher
        $offerName = $vmContainers.Attributes.Properties.Offer
        $SKU = $vmContainers.Attributes.Properties.SKU

        #disk info 
        $diskName = $vmContainers.Attributes.Properties.OSDiskName
        $disks = $vmContainers.Attributes.Properties.AttachedDisks
        #network info
        $nics = $vmContainers.Attributes.NetworkInterfaces
        $subnetName = $vmContainers.Attributes.VirtualNetwork.SubnetName
        $vnetName = $vmContainers.Attributes.VirtualNetwork.Name
        $vnetResourceGroup = $vmContainers.Attributes.VirtualNetwork.ResourceGroupName
                
        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
        
        $vmConfig = New-AzureRmVMConfig -VMSize $size -VMName $vmName

        if($availabilitySetName -ne $NULL -and $availabilitySetName -ne "")
        {
            Write-Host "Availability set supplied, getting its ID..."
            $availSetID = (Get-AzureRmAvailabilitySet -Name $availabilitySetName -ResourceGroupName $resourceGroupName).Id
            Write-Host "Availability set ok"
            
            $vmConfig = New-AzureRmVMConfig -VMSize $size -VMName $vmName -AvailabilitySetId $availSetID 
        }
       
        $vmConfig.Location = $resourceGroupLocation

        Write-Host "Checking VM $vmName ..."
        $azureVM = Find-AzureRmResource -ResourceType Microsoft.Compute/virtualMachines -ResourceNameContains $vmName  -ApiVersion 2015-11-01 -ResourceGroupNameContains $resourceGroupName

        if ($azureVM -eq $null -or $azureVM -eq "")
        {
	        Write-Host "VM not found..."
            
            Write-Host "Configuring VM credentials..."
            #credentials
            $credentialType = $vmContainers.Attributes.Properties.Credential.Type
            $credentials = $NULL
            if($credentialType -eq "Standard")
            {
                $username = $vmContainers.Attributes.Properties.Credential.Username
	            $password = ConvertTo-SecureString -String $($vmContainers.Attributes.Properties.Credential.Password) -AsPlainText -Force
                $credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $username,$password
            }
        
            if($credentialType -eq "PSCredentialPath")
            {
                $username = Get-Content $vmContainers.Attributes.Properties.Credential.Username
	            $password =  Get-Content $vmContainers.Attributes.Properties.Credential.Password  | ConvertTo-SecureString
                $credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $username,$password
            }

            if($credentials -eq $NULL)
            {
                throw "VM credential either invalid or not supplied!"
            }

            Write-Host "Checking VNET subnet..."
            $subnets = Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $vnetResourceGroup | Select Subnets
            foreach($subnet in $subnets.Subnets)
            {
                if($subnet.Name -eq $subnetName)
                {
                    Write-Host "Add VM to subnet $($subnet.Name) id $($subnet.Id)..."

                    Write-Host "Setting up NIC..."
                    

                    foreach($nicItem in $nics)
                    {
                        Write-Host "Setting up NIC... $($nicItem.Name)"
                        $hasPublicIP = ($nicItem.PublicIPAddress -ne "" -and $nicItem.PublicIPAddress -ne $NULL)
                        
                        if($hasPublicIP)
                        {
                            $publicIpAllocation = $nicItem.PublicIPAddress.AllocationMethod
                            $publicIpName = $nicItem.PublicIPAddress.Name

                            Write-Host "Configuring public IP address..."
                            $pip = New-AzureRmPublicIpAddress -Name $publicIpName -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation -AllocationMethod $publicIpAllocation -Force
                            $pipId = $pip.Id
                            Write-Host "Created public IP address..."
                        }else
                        {
                            $pipId = ""
                        }

                        $privateIp = $nicItem.PrivateIPAddress

                        Write-Host "Configuring NIC..."
                        $nicParams = @{
                            'Location' = $resourceGroupLocation;
                            'ResourceGroupName' = $resourceGroupName;
                            'Name' = $nicItem.Name;
                            'SubnetId' = $subnet.id;
                            'PublicIpAddressId' = $pipId;
                            'PrivateIpAddress' = $privateIp;
                            Force = $true;
                        } 

                        $nic = New-AzureRmNetworkInterface @nicParams
                        Write-Host "NIC configured, adding to VM configuration..."
                        if($nicItem.Primary.ToLower() -eq "true")
                        {
                            Write-Host "Adding primary NIC..."
                            $vmConfig = Add-AzureRmVMNetworkInterface -VM $vmConfig -Id $nic.Id -Primary    
                        }
                        else
                        {
                            Write-Host "Adding secondary NIC..."
                            $vmConfig = Add-AzureRmVMNetworkInterface -VM $vmConfig -Id $nic.Id
                        }
                        
                        Write-Host "VM configuration updated"
                    }
                }
            }

            Write-Host "Setting VM OS..."
            $vmConfig = Set-AzureRmVMOperatingSystem -VM $vmConfig -Windows -ComputerName $vmName -Credential $credentials -ProvisionVMAgent

            Write-Host "Setting Source Image..."
            $vmConfig = Set-AzureRmVMSourceImage -VM $vmConfig -PublisherName $publisherName -Offer $offerName -Skus $SKU -Version "latest"

            Write-Host "Setting OS Disk..."
            $storageAcc = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccName
            $osDiskUri = $storageAcc.PrimaryEndpoints.Blob.ToString() + "vhds/" + $vmName + $diskName  + ".vhd"
            $vmConfig = Set-AzureRmVMOSDisk -VM $vmConfig -Name $diskName -VhdUri $osDiskUri -CreateOption fromImage

            if($disks -ne "" -and $disks -ne $null)
            {
                Write-Host "Setting attached disks..."

                foreach($attachedDisk in $disks)
                {
                        $diskSize= $attachedDisk.Size
                        $diskLabel= $attachedDisk.Label
                        $diskName= $attachedDisk.Name
                        
                        $vhdURI=$storageAcc.PrimaryEndpoints.Blob.ToString() + "vhds/" + $vmName + $diskName  + ".vhd"
                        Add-AzureRmVMDataDisk -VM $vmConfig -Name $diskLabel -DiskSizeInGB $diskSize -VhdUri $vhdURI  -CreateOption empty
                }
            }

            Write-Host "Creating VM..."
	        New-AzureRmVM -ResourceGroupName $ResourceGroupName -Location $resourceGroupLocation -VM $vmConfig
	        Write-Host "VM created created!"
			
        }
        else
        {
            Write-Host "VM already exists, stopping..."

            Stop-AzureRmVM -Name $vmName -ResourceGroupName $resourceGroupName -Force

            Write-Host "VM stopped..."
            Write-Host "Checking VNET subnet..."
            $subnets = Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $vnetResourceGroup | Select Subnets
            foreach($subnet in $subnets.Subnets)
            {
                if($subnet.Name -eq $subnetName)
                {
                    Write-Host "Add VM to subnet $($subnet.Name) id $($subnet.Id)..."

                    foreach($nicItem in $nics)
                    {
                        Write-Host "Setting up NIC... $($nicItem.Name)"
                        $hasPublicIP = ($nicItem.PublicIPAddress -ne "" -and $nicItem.PublicIPAddress -ne $NULL)
                        
                        if($hasPublicIP)
                        {
                            $publicIpAllocation = $nicItem.PublicIPAddress.AllocationMethod
                            $publicIpName = $nicItem.PublicIPAddress.Name

                            Write-Host "Configuring public IP address..."
                            $pip = New-AzureRmPublicIpAddress -Name $publicIpName -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation -AllocationMethod $publicIpAllocation -Force
                            $pipId = $pip.Id
                            Write-Host "Created public IP address..."
                        }else
                        {
                            $pipId = ""
                        }

                        $privateIp = $nicItem.PrivateIPAddress

                        Write-Host "Configuring NIC..."
                        $nicParams = @{
                            'Location' = $resourceGroupLocation;
                            'ResourceGroupName' = $resourceGroupName;
                            'Name' = $nicItem.Name;
                            'SubnetId' = $subnet.id;
                            'PublicIpAddressId' = $pipId;
                            'PrivateIpAddress' = $privateIp;
                            Force = $true;
                        } 

                        $nic = New-AzureRmNetworkInterface @nicParams
                        
                        Write-Host "NIC configured, adding to VM configuration..."
                        if($nicItem.Primary.ToLower() -eq "true")
                        {
                            Write-Host "Adding primary NIC..."
                            $vmConfig = Add-AzureRmVMNetworkInterface -VM $vmConfig -Id $nic.Id -Primary    
                        }
                        else
                        {
                            Write-Host "Adding secondary NIC..."
                            $vmConfig = Add-AzureRmVMNetworkInterface -VM $vmConfig -Id $nic.Id
                        }

                    }
                }
            }

            if($disks -ne "" -and $disks -ne $null)
            {
                Write-Host "Setting attached disks..."

                foreach($attachedDisk in $disks)
                {
                        $diskSize= $attachedDisk.Size
                        $diskLabel= $attachedDisk.Label
                        $diskName= $attachedDisk.Name

                        $storageAcc = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccName
                        $vhdURI=$storageAcc.PrimaryEndpoints.Blob.ToString() + "vhds/" + $vmName + $diskName  + ".vhd"
                        Add-AzureRmVMDataDisk -VM $vmConfig -Name $diskLabel -DiskSizeInGB $diskSize -VhdUri $vhdURI  -CreateOption empty
                }
            }

            Write-Host "VM updating..."
            Update-AzureRmVM -VM $vmConfig -ResourceGroupName $resourceGroupName

            Write-Host "VM updated, starting..."
            Start-AzureRmVM -Name $vmName -ResourceGroupName $resourceGroupName

        }
    }

	Write-Host "Done executing  Publish-DODOAzureVM"
}

Export-ModuleMember -Function 'Publish-DODOAzureVM'

