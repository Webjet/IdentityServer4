
#https://azure.microsoft.com/en-us/documentation/articles/load-balancer-get-started-internet-arm-ps/

function Publish-DODOAzureLoadBalancer
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	
    Write-Host "Executing Publish-DODOAzureLoadBalancer"

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
        $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureLoadBalancer" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $deploymentJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureLoadBalancer" }
    }

    if($deploymentJson -eq $NULL)
    {
	    throw "AzureLoadBalancer container not found in json" + $ContainerName
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
        $publicIpAddress = $deployment.Attributes.Properties.PublicIPAddress
        $backendNetworkInterfaces = $deployment.Attributes.BackendPoolNetworkInterfaces
        $frontEndConfigName = $deployment.Attributes.Properties.ResourceManagerProperties.frontendIPConfigurations[0].name
        $backEndConfigName = $deployment.Attributes.Properties.ResourceManagerProperties.backendAddressPools[0].name
        $inboundNatRules = $deployment.Attributes.InboundNatRules
        $inboundNatPools = $deployment.Attributes.InboundNATPools
	    
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
        
        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation

        Write-Host "Checking AzureLoadBalancer dependencies..."

        if($publicIpAddress -ne "" -and $publicIpAddress -ne $NULL)
        {
            Internal-CreatePublicIpAddress -Name $publicIpAddress.Name -Location $resourceGroupLocation -ResourceGroupName $resourceGroupName -ResourceManagerProperties $publicIpAddress.ResourceManagerProperties
        }

        Write-Host "Checking load balancer $($name) ..."
        $lb = Find-AzureRmResource -ResourceNameContains $name -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Network/loadBalancers -ApiVersion 2015-11-01
        
        if ($lb -eq $null -or $lb -eq "")
        {
	        Write-Host "AzureLoadBalancer $name does not exist, creating..."
            New-AzureRmResource -Name $name -Location $resourceGroupLocation -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Network/loadBalancers -ApiVersion 2015-05-01-preview -Force
            Write-Host "AzureLoadBalancer created!"
        }
        else
        {
            Write-Host "AzureLoadBalancer $aseName exists, updating..."
            Set-AzureRmResource -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Network/loadBalancers -ResourceName $name -ApiVersion 2015-05-01-preview -Force
            Write-Host "AzureLoadBalancer updated!"
        }
        
        #Public Ip Address for external load balancer
        if($publicIpAddress -ne "" -and $publicIpAddress -ne $NULL)
        {
            #assign public up to the LB front end
            $pubIp = Get-AzureRmPublicIpAddress -Name $publicIpAddress.Name -ResourceGroupName $resourceGroupName
            
            $lb = Get-AzureRmLoadBalancer -Name $name -ResourceGroupName $resourceGroupName 
            Set-AzureRmLoadBalancerFrontendIpConfig -Name $frontEndConfigName -LoadBalancer $lb -PublicIpAddress $pubIp
            $lb | Set-AzureRmLoadBalancer
        }

        #Important! Get the updated load balancer so it has updated NAT rules!
        $loadBalancer = get-AzureRmLoadBalancer -Name $name -ResourceGroupName $resourceGroupName
        if($backendNetworkInterfaces -ne "" -and $backendNetworkInterfaces -ne $NULL)
        {
            $backend = Get-AzureRmLoadBalancerBackendAddressPoolConfig -Name $backEndConfigName -LoadBalancer $loadBalancer
            foreach($nic in $backendNetworkInterfaces)
            {
                Write-Host "Updating backend pool network interface $($nic.NetworkInterfaceName) ..."
                $backEndNic = Get-AzureRmNetworkInterface -Name $($nic.NetworkInterfaceName) -ResourceGroupName $resourceGroupName
                
                $backEndNic.IpConfigurations[0].LoadBalancerBackendAddressPools=$backend
                Set-AzureRmNetworkInterface -NetworkInterface $backEndNic
                
                Write-Host "NIC Updated!"
            }
        }
        
        if($inboundNatRules -ne "" -and $inboundNatRules -ne $NULL)
        {
            $rulesUpdated = $false
            
            $loadBalancer = get-AzureRmLoadBalancer -Name $name -ResourceGroupName $resourceGroupName
            
            foreach($inboundNatRule in $inboundNatRules)
            {
                Write-Host "Checking inboundNatRule: $($inboundNatRule.Name)..."

                $ruleFound = $false
                foreach($natRuleConfig in $loadBalancer.InboundNatRules)
                {
                    if($natRuleConfig.Name -eq $inboundNatRule.Name)
                    {
                        $ruleFound = $true
                        Write-Host "InboundNatRule exists, updating..."
                        
                        Set-AzureRmLoadBalancerInboundNatRuleConfig -LoadBalancer $loadBalancer -Name $($inboundNatRule.Name) -BackendPort $($inboundNatRule.BackEndPort) -FrontendPort $($inboundNatRule.FrontEndPort) -Protocol $($inboundNatRule.Protocol) -FrontendIpConfiguration $loadBalancer.FrontendIpConfigurations[0]
                        $rulesUpdated = $true
                    }
                }

                if(!$ruleFound)
                {
                    Write-Host "InboundNatRule not found, adding..."

                    $loadBalancer | Add-AzureRmLoadBalancerInboundNatRuleConfig -Name $($inboundNatRule.Name) -FrontendIpConfiguration $loadBalancer.FrontendIpConfigurations[0] -FrontendPort $($inboundNatRule.FrontEndPort) -Protocol $($inboundNatRule.Protocol) -BackendPort $($inboundNatRule.BackEndPort)
                    $rulesUpdated = $true
                }

                if($rulesUpdated)
                {
                    Write-Host "Saving loadbalancer..."
                    $loadBalancer | Set-AzureRmLoadBalancer
                    Write-Host "Load balancer saved!"
                }

                #Important! Get the updated load balancer so it has updated NAT rules!
                $loadBalancer = get-AzureRmLoadBalancer -Name $name -ResourceGroupName $resourceGroupName

                #Set the target if exists!
                if($inboundNatRule.TargetVirtualMachine -ne "" -and $inboundNatRule.TargetVirtualMachine -ne $NULL)
                {
                    Write-Host "Get target VM NIC: $($inboundNatRule.TargetVirtualMachine.NetworkInterface)... check rule : $($inboundNatRule.Name)"

                    $nic = Get-AzureRmNetworkInterface -Name $($inboundNatRule.TargetVirtualMachine.NetworkInterface) -ResourceGroupName $resourceGroupName
                    $natRuleConfigToAssign = Get-AzureRmLoadBalancerInboundNatRuleConfig -Name $inboundNatRule.Name -LoadBalancer $loadBalancer
                    
                    $nicRuleFound = $false
                    foreach($nicRule in $nic.IpConfigurations[0].LoadBalancerInboundNatRules)
                    {
                        Write-Host "Checking NIC: Rules : $($nic.IpConfigurations[0].LoadBalancerInboundNatRules.Count) Rule $($nicRule.Id)"
                      
                        if($natRuleConfigToAssign.Id -eq $nicRule.Id)
                        {
                            $nicRuleFound = $true
                        }
                    }

                    if(!$nicRuleFound)
                    {
                        Write-Host "NAT rule $($inboundNatRule.Name) not assigned to NIC $($nic.Name), assigning..."
                        $nic.IpConfigurations[0].LoadBalancerInboundNatRules.Add($natRuleConfigToAssign)

                        Set-AzureRmNetworkInterface -NetworkInterface $nic
                    }
                }

            }
        }

        if($inboundNatPools -ne "" -and $inboundNatPools -ne $NULL)
        {
            $loadBalancer = get-AzureRmLoadBalancer -Name $name -ResourceGroupName $resourceGroupName
            $poolsUpdated = $false
            foreach($inboundNatPool in $inboundNatPools)
            {
                Write-Host "Checking inboundNatPool: $($inboundNatPool.Name)..."
                
                $poolFound = $false

                foreach($natPoolConfig in $loadBalancer.InboundNatPools)
                {
                    if($natPoolConfig.Name -eq $inboundNatPool.Name)
                    {
                        $poolFound = $true
                        Write-Host "InboundNatPool exists, updating..."
                        
                        Set-AzureRmLoadBalancerInboundNatPoolConfig -BackendPort $($inboundNatPool.BackEndPort) -FrontendPortRangeEnd $($inboundNatPool.FrontEndPortRangeEnd) -FrontendPortRangeStart $($inboundNatPool.FrontEndPortRangeStart) -LoadBalancer $loadBalancer -Name $($inboundNatPool.Name) -Protocol $($inboundNatPool.Protocol) -FrontendIpConfiguration $loadBalancer.FrontendIpConfigurations[0]
                        $poolsUpdated = $true
                    }
                }

                if(!$poolFound)
                {
                    Write-Host "InboundNatPool not found, adding..."

                    $loadBalancer | Add-AzureRmLoadBalancerInboundNatPoolConfig -BackendPort $($inboundNatPool.BackEndPort) -FrontendPortRangeEnd $($inboundNatPool.FrontEndPortRangeEnd) -FrontendPortRangeStart $($inboundNatPool.FrontEndPortRangeStart) -Protocol $($inboundNatPool.Protocol) -Name ($inboundNatPool.Name) -FrontendIpConfiguration $loadBalancer.FrontendIpConfigurations[0]
                    $poolsUpdated = $true
                }

                if($poolsUpdated)
                {
                    Write-Host "Saving loadbalancer..."
                    $loadBalancer | Set-AzureRmLoadBalancer
                    Write-Host "Load balancer saved!"
                }

            }
            
        }
    }

    Write-Host "Done executing  Publish-DODOAzureLoadBalancer"
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
        $domainLabel = ""

        #Default domain label to pub ip name
        if($ResourceManagerProperties.dnsSettings -eq "" -or $ResourceManagerProperties.dnsSettings -eq $NULL)
        {
            $domainLabel = $Name
        }
        else {
            $domainLabel = $ResourceManagerProperties.dnsSettings.domainNameLabel
        }

        New-AzureRmPublicIpAddress -Name $Name -ResourceGroupName $ResourceGroupName -Location $Location -AllocationMethod $ResourceManagerProperties.publicIPAllocationMethod -DomainNameLabel $domainLabel
        Write-Host "AzurePublicIpAddress $Name created"
    }
    else
    {
        Write-Host "AzurePublicIpAddress $Name exists, updating..."
        $ResourceManagerProperties
        
        <#$publicIP = Get-AzureRmPublicIpAddress -Name $Name -ResourceGroupName $ResourceGroupName
        $publicIP.PublicIpAllocationMethod = $ResourceManagerProperties.publicIPAllocationMethod
        
        $publicIP | Set-AzureRmPublicIpAddress#>
        Set-AzureRmResource -PropertyObject $ResourceManagerProperties -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Network/publicIPAddresses -ResourceName $Name -ApiVersion 2016-03-30 -Force

        Write-Host "AzurePublicIpAddress $Name updated!"
    }
}

Export-ModuleMember -Function 'Publish-DODOAzureLoadBalancer'


