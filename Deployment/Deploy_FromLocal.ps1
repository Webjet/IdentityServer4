cls
#Environment Variables similar to teamCity build http://wj-teamcity-01.cloudapp.net/admin/editBuildParams.html?id=buildType:AdminPortal_AdminPortalWebDevDeploy
#    $env:JsonParameterFileName = Coalesce($env:JsonParameterFileName,$JsonParameterFileName)
#    $env:JsonTemplateFileName = Coalesce($env:JsonTemplateFileName,$JsonTemplateFileName)
#    $env:SetParametersFileName = Coalesce($env:SetParametersFileName,$SetParametersFileName)
#    $env:DeploymentContainerName = Coalesce($env:DeploymentContainerName,$DeploymentContainerName)
    $env:DeploymentUser = 'Michael.Freidgeim@webjet.com.au'
    $env:DeploymentPassword = 'Mnf25@62'
#    $env:PerformSwap = $env:env:PerformSwap
#    $env:PerformDeploy = $env:PerformDeploy


& $PSScriptRoot\Deploy.ps1 
if ($host.name -eq 'ConsoleHost') 
{
  Read-Host -Prompt "Press_Enter_to_continue"
}
 