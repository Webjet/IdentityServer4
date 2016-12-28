#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureSQLServer
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureSQLServer"
	 
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
        $sqlJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureSQLServer" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $sqlJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureSQLServer" }
    }

    if($sqlJson -eq $NULL)
    {
	    throw "AzureSQLServer container not found in json" + $ContainerName
    }
	
    foreach($sqlContainers in $sqlJson)
    {
        $sqlServerName = $sqlContainers.Attributes.Properties.Name
        $subscriptionID = $sqlContainers.Attributes.Properties.SubscriptionID
        $tenantID = $sqlContainers.Attributes.Properties.TenantID

        $resourceGroupName = $sqlContainers.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $sqlContainers.Attributes.ResourceGroup.Location
		
        Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID

        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation

        Write-Host "Checking database server $sqlServerName ..."
        $sqlServer = Find-AzureRmResource -ResourceNameContains $sqlServerName -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Sql/servers -ApiVersion 2015-11-01
        
        if ($sqlServer -eq $null -or $sqlServer -eq "")
        {
	        Write-Host "AzureSQL Server $sqlServerName does not exist, creating..."
            
            if($sqlContainers.Attributes.Properties.PSCredential -ne "" -and $sqlContainers.Attributes.Properties.PSCredential -ne $NULL)
            {
                #Keep for backwards compatibility
                $username = Get-Content $sqlContainers.Attributes.Properties.PSCredential.Username
                $password =  Get-Content $sqlContainers.Attributes.Properties.PSCredential.Password  | ConvertTo-SecureString

                $credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $username,$password
            }
            else {
                #using credential type from v1.2.1 onwards
                $credentialType = $sqlContainers.Attributes.Properties.Credential.Type
                $credentials = $NULL
                if($credentialType -eq "Standard")
                {
                    $username = $sqlContainers.Attributes.Properties.Credential.Username
                    $password = ConvertTo-SecureString -String $($sqlContainers.Attributes.Properties.Credential.Password) -AsPlainText -Force
                    $credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $username,$password
                }
            
                if($credentialType -eq "PSCredentialPath")
                {
                    $username = Get-Content $sqlContainers.Attributes.Properties.Credential.Username
                    $password =  Get-Content $sqlContainers.Attributes.Properties.Credential.Password  | ConvertTo-SecureString
                    $credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $username,$password
                }

                if($credentials -eq $NULL)
                {
                    throw "VM credential either invalid or not supplied!"
                }
            }
            
	        New-AzureRmSqlServer -Location $resourceGroupLocation -ResourceGroupName $resourceGroupName -ServerName $sqlServerName -SqlAdministratorCredentials $credentials
	        Write-Host "AzureSQL Server created!"
        }
        else
        {
            Write-Host "AzureSQL Server $sqlServerName exists!"
        }

        Write-Host "Configuring Public IP Address access"
        $publicIpAddresses = $sqlContainers.Attributes.PublicIpAddresses

        if( $publicIpAddresses -ne "" -and $publicIpAddresses -ne $NULL)
        {
            foreach($publicIpAddress in $publicIpAddresses)
            {
                Internal-CreateFirewallRuleForPublicIP -PublicIpAddressName $($publicIpAddress.Name) -ResourceGroupName $($publicIpAddress.ResourceGroupName) -RuleName $($publicIpAddress.RuleName) -SqlServerName $sqlServerName -SqlServerResourceGroupName $resourceGroupName
            }
        }

        $databases = $sqlContainers.Attributes.Databases

        foreach($database in $databases)
        {
            Write-Host "Checking database $($database.Name) ..."

            $dbResource = $sqlServerName + "/" + $database.Name
            $databaseItem = Find-AzureRmResource -ResourceNameContains $dbResource -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Sql/servers/databases -ApiVersion 2015-11-01
           
            if ($databaseItem -eq $null -or $databaseItem -eq "")
            {
                Write-Host "AzureSQL database $($database.Name) does not exist, creating..."
                     
                New-AzureRmSqlDatabase -DatabaseName $database.Name -ResourceGroupName $resourceGroupName -ServerName $sqlServerName

                Write-Host "AzureSQL database created!"
               
            }
            else
            {
                Write-Host "AzureSQL database $($database.Name) exists!"
            }
        }
    }

	Write-Host "Done executing  Publish-DODOAzureSQLServer"
}

function Internal-CreateFirewallRuleForPublicIP
{
    [CmdletBinding()]
    param(
    [Parameter(Position=0,Mandatory=1)] [string]$PublicIpAddressName,
    [Parameter(Position=1,Mandatory=1)] [string]$ResourceGroupName,
    [Parameter(Position=2,Mandatory=1)] [string]$RuleName,
    [Parameter(Position=3,Mandatory=1)] [string]$SqlServerName,
    [Parameter(Position=4,Mandatory=1)] [string]$SqlServerResourceGroupName
    )

    Write-Host "Checking PublicIP $PublicIpAddressName ..."

    $pip = Get-AzureRmPublicIpAddress -Name $PublicIpAddressName -ResourceGroupName $ResourceGroupName -ErrorVariable s -ErrorAction SilentlyContinue

    if ($s[0] -eq $null -and $pip -ne $NULL)
    {
       Write-Host "Public IP $($pip.IpAddress) exists, adding firewall rule $RuleName to server AzureSQL $SqlServerName ..."
       New-AzureRmSqlServerFirewallRule -FirewallRuleName $RuleName -ServerName $SqlServerName -ResourceGroupName $SqlServerResourceGroupName -StartIpAddress $($pip.IpAddress) -EndIpAddress $($pip.IpAddress)
       Write-Host "SQL Azure firewall rule added!" 
    }
    else
    {
        Write-Host "Public IP $PublicIpAddressName does not exist"
    }
}


Export-ModuleMember -Function 'Publish-DODOAzureSQLServer'

