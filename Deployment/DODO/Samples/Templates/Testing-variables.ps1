

$json = "$PSScriptRoot\Testing-variables.json"
$jsonObj = Get-Content -Raw -Path $json | ConvertFrom-Json;

foreach($container in $jsonObj.Containers)
{
	Write-Host "Name $($container.Name) Type $($container.Type) Subscription $($container.Attributes.Properties.SubscriptionID)"
}

$newJson = Set-InternalDODOVariables -ConfigurationJSONObject $jsonObj

foreach($container in $newJson.Containers)
{
	Write-Host "Name $($container.Name) Type $($container.Type) Subscription $($container.Attributes.Properties.SubscriptionID)"
}