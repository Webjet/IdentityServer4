
cls

#region Make sure we have latest D.O.D.O!!
$module = Get-Module dodo
$moduleVersion = "1.5.0"
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
Import-Module -Name dodo -RequiredVersion $moduleVersion
Write-Host "DODO version : $moduleVersion is now applied"
#endregion


$loadBalanceTest = "$PSScriptRoot\Samples\Templates\dodo-2vms-1vnet-availabilityset-loadbalanced.json"

$ErrorActionPreference = "Stop"

Publish-DODOAzureVNet -ConfigurationJSONPath $loadBalanceTest -ContainerName "Virtual Network 1"
Publish-DODOAzureStorageAccount -ConfigurationJSONPath $loadBalanceTest -ContainerName "Virtual Machine Storage"
Publish-DODOAzureAvailabilitySet -ConfigurationJSONPath $loadBalanceTest -ContainerName "An AvailabilitySet"
Publish-DODOAzureVM -ConfigurationJSONPath $loadBalanceTest -ContainerName "Test1"
Publish-DODOAzureVM -ConfigurationJSONPath $loadBalanceTest -ContainerName "Test2"
Publish-DODOAzureLoadBalancer -ConfigurationJSONPath $loadBalanceTest -ContainerName "External Loadbalancer"

Write-Host "Environment up, waiting 3min so you can verify before spinning down..."
Start-Sleep 180 

Write-Host "Spinning down test environment"
Remove-AzureRmResourceGroup -Name "test-resources" -Force
<#

#Storage 
Publish-DODOAzureStorageAccount -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "Web App Log Storage"

Publish-DODOAzureWebsiteConfiguration -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "MyWebApp1"
Publish-DODOAzureRedis -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName  "MyRedisCache1"
Publish-DODOAzureWebjob -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "MyWebJob1"
Publish-DODOAzureAutomationAccount -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "DevOpsAutomationAccount"
Publish-DODOAzureCloudService  -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "MyCloudService1"
Publish-DODOAzureSqlServer -ConfigurationJSONPath $ConfigurationJSONPath 


#Virtual Network:
Publish-DODOAzureVNet -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "VNET"
Publish-DODOAzureClassicVnet -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "Classic VNET"

#Reserved IP
Publish-DODOAzureReservedIp -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "A Reserved IP"

#Azure App Service Environment 
Publish-DODOAzureAppServiceEnvironment -ConfigurationJSONPath $json -ContainerName "App Service Environment 01"


$containerJson = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json
Publish-DODOAzureAutomationAccount -ConfigurationJSONObject $containerJson

#Virtual Machines:
Publish-DODOAzureVM  -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "devopstestvm"


# Web App regressio
Publish-DODOAzureWebApp -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "MyWebApp1" -PackagePath "c:\temp\SecurePay.Gateway.zip" -SetParametersPath "C:\temp\SecurePay.Gateway.SetParameters.xml"
Publish-DODOAzureWebApp -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "MyWebApp1"
Switch-DODOAzureWebApp -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "MyWebApp1"

# Web App regression
Publish-DODOAzureWebAppConfiguration -ConfigurationJSONPath $ConfigurationJSONPath -ContainerName "MyWebApp1" -DeploymentSlot "production"
#>
