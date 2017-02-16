 param(
    [Parameter(Position=0,Mandatory=0)] [string]$JsonParameterFileName = "devtest01.json",
    [Parameter(Position=1,Mandatory=0)] [string]$JsonTemplateFileName = "webapp-template.json",
    [Parameter(Position=2,Mandatory=0)] [string]$SetParametersFileName = "dev-SetParameters.xml",
    [Parameter(Position=3,Mandatory=0)] [string]$DeploymentContainerName = "Admin Portal",
    [Parameter(Position=4,Mandatory=0)] [string]$BuildVersion = "1-Local",
    [Parameter(Position=5,Mandatory=0)] [string]$DeploymentUser = "",
    [Parameter(Position=6,Mandatory=0)] [string]$DeploymentPassword = "",
    [Parameter(Position=6,Mandatory=0)] [string]$PerformSwap = "false", # do not swap by default
    [Parameter(Position=7,Mandatory=0)] [string]$PerformDeploy = "true"
   ,[Parameter(Position=7,Mandatory=0)] [bool]$TagBuild = $false  # do we need it?
)
function Main {
$ErrorActionPreference = "Stop" 
$DebugPreference = "Continue" #can be temporary enabled
Write-Debug "Use `$DebugPreference = `"Continue`" only if you really need to debug. Write-Debug  can save SENSITIVE information to the logs."
try
{
    ImportingModules
	
## Variables set from TeamCity
if($env:DeploymentUser -ne $null)
{
    Write-Host "TeamCity variables used"
    $JsonParameterFileName = Coalesce $env:JsonParameterFileName $JsonParameterFileName
    $JsonTemplateFileName = Coalesce $env:JsonTemplateFileName  $JsonTemplateFileName
    $SetParametersFileName = Coalesce $env:SetParametersFileName $SetParametersFileName  
    $DeploymentContainerName = Coalesce $env:DeploymentContainerName $DeploymentContainerName 
    $DeploymentUser = $env:DeploymentUser
    $DeploymentPassword = $env:DeploymentPassword
    $PerformSwap = Coalesce $env:PerformSwap $PerformSwap
    $PerformDeploy = Coalesce $env:PerformDeploy $PerformDeploy
    $BuildVersion = Coalesce  $env:buildVersion "1.0" 
}
#locally Deployment is subfolder of root, in teamcity it's subfolder of Output
$parentPath = $((Get-Item ($PSScriptRoot)).Parent.FullName)
$OutputPath = if($parentPath.EndsWith("Output")) { $parentPath} else {  "$parentPath\Output"}
  	  
   
    $DODOJsonParameters = Get-Content -Path "$PSScriptRoot\Parameters\$JsonParameterFileName" -Raw | StripComments| ConvertFrom-Json
    $DODOJsonTemplate = Get-Content -Path "$PSScriptRoot\Templates\$JsonTemplateFileName" -Raw | StripComments | ConvertFrom-Json

 Write-Output "DebugPreference : $DebugPreference $($MyInvocation.MyCommand) $(Get-CurrentFileName) $(Get-CurrentLineNumber) "
Write-Output "JsonParameterFileName : $JsonParameterFileName  JsonTemplateFileName : $JsonTemplateFileName "

$ZipFileName = "$OutputPath\AdminPortal.zip" #-$BuildVersion

    $args = @{
    
        PackagePath = $ZipFileName 
        SetParametersPath = $("$parentPath\Configuration\$SetParametersFileName")
    }

    $arguments = $args | ConvertTo-json

    #Set Deployment Credentials
    $DODOJsonParameters.Parameters | % {$_.AzureUsername = $DeploymentUser} 
    $DODOJsonParameters.Parameters | % {$_.AzurePassword = $DeploymentPassword}
    $DODOJsonParameters.Parameters | % {$_.BuildVersion = $BuildVersion}

    if($PerformDeploy -eq "true")
    {
        #Deploy Webapp
        Run-DODO -Command "Publish-DODOAzureWebApp" -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "$DeploymentContainerName"  -Arguments $arguments -ParametersJSONObject $DODOJsonParameters

		#Deploy appsettings
        Publish-DODOAzureWebAppConfiguration -ConfigurationJSONObject $DODOJsonTemplate -DeploymentSlot "staging" -ContainerName "$DeploymentContainerName" -ParametersJSONObject $DODOJsonParameters

		#Invoke Warmup
        Run-DODO  -Command "Invoke-DODOAzureWebsiteWarmup" -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "$DeploymentContainerName"-ParametersJSONObject $DODOJsonParameters
    }
    
    if($PerformSwap -eq "true")
    {
        #Swap
        Run-DODO -Command "Switch-DODOAzureWebApp" -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "$DeploymentContainerName"  -ParametersJSONObject $DODOJsonParameters
    }

    #Tag the build in TeamCity!
    $buildVersion = ($env:buildVersion, '0' -ne $null)[0]
    #only tag build in TeamCity if you are running in TeamCity and have valid build version!
    if($buildVersion -ne '0' -and $TagBuild )
    {
        $teamCityBuildId = ($env:buildId, '0' -ne $null)[0]
        $teamcityServerUrl = ($env:teamcityServerUrl, "" -ne $null)[0]
        . "$PSScriptRoot\build-tag.ps1"
        Write-Host "Tagging build..."
        TagBuild -tagName $buildVersion -buildId $teamCityBuildId -serverUrl $teamcityServerUrl
    }
#   exit $LastExitCode

 
}
Catch
{
   LogErrorAndExit
}
} #end of main
function ImportingModules
{
    Write-Host "Importing DODO"
    $dodoExe = "$PSScriptRoot\dodo.exe"
   & $dodoExe --export
  Write-Debug "Debug : $PSScriptRoot\DODO\Local"

	if (Test-Path "$PSScriptRoot\DODO\Local" ){
	    # overwrite server dodo files with local dodo files (if required)
		Copy-Item -Path "$PSScriptRoot\DODO\Local\*"  -Destination  "$PSScriptRoot\DODO"  
    }
	else{
		Write-Error "$PSScriptRoot\DODO\Local folder not found" -ErrorAction:Continue
	}
  Write-Output "DebugPreference : $DebugPreference"
    Import-Module "$PSScriptRoot\DODO\dodo.psd1"
    Import-Module "$PSScriptRoot\DODO\dodo.psm1" # todo include in dodo.psd1
    Import-Module "$PSScriptRoot\DODO\dodo-Json.psm1" # todo include in dodo.psd1
    Import-Module "$PSScriptRoot\DODO\dodo-general.psm1" # todo include in dodo.psd1
	Import-Module "$PSScriptRoot\DODO\dodo-azure-webapps.psm1" -Force #to force refresh  from local
	
	Write-Output "DODO Modules Imported"
	$installPscx=$false # current install cause  error see https://github.com/Pscx/Pscx/issues/14 
	if ( $installPscx -and (Get-Module -ListAvailable -Name "PowerShellGet")) {
		Write-Output "Importing PowerShell Community Extensions (PSCX)  Module"
	#	Administrator rights are required to install modules in 'C:\Program Files\WindowsPowerShell\Modules'.
	#Log on to the computer with an account that has Administrator rights,try running the Windows PowerShell session with elevated rights (Run as Administrator) 
	# Or adding "-Scope CurrentUser" to your command.  
		Install-Module -Name Pscx -Scope CurrentUser #from https://www.powershellgallery.com/packages/Pscx/3.2.2 
		Import-Module -Name Pscx
		Write-Output "PowerShell Community Extensions (PSCX)  Module Imported"
    }
	else{ 
	  #"Unable To use Install-Module Required V5 - Current Version $PSVersionTable.PSVersion" 
	  #use local $PSScriptRoot\Modules\ instead
		Import-Module "$PSScriptRoot\Modules\Pscx\3.2.2\Pscx.psd1"
		Write-Output "PowerShell Community Extensions (PSCX)  Module Imported locally"
		#Write-Output "Unable To Install PowerShell Community Extensions (PSCX)Required V5 - Current Version $PSVersionTable.PSVersion"
	}
 Write-Output "DebugPreference : $DebugPreference $(Get-CurrentFileName) $(Get-CurrentLineNumber) "
}
function LogErrorAndExit{
   try {
        if (Get-Module -Name "Pscx") {
          Resolve-ErrorRecord
	    }
		else{
			Write-Error $_.Exception -ErrorAction:Continue
		}
		get-module | Remove-Module -Force
	    Exit(-1)
	}
	Catch
	{
		Write-Error $_.Exception -ErrorAction:Continue
	    Exit(-1)
	}
}

get-module | Remove-Module -Force #to verify why local deployment not working
Main
if ($host.name -eq 'ConsoleHost') 
{
  Read-Host -Prompt "Press_Enter_to_continue"
}