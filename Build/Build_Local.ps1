cls
#set environments as applicable
#$env:runIntegrationUnitTests='true'
#$env:runCSUnitTestCodeCoverage='false'
#$env:runDevCoverage='false'
#$env:openCoverageReport='true'

& $PSScriptRoot\build.ps1   #CsTest OpenCoverOnly #CsTest  # Octopack # VerifyAllConfigs
if ($host.name -eq 'ConsoleHost') 
{
  Read-Host -Prompt "Press_Enter_to_continue"
}
 