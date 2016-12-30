
 param(
    [Parameter(Position=0,Mandatory=0)] [string]$JsonParameterFileName = "dev.json",
    [Parameter(Position=1,Mandatory=0)] [string]$JsonTemplateFileName = "webapp-template.json",
    [Parameter(Position=2,Mandatory=0)] [string]$SetParametersFileName = "dev-SetParameters.xml",
    [Parameter(Position=3,Mandatory=0)] [string]$DeploymentContainerName = "Admin Portal",
    [Parameter(Position=4,Mandatory=0)] [string]$BuildVersion = "1-Local",
    [Parameter(Position=5,Mandatory=0)] [string]$DeploymentUser = "",
    [Parameter(Position=6,Mandatory=0)] [string]$DeploymentPassword = "",
    [Parameter(Position=6,Mandatory=0)] [string]$PerformSwap = "true",
    [Parameter(Position=7,Mandatory=0)] [string]$PerformDeploy = "true"
)
$ErrorActionPreference = "Stop" 

try
{
 Write-Host "Importing DODO"
    $dodoExe = "$PSScriptRoot\dodo.exe"
    & $dodoExe --export
    Import-Module "$PSScriptRoot\DODO\dodo.psd1"
    Import-Module "$PSScriptRoot\DODO\dodo-Json.psm1" # todo include in dodo.psd1
    Import-Module "$PSScriptRoot\DODO\dodo-general.psm1" # todo include in dodo.psd1
## Variables set from TeamCity
if($env:DeploymentUser -ne $null)
{
    Write-Host "TeamCity variables used"
    $JsonParameterFileName = Coalesce $env:JsonParameterFileName $JsonParameterFileName
    $JsonTemplateFileName = Coalesce $env:JsonTemplateFileName  $JsonTemplateFileName
    $SetParametersFileName = Coalesce $env:SetParametersFileName $SetParametersFileName  
    $DeploymentContainerName = Coalesce $env:DeploymentContainerName $DeploymentContainerName 
    $DeploymentUser = $env:DeploymentUser
    $DeploymentPassword = $env:DeploymentPassword
    $PerformSwap = Coalesce $env:PerformSwap $PerformSwap
    $PerformDeploy = Coalesce $env:PerformDeploy $PerformDeploy
    $BuildVersion = Coalesce  $env:buildVersion "1.0" 
}

$parentPath = $((Get-Item ($PSScriptRoot)).Parent.FullName)


   
    $DODOJsonParameters = Get-Content -Path "$PSScriptRoot\Parameters\$JsonParameterFileName" -Raw | StripComments| ConvertFrom-Json
    $DODOJsonTemplate = Get-Content -Path "$PSScriptRoot\Templates\$JsonTemplateFileName" -Raw | StripComments | ConvertFrom-Json

 
 
    $args = @{
        PackagePath = $("$parentPath\Webjet.FlightMerchandising.AspNetHost.zip") #to replace
        SetParametersPath = $("$parentPath\Configuration\$SetParametersFileName")
    }

    $arguments = $args | ConvertTo-json

    #Set Deployment Credentials
    $DODOJsonParameters.Parameters | % {$_.AzureUsername = $DeploymentUser} 
    $DODOJsonParameters.Parameters | % {$_.AzurePassword = $DeploymentPassword}
    $DODOJsonParameters.Parameters | % {$_.BuildVersion = $BuildVersion}

    if($PerformDeploy -eq "true")
    {
        #Deploy Webapp
        Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "$DeploymentContainerName" -Command "Publish-DODOAzureWebApp" -Arguments $arguments -ParametersJSONObject $DODOJsonParameters
        
        #Deploy appsettings
        Publish-DODOAzureWebAppConfiguration -ConfigurationJSONObject $DODOJsonTemplate -DeploymentSlot "staging" -ContainerName "$DeploymentContainerName" -ParametersJSONObject $DODOJsonParameters

        #Invoke Warmup
        Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "$DeploymentContainerName" -Command "Invoke-DODOAzureWebsiteWarmup" -ParametersJSONObject $DODOJsonParameters
    }
    
    if($PerformSwap -eq "true")
    {
        #Swap
        Run-DODO -ConfigurationJSONObject $DODOJsonTemplate -ContainerName "$DeploymentContainerName" -Command "Switch-DODOAzureWebApp" -ParametersJSONObject $DODOJsonParameters
    }

    #Tag the build in TeamCity!
    $buildVersion = ($env:buildVersion, '0' -ne $null)[0]
    #only tag build in TeamCity if you are running in TeamCity and have valid build version!
    if($buildVersion -ne '0')
    {
        $teamCityBuildId = ($env:buildId, '0' -ne $null)[0]
        $teamcityServerUrl = ($env:teamcityServerUrl, "" -ne $null)[0]
        . "$PSScriptRoot\build-tag.ps1"
        Write-Host "Tagging build..."
        TagBuild -tagName $buildVersion -buildId $teamCityBuildId -serverUrl $teamcityServerUrl
    }
    

}
Catch
{
    Write-Error $_.Exception
    Exit(1)
}
