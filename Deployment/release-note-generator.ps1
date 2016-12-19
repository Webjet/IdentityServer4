$github_repo_name =  @{$true="AdminPortal-Web";$false=$env:github_repo_name}[$env:github_repo_name -eq $null]
$release_version = @{$true="v1.0.6";$false=$env:version}[$env:version -eq $null]

#TODO move to common, extract parameters 
Function GetLastProductionDeploymentDateTimeFromTeamCity
{
	$build_type_id =  @{$true="AdminPortal_AdminPortalApiProduction_AdminPortalWebSwap_AdminPortalWebProdSwapMain";$false=$env:build_type_id}[$env:build_type_id -eq $null]
	Write-Host "Build Type:$build_type_id"
	$token = @{$true="dmluaC5uZ286R2VlbG9uZzU1JQ";$false=$env:teamcity_auth_token}[$env:teamcity_auth_token -eq $null]
	$serverUrl = @{$true="http://wj-teamcity-01.cloudapp.net";$false=$env:serverUrl}[$env:serverUrl -eq $null]	
	#Example: http://wj-teamcity-01.cloudapp.net/httpAuth/app/rest/builds/buildType:AdminPortal_AdminPortalApiProduction_AdminPortalWebSwap_AdminPortalWebSwapChain_SgZuhkWebSwa,status:SUCCESS/finishDate
	$teamcity_url = "$serverUrl/httpAuth/app/rest/builds/buildType:$build_type_id,status:SUCCESS/finishDate"
	Write-Host "TeamCity URL: $teamcity_url"
	
	$last_production_date = Invoke-RestMethod -Method Get -Uri $teamcity_url -Headers @{Authorization = "Basic $token==" } -ContentType "application/json"
	Write-Host "Last Production Deployment Date: $last_production_date"
	
	$a = [DateTime]::ParseExact($last_production_date,"yyyyMMdd'T'HHmmsszzz",$null).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
	
	$last_production_date
}

Function GetListOfMergePullRequestsfromGitHub($last_production_date, $github_repo_name, $username, $token)
{		
	#ISO 8601 format: YYYY-MM-DDTHH:MM:SSZ.  (Z = UTC)	 
	$since = [DateTime]::ParseExact($last_production_date,"yyyyMMdd'T'HHmmsszzz",[CultureInfo]::InvariantCulture).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
	$url = "https://api.github.com/repos/Webjet/$github_repo_name/commits?since=$since"	
	Write-Host "GitCommit URL: $url"
	
	$target_commit = "Merge pull request"
	
	$commits = Invoke-RestMethod -Method Get -Uri $url -Headers @{Authorization = "token $token" } -ContentType "application/json"

	$hash = @{}
	$commits  | Where-Object {$_.Commit -Match $target_commit } | Foreach { $hash[$_.url] = $_.commit.message.Substring($_.commit.message.IndexOf("`n") + 2) }	
	
	$hash.Values
}

Function GenerateReleaseNoteInMarkDownFormat($git_merge_pull_requests, $release_version)
{
	$release_note = "# $(Get-Date -format 'dd-mm-yyyy') Release Note (Version: $release_version)`n"
	$baseDir = resolve-path .
	$baseParent = "$((Get-Item $baseDir).Parent.FullName)"
	$artifact_path = "$baseParent\Output"
	
	Write-Host "Output:$artifact_path"
	
	$git_merge_pull_requests | Foreach { $release_note += " * $_`n" }

	Write-Host $release_note

	$release_note | Out-File "$artifact_path\release-note.md"
	
	$release_note
}

#main

try
{   	
	$token = $env:github_token
	$username = $env:github_username
	
	$last_production_date = GetLastProductionDeploymentDateTimeFromTeamCity	
	$git_merge_pull_requests = GetListOfMergePullRequestsfromGitHub $last_production_date $github_repo_name $username $token
    Write-Output "Currently GenerateReleaseNoteInMarkDownFormat disabled. Enable if required"
#	GenerateReleaseNoteInMarkDownFormat $git_merge_pull_requests  $release_version
}
Catch
{
    Write-Error $_.Exception.Message
    Exit(1)
}

