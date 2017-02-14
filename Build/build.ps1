cls
$SolutionRoot = $psscriptroot + "\..\Source" 

$ProjectJsonDir =  "$SolutionRoot\src\AdminPortal\" #
$ZipFilePath = $SolutionRoot + "\src\AdminPortal\bin\release\net461"
$BuildVersion = $env:Build
$ZipFileName = "AdminPortal-$BuildVersion.zip"
	$slackDetails = @{channel =  "#adminportal";
					username = "@mfreidgeim";#adminportal
					#icon_url = "http://besticons.net/sites/default/files/departing-flight-icon-3634.png"
                   }
. "$PSScriptRoot\BuildScripts\CoveragePercentUpdate.ps1" #Including Slack
$env:path +=";C:\Program Files (x86)\Microsoft Visual Studio 14.0\Web\External;"

function Restore ($projectjsondir)
{
Write-host "Restoring dependencies"
dotnet restore $projectjsondir\project.json
Write-host "Finish Restoring dependencies"

}

function Build ($projectjsondir)
{
write-host "BUILDING"
dotnet build $projectjsondir\project.json --configuration release

write-host "Finished Building the project"
}

function Publish($projectjsondir)
{
write-host "PUBLISHING"
dotnet --verbose publish $projectjsondir\project.json --configuration release 
write-host "Finished Publishing"

if ($lastexitcode -eq 1)
{
SendSlack "AdminPortal Build" "Publishing AdminPortal  project failed" $slackDetails
}

}
function ArchiveAndCopy($zipfilepath)
{
Remove-Item $zipfilepath\AdminPortal.*-force
Invoke-Expression "$psscriptroot\Tools\7za.exe a -tzip $zipfilepath\$ZipFileName $psscriptroot\..\Source\src\AdminPortal\bin\release\net461\win7-x64\publish\*"


Write-host "Removing old OUTPUT FOLDER"
Remove-Item $psscriptroot\..\OUTPUT -force -recurse
Write-host "Creating OUTPUT Directory"
md $psscriptroot\..\OUTPUT
write-host "Copying Items to OUTPUT FOLDER "
Copy-Item $zipfilepath\AdminPortal-$BuildVersion.zip -Destination $psscriptroot\..\OUTPUT -recurse -force
Copy-Item $psscriptroot\..\Deployment\ -Destination $psscriptroot\..\OUTPUT -recurse -force   
Copy-Item $psscriptroot\..\Configuration\ -Destination $psscriptroot\..\OUTPUT -recurse -force   
}

function UnitTest()
{
cd $psscriptroot\..\Source\test\AdminPortal.UnitTests
dotnet restore
dotnet test
}

Restore $SolutionRoot #$ProjectJsonDir

ArchiveAndCopy $ZipFilePath
UnitTest

Publish $ProjectJsonDir