cls

#region Load up DODO!
$repoBase = "$((Get-Item $(Resolve-Path -Path $PSScriptRoot)).Parent.Parent.FullName)"
$dodoExe = "$repoBase\Exe\dodo.exe"
& $dodoExe --export
Import-Module "$repoBase\Exe\DODO\dodo.psd1"
#endregion

#load up deployment template
$json = Get-Content -Path "$PSScriptRoot\dodo-vm-dsc.json" -Raw | ConvertFrom-Json

#prepare parameters
$params = @{
    Parameters = @{
        DSCScriptPath_dodo = "$($PSScriptRoot.Replace("\","\\"))\\dodosample-azure-dsc-installdodo.ps1"
        DSCPackagePath_dodo = "$($dodoExe.Replace("\","\\"))"
        DSCScriptPath_arr = "$($PSScriptRoot.Replace("\","\\"))\\dodosample-azure-dsc-installarr.ps1"
    }
} | ConvertTo-Json
$parameters = $params | ConvertFrom-Json

#Run-DODO -ConfigurationJSONObject $json -ParametersJSONObject $parameters
Run-DODO -ConfigurationJSONObject $json -ParametersJSONObject $parameters -ContainerName "Virtual Machine Storage"
Run-DODO -ConfigurationJSONObject $json -ParametersJSONObject $parameters -ContainerName "DODO DSC"
Run-DODO -ConfigurationJSONObject $json -ParametersJSONObject $parameters -ContainerName "ARR DSC"

#Apply my DSC configuration
Apply-DODOAzureVMExtension -ConfigurationJSONObject $json -ContainerName "Test1" -ParametersJSONObject $parameters