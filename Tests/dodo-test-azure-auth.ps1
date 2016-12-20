cls
#region Make sure we have latest D.O.D.O!!
$module = Get-Module dodo
$moduleVersion = "2.3.0"
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

<# SAMPLE OrgUserPSCredentialPath
{
    "Name" : "My azure credentials",
    "Type" : "AzureCredentials",
    "Attributes" : 
    {
        "AuthType" : "OrgUserPSCredentialPath",
        "Username" : "C:\\temp\\AzureCreds\\AzureSecureCredentialUsername.txt",
        "Password" : "C:\\temp\\AzureCreds\\AzureSecureCredential.txt",
        "SubscriptionID": "[variables('SubscriptionID')]",
        "TenantID": "[variables('TenantID')]"
    }
        
}
#>

<# SAMPLE OrgUserCredentials
{
    "Name" : "My azure credentials",
    "Type" : "AzureCredentials",
    "Attributes" : 
    {
        "AuthType" : "OrgUserCredentials",
        "Username" : "your username goes here",
        "Password" : your credentials go here",
        "SubscriptionID": "[variables('SubscriptionID')]",
        "TenantID": "[variables('TenantID')]"
    }
        
}
#>

$json = @"
{
    "Variables": {
        "Name": "samplerediscache",
        "Subscription": "Pay - Development",
        "SubscriptionID": "a52a08f7-c647-43d2-aa3e-fd049f0edc63",
        "TenantID": "5de0e68c-0afd-4a67-b089-31f168aa4ca0",
        "ResourceGroupName": "test-resources",
        "ResourceGroupLocation": "Southeast Asia"
    },
   "Containers" :
	[
        {
			"Name" : "Dummy redis cache to test azure authentication",
			"Type": "AzureRedisCache",
			"Attributes": {
				"Properties": {
				  "Name": "[variables('Name')]",
				  "Size": "C1",
				  "SKU":"Standard",
				  "Subscription": "[variables('Subscription')]",
				  "SubscriptionID": "[variables('SubscriptionID')]",
				  "TenantID": "[variables('TenantID')]"
				},
				"ResourceGroup":{
					"Name" : "[variables('ResourceGroupName')]",
					"Location" : "[variables('ResourceGroupLocation')]"
				}			
			
			}
		},
        {
            "Name" : "My azure credentials",
            "Type" : "AzureCredentials",
            "Attributes" : 
            {
                "AuthType" : "OrgUserPSCredentialPath",
                "Username" : "C:\\temp\\AzureCreds\\AzureSecureCredentialUsername.txt",
                "Password" : "C:\\temp\\AzureCreds\\AzureSecureCredential.txt",
                "SubscriptionID": "[variables('SubscriptionID')]",
			    "TenantID": "[variables('TenantID')]"
            }
        } 
    ]
}
"@ | ConvertFrom-Json

#To test authentication standalone
Set-DODOAzureRmAuthentication -ConfigurationJSONObject $json -SubscriptionId "a52a08f7-c647-43d2-aa3e-fd049f0edc63"

#Test not passing a json and just logging into subscription -without prompt
#Set-DODOAzureRmAuthentication -SubscriptionId "a52a08f7-c647-43d2-aa3e-fd049f0edc63" -TenantId "5de0e68c-0afd-4a67-b089-31f168aa4ca0"

#To test it from a module
#Run-DODO -ConfigurationJSONObject $json