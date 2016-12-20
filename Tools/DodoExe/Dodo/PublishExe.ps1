param(
	[string]$ExePath
)

#DODO Root path
$path = (Get-Item $PSScriptRoot).Parent.Parent.Parent
Copy-Item -Path $ExePath -Destination "$($path.FullName)\Exe\dodo.exe"
