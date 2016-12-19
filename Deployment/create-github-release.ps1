	$github_repo_name =  @{$true="AdminPortal-Web";$false=$env:github_repo_name}[$env:github_repo_name -eq $null]
	$release_version = @{$true="v1.0.7";$false=$env:version}[$env:version -eq $null]

Function CreateGithubReleasePage($release_note, $release_version, $github_repo_name, $username, $token)
{	
	$url = "https://api.github.com/repos/Webjet/$github_repo_name/releases"
	Write-Host "Github Release URL: $url"
	Write-Host $token
	
	#Github Release Note
	$git_release_note = @{
	  tag_name = $release_version
	  target_commitish = "master"
	  name = $release_version
	  body = $release_note
	}

	$json = $git_release_note | ConvertTo-Json
	
	Write-Host $json
	
	Invoke-RestMethod -Method Post  -Uri $url -Body $json -Headers @{Authorization = "token $token" } -ContentType "application/json"
}

#main

try
{   
	
	$token = $env:github_token
	$username = $env:github_username
	$file_path = @{$true="release-node.md";$false=$baseDir+"..\"+$env:release_note_path}[$env:release_note_path -eq $null];	
	Write-Host $file_path 
	
	$release_note = Get-Content -Path $file_path | Out-String
	Write-Output "Currently CreateGithubReleasePage disabled. Enable if required"

	#CreateGithubReleasePage $release_note $release_version $github_repo_name $username $token
}
Catch
{
    Write-Error $_.Exception.Message
    Exit(1)
}

