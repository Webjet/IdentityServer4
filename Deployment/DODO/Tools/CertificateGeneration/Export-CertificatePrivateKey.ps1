#requires -version 2.0

param (
    [parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path -Path $_ -PathType Leaf })]
	[string]
    $Path,

    [parameter(Mandatory = $true)]
	[string]
	$Destination
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Cert = New-Object `
    -TypeName System.Security.Cryptography.X509CertificateS.X509Certificate2 `
    -ArgumentList $Path

Get-ChildItem -Path Cert: -Recurse |
    Where-Object {
        -not $_.PSIsContainer -and
        $_.Thumbprint -eq $Cert.Thumbprint -and
        $_.Subject -eq $Cert.Subject
    } |
    Select-Object -First 1 |
    ForEach-Object {
        [IO.File]::WriteAllBytes(
            $Destination,
            $_.Export('Pkcs12')
        )
    }
