
#region Load up DODO!
$repoBase = "$((Get-Item $(Resolve-Path -Path $PSScriptRoot)).Parent.FullName)"
$dodoExe = "$repoBase\Exe\dodo.exe"
& $dodoExe --export
Import-Module "$repoBase\Exe\DODO\dodo.psd1"
#endregion

#$parameters = "$PSScriptRoot\parameters.json" #change this to your parameter file 
$template = "$repoBase\Samples\Templates\dodosample-azure-trafficmanager.json" #point this to a template

$templateJson = Get-Content -Path $template -Raw | ConvertFrom-Json
#$parametersJson = Get-Content -Path $parameters -Raw | ConvertFrom-Json

Run-DODO -ConfigurationJSONObject $templateJson -ParametersJSONObject $parametersJson
