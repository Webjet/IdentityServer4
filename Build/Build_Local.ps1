cls
#set environments as applicable
$env:runIntegrationUnitTests='true'
$env:runCSUnitTestCodeCoverage='true'
$env:runDevCoverage='true' #false
$env:openCoverageReport='true'
$env:Build="0.0.1"
$env:packagesRoot ="$env:USERPROFILE\.nuget\packages"

& $PSScriptRoot\build.ps1   
if ($host.name -eq 'ConsoleHost') 
{
  Read-Host -Prompt "Press_Enter_to_continue"
}
 