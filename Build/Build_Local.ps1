cls
#set environments as applicable
#$env:runIntegrationUnitTests='true'
#$env:runCSUnitTestCodeCoverage='false'
$env:runDevCoverage='false' #false
#$env:openCoverageReport='true'
#$env:Build="0.0.1"

& $PSScriptRoot\build.ps1   
if ($host.name -eq 'ConsoleHost') 
{
  Read-Host -Prompt "Press_Enter_to_continue"
}
 