#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx
#https://azure.microsoft.com/en-us/documentation/articles/virtual-machines-ps-create-preconfigure-windows-resource-manager-vms/

function Publish-DODOAzureContainerService
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureContainerService"
	
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureContainerService" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureContainerService" }
    }

    if($json -eq $NULL)
    {
	    throw "AzureContainerService container not found in json" + $ContainerName
    }
	#endregion

    foreach($container in $json)
    {
        #authentication
        $subscriptionID = $container.Attributes.Properties.SubscriptionID
        $tenantID = $container.Attributes.Properties.TenantID
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
        
        #resource group info
        $resourceGroupName = $container.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $container.Attributes.ResourceGroup.Location
        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
        
        #Properties
        $name = $container.Attributes.Properties.Name
        $size = $container.Attributes.Properties.AgentVMSize
        $location = $container.Attributes.Properties.Location
        $agentCount = $container.Attributes.Properties.AgentCount
        $masterCount = $container.Attributes.Properties.MasterCount
        $dnsPrefix = $container.Attributes.Properties.DNSPrefix
        $masterPrefix = $container.Attributes.Properties.MasterDNSPrefix
        $orchestratorType = $container.Attributes.Properties.OrchestratorType
        $sshPublicKey = $container.Attributes.Properties.SSHPublicKey
        $sshUserName = $container.Attributes.Properties.SSHUserName
        $diagnosticsEnabled = [System.Convert]::ToBoolean($container.Attributes.Properties.DiagnosticsEnabled)

        Write-Host "Checking container service $name ..."
        $azureContainerService = Find-AzureRmResource -ResourceType Microsoft.ContainerService/containerServices -ResourceNameContains $name  -ApiVersion 2016-09-01 -ResourceGroupNameContains $resourceGroupName

        if ($azureContainerService -eq $null -or $azureContainerService -eq "")
        {
	        Write-Host "ContainerService not found..."
            Write-Host "Creating configuration..."
 
            $containerServiceConfig = New-AzureRmContainerServiceConfig -Location $location `
                                    -OrchestratorType $orchestratorType `
                                    -MasterCount $masterCount `
                                    -MasterDnsPrefix $masterPrefix `
                                    -SshPublicKey $sshPublicKey `
                                    -AdminUsername $sshUserName `
                                    -VmDiagnosticsEnabled $diagnosticsEnabled
            Write-Host "SSH Username : $sshUserName"                        
            $containerServiceConfig.LinuxProfile.AdminUsername = $sshUserName
            
            Write-Host "Adding agent profiles... $dnsPrefix "                        
            Add-AzureRmContainerServiceAgentPoolProfile -ContainerService $containerServiceConfig -Count $agentCount -Name "agentpools" -VmSize $size -DnsPrefix $dnsPrefix 

            Write-Host "Creating container service..."
            New-AzureRmContainerService -ContainerService $containerServiceConfig -Name $name -ResourceGroupName $resourceGroupName
 
	        Write-Host "Container service created!"
			
        }
        else
        {
            Write-Host "Azure container service already exists!"
        }
    }

	Write-Host "Done executing  Publish-DODOAzureContainerService"
}

Export-ModuleMember -Function 'Publish-DODOAzureContainerService'

