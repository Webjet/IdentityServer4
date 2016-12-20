cls

#region Make sure we have latest D.O.D.O!!
$module = Get-Module dodo
$moduleVersion = "2.2.0"
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

#Log into a subscriptions that needs a service principle

$subscriptionID = ""
$tenantID = ""
$servicePrincipalName = "OctopusDeploy"
$servicePrincipalDisplayName = "OctopusDeploy"

Set-DODOAzureRmAuthentication -SubscriptionId $subscriptionID -TenantId $tenantID

#$clientID = "91c2c38d-355e-4461-ae50-b53cc9c02dbe"
#New-AzureRmRoleAssignment -ObjectId $clientID -RolDefinitionName Contributor -Scope /subscriptions/a52a08f7-c647-43d2-aa3e-fd049f0edc63

$servicePrincipalName = 'https://devgateway.webjet.com.au'
$displayName = 'dev-pay-serviceprincipal'
$password = 'Webjet123!'

Write-Host "Creating Azure AD Application..."
$azureAdApplication = New-AzureRmADApplication -DisplayName $displayName -HomePage "https://devgateway.webjet.com.au" -IdentifierUris $servicePrincipalName -Password $password

Write-Host "Creating Service Principle..."
New-AzureRmADServicePrincipal -ApplicationId $azureAdApplication.ApplicationId

Write-Host "Assign SPN to a role..."
New-AzureRmRoleAssignment -RoleDefinitionName Contributor -ServicePrincipalName $azureAdApplication.ApplicationId

New-AzureRmRoleAssignment -RoleDefinitionName "dev-pay-contributor" -ServicePrincipalName $azureAdApplication.ApplicationId -ResourceGroupName "dev-pay-resources"