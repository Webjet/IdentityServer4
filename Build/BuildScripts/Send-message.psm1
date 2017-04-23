function NotifyMsTeams ($MSTeamsChannelhook,$notifytext)
{
    $webhook = $MSTeamsChannelhook.channel

    $Body = @{
        title = "Admin Portal Bot";
        text = $notifytext;
        themeColor = "EA4300";
    };

    Write-Host $Body
    $params = @{
    Headers = @{'accept'='application/json'}
    Body = $Body | convertto-json
    Method = 'Post'
    URI = $webhook 
}
write-host @params
Invoke-RestMethod @params
}