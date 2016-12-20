
function Publish-DODOAzureAutomationAccount
{
	[CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		 [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
		 [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
     )

	Write-Host "Executing Publish-DODOAzureAutomationAccount"
	
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
        $automationJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureAutomationAccount" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $automationJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureAutomationAccount" }
    }

    if($automationJson -eq $NULL)
	{
		throw "AzureAutomationAccount container not found in json" + $ContainerName
	}
	
	foreach($automationContainers in $automationJson)
	{
		$name = $automationContainers.Attributes.Properties.Name
		$subscriptionId = $automationContainers.Attributes.Properties.SubscriptionID
		$resourceGroupName = $automationContainers.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $automationContainers.Attributes.ResourceGroup.Location
		$tenantId = $automationContainers.Attributes.Properties.TenantID
        $plan = $automationContainers.Attributes.Properties.Plan
		
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
		Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 
		
		Write-Host "Checking azure automation account..."
		$automationAccount = Get-AzureRmAutomationAccount -Name $name -ResourceGroupName $resourceGroupName  -ErrorVariable e -ErrorAction SilentlyContinue
		if ($e[0] -ne $null)
		{
			if($e[0] -Match "ResourceNotFound")
			{
				Write-Host "Azure automation account not found, creating $($name)..."
				New-AzureRmAutomationAccount -ResourceGroupName $resourceGroupName -Name $name -Location $resourceGroupLocation -Plan $plan
				Write-Host "Azure automation account created successfully!"
			}
			else
			{
				throw "Unable to create automation account -" + $e
			}
		}
        else
        {
            Write-Host "Azure automation account $name already exists"
        }
        
        #Create automation credentials
        $automationCredentials = $automationContainers.Attributes.AutomationCredentials
        
        if($automationCredentials -ne $NULL -and $automationCredentials -ne "")
        {
           foreach($automationCredential in $automationCredentials)
           {
                $credentialName = $automationCredential.Name
                Write-Host "Checking azure automation credential $credentialName ..."
                $azureCredential = Get-AzureRmAutomationCredential -AutomationAccountName $name -Name $credentialName -ResourceGroupName $resourceGroupName  -ErrorVariable c -ErrorAction SilentlyContinue
		        if ($c[0] -ne $null -or $azureCredential -eq $NULL)
		        {
			        if($c[0] -Match "ResourceNotFound" -or $azureCredential -eq $NULL)
			        {
				        Write-Host "Azure automation credential not found, creating $credentialName ..."

                        $username = Read-Host "Please enter username for Microsoft account $credentialName :"
                        $passwordSecure = Read-Host "Please enter password for Microsoft account $credentialName :" -AsSecureString
                        $credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $username,$passwordSecure
				        
                        New-AzureRmAutomationCredential -AutomationAccountName $name -Name $credentialName -ResourceGroupName $resourceGroupName -Value $credentials

				        Write-Host "Azure automation credential created successfully!"
			        }
			        else
			        {
				        throw "Unable to create automation credential -" + $c
			        }
		        }
                else
                {
                    Write-Host "Azure automation credential $($automationCredential.Name) already exists"
                }
           }
        }
    }

	 Write-Host "Done executing Publish-DODOAzureAutomationAccount"
}

Export-ModuleMember -Function 'Publish-DODOAzureAutomationAccount'