Framework "4.6.1"

Include "$PSScriptRoot\BuildSystem\psake_ext.ps1"

properties {
    $currentDir = resolve-path . #  location of build-steps.ps1 ,i.e. 'build'
	$baseDir = "$((Get-Item $currentDir).Parent.FullName)"
	$binariesDir = "$baseDir\Binaries"
	$outputPath = "$baseDir\Output"
#	$webJobOutPath = "$outputPath\WebJobs"
	$solution = "$baseDir\AdminPortal.sln"
	$buildConfiguration = 'Release'
	$buildPlatform = 'Any CPU'
	$visualStudioVersion = Get-Highest-Installed-Version-Of-Visual-Studio
#	$visualStudioVersion = ( $visualStudioVersion,'12.0' -ne $null)[0]
#test/coverage configurations 
	$runCsCodeCoverage =  ($env:runCsCodeCoverage, 'true' -ne $null)[0] #true default
    $runDevCoverage =  ($env:runDevCoverage,  'false' -ne $null)[0] 
	$csTestProjects = @('AdminPortal.Tests\bin\Release\AdminPortal.Tests.dll')
    $runIntegrationUnitTests =  ($env:runIntegrationUnitTests, 'false' -ne $null)[0] #false default
#	$csIntegrationUnitTestAssemblies = @('test\Integration\TSA.Database.IntegrationTests\bin\release\Tests.TSAIntegrationTests.dll')
#	$TestRunner = "C:\Program Files (x86)\Microsoft Visual Studio " + $visualStudioVersion +"\Common7\IDE\MSTest.exe" 
    $TestRunner = "C:\Program Files (x86)\Microsoft Visual Studio " + $visualStudioVersion +"\Common7\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.Console.exe" 
	$PackagePath = "$baseDir\Binaries\_PublishedWebsites\AdminPortal\*"	

	$BuildSystem="$baseDir\Build\BuildSystem"
	$buildRevision = ($env:build_revision, '0' -ne $null)[0] 
    $buildPackageTag = ($env:build_package_tag, 'Local' -ne $null)[0] 
	$nugetPackageVersion = [string]::Format("{0:yyyy}.{0:MM}.{0:dd}.{1}-{2}", [DateTime]::Today, $buildRevision, $buildPackageTag)
    $nugetExe = "$BuildSystem\NuGet\NuGet.exe";
}

task default -depends CsTest

task Compile -depends Clean -description 'Compiles the solution' { 

 #Do we need to check configs first?
  $configFolder = "$baseDir\Configuration"
#    $parameterFile = "$baseDir\Packages.Web\parameters.xml"

#    & "$baseDir\Deployment\AzureDeploymentFramework\WebDeployParameterVerifier\WebDeployParameterVerifier.exe" /ParameterXmlPath="$parameterFile" /SetParameterDir="$configFolder"
#    if($LASTEXITCODE -ne 0){
#        throw "Configuration Verification Tool threw an error. Please check the logs for more info"
#    }
	Write-Host "Building $solution "
	.$nugetExe restore $solution 
	msbuild $solution /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform   /p:VisualStudioVersion=$visualStudioVersion /p:SkipInvalidConfigurations=true
	
	Write-Host "Copying Site and Configs" # should it be a separate step?
  
    DeleteIfExistsAndCreateEmptyFolder $outputPath

	Copy-Item $PackagePath $outputPath -recurse
	Copy-Item $configFolder $outputPath -recurse
	Copy-Item "$baseDir\Deployment\*" $outputPath -recurse
	
	Write-Host "Packaging and Copying to out put path done!"

}
#TODO consider to move common parts to psake_ext.ps1
task CsTest -description 'Runs given CSharp tests in solution' -depends Compile {
	 
	 #global test settings
	 $testSettings = "$baseDir\Local.TestSettings" #test settings global to TSA
	  if($runCsCodeCoverage.ToLower() -ne 'true') {
		 #run standard unit tests
		 foreach($testAssembly in $csTestProjects){
			$test = "$baseDir\$testAssembly"
		    CallTestRunner -test $test -testSettings $testSettings
		 }
		 #run integration unit tests
		 if($runIntegrationUnitTests.ToLower() -eq 'true'){
			 foreach($testAssembly in $csIntegrationUnitTestAssemblies){
				$test = "$baseDir\$testAssembly"
				CallTestRunner -test $test -testSettings $testSettings
			 }
		 }
	   }
	  #code coverage
	  if($runCsCodeCoverage.ToLower() -eq 'true'){
		#OpenCover on TeamCity by some reason doesn't work with VStest.Console - still investigating 
		#$TestRunner = "C:\Program Files (x86)\Microsoft Visual Studio " + $visualStudioVersion +"\Common7\IDE\MSTest.exe" 
	  
        DeleteIfExistsAndCreateEmptyFolder $outputPath\opencover
	    DeleteIfExistsAndCreateEmptyFolder $outputPath\reportgenerator
		
		$openCoverPath = "$BuildSystem\opencover\Tools\OpenCover.Console.exe"
		$container = if (IsMSTestRunner($testRunner)) { "/testcontainer:"} Else {" "}
		#build the list of test arguments
 		$openCoverTargetArgs = if (IsMSTestRunner($testRunner)){ "/category:!Broken&!Fakes /testsettings:$testSettings" } 
								 Else {"/TestCaseFilter:""TestCategory!=Broken"" /Enablecodecoverage "} 
		foreach($testAssembly in $csTestProjects){
			$test =$container + "$baseDir\$testAssembly"
			$openCoverTargetArgs = "$openCoverTargetArgs $test"
		}
		
		foreach($testAssembly in $csIntegrationUnitTestAssemblies){
			$test =$container + "$baseDir\$testAssembly"
			$openCoverTargetArgs = "$openCoverTargetArgs $test"
		}
		
		$arguments = [string[]]@(
		"-register:user",
		"-target:`"$TestRunner`"",
		"-returntargetcode:0", # to stop if test failed
		"-log:Verbose",#Default :Info  [Off|Fatal|Error|Warn|Info|Debug|Verbose|All] 
		"-targetargs:`"$openCoverTargetArgs`"",
		"-filter:`"+[*]* -[DbUpWrapper]* " + 
			"    -[FluentAssertions*]* -[MSTestHacks]* -[Microsoft.*]* -[Flurl.*]* -[KellermanSoftware.CompareNetObjects]* " + 
			"  -[*Tests]*  `"",
        "-output:$outputPath\opencover\output.xml" 
        )
		 
	 	RunProcess -processPath $openCoverPath -arguments $arguments 

		#run report generator
		$reportGenerator = "$BuildSystem\reportgenerator\tools\ReportGenerator.exe"
		if($runDevCoverage.ToLower() -ne 'true'){
			$arguments = [string[]]@(
			"-reports:$outputPath\opencover\output.xml",
			"-targetdir:$outputPath\reportgenerator\",
			"-reporttypes:HtmlSummary")
		
			RunProcess -processPath $reportGenerator -arguments $arguments
		
			#Extract coverage percentage
			$reportGeneratorOutputFile = "$outputPath\reportgenerator\summary.htm"

			$regex = 'Line coverage:.+?<td>([0-9\.]+)%<\/td>'
			$coverCoverPercentage = Select-String -Path $reportGeneratorOutputFile -Pattern $regex  | % {$_.Matches} | % {$_.Groups[1].Value}
			Write-Host  "coverCoverPercentage:"$coverCoverPercentage

			$regex1 = 'Coverable lines:.+?<td>([0-9\.]+)<\/td>'
			$coverableLines = Select-String -Path $reportGeneratorOutputFile -Pattern $regex1  | % {$_.Matches} | % {$_.Groups[1].Value}
			Write-Host  "coverableLines:"$coverableLines

			$regex2 = '<th>Covered lines:.+?<td>([0-9\.]+)<\/td>'
			$coveredLines = Select-String -Path $reportGeneratorOutputFile -Pattern $regex2  | % {$_.Matches} | % {$_.Groups[1].Value}
			Write-Host  " coveredLines:"$coveredLines
		
			if(($coveredLines  -gt 0) -and ($coverableLines  -gt 0)){
				$coverCoverPercentage=($coveredLines/$coverableLines)*100
			}

			#write it to artefacts
			if($coverCoverPercentage -gt 0){
						[System.IO.File]::WriteAllText("$outputPath\CodeCoveragePercent.txt",$coverCoverPercentage)
			}
			Write-Host "CodeCover Percentage=" + $coverCoverPercentage
		}else{
			$arguments = [string[]]@(
			"-reports:$outputPath\opencover\output.xml",
			"-targetdir:$outputPath\reportgenerator\",
			"-reporttypes:Html")
		
			RunProcess -processPath $reportGenerator -arguments $arguments
			
			$reportGeneratorOutputFile = "$outputPath\reportgenerator\index.htm"
		}
		if(($env:openCoverageReport -ne $null) -and ($env:openCoverageReport.ToLower() -eq 'true')){
			Start "$reportGeneratorOutputFile"
		}
	}
}
#Not USED - can be deleted
task Package -depends CsTest -description 'Package WebApp ' { 

	#make the output path and directory for msbuild
    DeleteIfExistsAndCreateEmptyFolder $outputPath
    DeleteIfExistsAndCreateEmptyFolder $binariesDir


	
	#make the output path and directory for webjobs
#	if (!(Test-Path $webJobOutPath)) {
#		md $webJobOutPath
#	}
#	
#	$zipper = "$baseDir\Build\BuildSystem\bin\7za.exe"
#	$ImportSurveys = "$zipper a $webJobOutPath\ImportSurveys$nugetPackageVersion.zip $baseDir\WebJobs.ImportSurveys\bin\$buildConfiguration\"

#	invoke-expression $ImportSurveys
		
	#make a directory where the NuGet packages are output to
	$nuGetPublishFolder = "$outputPath\NuGet-Output"
    DeleteIfExistsAndCreateEmptyFolder $nuGetPublishFolder

		
	 Write-Host "Building $solution"
	 msbuild $solution /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform /p:OutDir="$binariesDir\" /p:OutputPath="$outputPath\" /p:VisualStudioVersion=$visualStudioVersion /p:SkipInvalidConfigurations=true
	 
	 
#	 Write-Host "For Site DeploySwap!", not required for Webjobs
#	 .$nugetExe pack "$baseDir\Build\nuspecs\SentimentAnalysis.DeploySwap.nuspec" -NoPackageAnalysis -OutputDirectory $nuGetPublishFolder -Version $nugetPackageVersion
#	 Write-Host "SentimentAnalysis DeploySwap done!"
} 

task Clean { 
	exec { msbuild $solution /target:clean /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform /p:VisualStudioVersion=$visualStudioVersion /verbosity:quiet }
}

task ? -Description "Helper to display task info" {
	Write-Documentation
}