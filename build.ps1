cls
cd $PSScriptRoot
Import-Module .\build\buildsystem\psake\psake.psm1
Invoke-Psake .\build\build-steps.ps1 @args
Remove-Module psake
exit $LASTEXITCODE