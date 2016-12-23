cls
#Environment Variables similar to teamCity build http://wj-teamcity-01.cloudapp.net/admin/editBuildParams.html?id=buildType:AdminPortal_AdminPortalWebDevDeploy
$env:runIntegrationUnitTests='true'
$env:​build_​configuration='Release'
$env:​build_​revision	='1' #%build.counter%	
$env:​DeploymentContainerName	='AdminPortal WEB'	
$env:​DeploymentResourceType	='webapp'	
$env:​JsonParameterFileName	='dev-subscription.json'	 #in Deployment\dodo-deployment-scripts
$env:​JsonTemplateFileName	='AdminPortal.json'	 #in Deployment\dodo-deployment-scripts\Templates	
$env:​SetParametersFileName	='AdminPortal-dev-SetParameters.xml' #in /configuration	
$env:​version	='1' #%dep.PackagesWeb_BuildAndTests.$env:version%

& $PSScriptRoot\Deploy.ps1 
if ($host.name -eq 'ConsoleHost') 
{
  Read-Host -Prompt "Press_Enter_to_continue"
}
 