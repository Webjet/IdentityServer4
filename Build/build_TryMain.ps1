cls
cd $PSScriptRoot
$ErrorActionPreference = "Stop" 
$DebugPreference="Continue"	
function Main {
try {
 Write-Host "Test-Invoke-Executable calling..."
Test-Invoke-Executable
 Write-Host "Test-Invoke-Executable returned"
	
	    $buildTask = ($env:build_task, 'Test' -ne $null)[0]
	    #restoring NuGet packages to get PSAKE
	    $parentPath = $((Get-Item (Resolve-Path .)).Parent.FullName)
	    $solutionFile = "$parentPath\Source\AdminPortal.sln"
	 #	$solutionFile = "$parentPath\Source\AdminPortalCore.sln"# TEMP to remove later
	    $nugetExe = "$PSScriptRoot\Tools\Nuget\Nuget.exe"
	    ImportingModules

	    Write-Host "Restoring packages..."
		.$nugetExe restore $solutionFile
		ValidateExitCode(0)
		Write-Host "Packages restored"
	   

	    Invoke-psake -buildFile .\build-steps.ps1 -taskList $buildTask -framework 4.6.1 

		if  ( $psake.build_success -eq $false -and  $LastExitCode -eq 0 ) { $LastExitCode=-1 }
		Write-Host "Build exit code:" $LastExitCode

		exit $LastExitCode
	}
	Catch
	{
#	  Write-Error $_.Exception -ErrorAction:Continue
 	  LogErrorAndExit
	}
}
function Test-Invoke-Executable()
{
 Write-Host "Test-Invoke-Executable started..."
$processPath="dotnet" 
$arguments =" build"
$TimeoutMilliseconds=100000
$result=  Invoke-Executable  $processPath $arguments -TimeoutMilliseconds $TimeoutMilliseconds
 Write-Host "Test-Invoke-Executable ended:  $result "
Write-Output  "Test-Invoke-Executable ended:  $result "
}

function LogErrorAndExit
{
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
function ImportingModules
{

remove-module [p]sake
import-module "$PSScriptRoot\BuildScripts\psake_ext.ps1"

    $psakeModule = (Get-ChildItem ("..\Source\Packages\psake.*\tools\psake.psm1")).FullName | Sort-Object $_ | select -last 1
	
    Import-Module $psakeModule
# 	if ( (Get-Module -ListAvailable -Name "PowerShellGet")) {
#	# https://github.com/Pscx/Pscx/issues/14 PSCX gcb conflicts with existing gcb on Windows 10
#		Write-Output "Importing PowerShell Community Extensions (PSCX)  Module"
#		Install-Module -Name Pscx -Scope CurrentUser #from https://www.powershellgallery.com/packages/Pscx/3.2.2 
#		Import-Module -Name Pscx
#		Write-Output "PowerShell Community Extensions (PSCX)  Module Imported"
#    }
#	else
    {
		Import-Module "$PSScriptRoot\..\Deployment\Modules\Pscx\3.2.2\Pscx.psd1"
		Write-Output "PowerShell Community Extensions (PSCX)  Module Imported locally"
		#Write-Output "Unable To Install PowerShell Community Extensions (PSCX)Required V5 - Current Version $PSVersionTable.PSVersion"
	}
}

 Main 
