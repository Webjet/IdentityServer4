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
