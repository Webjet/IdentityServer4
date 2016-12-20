Import-Module -Name dodo -RequiredVersion 0.0.2

#check if the context is Octopus or ISE
$isOctopus = Test-Path variable:global:OctopusParameters

if(!$isOctopus){
    Write-Host "Context: Powershell script ISE"
    
	#These below variables need to be defined in Octopus for the script to run in Octo successfully
	$AzureWebAppName = "DevOpsDeploymentFramework"
	$AzureWebsiteJobName = "SampleWebjob"
    $SubscriptionName = "Development" 
    $SubscriptionId = "69a0c839-7052-425a-8213-0453c025c751"

}else{
    Write-Host "Context: Octopus" #Octopus will fill out the parameters! :)
}

Set-DODOAzureAuthentication -SubscriptionName $SubscriptionName -SubscriptionId $SubscriptionId
Publish-DODOAzureWebjob -AzureWebAppName $AzureWebAppName -AzureWebsiteJobName $AzureWebsiteJobName -Slot "staging" -JobType "Continuous" -JobFile "$PSScriptRoot\Webjobs\SampleWebjob.zip"  

Remove-Module -Name dodo