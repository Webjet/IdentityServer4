Import-Module .\psake\psake.psm1
Invoke-Psake .\build-steps.ps1 @args
Remove-Module psake
exit $LASTEXITCODE