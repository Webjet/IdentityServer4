cls

#region Make sure we have latest D.O.D.O!!
$module = Get-Module dodo
$moduleVersion = "2.0.2"
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

$json = "$PSScriptRoot\dodo-2vms-1vnet-availabilityset-loadbalanced.json"

#DO stuff here...
Publish-DODOAzureVNet -ConfigurationJSONPath $json -ContainerName "Virtual Network 1"

Publish-DODOAzureStorageAccount -ConfigurationJSONPath $json -ContainerName "Virtual Machine Storage"

Publish-DODOAzureAvailabilitySet -ConfigurationJSONPath $json -ContainerName "An AvailabilitySet"
Publish-DODOAzureVM -ConfigurationJSONPath $json -ContainerName "Test1"
Publish-DODOAzureVM -ConfigurationJSONPath $json -ContainerName "Test2"
Publish-DODOAzureLoadBalancer -ConfigurationJSONPath $json -ContainerName "External Loadbalancer"

 
