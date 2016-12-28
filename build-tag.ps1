function Invoke-HTTPPostCommand() 
{
    param(
        [string] $target = $null,
        [string] $postBody
    )

    $webRequest = [System.Net.WebRequest]::Create($target)
    $webRequest.ContentType = "application/xml"
    $PostStr = [System.Text.Encoding]::UTF8.GetBytes($postBody)
    $webrequest.ContentLength = $PostStr.Length
    $webRequest.ServicePoint.Expect100Continue = $false

    $username = $env:SystemTeamCityAuthUserID
    $password = $env:SystemTeamCityAuthPassword
    $credentials = [Convert]::ToBase64String([System.Text.Encoding]::Default.GetBytes($username + ":" + $password))
    $webRequest.Headers.Add("AUTHORIZATION", "Basic $credentials"); #basic authentication using base 64 encoded username and password: [user]:[pass]

    $webRequest.PreAuthenticate = $true
    $webRequest.Method = "POST"

    $requestStream = $webRequest.GetRequestStream()
    $requestStream.Write($PostStr, 0,$PostStr.length)
    $requestStream.Close()

    [System.Net.WebResponse] $resp = $webRequest.GetResponse();
    $rs = $resp.GetResponseStream();
}

function TagBuild () 
{
    param(
        [string] $tagName,
        [string] $buildId,
        [string] $serverUrl
    )

    if ([string]::IsNullOrEmpty($tagName) -eq $false)
    {
        $post = "<tags count=`"1`"><tag name=`"" + $tagName + "`"></tag></tags>"
        $URL = $serverUrl + "/httpAuth/app/rest/builds/id:" + $buildId + "/tags"
        Write-Host "Tagging request url : $($URL) with post $($post)"
        Invoke-HTTPPostCommand $URL $post
    }
}