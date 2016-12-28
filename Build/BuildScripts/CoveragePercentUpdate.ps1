
function CoveragePercentUpdate() 
{
    param(
        [string] $reportGeneratorOutputFile,# = "$coverOut\cover\index.htm"
        [string] $coverageThresholdFile ,    # "$baseParent\Build\covercoveragethreshold.txt"
		[string] $codeCoveragePercentFile,  # "$coverOut\codecoveragepercent.txt"
		$slackDetails  # if not null, should specify channel(e.g "#adminportal"); username(e.g. # "flightBOT") ;[icon_url]
    )

 	
		$regex = 'Line coverage:.+?<td>([0-9\.]+)%<\/td>'
		$coverPercentage = Select-String -Path $reportGeneratorOutputFile -Pattern $regex  | % {$_.Matches} | % {$_.Groups[1].Value}
		Write-Output  "coverCoverPercentage on summary:"$coverPercentage

		$regex1 = 'Coverable lines:.+?<td>([0-9\.]+)<\/td>'
		$coverableLines = Select-String -Path $reportGeneratorOutputFile -Pattern $regex1  | % {$_.Matches} | % {$_.Groups[1].Value}
		Write-Output  "coverableLines:"$coverableLines

		$regex2 = '<th>Covered lines:.+?<td>([0-9\.]+)<\/td>'
		$coveredLines = Select-String -Path $reportGeneratorOutputFile -Pattern $regex2  | % {$_.Matches} | % {$_.Groups[1].Value}
		Write-Output  " coveredLines:"$coveredLines
		
		$coverPercentage = 0;
		if($coverableLines -ne 0){
			$coverPercentage=($coveredLines/$coverableLines)*100
		}
		#write it to artefacts
		[System.IO.File]::WriteAllText($codeCoveragePercentFile,$coverPercentage)
		$threshold = [System.Convert]::ToDecimal([System.IO.File]::ReadAllText("$coverageThresholdFile"))
		
		Write-Output "CodeCover Percentage=$($coverPercentage)"
		Write-Output "CodeCover Threshold=$($threshold)"
		Write-Output "CodeCover Threshold file used: $coverageThresholdFile"

		if($coverPercentage -gt $threshold)
		{
			Write-Output "Code Coverage is above the threshold!"
			[System.IO.File]::WriteAllText($coverageThresholdFile,$coverPercentage)
		}
		elseif($coverPercentage -eq $threshold )
		{
			Write-Output "Code Coverage is acceptable!"
		}
		else 
		{
			$msg = "Code coverage has gone down to $($coverPercentage) under threshold $($threshold). Please check coverage report!"
			SendSlack "Code Coverage degradation!" $msg $slackDetails
			throw $msg
		}
}
	function SendSlack($title, $message,$slackDetails){
		if($notifySlack -ne "")
		{
			$hook = "https://hooks.slack.com/services/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";
			$payload = @{
					channel = $slackDetails.channel # "#adminportal";
					username = $slackDetails.username # "@mfreidgeim";# flightBOT
					icon_url = $slackDetails.icon_url #"http://besticons.net/sites/default/files/departing-flight-icon-3634.png";
					attachments = @(
						@{
							fallback = $message;
							color = "warning";
							title = $title;
							fields = @(
								@{
									value = $message;
								});
						};
					);
				}
        	Invoke-Restmethod -Method POST -Body ($payload | ConvertTo-Json -Depth 4) -Uri $hook
		}
	}
$Debug=$false # true only for local debugging
if ($Debug){
cls
$baseParent="C:\GitRepos\AdminPortal"
		$reportGeneratorOutputFile = "$baseParent\Output\Test-Output\cover\index.htm" #"$coverOut\cover\index.htm"
		$codeCoveragePercentFile = "$baseParent\Output\Test-Output\cover\codecoveragepercent.txt"
		$slackDetails = @{channel =  "#adminportal";
					username = "@mfreidgeim";
			#		icon_url = "http://besticons.net/sites/default/files/departing-flight-icon-3634.png";
                   }
		CoveragePercentUpdate $reportGeneratorOutputFile  "$baseParent\Build\CoverageThreshold.txt"  $codeCoveragePercentFile $slackDetails
}