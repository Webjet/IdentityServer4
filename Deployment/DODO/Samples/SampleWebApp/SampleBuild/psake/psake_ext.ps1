function Run-CS-Tests {
    param(
        [string]$BaseDir,
        [string[]]$TestProjectsDir,
        [string]$TestProjectsBaseDir,
        [string]$BuildConfiguration,
        [string]$TestCategory,
        [string]$TestSettingsFileName,
        [string]$TestRunConfigFileName,
        [bool]$EnableCodeMetrics,
        [string]$TestCoverageExclusion,
        [string]$BuildPlatform,
        [string]$VisualStudioVersion,
		[string]$ToolsPath
    )

    if (!$VisualStudioVersion) {
        $VisualStudioVersion = Get-Highest-Installed-Version-Of-Visual-Studio
    }

    $msTestPath = Get-MsTest-Path $VisualStudioVersion

    Create-Output-Directories

    $categoryArg = if ($TestCategory) {
        " /category:$TestCategory"
    } else { '' }

    $testSettingsArg = if ($TestSettingsFileName) {
        " /testsettings:`"$BaseDir\$TestSettingsFileName`""
    } else { '' }

    $testRunConfigArg = if ($TestRunConfigFileName) {
        " /runconfig:`"$BaseDir\$TestRunConfigFileName`""
    } else { '' }

    foreach ($testProjectDir in $TestProjectsDir) {
        Write-Verbose "Running tests for project $testProjectDir"

        foreach ($testDllFileName in Get-ChildItem -Path "$TestProjectsBaseDir\$testProjectDir\bin\$BuildConfiguration" -Recurse '*Tests.dll') {
            Write-Verbose "Running tests for DLL $testDllFileName"

            $trxFilePath = 'Output\TestResults\{0}.trx' -F $(Nicify-TestProjectDir $testProjectDir)
      
            if(Test-Path $trxFilePath){
                Remove-Item $trxFilePath
            }
      
            if ($EnableCodeMetrics -eq $true) {
                Write-Verbose "--- Running tests with coverage on $file ---"
                Run-CS-Tests-With-Coverage $testProjectDir $trxFilePath $msTestPath $(($testDllFileName).FullName) $TestCoverageExclusion $categoryArg $testSettingsArg $testRunConfigArg $VisualStudioVersion $ToolsPath
            } else {
                Write-Verbose "--- Running tests without coverage on $file ---"
                Run-CS-Tests-Without-Coverage $testProjectDir $trxFilePath $msTestPath $(($testDllFileName).FullName) $categoryArg $testSettingsArg $testRunConfigArg $VisualStudioVersion
            }
        }
    } 
}

function Create-Output-Directories {

    if(!(Test-Path -Path "Output" )){
        New-Item -ItemType directory -Path "Output" | Out-Null
    }

    if(!(Test-Path -Path 'Output\TestResults' )){
        New-Item -ItemType directory -Path 'Output\TestResults' | Out-Null
    }
    else {
        Remove-Item 'Output\TestResults\*' -Recurse | Out-Null
    }
    
    if(!(Test-Path -Path 'Output\Coverage' )){
        New-Item -ItemType directory -Path 'Output\Coverage' | Out-Null
    }
}

function Get-Highest-Installed-Version-Of-Visual-Studio {
    $supportedVersions = @( '12.0', '11.0', '10.0' )
    $highestVersion = $null
    
    Write-Debug 'Finding the highest installed version of VS'
    
    foreach ($supportedVersion in $supportedVersions) {
        Write-Debug "Trying $supportedVersion"
        $environmentVariableName = "VS$($supportedVersion.replace('.', ''))COMNTOOLS"
        $toolsPath = (get-item env:$environmentVariableName).Value
        
        if (Test-Path -Path $toolsPath) {
            $highestVersion = $supportedVersion
            break
        }
    }
    
    if (!$highestVersion) {
        throw ('Your version of Visual Studio is not supported.')
    }
    
    Write-Debug "Highest VS version is $highestVersion"
    
    return $highestVersion
}

function Get-Visual-Studio-Installation-Path {
    param(
        [string]$VisualStudioVersion
    )

    $registryKeyTemplate = 'HKLM:\Software\Microsoft\VisualStudio\{0}'
    
    if ([System.IntPtr]::Size -ne 4) {
        $registryKeyTemplate = 'HKLM:\Software\Wow6432Node\Microsoft\VisualStudio\{0}'
    }
    
    Write-Debug "registryKeyTemplate:$registryKeyTemplate"
    
    $registryKey = $registryKeyTemplate -F $VisualStudioVersion
    $installationPath = (Get-ItemProperty $registryKey).InstallDir
    
    Write-Debug "registryKey:$registryKey"
    Write-Debug "installationPath:$installationPath"
    
    if (!(Test-Path -Path $installationPath)){
        throw ("VS $VisualStudioVersion is not installed on your system. It is needed to run MsTest.")
    }
    
    return $installationPath
}

function Get-MsTest-Path {
    param(
        [string]$VisualStudioVersion
    )
    
    $installationPath = Get-Visual-Studio-Installation-Path $VisualStudioVersion
    $msTestPath = join-path -path $installationPath -childpath 'MsTest.exe'
    
    Write-Debug "msTestPath:$msTestPath"
  
    if (!(Test-Path $msTestPath)){
        throw ('VS ' + $VisualStudioVersion + ' is not installed on your system. It is needed to run MsTest.')
    }
  
    return $msTestPath
}

function Run-CS-Tests-With-Coverage {
    param(
        [string]$TestProjectDir,
        [string]$TrxFilePath,
        [string]$MsTestPath,
        [string]$TestDllFileName,
        [string]$TestCoverageExclusion,
        [string]$CategoryArg,
        [string]$TestSettingsArg,
        [string]$TestRunConfigArg,
        [string]$VisualStudioVersion,
		[string]$ToolsPath
    )
    
    $nicifiedName = Nicify-TestProjectDir $TestProjectDir
 
    Run-Coverage-Tool $TestProjectDir $nicifiedName $TrxFilePath $MsTestPath $TestDllFileName $TestCoverageExclusion $CategoryArg $TestSettingsArg $TestRunConfigArg $VisualStudioVersion $ToolsPath
    
    Run-Report-Tool $nicifiedName $ToolsPath
    
    Check-Coverage-Threshold $nicifiedName
}

function Run-Coverage-Tool {
    param(
        [string]$TestProjectDir,
        [string]$nicifiedName,
        [string]$TrxFilePath,
        [string]$MsTestPath,
        [string]$TestDllFileName,
        [string]$TestCoverageExclusion,
        [string]$CategoryArg,
        [string]$TestSettingsArg,
        [string]$TestRunConfigArg,
        [string]$VisualStudioVersion,
		[string]$ToolsPath
    )
    
    if (!$TestCoverageExclusion) {
        $TestCoverageExclusion =  ''
    }
    
    Write-Debug "Running Test with coverage"
    
    
    
    $coverageArgs = @(
        "-target:`"$MsTestPath`""
        '-register:user'
        '-returntargetcode'
        "-output:`"Output\Coverage\$nicifiedName.xml`""
        "-targetargs:`"/testcontainer:`"`"$TestDllFileName`"`" $CategoryArg $TestSettingsArg $TestRunConfigArg /resultsfile:`"`"$TrxFilePath`"`" `""
        "-filter:`"+[*]* $TestCoverageExclusion`""
        "-log:Error"
        "-mergebyhash"
        )
        
    Write-Verbose ($coverageArgs | out-string)
    
    $coverageResult = (Start-Process -PassThru -FilePath "$ToolsPath/opencover/opencover.console.exe" -Wait -NoNewWindow -ArgumentList $coverageArgs)

    if ($coverageResult.ExitCode) {
    
		Write-Error "One or more tests failed"

        Open-MsTestResultFile $VisualStudioVersion $TrxFilePath
    
        throw ("One or more tests may have failed. Error generating coverage for: $TestProjectDir")
    }
}

function Open-MsTestResultFile {
    param(
        [string]$VisualStudioVersion,
        [string]$TrxFilePath
    )
    try {
	    $dte = [System.Runtime.InteropServices.Marshal]::GetActiveObject("VisualStudio.DTE.$VisualStudioVersion")

	} catch {
		Write-Error "Cannot open dte - trying to open the test result file $($_.Exception.ToString())"
	}

	if ($dte -ne $null) {
		$dte.ExecuteCommand("File.OpenFile", (Get-item $TrxFilePath).FullName)
	}
}

function Nicify-TestProjectDir {
    param(
        [string]$TestProjectDir
    )
    
    $output = $TestProjectDir.replace('\', '.')
    
    return $output
}

function Run-Report-Tool {
    param(
        [string]$nicifiedName,
		[string]$ToolsPath
    )
    
    Write-Debug "Running Reporting"
    
    $reportArgs = @(
        "-reports:`"Output\Coverage\$nicifiedName.xml`""
        "-targetdir:`"Output\Coverage\$nicifiedName`""
        "-verbosity:Info"
    )

    $reportResult = (Start-Process -PassThru -FilePath "$ToolsPath/reportgenerator/bin/ReportGenerator.exe" -Wait -NoNewWindow -ArgumentList $reportArgs)

    if ($reportResult.ExitCode) {
        throw ('Error generating coverage report')
    }
}

function Check-Coverage-Threshold {
    param(
        [string]$nicifiedName
    )

    $actualTestCoverage = Parse-CS-Test-Coverage "Output\Coverage\$nicifiedName.xml"
    
    write-host "Test Coverage: $actualTestCoverage"
    
    Write-Coverage-Threshold $nicifiedName $actualTestCoverage

    if ($actualTestCoverage -lt $testCoverageThreshold) {
        throw ("CS Test coverage below minimum threshold (expected: $testCoverageThreshold%, actual: $actualTestCoverage%)")
    }
}

function Write-Coverage-Threshold {
    param(
        [string]$nicifiedName,
        [Double]$testCoverage
    )
    
    $filePath = "Output\$nicifiedName.log"
    
    (Get-Date -Format "yyyy/MM/dd HH:mm:ss"),"$testCoverage%" -Join ' ' | Out-File -Append -FilePath $filePath
}

function Parse-CS-Test-Coverage {
    param(
        [string]$fileName
    )

    [System.Xml.XmlDocument]$xmlDoc = new-object System.Xml.XmlDocument
    $xmlDoc.load((Get-Item $fileName).FullName)
    return $xmlDoc.SelectSingleNode("/CoverageSession/Summary").sequenceCoverage -as [Double]
}

function Run-CS-Tests-Without-Coverage {
    param(
        [string]$TestProjectDir,
        [string]$TrxFilePath,
        [string]$MsTestPath,
        [string]$TestDllFileName,
        [string]$CategoryArg,
        [string]$TestSettingsArg,
        [string]$TestRunConfigArg,
        [string]$VisualStudioVersion
    )

    $args = @("/testcontainer:`"$TestDllFileName`" $CategoryArg $TestSettingsArg $TestRunConfigArg /resultsfile:`"$TrxFilePath`"")
    
    Write-Debug "Calling MsTest with args: $args"

    $result = (Start-Process -PassThru -FilePath "$MsTestPath" -Wait -NoNewWindow -ArgumentList $args)

    if ($result.ExitCode -ne 0) {
        
		Write-Error "One or more tests failed"

        Open-MsTestResultFile $VisualStudioVersion $TrxFilePath

        throw ("Error executing tests for: $TestProjectDir ")
    }
}

function Run-JS-Tests {
  param(
    [string]$test_projects,
    [bool]$enable_code_metrics
  )

  if(!(Test-Path -Path "Output" )){
    New-Item -ItemType directory -Path "Output" | Out-Null
  }

  if(!(Test-Path -Path "Output\Coverage" )){
    New-Item -ItemType directory -Path "Output\Coverage" | Out-Null
  }
  
  foreach ($test_project in $test_projects) {
    if ($EnableCodeMetrics -eq $true) {
      exec { BuildSystem\chutzpah\chutzpah.console.exe $baseDir\$test_project\specs /coverage }
      move _Chutzpah.coverage.html Output\Coverage\$test_project.html -Force
      move _Chutzpah.coverage.json Output\Coverage\$test_project.json -Force
    } else {
      exec { BuildSystem\chutzpah\chutzpah.console.exe $baseDir\$test_project\specs }
    }
  }
}