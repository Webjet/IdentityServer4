cls
#set environments as applicable
#$env:Build="0.0.1"

& $PSScriptRoot\Deploy.ps1   
if ($host.name -eq 'ConsoleHost') 
{
  Read-Host -Prompt "Press_Enter_to_continue"
}
 