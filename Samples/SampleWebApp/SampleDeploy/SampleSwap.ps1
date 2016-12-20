Import-Module -Name dodo -RequiredVersion 0.0.2

#Note: Remember to set WEBSITE_WEBDEPLOY_USE_SCM to false on Azure Management Portal Configuration settings under App Settings
#check if the context is Octopus or ISE
$isOctopus = Test-Path variable:global:OctopusParameters

if(!$isOctopus){
    Write-Host "Context: Powershell script ISE"
    
	#These below variables need to be defined in Octopus for the script to run in Octo successfully
	$AzureWebAppName = "DevOpsDeploymentFramework"
    $SubscriptionName = "Development" 
    $SubscriptionId = "69a0c839-7052-425a-8213-0453c025c751"
    $AzureServer = "waws-prod-sg1-007.publish.azurewebsites.windows.net:443"

}else{
    Write-Host "Context: Octopus" #Octopus will fill out the parameters! :)
}

Set-DODOAzureAuthentication -SubscriptionName $SubscriptionName -SubscriptionId $SubscriptionId
Switch-DODOAzureWebsite -AzureWebAppName $AzureWebAppName -SlotName "staging"

Remove-Module -Name dodo