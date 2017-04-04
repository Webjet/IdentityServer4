cls
function Main
{
$ErrorActionPreference = "Stop" 
$baseParent = "$((Get-Item  $psscriptroot).Parent.FullName)"
$SolutionRoot =$baseParent + "\Source" 
$ProjectJsonDir =  "$SolutionRoot\src\AdminPortal\"  
$ZipFilePath = $SolutionRoot + "\src\AdminPortal\bin\release\net461"
$outPath="$psscriptroot\..\OUTPUT"
$packagesRoot ="$SolutionRoot\packages"
#$BuildVersion = $env:Build
$ZipFileName = "AdminPortal.zip" # -$BuildVersion
	$slackDetails = @{channel =  "#adminportal";
					username = "@mfreidgeim";#adminportal
					#icon_url = "http://besticons.net/sites/default/files/departing-flight-icon-3634.png"
                   }
#$DebugPreference="Continue"				   
#import-module "$PSScriptRoot\BuildScripts\psake_ext.ps1"
. "$PSScriptRoot\BuildScripts\psake_ext.ps1"
. "$PSScriptRoot\BuildScripts\CoveragePercentUpdate.ps1" #Including Slack
#import-module "$PSScriptRoot\..\Deployment\DODO\dodo-general.psm1" #consider to copy to build scripts. Includes Coalesce
$CoverageThresholdTolerance = Coalesce $env:CoverageThresholdTolerance 0.
$env:runDevCoverage=Coalesce $env:runDevCoverage 'true'


#for bower http://stackoverflow.com/questions/20666989/bower-enogit-git-is-not-installed-or-not-in-the-path
$env:path +=";C:\Program Files (x86)\Microsoft Visual Studio 14.0\Web\External;C:\Program Files (x86)\Microsoft Visual Studio 14.0\Web\External\git;"
  Write-Debug $env:path
  WriteDebug-Dir-IfExists "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Web\External"
  WriteDebug-Dir-IfExists "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Web\External\git"

#test/coverage configurations
#	$csTestRunner = "`"$baseParent\Source\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe`""
#	$TestRunner = "C:\Program Files (x86)\Microsoft Visual Studio " + $visualStudioVersion +"\Common7\IDE\MSTest.exe" 
#    $csTestRunner = "C:\Program Files (x86)\Microsoft Visual Studio " + $visualStudioVersion +"\Common7\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.Console.exe" 
$packagesRoot= "$SolutionRoot\packages"
    $csTestRunner = "C:\Program Files\dotnet\dotnet.exe" 
	$csTestAssemblies =  "`"$SolutionRoot\test\AdminPortal.UnitTests`" " # `"$binariesDir\AdminPortal.IntegrationTests.dll`" "  
	$openCover = "$packagesRoot\OpenCover.4.6.519\tools\OpenCover.Console.exe"
	$reportGen = "$packagesRoot\ReportGenerator.2.5.1\tools\ReportGenerator.exe"
	$openCoverageReport=($env:openCoverageReport, 'true' -ne $null)[0]
#    $runIntegrationUnitTests =  ($env:runIntegrationUnitTests, 'false' -ne $null)[0] #false default

	Restore $SolutionRoot #$ProjectJsonDir
	DeleteIfExistsAndCreateEmptyFolder $outPath
	Build $SolutionRoot

	Write-Host "CodeCoverage : $env:runDevCoverage"
	if($env:runDevCoverage -eq 'true') {
	RestoreFullNetPackages "$SolutionRoot\AdminPortal.sln"
	CodeCoverage
	}
	else {
	UnitTest
	}
	Publish $ProjectJsonDir
	ArchiveAndCopy $ZipFilePath #what was published
}

function Restore ($projectjsondir)
{
Write-host "Restoring dependencies"
dotnet restore $projectjsondir #\project.json
Write-host "Finish Restoring dependencies"

}
function RestoreFullNetPackages ($solutionFile)
{
     $nugetExe = "$PSScriptRoot\Tools\Nuget\Nuget.exe"
    
    Write-Host "Restoring packages for Full.Net..."
	.$nugetExe restore $solutionFile  Verbosity detailed
	ValidateExitCode(0)
	Write-Host "Packages for Full.Net restored"
}

function Build ($solutionDir)
{
write-host "BUILDING $solutionDir"
#dotnet build $solutionDir #\project.json --configuration release
#http://stackoverflow.com/questions/37961691/net-core-build-a-solution#
cd $solutionDir
dotnet build "*\**\project.json"

write-host "Finished Building the project"
}

function Publish($projectjsondir)
{
write-host "PUBLISHING"
write-debug "Path= $env:Path"

dotnet --verbose publish $projectjsondir\project.json --configuration release 
write-host "Finished Publishing"

if ($lastexitcode -eq 1)
{
SendSlack "AdminPortal Build" "Publishing AdminPortal  project failed" $slackDetails
}

}
function ArchiveAndCopy($zipfilepath)
{
$zipFiles="$zipfilepath\AdminPortal.*"
if(Test-Path $zipFiles) { 
   Remove-Item $zipfilepath\AdminPortal.* -force
}
Invoke-Expression "$psscriptroot\Tools\7za.exe a -tzip $zipfilepath\$ZipFileName $psscriptroot\..\Source\src\AdminPortal\bin\release\net461\win7-x64\publish\*"

write-host "Copying Items to OUTPUT FOLDER "
Copy-Item $zipfilepath\$ZipFileName -Destination $outPath -recurse -force
Copy-Item $psscriptroot\..\Deployment\ -Destination $outPath -recurse -force   
Copy-Item $psscriptroot\..\Configuration\ -Destination $outPath -recurse -force   
}

function UnitTest()
{
cd $SolutionRoot\test\AdminPortal.UnitTests
dotnet restore
dotnet test
}
 #  task Test -description "Run unit tests in solution" -depends Compile 
 function CodeCoverage()
 {
    	Write-Host 'Starting Tests!'
  WriteDebug-Dir-IfExists $packagesRoot
  WriteDebug-Dir-IfExists "$packagesRoot\OpenCover.4.6.519\"
  WriteDebug-Dir-IfExists "$packagesRoot\OpenCover.4.6.519\tools\"

		$coverOut = "$outPath\Test-Output"    
    	$coverOutPath = "`"$coverOut\projectCoverageReport.xml`""
    	$coverReportOut = "`"$coverOut\cover`""
		CreateFolderIfNotExists $coverOut
		
$targetargs=" test $($csTestAssemblies) " # folder, not DLL
		Write-Host "OpenCover targetargs - $($csTestAssemblies)"
# it is case-sensitive `"-[xunit.assert]*`" `"-[xunit.core]*`"
# https://github.com/opencover/opencover/wiki/Usage
#`"-[Serilog.Sinks.SumoLogic]*`" `"-[Serilog.Enrichers.NancyContext]*`" -[KellermanSoftware.CompareNetObjects]*
		$filters = "`"+[*]*`" `"-[*.*Tests]*`"    `"-[Build]*`" `"-[Microsoft.WindowsAzure.Storage]*`"   " +
   		 " -[DbUpWrapper]*  -[FluentAssertions*]* -[MSTestHacks]* -[Microsoft.*]* -[Flurl.*]*  "  
			#-noshadow for XUnit
#example from http://stackoverflow.com/questions/38425936/how-to-measure-code-coverage-in-asp-net-core-projects-in-visual-studio			
#.$openCover -register:user -mergebyhash -skipautoprops -target:$csTestRunner -targetargs:"$($csTestAssemblies) " -returntargetcode -output:$coverOutPath -filter:$filters
		#-log:,#Default :Info  [Off|Fatal|Error|Warn|Info|Debug|Verbose|All]
		$arguments="-register:user -mergebyhash -skipautoprops -target:`"$csTestRunner`" -targetargs:`"$targetargs`" -skipautoprops -returntargetcode -output:$coverOutPath -filter:`"$filters`" -log:Verbose "
 

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
		                        [System.Convert]::ToBoolean($openCoverageReport),[System.Convert]::ToDecimal($CoverageThresholdTolerance)

    }

  Main

