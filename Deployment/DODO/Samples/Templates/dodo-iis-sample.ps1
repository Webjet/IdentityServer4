cls

#region Make sure we have latest D.O.D.O!!
$module = Get-Module dodo
$moduleVersion = "2.0.3"
if($module -eq $null)
{
    Write-Host "DODO is not imported"

}else
{
    Write-Host "DODO is imported, removing for reimport..."
    Remove-Module dodo
    Write-Host "DODO removed"
}

Write-Host "Importing DODO version $moduleVersion ..."
$env:PSModulePath = [System.Environment]::GetEnvironmentVariable("PSModulePath","Machine")
Import-Module -Name dodo -RequiredVersion $moduleVersion -ErrorAction Stop
Write-Host "DODO version : $moduleVersion is now applied"
#endregion

$json = "$PSScriptRoot\dodo-iis-sample.json"

Publish-DODOIISWebApplication -ConfigurationJSONPath $json -ContainerName "Sample IIS WebApplication"