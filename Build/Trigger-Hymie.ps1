$hymieUrl = If($env:hymie_jobs_url -eq $null) { "http://jenkins.webjet.com.au/view/Admin_Portal/api/json" } Else { $env:hymie_jobs_url }
$username = $env:username
#$username = "mafaz.udayar"
#$apiToken = "36cc9ef8d324270f46f273e4e354686d"
$apiToken = $env:apiToken
$brand = If($env:brand -eq $null) { "WAU" } Else { $env:brand }

Function Test-Any {

    [CmdletBinding()]
    param(
        [ScriptBlock] $Filter,
        [Parameter(ValueFromPipeline = $true)] $InputObject
    )

    process {
      if (-not $Filter -or (Foreach-Object $Filter -InputObject $InputObject)) {
          $true # Signal that at least 1 [matching] object was found
          # Now that we have our result, stop the upstream commands in the
          # pipeline so that they don't create more, no-longer-needed input.
          (Add-Type -Passthru -TypeDefinition '
            using System.Management.Automation;
            namespace net.same2u.PowerShell {
              public static class CustomPipelineStopper {
                public static void Stop(Cmdlet cmdlet) {
                  throw (System.Exception) System.Activator.CreateInstance(typeof(Cmdlet).Assembly.GetType("System.Management.Automation.StopUpstreamCommandsException"), cmdlet);
                }
              }
            }')::Stop($PSCmdlet)
      }
    }
    end { $false }
}

Function Invoke-Build {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True)] [string] $UrlKey,
        [Parameter(Mandatory=$True)] [string] $Authentication,
        [Parameter(Mandatory=$True)] [string] $Branch
    )

    process {
        $jobDetails = Invoke-RestMethod -Method Get -Uri "$UrlKey/api/json"
        $parameters =  $jobDetails.actions | ? { $_._class -eq "hudson.model.ParametersDefinitionProperty" } | Select -First 1

        $url = "$UrlKey/build"

        if($parameters -ne $null -and $parameters -ne {})
        {
            if($parameters.parameterDefinitions.Count -ne 1) { Write-Error "Too many parameters expected" }
            elseif($parameters.parameterDefinitions[0].name -ne "Branch") { Write-Error "Parameter other than Branch expected" }
            else { $url = "$UrlKey/buildWithParameters?branch=$branch" }
        }
        Invoke-WebRequest -Method Post -Uri $url -Headers @{ "Authorization" = $Authentication }
    }
}

Function Watch-BuildStatus {

    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True)] [hashtable] $JobStatus,
        [Parameter(Mandatory=$False)] [int] $SecondsToWait = 60
    )

    process {
        do
        {
            Write-Host "Waiting for $(($JobStatus.GetEnumerator() | ? { $_.value -eq $null}).Count) builds to finish..."
            Start-Sleep -Seconds $SecondsToWait
            foreach($jStat in $JobStatus.GetEnumerator() | ? { $_.value -eq $null})
            {
                $result = Invoke-RestMethod -Method Get -Uri ($jStat.key + "/lastBuild/api/json")
                if($result.building -eq $false) { $JobStatus[$jStat.key] = ($result.result -eq "SUCCESS") }
            }

        }
        while ($JobStatus.GetEnumerator() | Test-Any { $_.value -eq $null })
    }
}

Function GetOverallResult{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True)] [hashtable] $JobStatus
    )

    process {
        !($jobStatus.GetEnumerator() | Where-Object { Should-Consider $_.Key } | Test-Any { $_.value -eq $False })
    }
}

Function Should-Consider{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True)] [string] $jobName
    )

    process {
        !($jobName -ne $null -and ($jobName.ToLower().Contains("ie") -or $jobName.ToLower().Contains("safari") -or $jobName.ToLower().Contains("passngerinfolink")))
    }
}

Try{
    # Get Current Jobs in View
    $jobs = Invoke-RestMethod -Method Get -Uri $hymieUrl

    $maxReRuns = 2
    $jobStatus = @{}
    $jobs.jobs |? {$_.name -match "($brand)" -and $_.name -notmatch "MM_[S]?CC$"} | foreach { $jobStatus[$_.url] = $null }

    # Trigger builds for each view
    $branch = "develop"
    $authentication = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($username):$($apiToken)"))
    

    $i = 0
    $result = $false
    do{
        write-host $i
        foreach($job in $($jobStatus).GetEnumerator() | Where-Object {$_.Value -eq $null -or !$_.Value}) { 
            $jobStatus[$job.Key] = $null
            Invoke-Build -UrlKey $job.key -Authentication $authentication -Branch $branch 
        }
        Watch-BuildStatus -JobStatus $jobStatus
        $result = GetOverallResult -JobStatus $jobStatus
        $i++
    } while($maxReRuns -ge $i -and !$result)

    Write-Output -InputObject $jobStatus | Format-Table -AutoSize

    if ($result){
        Write-Host "Hymie tests are successful"
    }else
    {
        Write-Host "Hymie test(s) are failing"
        # Comment/uncomment below line to stop/ignore build fail on test job fail
        Exit(1)
    }
}
Catch
{
    Write-Error $_.Exception.Message
    Exit(1)
}