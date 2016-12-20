Run like this:



#check configs first!
    $configFolder = "$baseParent\Configuration"
    $parameterFile = "$baseParent\CMS.Web\parameters.xml"

& "$configFolder\WebDeployParameterVerifier.exe" /ParameterXmlPath="$parameterFile" /SetParameterDir="$configFolder"
if($LASTEXITCODE -ne 0){
        throw "Configuration Verification Tool threw an error. Please check the logs for more info"
}