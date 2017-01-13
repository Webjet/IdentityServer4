Include "$PSScriptRoot\BuildScripts\psake_ext.ps1"
. "$PSScriptRoot\BuildScripts\CoveragePercentUpdate.ps1"
	properties {
		$baseDir = resolve-path .
		$baseParent = "$((Get-Item $baseDir).Parent.FullName)"
		$solutionFile = "$parentPath\Source\AdminPortal.sln"
	#to remove 	$solutionFile = "$parentPath\AdminPortal.sln" #to remove later

		$buildConfiguration = "Release"
		$buildPlatform = "Any CPU"
		$solutionDirectory = (Get-Item $solutionFile).DirectoryName
		$outPath = "$baseParent\Output"
		$binariesDir = "$baseParent\Binaries"
		#$visualStudioVersion = '14.0'
	    $visualStudioVersion = Get-Highest-Installed-Version-Of-Visual-Studio
		$buildRevision = ($env:build_revision, '0' -ne $null)[0]
		$teamCityBuildId = ($env:buildId, '0' -ne $null)[0]
		$teamcityServerUrl = ($env:teamcityServerUrl, "" -ne $null)[0]
		$buildPackageTag = $env:build_package_tag
		$nugetPackageVersion = [string]::Format("{0:yyyy}.{0:MM}.{0:dd}.{1}{2}", [DateTime]::Today, $buildRevision, @{$true='';$false='-' + $buildPackageTag}[$buildPackageTag -eq '' -OR $buildPackageTag -eq $null])
		$nugetExe = "$baseDir\Tools\NuGet\NuGet.exe"
		$PackagePath = "$baseParent\Binaries\_PublishedWebsites\AdminPortal_Package\*"	
		$gitCommitHash = ($env:vcs_commit_hash, 'local123' -ne $null)[0]
		$buildVersion = "$($buildRevision)-$($gitCommitHash.Substring(0,8))"
		$notifySlack = ($env:notifySlack, '' -ne $null)[0]
		$packagesRoot ="$baseParent\Source\packages"
#test/coverage configurations
#	$csTestRunner = "`"$baseParent\Source\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe`""
#	$TestRunner = "C:\Program Files (x86)\Microsoft Visual Studio " + $visualStudioVersion +"\Common7\IDE\MSTest.exe" 
    $csTestRunner = "C:\Program Files (x86)\Microsoft Visual Studio " + $visualStudioVersion +"\Common7\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.Console.exe" 
	$csTestAssemblies =  "`"$binariesDir\AdminPortal.UnitTests.dll`" `"$binariesDir\AdminPortal.IntegrationTests.dll`" "  
	$openCover = "$packagesRoot\OpenCover.4.6.519\tools\OpenCover.Console.exe"
	$reportGen = "$packagesRoot\ReportGenerator.2.5.1\tools\ReportGenerator.exe"
	$openCoverageReport=($env:openCoverageReport, 'true' -ne $null)[0]
#    $runIntegrationUnitTests =  ($env:runIntegrationUnitTests, 'false' -ne $null)[0] #false default

	}

    task default -depends Test

    task Test -description "Run unit tests in solution" -depends Compile  {
    	Write-Host 'Starting Tests!'

		$coverOut = "$outPath\Test-Output"    
    	$coverOutPath = "`"$coverOut\projectCoverageReport.xml`""
    	$coverReportOut = "`"$coverOut\cover`""

		if (!(Test-Path $coverOut)) {
            md $coverOut 
    	}

		Write-Host "OpenCover targetargs - $csTestAssemblies"
		Write-Host $openCover
# it is case-sensitive `"-[xunit.assert]*`" `"-[xunit.core]*`"
# https://github.com/opencover/opencover/wiki/Usage
#`"-[Serilog.Sinks.SumoLogic]*`" `"-[Serilog.Enrichers.NancyContext]*`" -[KellermanSoftware.CompareNetObjects]*
		$filters = "`"+[*]*`" `"-[*.*Tests]*`"    `"-[Build]*`" `"-[Microsoft.WindowsAzure.Storage]*`"   " +
   		 " -[DbUpWrapper]*  -[FluentAssertions*]* -[MSTestHacks]* -[Microsoft.*]* -[Flurl.*]*  "  
			#-noshadow for XUnit
#.$openCover -register:user -mergebyhash -skipautoprops -target:$csTestRunner -targetargs:"$($csTestAssemblies) " -returntargetcode -output:$coverOutPath -filter:$filters
		$arguments="-register:user -mergebyhash -skipautoprops -target:`"$csTestRunner`" -targetargs:`"$($csTestAssemblies) `" -returntargetcode -output:$coverOutPath -filter:`"$filters`" "
		RunProcess -processPath $openCover -arguments $arguments
		.$reportGen -reports:$coverOutPath -targetdir:$coverReportOut
		
    	Write-Host "Testing Done! Checking code coverage..."


		#Extract coverage percentage
		$reportGeneratorOutputFile = "$coverOut\cover\index.htm"
		$slackDetails = @{channel =  "#adminportal";
					username = "@mfreidgeim";#adminportal
					#icon_url = "http://besticons.net/sites/default/files/departing-flight-icon-3634.png"
                   }
		CoveragePercentUpdate $reportGeneratorOutputFile "$baseParent\Build\coveragethreshold.txt" "$coverOut\CoveragePercent.txt" $slackDetails
		                        [System.Convert]::ToBoolean($openCoverageReport)

    }

    task Compile `
    -depends Clean `
    -description "Compile the code" `
    -requiredVariables solutionFile, buildConfiguration, buildPlatform `
	{
		Write-Host 'Builds solution and Packages the solution ' $solutionFile
	    DeleteIfExistsAndCreateEmptyFolder $outPath
		DeleteIfExistsAndCreateEmptyFolder $binariesDir

		Exec {
			Write-Host "Building $solutionFile"
			msbuild $solutionFile /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform /p:OutDir="$binariesDir\" /p:OutputPath="$outPath\" /p:VisualStudioVersion=$visualStudioVersion /p:SkipInvalidConfigurations=true
		}

		Write-Host "Copying artifacts to output folder..."
		Copy-Item $PackagePath $outPath -recurse
		Copy-Item "$baseParent\Configuration" $outPath -recurse
		Copy-Item "$baseParent\Deployment" $outPath -recurse
		
		$buildTagScript="$baseParent\Build\BuildScripts\build-tag.ps1"
		Copy-Item $buildTagScript "$outPath\Deployment\"
		Write-Host "Copying artifacts complete"

		ValidateExitCode(0)

		#only tag build in TeamCity if you are running in TeamCity and have valid build id!
		if($teamCityBuildId -ne '0')
		{
			. $buildTagScript
			Write-Host "Tagging build..."
			TagBuild -tagName $buildVersion -buildId $teamCityBuildId -serverUrl $teamcityServerUrl

			Write-Host "Setting build version to : $($buildVersion)"
			write-host "##teamcity[setParameter name='env.buildVersion' value='$buildVersion']"
		}
	}

    task Clean -description "Remove temporary files" {

#		if ((Test-Path $outPath)) {
#			Remove-Item -Recurse "$outPath\**" | Where { ! $_.PSIsContainer }
#        }
#		if ((Test-Path $binariesDir)) {
#			Remove-Item -Recurse "$binariesDir\**" | Where { ! $_.PSIsContainer }
#        }
		DeleteFilesInFolder $outPath
		DeleteFilesInFolder $binariesDir

		exec { msbuild $solutionFile /target:clean /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform /p:VisualStudioVersion=$visualStudioVersion /verbosity:quiet }
			Write-Host 'Executed Clean!'    
	}
# NOT used yet
	task PerfTest {

    $projectPath = "$baseParent\Source\Tests\Webjet.FlightMerchandising.Test.PerformanceTest\Webjet.FlightMerchandising.Test.PerformanceTest.csproj"
 
	Write-Host "Starting performance test build!"
	msbuild $projectPath /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform /p:OutDir="$binariesDir\" /p:OutputPath="$outPath\" /p:VisualStudioVersion=$visualStudioVersion /p:SkipInvalidConfigurations=true
	
    ValidateExitCode(0)

    Write-Host "Build done!"

    Write-Host "Starting performance test!"

    $SiteVersion = $([System.DateTime]::Now.ToString("dd-MM-yyyy-HHmmss"))
    $RunSetting = "DEV"
    $LoadTest = "ConstantLoad.loadtest"
    $description = "Flights Merchandising Service Development"
    Copy-Item -Path "$baseParent\Source\Tests\Webjet.FlightMerchandising.Test.PerformanceTest\Local.testsettings" -Destination "$binariesDir" -Force
    cd $binariesDir
    .\RunLoadTest.ps1 -LoadTestRunSetting $RunSetting -LoadTestToRun $LoadTest -SiteVersion $SiteVersion -LoadTestType "ConstantLoad" -SiteDescription $description -NotifySlack $true
    Write-Host "Performance test done!"

	ValidateExitCode(0)
}