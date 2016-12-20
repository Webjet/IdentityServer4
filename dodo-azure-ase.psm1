#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureAppServiceEnvironment
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	
    Write-Host "Executing Publish-DODOAzureAppServiceEnvironment"

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
        $aseJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureAppServiceEnvironment" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $aseJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureAppServiceEnvironment" }
    }

    if($aseJson -eq $NULL)
    {
	    throw "AzureAppServiceEnvironment container not found in json" + $ContainerName
    }
    #endregion
	
    foreach($aseContainers in $aseJson)
    {
        $aseName = $aseContainers.Attributes.Properties.Name
        $subscriptionID = $aseContainers.Attributes.Properties.SubscriptionID
        $tenantID = $aseContainers.Attributes.Properties.TenantID
        $resourceGroupName = $aseContainers.Attributes.ResourceGroup.Name
        $resourceGroupLocation = $aseContainers.Attributes.ResourceGroup.Location
        $rmProperties = $aseContainers.Attributes.Properties.ResourceManagerProperties

	    Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
       
        Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation

        Write-Host "Checking app service environment $aseName ..."
         
        $ase = Find-AzureRmResource -ResourceGroupName $resourceGroupName -ResourceNameContains $aseName -ResourceType Microsoft.Web/hostingEnvironments -ApiVersion 2015-11-01
        
        if ($ase -eq $null -or $ase -eq "")
        {
	        Write-Host "Azure app service environment $aseName does not exist, creating... (This may take up to 2 hours!)"
            New-AzureRmResource -Name $aseName -Location $resourceGroupLocation -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/hostingEnvironments -ApiVersion 2015-08-01 -Force     
	        Write-Host "Azure app service environment created!"
        }
        else
        {
            Write-Host "Azure app service environment $aseName exists...updating... (This may take up to 2 hours!)"
            Set-AzureRmResource -PropertyObject $rmProperties -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/hostingEnvironments -ResourceName $aseName -ApiVersion 2015-08-01 -Force
            Write-Host "Azure app service environment updated!"
        }
    }

    Write-Host "Done executing  Publish-DODOAzureAppServiceEnvironment"
}

Export-ModuleMember -Function 'Publish-DODOAzureAppServiceEnvironment'


