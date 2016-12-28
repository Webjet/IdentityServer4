#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

#security groups
#https://azure.microsoft.com/en-us/documentation/articles/virtual-networks-nsg/
#https://azure.microsoft.com/en-us/documentation/articles/virtual-networks-create-nsg-arm-ps/

###########################################
#Azure RM V2 VNET
###########################################

function Publish-DODOAzureVNet
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureVNet"
	
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
        $vnetJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVirtualNetwork" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $vnetJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVirtualNetwork" }
    }

    if($vnetJson -eq $NULL)
    {
	    throw "AzureVirtualNetwork container not found in json" + $ContainerName
    }
    #endregion
	
    foreach($vnetContainers in $vnetJson)
    {
        $vnetName = $vnetContainers.Attributes.Properties.Name
        $subscriptionID = $vnetContainers.Attributes.Properties.SubscriptionID
        $tenantID = $vnetContainers.Attributes.Properties.TenantID
        $addressPrefix = $vnetContainers.Attributes.Properties.AddressPrefix
        $resourceGroupName = $vnetContainers.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $vnetContainers.Attributes.ResourceGroup.Location
        
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID

        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
        
        Write-Host "Registering Microsoft.Network provider..."
		Register-AzureRmResourceProvider -ProviderNamespace "Microsoft.Network" -Force

        Write-Host "Checking virtual network $vnetName ..."
        $vnet = Find-AzureRmResource -ResourceGroupName $resourceGroupName -ResourceNameContains $vnetName -ResourceType Microsoft.Network/virtualnetworks -ApiVersion 2015-11-01
        
        if ($vnet -eq $null -or $vnet -eq "")
        {
	        Write-Host "Azure virtual network $vnetName does not exist, creating..."
	        $vnet = New-AzureRmVirtualNetwork -Location $resourceGroupLocation -ResourceGroupName $resourceGroupName -Name $vnetName -AddressPrefix $addressPrefix
	        
        }
        else
        {
            Write-Host "Azure virtual network $vnetName exists!"
        }

        $securityGroups = $vnetContainers.Attributes.NetworkSecurityGroups
        
        foreach($securityGroup in $securityGroups)
        {
            Internal-CreateNetworkSecurityGroup -Name $($securityGroup.Name) -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation -Diagnostics $securityGroup.Diagnostics

            $rules = $securityGroup.Rules
            if($rules -ne $null -and $rules -ne "")
            {
                foreach($rule in $rules)
                {
                    Internal-CreateNSGRule -NSGRule $rule -NSGName $($securityGroup.Name) -NSGResourceGroupName $resourceGroupName
                }
            }
        }

        $subnets = $vnetContainers.Attributes.Subnets

        foreach($subnet in $subnets)
        {
            Internal-CreateVNetSubnet -Name $($subnet.Name) -AddressPrefix $($subnet.AddressPrefix) -VirtualNetworkName $vnetName -ResourceGroupName $resourceGroupName -NetworkSecurityGroupName $($subnet.NetworkSecurityGroup)
        }

    }

	Write-Host "Done executing  Publish-DODOAzureVNet"
}

function Internal-CreateVNetSubnet
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1)] [string]$Name,
		[Parameter(Position=1,Mandatory=1)] [string]$AddressPrefix,
        [Parameter(Position=2,Mandatory=1)] [string]$VirtualNetworkName,
        [Parameter(Position=3,Mandatory=1)] [string]$ResourceGroupName,
        [Parameter(Position=4,Mandatory=0)] [string]$NetworkSecurityGroupName
     )
    
    $VirtualNetwork = (Get-AzureRmVirtualNetwork -Name $VirtualNetworkName -ResourceGroupName $ResourceGroupName)

    Write-Host "Checking virtual network : $($VirtualNetwork.Name) for subnet $Name ..."
    $subnets = Get-AzureRmVirtualNetworkSubnetConfig -VirtualNetwork $VirtualNetwork 

    $foundSubnet = $false
    if($subnets -ne $null)
    {
        foreach($subnet in $subnets)
        {
            if($subnet.Name -eq $Name)
            {
                $foundSubnet = $true
            }
        }
    }

    if($foundSubnet)
    {
        Write-Host "Azure subnet $Name exists, updating subnet $Name on virtual network $($VirtualNetwork.Name)..."
        Set-AzureRmVirtualNetworkSubnetConfig -AddressPrefix $AddressPrefix -Name $Name -VirtualNetwork $VirtualNetwork
        Write-Host "Subnet updated!"
    }
    else
    {
        Write-Host "Azure subnet $Name does not exist, creating for virtual network $($VirtualNetwork.Name)..."
	    $VirtualNetwork = Add-AzureRmVirtualNetworkSubnetConfig -AddressPrefix $AddressPrefix -Name $Name -VirtualNetwork $VirtualNetwork
        Set-AzureRmVirtualNetwork -VirtualNetwork $VirtualNetwork
        Write-Host "Azure subnet created and added to virtual network!"
    }

    if($NetworkSecurityGroupName -ne $null -and $NetworkSecurityGroupName -ne "")
    {
        Write-Host "Assigning NSG : $NetworkSecurityGroupName to subnet $Name ..."
        $NSG = (Get-AzureRmNetworkSecurityGroup -Name $NetworkSecurityGroupName -ResourceGroupName $ResourceGroupName -ErrorAction Stop)
        if($NSG -ne $null)
        {
            Set-AzureRmVirtualNetworkSubnetConfig -AddressPrefix $AddressPrefix -Name $Name -VirtualNetwork $VirtualNetwork -NetworkSecurityGroup $NSG
            Set-AzureRmVirtualNetwork -VirtualNetwork $VirtualNetwork
        }
        else
        {
            "Cannot find NSG to apply to subnet"
        }
    }
}

function Internal-CreateNetworkSecurityGroup
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1)] [string]$Name,
        [Parameter(Position=1,Mandatory=1)] [string]$Location,
        [Parameter(Position=2,Mandatory=1)] [string]$ResourceGroupName,
        [Parameter(Position=3,Mandatory=0)] [PSCustomObject]$Diagnostics
     )

    Write-Host "Checking Network Security Group $Name ..."
    $nsg = Find-AzureRmResource -ResourceNameContains $Name -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Network/networkSecurityGroups -ApiVersion 2015-11-01

    if ($nsg -eq $NULL -or $nsg -eq "")
    {
	    Write-Host "Network security group $Name does not exist, creating..."
        
        $PropertiesObject = @{}
        New-AzureRmResource -Name $Name -Location $Location -PropertyObject $PropertiesObject -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Network/networkSecurityGroups -ApiVersion 2015-05-01-preview -Force
	    Write-Host "Network security group created!"
       
    }
    else
    {
        Write-Host "Network security group $Name exists!"
    }
    
    if($Diagnostics -ne "" -and $Diagnostics -ne $NULL)
    {
        Write-Host "Registering Microsoft.Insights provider..."
		Register-AzureRmResourceProvider -ProviderNamespace "Microsoft.Insights" -Force
        
        Write-Host "Applying diagnostics on NSG: $Name to storage account: $($Diagnostics.StorageAccountName) resourcegroup : $($Diagnostics.ResourceGroupName) ..."
        
        $nsgID = (Get-AzureRmNetworkSecurityGroup -Name $Name -ResourceGroupName $ResourceGroupName).Id
        $storageAccountID = (Get-AzureRmStorageAccount -Name $($Diagnostics.StorageAccountName) -ResourceGroupName $($Diagnostics.ResourceGroupName)).Id
        $retentionEnabled = [System.Convert]::ToBoolean($Diagnostics.RetentionEnabled)
        
        Set-AzureRmDiagnosticSetting -Enabled $true -ResourceId $nsgID -StorageAccountId $storageAccountID -RetentionEnabled $retentionEnabled -RetentionInDays $($Diagnostics.RetentionInDays)
        
        Write-Host "Diagnostic setting applied!"
    }
}

function Internal-CreateNSGRule
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1)] [PSCustomObject]$NSGRuleJson,
        [Parameter(Position=1,Mandatory=1)] [string]$NSGName,
        [Parameter(Position=2,Mandatory=1)] [string]$NSGResourceGroupName
     )

    Write-Host "Checking network security group $NSGName ..."
    $NSG = (Get-AzureRmNetworkSecurityGroup -Name $NSGName -ResourceGroupName $NSGResourceGroupName)

    $rules = Get-AzureRmNetworkSecurityRuleConfig -NetworkSecurityGroup $NSG

    $foundRule = $false
    if($rules -ne $null)
    {
        foreach($rule in $rules)
        {
            if($rule.Name -eq $NSGRuleJson.Name)
            {
                $foundRule = $true
            }
        }
    }

    $ruleSplat = @{
        "Name" = $NSGRuleJson.Name;
		"Description" = $NSGRuleJson.Description;
		"Access" = $NSGRuleJson.Access;
		"Protocol" = $NSGRuleJson.Protocol;
		"Direction" = $NSGRuleJson.Direction;
		"Priority" = $NSGRuleJson.Priority;
		"SourceAddressPrefix" = $NSGRuleJson.SourceAddressPrefix;
		"SourcePortRange" = $NSGRuleJson.SourcePortRange;
		"DestinationAddressPrefix" = $NSGRuleJson.DestinationAddressPrefix;
		"DestinationPortRange" = $NSGRuleJson.DestinationPortRange;
    }

    if($foundRule)
    {
        #UPDATE
        Write-Host "Network security group rule $($NSGRuleJson.Name) exists on NSG $NSGName, updating ..."
        Set-AzureRmNetworkSecurityRuleConfig @ruleSplat -NetworkSecurityGroup $NSG
        Set-AzureRmNetworkSecurityGroup -NetworkSecurityGroup $NSG
        Write-Host "Rule updated!"
    }
    else
    {
        #CREATE
        Write-Host "Network security group rule $($NSGRuleJson.Name) does not exist on NSG $NSGName, creating..."
	    $ruleItem = Add-AzureRmNetworkSecurityRuleConfig @ruleSplat -NetworkSecurityGroup $NSG
        Set-AzureRmNetworkSecurityRuleConfig @ruleSplat -NetworkSecurityGroup $NSG
        Set-AzureRmNetworkSecurityGroup -NetworkSecurityGroup $NSG
        Write-Host "Network security group rule created and added to NSG!"
    }
}

###########################################
#Azure Classic VNET
###########################################

function Publish-DODOAzureClassicVnet
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )

    Write-Host "Executing Publish-DODOAzureClassicVnet"
	
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
        $vnetJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureClassicVirtualNetwork" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $vnetJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureClassicVirtualNetwork" }
    }

    if($vnetJson -eq $NULL)
    {
	    throw "AzureClassicVirtualNetwork container not found in json" + $ContainerName
    }
    #endregion

    foreach($vnetContainers in $vnetJson)
    {
        $vnetName = $vnetContainers.Attributes.Properties.Name
        $subscriptionID = $vnetContainers.Attributes.Properties.SubscriptionID
        $tenantID = $vnetContainers.Attributes.Properties.TenantID
        $rmProperties = $vnetContainers.Attributes.Properties.ResourceManagerProperties
        $resourceGroupName = $vnetContainers.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $vnetContainers.Attributes.ResourceGroup.Location

		Select-AzureRmSubscription -SubscriptionId $subscriptionID

        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation

        Write-Host "Checking classic virtual network $vnetName ..."
        $vnet = Find-AzureRmResource -ResourceGroupName $resourceGroupName -ResourceNameContains $vnetName -ResourceType microsoft.classicnetwork/virtualNetworks -ApiVersion 2015-11-01
        
        if ($vnet -eq $null -or $vnet -eq "")
        {
	        Write-Host "Classic Azure virtual network $vnetName does not exist, creating..."
            
            New-AzureRmResource -Name $vnetName -Location $resourceGroupLocation -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType microsoft.classicnetwork/virtualNetworks -ApiVersion 2015-06-01 -Force     
	        Write-Host "Classic Azure virtual network created!"
        }
        else
        {
            Write-Host "Azure virtual network $vnetName exists...updating"
 
            Set-AzureRmResource -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType microsoft.classicnetwork/virtualNetworks -ResourceName $vnetName -ApiVersion 2015-06-01 -Force

            Write-Host "Classic VNET settings updated!"
        }

        Write-Host "Processing security groups..."
        $securityGroups = $vnetContainers.Attributes.NetworkSecurityGroups

        foreach($securityGroup in $securityGroups)
        {
            Internal-CreateClassicNetworkSecurityGroup -Name $($securityGroup.Name) -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation

            $rules = $securityGroup.Rules
            if($rules -ne $null -and $rules -ne "")
            {
                foreach($rule in $rules)
                {
                    Internal-CreateClassicNSGRule -NSGRule $rule -NSGName $($securityGroup.Name) -NSGResourceGroupName $resourceGroupName
                }
            }
            
        }

        $subnets = $vnetContainers.Attributes.Subnets

    }
}

###########################################
#Azure Classic NSG
###########################################

function Internal-CreateClassicNetworkSecurityGroup
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1)] [string]$Name,
        [Parameter(Position=0,Mandatory=1)] [string]$ResourceGroupName,
        [Parameter(Position=1,Mandatory=1)] [string]$Location

     )

    #Note : Overwrite $ResourceGroupName to "Default-Networking" as classic does not support custom resourcegroups
    $ResourceGroupName = "Default-Networking"

    Write-Host "Checking Classic network Security Group $Name ..."
    $nsg = Find-AzureRmResource -ResourceGroupName $ResourceGroupName -ResourceNameContains $Name -ResourceType Microsoft.ClassicNetwork/networkSecurityGroups  -ApiVersion 2015-11-01

    if ($nsg -eq "" -or $nsg -eq $NULL)
    {
            $PropertyObject = @{}
	        Write-Host "Classic network security group $Name does not exist, creating..."
	        New-AzureRmResource -Name $Name -Location $Location -PropertyObject $PropertyObject -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.ClassicNetwork/networkSecurityGroups -ApiVersion 2015-12-01 -Force
	        Write-Host "Network security group created!"
    }
    else
    {
        Write-Host "Network security group $Name exists!"
    }
}

###########################################
#Azure Classic Security Rules
###########################################

function Internal-CreateClassicNSGRule
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1)] [PSCustomObject]$NSGRuleJson,
        [Parameter(Position=1,Mandatory=1)] [string]$NSGName,
        [Parameter(Position=2,Mandatory=1)] [string]$NSGResourceGroupName
     )

    #Note : Overwrite $NSGResourceGroupName to "Default-Networking" as classic does not support custom resourcegroups
    $NSGResourceGroupName = "Default-Networking"
   
    Write-Host "Checking network security group $NSGName ..."
    $NSG = (Get-AzureNetworkSecurityGroup -Name $NSGName) 

    $rules = $NSG.Rules

    $ruleSplat = @{
        "Action" = $NSGRuleJson.Action;
        "DestinationAddressPrefix" = $NSGRuleJson.DestinationAddressPrefix;
        "DestinationPortRange" = $NSGRuleJson.DestinationPortRange;
        "Name" = $NSGRuleJson.Name;
        "Priority" = $NSGRuleJson.Priority;
		"Protocol" = $NSGRuleJson.Protocol;
		"Type" = $NSGRuleJson.Type;
		"SourceAddressPrefix" = $NSGRuleJson.SourceAddressPrefix;
		"SourcePortRange" = $NSGRuleJson.SourcePortRange;
    }

    Write-Host "Processing rule $($NSGRuleJson.Name) on NSG $($NSGName) ..."
    Set-AzureNetworkSecurityRule @ruleSplat -NetworkSecurityGroup $NSG
    Write-Host "Rule $($NSGRuleJson.Name) updated"    
}

function Publish-DODOAzureVirtualNetworkGateway
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureVirtualNetworkGateway"
	
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVirtualNetworkGateway" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVirtualNetworkGateway" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureVirtualNetworkGateway container not found in json" + $ContainerName
    }
    #endregion
	
    foreach($container in $json)
    {
        $subscriptionID = $container.Attributes.Properties.SubscriptionID
		$resourceGroupName = $container.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $container.Attributes.ResourceGroup.Location
		$tenantID = $container.Attributes.Properties.TenantID
		$name = $container.Attributes.Properties.Name
		$ipconfigName = $container.Attributes.Properties.IpConfigName
		$subnetName = $container.Attributes.Properties.Subnet
        $publicIp = $container.Attributes.PublicIPAddress
        $virtualNetwork = $container.Attributes.VirtualNetwork
        $gatewayType = $container.Attributes.Properties.GatewayType
        $vpnType = $container.Attributes.Properties.VpnType
        $sku = $container.Attributes.Properties.GatewaySku
        $connections = $container.Attributes.Connections
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
		Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 

        Write-Host "Fetching public IP $($publicIp.Name)..."

        $pubIp = Get-AzureRmPublicIpAddress -Name $publicIp.Name -ResourceGroupName $publicIp.ResourceGroupName

        if($pubIp -eq "" -or $pubIp -eq $null)
        {
            throw "Cannot find public ip address $($publicIp.Name) under resource group $($publicIp.ResourceGroupName)"
        }
		
        Write-Host "Fetching virtual network $($virtualNetwork.Name)..."

        $vnet = Get-AzureRmVirtualNetwork -Name $virtualNetwork.Name -ResourceGroupName $virtualNetwork.ResourceGroupName
        
        if($vnet -eq "" -or $vnet -eq $null)
        {
            throw "Cannot find virtual network $($virtualNetwork.Name) under resource group $($virtualNetwork.ResourceGroupName)"
        }

        Write-Host "Fetching subnet $($subnetName)..."
        $subnet = Get-AzureRmVirtualNetworkSubnetConfig -Name "GatewaySubnet" -VirtualNetwork $vnet
        
        Write-Host "Creating Gateway ip config..."
        $ipconfig = New-AzureRmVirtualNetworkGatewayIpConfig -Name $ipconfigName -Subnet $subnet -PublicIpAddress $pubIp

        Write-Host "Checking virtual network gateway $($name)"
        $gateway = Get-AzureRmVirtualNetworkGateway -Name $name -ResourceGroupName $resourceGroupName -ErrorVariable e -ErrorAction SilentlyContinue
		if ($e[0] -ne $null)
		{
			if($e[0] -Match "was not found")
			{
				Write-Host "Creating a new VPN gateway named $($name)..."
				New-AzureRmVirtualNetworkGateway -Name $name -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation -IpConfigurations $ipconfig -GatewayType $gatewayType -VpnType $vpnType -GatewaySku $sku 
				Write-Host "VPN gateway created successfully!"
			}
			else
			{
				throw "Unable to create VPN gateway -" + $e
			}
		}
        else {
            Write-Host "Virtual network gateway exists."

        }

        if($connections -ne "" -and $connections -ne $null)
        {
            foreach($connection in $connections)
            {
                Write-Host "Performing VPN connection $($connection.Name)..."
                $vnet1 = Get-AzureRmVirtualNetworkGateway -Name $connection.VirtualNetworkGateway1.Name -ResourceGroupName $connection.VirtualNetworkGateway1.ResourceGroupName
                $vnet2 = Get-AzureRmVirtualNetworkGateway -Name $connection.VirtualNetworkGateway2.Name -ResourceGroupName $connection.VirtualNetworkGateway2.ResourceGroupName
                
                $vpnConnection = Get-AzureRmVirtualNetworkGatewayConnection -Name $connection.Name -ResourceGroupName $connection.ResourceGroupName -ErrorVariable f -ErrorAction SilentlyContinue
                if ($f[0] -ne $null)
                {
                    if($f[0] -Match "was not found")
                    {
                        Write-Host "Creating a new VPN connection named $($connection.Name)..."
                        New-AzureRmVirtualNetworkGatewayConnection -Name $connection.Name -ResourceGroupName $connection.ResourceGroupName -VirtualNetworkGateway1 $vnet1 -VirtualNetworkGateway2 $vnet2 -Location $connection.Location -ConnectionType $connection.ConnectionType -SharedKey $connection.SharedKey 
                        Write-Host "VPN connection created successfully!"
                    }
                    else
                    {
                        throw "Unable to create VPN gateway -" + $f
                    }
                }
                else {
                    Write-Host "VPN connection exists."
                }


        
            }
        }

        
    }

     Write-Host "Done executing Publish-DODOAzureVirtualNetworkGateway"
}

function Publish-DODOAzureVirtualNetworkGatewayConnection
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureVirtualNetworkGatewayConnection"
	
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVirtualNetworkGatewayConnection" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureVirtualNetworkGatewayConnection" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureVirtualNetworkGatewayConnection container not found in json" + $ContainerName
    }
    #endregion
	
    foreach($container in $json)
    {
        $subscriptionID = $container.Attributes.Properties.SubscriptionID
		$resourceGroupName = $container.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $container.Attributes.ResourceGroup.Location
		$tenantID = $container.Attributes.Properties.TenantID
		$name = $container.Attributes.Properties.Name
        $vnetGateway1 = $container.Attributes.Properties.VirtualNetworkGateway1
        $vnetGateway2 = $container.Attributes.Properties.VirtualNetworkGateway2
        $connectionType = $container.Attributes.Properties.ConnectionType
        $sharedKey = $container.Attributes.Properties.SharedKey

        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
		Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 

        Write-Host "Performing VPN connection $name..."
        $gw1 = Get-AzureRmVirtualNetworkGateway -Name $vnetGateway1.Name -ResourceGroupName $vnetGateway1.ResourceGroupName
        $gw2 = Get-AzureRmVirtualNetworkGateway -Name $vnetGateway2.Name -ResourceGroupName $vnetGateway2.ResourceGroupName
        
        $vpnConnection = Get-AzureRmVirtualNetworkGatewayConnection -Name $name -ResourceGroupName $resourceGroupName -ErrorVariable f -ErrorAction SilentlyContinue
        if ($f[0] -ne $null)
        {
            if($f[0] -Match "was not found")
            {
                Write-Host "Creating a new VPN connection named $name..."
                New-AzureRmVirtualNetworkGatewayConnection -Name $name -ResourceGroupName $resourceGroupName -VirtualNetworkGateway1 $gw1 -VirtualNetworkGateway2 $gw2 -Location $resourceGroupLocation -ConnectionType $connectionType -SharedKey $sharedKey 
                Write-Host "VPN connection created successfully!"
            }
            else
            {
                throw "Unable to create VPN gateway connection -" + $f
            }
        }
        else {
            Write-Host "VPN connection exists."
        }
    }

     Write-Host "Done executing Publish-DODOAzureVirtualNetworkGatewayConnection"
}

Export-ModuleMember -Function 'Publish-DODOAzureVNet'
Export-ModuleMember -Function 'Publish-DODOAzureClassicVnet'
Export-ModuleMember -Function 'Publish-DODOAzureVirtualNetworkGateway'
Export-ModuleMember -Function 'Publish-DODOAzureVirtualNetworkGatewayConnection'

