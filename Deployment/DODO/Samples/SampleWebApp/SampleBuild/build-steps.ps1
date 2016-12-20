Include '.\psake\psake_ext.ps1'

properties {
    $baseDir = resolve-path $PSScriptRoot
	$baseParent = "$((Get-Item $baseDir).Parent.FullName)"
	$buildConfiguration = 'Release'
    $buildPlatform = 'Any CPU'
    $buildRevision = ($env:build_revision, '0' -ne $null)[0] 
    $buildPackageTag = "master"
	$outPath = "$baseParent\SampleOutput"
	$outDir = "$baseParent\SampleBinaries"
	$nugetPublishFolder = "$outPath\NuGet-Output"
	$nugetPackageVersion = [string]::Format("{0:yyyy}.{0:MM}.{0:dd}.{1}-{2}", [DateTime]::Today, $buildRevision, $buildPackageTag)
	$nugetPackageName = $nugetPackageVersion + ".nupkg"
	$visualStudioVersion = '14.0' # Set this to the version of Visual Studio you have (12.0 = 2013, 11.0 = 2012, 10.0 = 2010)
	$nugetExe = "$baseParent\SampleBuild\NuGet.exe"
	$7zipExe = "$baseParent\SampleBuild\7za.exe"
}

task default -description "Default DevOps Build Task" -depends Clean,CompileWebApp, CompileWebjob

#Cleans the output folders and creates output structure
task Clean {
		if ((Test-Path $outPath)) {
			Remove-Item -Recurse "$outPath\**" | Where { ! $_.PSIsContainer }
        }
		
		if ((Test-Path $outDir)) {
			Remove-Item -Recurse "$outDir\**" | Where { ! $_.PSIsContainer }
        }
		
		Write-Host "Nuget output folder: " + $nugetPublishFolder
        if (!(Test-Path $nugetPublishFolder)) {
             md $nugetPublishFolder 
        }
}


task CompileWebApp -depends Clean {
     
	$solution = "$baseParent\SampleWebApp.sln"
 
	Write-Host "Starting build!"
	msbuild $solution /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform /p:OutDir="$outDir\" /p:OutputPath="$outPath\" /p:VisualStudioVersion=$visualStudioVersion /p:SkipInvalidConfigurations=true
	Write-Host "Build done!"
	
	Write-Host "Starting packaging!"
	.$nugetExe pack "$baseParent\SampleBuild\SampleWebApp.nuspec" -NoPackageAnalysis -OutputDirectory $nugetPublishFolder -Version $nugetPackageVersion
	.$nugetExe pack "$baseParent\SampleBuild\SampleWebAppSwap.nuspec" -NoPackageAnalysis -OutputDirectory $nugetPublishFolder -Version $nugetPackageVersion
	Write-Host "Packaging Done!"
}

task CompileWebjob -depends CompileWebApp {
	
	$webjobProject = "$baseParent\SampleWebjob\SampleWebjob.csproj"
	Write-Host "Starting webjobs build!"
	msbuild $webjobProject /p:Configuration=$buildConfiguration /p:Platform=$buildPlatform /p:OutDir="$outDir\SampleWebjob\" /p:OutputPath="$outPath\" /p:VisualStudioVersion=$visualStudioVersion /p:SkipInvalidConfigurations=true
	Write-Host "Build webjobs done!"
	
	Write-Host "Zip webjobs!"
	$sampleWebJobCmd = "$7zipExe a $outDir\SampleWebjob\SampleWebjob.zip $outDir\SampleWebjob\"
	invoke-expression $sampleWebJobCmd
	Write-Host "Zip webjobs done!"
	
	Write-Host "Starting packaging!"
	.$nugetExe pack "$baseParent\SampleBuild\SampleWebjob.nuspec" -NoPackageAnalysis -OutputDirectory $nugetPublishFolder -Version $nugetPackageVersion
	Write-Host "Packaging Done!"
}




