

cls

cd "C:\git\DODO\Exe"

Get-Module
Remove-Module dodo -Force

Write-Host "Importing DODO"
$dodoExe = "$PSScriptRoot\dodo.exe"
& $dodoExe --export
Import-Module "$PSScriptRoot\DODO\dodo.psd1"


$parameters = @"
{
    "Parameters": 
    {
        "SSHPublicKey" : "ssh-rsa AAAAB3NzaC1yc2EAAAABJQAAAQEAyceuIx89LU31lS4NiLKfyS1uNQ5zULGUvxfe1yom2g1C3EuYtw8uCken6nIaG0IDkNcIaAoTytAJZLDQ+Cu5njexGzMaWoV/a56667Rhs3UuocAA65/kIs7hBO9RJutaGRLE0i8V1B26SeX17rXcHuBMj13jmVLtD8FmEUMVXCcphwyGxArLLnE3MOTM83FEUVuAigq6WOHg1A2qRdmzqrH0ubmh3yKfQ+44yQfdPb1BD3qPzwaOjrEAiawXUSqKGLPxp70LPYb3A3o9Eki8w6MxboS2nvFQX3THtOGIDxqeEmi4osT9ebvdSGyHQ4xsrISMtpJFRe3ibkHyxRmmww== rsa-key-20161114",
		"SSHUserName" : "webjet"
    }
}
"@ | ConvertFrom-Json


$json = Get-Content -Path "C:\Git\DODO\Samples\Templates\dodosample-dcos-mesos-service.json" -Raw | ConvertFrom-Json
#$json = Get-Content -Path "C:\Git\DODO\Samples\Templates\dodosample-azure-container-service-dcos.json" -Raw | ConvertFrom-Json

#Run-DODO -ConfigurationJSONObject $json -ParametersJSONObject $parameters 
Run-DODO -ConfigurationJSONObject $json -ParametersJSONObject $parameters -ContainerName "Lookup Application"


