#Note: Remember to set WEBSITE_WEBDEPLOY_USE_SCM to false on Azure Management Portal Configuration settings under App Settings
#check if the context is Octopus or ISE
$isOctopus = Test-Path variable:global:OctopusParameters

if(!$isOctopus){
    Write-Host "Context: Powershell script ISE"
    
	#These below variables need to be defined in Octopus for the script to run in Octo successfully
    $SetParametersFile = "DevOpsDeploymentFramework-SetParameters.xml"
    $DODOJsonTemplate = Get-Content -Path "$PSScriptRoot\Configuration\dev.json" -Raw | ConvertFrom-Json

	#$OctopusAzureUserName = ""
	#$OctopusAzurePassword = "
	$DODOJsonParameters = @"
	{
		"Parameters": 
		{
			"AzureUserName" : "$($OctopusAzureUserName)",
			"AzurePassword" : "$($OctopusAzurePassword)"
		}
}

"@ | ConvertFrom-Json

}else{
    Write-Host "Context: Octopus" #Octopus will fill out the parameters! :)
	$DODOJsonTemplate = $DODODeploymentTemplate | ConvertFrom-Json
	$DODOJsonParameters = $OctopusDODOParameters | ConvertFrom-Json
}

$DODOJsonTemplate

Write-Host "Importing DODO"
$dodoExe = "$PSScriptRoot\dodo.exe"
& $dodoExe --export
Import-Module "$PSScriptRoot\DODO\dodo.psd1"

$args = @{
    PackagePath = $("$PSScriptRoot\Website\SampleWebApp.zip")
    SetParametersPath = $("$PSScriptRoot\Configuration\$SetParametersFile")
}

$arguments = $args | ConvertTo-json
    
#Deploy
Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "Sample Azure Web App" -Command "Publish-DODOAzureWebApp" -Arguments $arguments -ParametersJSONObject $DODOJsonParameters

#Invoke Warmup
Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "Sample Azure Web App" -Command "Invoke-DODOAzureWebsiteWarmup" -ParametersJSONObject $DODOJsonParameters

#Swap
Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "Sample Azure Web App" -Command "Switch-DODOAzureWebApp" -ParametersJSONObject $DODOJsonParameters
