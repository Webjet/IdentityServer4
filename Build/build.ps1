cls
cd $PSScriptRoot
$ErrorActionPreference = "Stop" 
remove-module [p]sake

    $buildTask = ($env:build_task, 'Test' -ne $null)[0]
    #restoring NuGet packages to get PSAKE
    $parentPath = $((Get-Item (Resolve-Path .)).Parent.FullName)
    $solutionPath = "$parentPath\Source\AdminPortal.sln"
    $nugetExe = "$PSScriptRoot\Tools\Nuget\Nuget.exe"
    
    Write-Host "Restoring packages..."
	.$nugetExe restore $solutionPath
	Write-Host "Packages restored"
    $psakeModule = (Get-ChildItem ("..\Source\Packages\psake.*\tools\psake.psm1")).FullName | Sort-Object $_ | select -last 1

    Import-Module $psakeModule

    Invoke-psake -buildFile .\build-steps.ps1 -taskList $buildTask -framework 4.6.1 

	if  ( $psake.build_success -eq $false -and  $LastExitCode -eq 0 ) { $LastExitCode=-1 }
	Write-Host "Build exit code:" $LastExitCode

	exit $LastExitCode
