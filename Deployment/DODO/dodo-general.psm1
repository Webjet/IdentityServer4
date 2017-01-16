#from http://stackoverflow.com/questions/10623907/null-coalescing-in-powershell
function Coalesce($a, $b) { if ($a -ne $null) { $a } else { $b } }
# $s = Coalesce $myval "new value"
function IfNull($a, $b, $c) { if ($a -eq $null) { $b } else { $c } }
#$s = IfNull $myval "new value" $myval
function IfTrue($a, $b, $c) { if ($a) { $b } else { $c } }
#$x = IfTrue ($myval -eq $null) "" $otherval
New-Alias "??" Coalesce
#$s = ?? $myval "new value"
New-Alias "?:" IfTrue
#$ans = ?: ($q -eq "meaning of life") 42 $otherval

#region Script Diagnostic Functions
# From https://poshoholic.com/2009/01/19/powershell-quick-tip-how-to-retrieve-the-current-line-number-and-file-name-in-your-powershell-script/
# Don’t simply use $MyInvocation.ScriptLineNumber in your script if you want the number of the line where you get that property,
# it will show caller's line and file.
function Get-CurrentLineNumber { 
    [string]$MyInvocation.ScriptLineNumber 
}

New-Alias -Name __LINE__ -Value Get-CurrentLineNumber –Description 'Returns the current line number in a PowerShell script file.'

function Get-CurrentFileName { 
    $MyInvocation.ScriptName 
}

New-Alias -Name __FILE__ -Value Get-CurrentFileName -Description 'Returns the name of the current PowerShell script file.'

Function Write-Output-WithPosition ($message)
{
  Write-Output "$message   $($MyInvocation.ScriptName):$($MyInvocation.ScriptLineNumber) "
}
# http://stackoverflow.com/questions/3689543/is-there-a-way-to-retrieve-a-powershell-function-name-from-within-a-function
# to determine Function Name $MyInvocation.MyCommand  should be called directly from code (not wrapped in function as required for file and linenumber) 

<#
Examples of call:
  Write-WithPosition "Message logged at $($MyInvocation.MyCommand)"
  Write-Warning "Message logged at  $($MyInvocation.MyCommand) $(Get-CurrentFileName):$(Get-CurrentLineNumber) "  
  Write-Warning "Message logged at $($MyInvocation.MyCommand) $(__FILE__):$(__LINE__)  "  
#>

#endregion
