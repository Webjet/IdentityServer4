Try {
    
    # Make sure if anything goes wrong in the script we get an exception
    $ErrorActionPreference = "Stop"

	$baseDir = resolve-path .

	Write-Host "Base Dir : $baseDir"

		$JsonParameterFileName = $env:JsonParameterFileName
		$JsonTemplateFileName = $env:JsonTemplateFileName
		$DeploymentContainerName = $env:DeploymentContainerName
		$SetParametersFileName = $env:SetParametersFileName
		$DeploymentResourceType = $env:DeploymentResourceType
		$DeploymentUser = $env:DeploymentUser
		$DeploymentPassword = $env:DeploymentPassword
		$DeploymentEnvironment = $env:DeploymentEnvironment
		$version = $env:version
			
	Write-Host "JsonParameterFileName: $JsonParameterFileName"
	Write-Host "JsonTemplateFileName: $JsonTemplateFileName"
	Write-Host "DeploymentContainerName: $DeploymentContainerName"
	Write-Host "SetParametersFileName: $SetParametersFileName"
	Write-Host "DeploymentResourceType: $DeploymentResourceType"
	Write-Host "DeploymentUser: $DeploymentUser"
	Write-Host "DeploymentUser: $DeploymentEnvironment"
	Write-Host "version: $version"

	$DODOJsonParameters = Get-Content -Path "$baseDir\dodo-deployment-scripts\$JsonParameterFileName" -Raw | ConvertFrom-Json
	$DODOJsonTemplate = Get-Content -Path "$baseDir\dodo-deployment-scripts\Templates\$JsonTemplateFileName" -Raw | ConvertFrom-Json

	Write-Host "Importing DODO"
	$dodoExe = "$baseDir\dodo.exe"
	& $dodoExe --export
	Import-Module "$baseDir\DODO\dodo.psd1"
	#Import-Module "$baseDir\Helper.psm1"
	
	$args = @{
		PackagePath = $("$baseDir\Packages.Web.zip")
		SetParametersPath = $("$baseDir\Configuration\$SetParametersFileName")
	}

	$arguments = $args | ConvertTo-json

	#Set Deployment Credentials
	$DODOJsonParameters.Parameters | % {$_.AzureUsername = $DeploymentUser} 
	$DODOJsonParameters.Parameters | % {$_.AzurePassword = $DeploymentPassword}
	$DODOJsonParameters.Parameters | % {$_.ReleaseVersion = $version}
	
	if ($DeploymentResourceType -eq "webapp" )
	{	
		#Deploy Webapp
		Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName $DeploymentContainerName -Command "Publish-DODOAzureWebApp" -Arguments $arguments -ParametersJSONObject $DODOJsonParameters

		Start-sleep -s 30

		#Deploy appsettings 
		$deploymentSlot = ($DODOJsonTemplate.Containers |? { $_.Name -eq $DeploymentContainerName} | Select-Object -First 1 -ExpandProperty Attributes | Select-Object -ExpandProperty Properties | Select-Object -ExpandProperty Slot) 
		Publish-DODOAzureWebAppConfiguration -ConfigurationJSONObject $DODOJsonTemplate -DeploymentSlot $deploymentSlot -ContainerName "$DeploymentContainerName" -ParametersJSONObject $DODOJsonParameters 

		Start-sleep -s 30
		
		#Invoke Warmup
		Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName $DeploymentContainerName -Command "Invoke-DODOAzureWebsiteWarmup" -ParametersJSONObject $DODOJsonParameters

		Start-sleep -s 60
		
		if($DeploymentEnvironment -ne "prod"){
			#Swap
			Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "$DeploymentContainerName" -Command "Switch-DODOAzureWebApp" -ParametersJSONObject $DODOJsonParameters
						Start-sleep -s 60

		}
	}
	Write-Host "Finished the deployment"
 }
Catch
{
	Write-Host "Catch the exception"   
    #throw $_.Exception.Message
    # These give less pretty/helpful error messages
	Write-Host $_.Exception.Message	
    Write-Host $_.Exception
	Write-Host $_.InvocationInfo.ScriptLineNumber
	Write-Host $_.InvocationInfo.OffsetInLine
    Write-Host $_.Exception.Message	
	Exit(1)
}