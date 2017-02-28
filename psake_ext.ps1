function DeleteIfExistsAndCreateEmptyFolder($dir){
    DeleteFolder $dir
    New-Item -ItemType Directory -Force -Path $dir
}
function DeleteFolder($dir){
    if ( Test-Path $dir ) {
               Get-ChildItem -Path  $dir -Force -Recurse | Remove-Item -force -recurse
               Remove-Item $dir -Force
    }
}
function DeleteFilesInFolder($dir){
		if ((Test-Path $dir)) {
			# Remove-Item -Recurse "$dir\**" | Where { ! $_.PSIsContainer }
			 Get-ChildItem -Path  $dir -Force -Recurse | Where { ! $_.PSIsContainer }| Remove-Item -force -recurse
        }
}
function IsMSTestRunner($testRunner) { #check do we using MSTest or VSTest
  $b= $testRunner -like "*MSTest.Exe"
  return $b
}
<#
.Summary
Function execute process and exit script if result ExitCode <>0
#>
function RunProcess ($processPath, $arguments,
					 $TimeoutMilliseconds=1800000 #30min 
					)
{
	Write-Host $processPath $arguments
	$result=  Invoke-Executable  $processPath $arguments -TimeoutMilliseconds $TimeoutMilliseconds
	$result.StdOut
	$result.StdErr
	if($result.ExitCode -ne 0){
		Write-Host "Process returned exit code:"$result.ExitCode
		exit $result.ExitCode
	}
}
function ValidateExitCode($expectedExitCode)
{
	Write-Host "Last exit code $LASTEXITCODE"
	if (-not ($LASTEXITCODE -eq $expectedExitCode)) 
	{
		throw [System.ArgumentOutOfRangeException] "Exit code should not be $LASTEXITCODE, expected value $expectedExitCode."
	}
}
function Invoke-Executable {
# from http://stackoverflow.com/a/24371479/52277
	# Runs the specified executable and captures its exit code, stdout
    # and stderr.
    # Returns: custom object.
param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [String]$sExeFile,
        [Parameter(Mandatory=$false)]
        [String[]]$cArgs,
        [Parameter(Mandatory=$false)]
        [String]$sVerb,
		[Parameter(Mandatory=$false)]
        [Int]$TimeoutMilliseconds=1800000 #30min
    )
    # Setting process invocation parameters.
    $oPsi = New-Object -TypeName System.Diagnostics.ProcessStartInfo
    $oPsi.CreateNoWindow = $true
    $oPsi.UseShellExecute = $false
    $oPsi.RedirectStandardOutput = $true
    $oPsi.RedirectStandardError = $true
    $oPsi.FileName = $sExeFile
    if (! [String]::IsNullOrEmpty($cArgs)) {
        $oPsi.Arguments = $cArgs
    }
    if (! [String]::IsNullOrEmpty($sVerb)) {
        $oPsi.Verb = $sVerb
    }

    # Creating process object.
    $oProcess = New-Object -TypeName System.Diagnostics.Process
    $oProcess.StartInfo = $oPsi


    # Starting process.
    [Void]$oProcess.Start()
# Tasks used based on http://www.codeducky.org/process-handling-net/	
 $outTask = $oProcess.StandardOutput.ReadToEndAsync();
 $errTask = $oProcess.StandardError.ReadToEndAsync();
 $bRet=$oProcess.WaitForExit($TimeoutMilliseconds)
	if (-Not $bRet)
	{
	 $oProcess.Kill();
	#  throw [System.TimeoutException] ($sExeFile + " was killed due to timeout after " + ($TimeoutMilliseconds/1000) + " sec ") 
	}
	$outText = $outTask.Result;
	$errText = $errTask.Result;
	if (-Not $bRet)
	{
		$errText =$errText + ($sExeFile + " was killed due to timeout after " + ($TimeoutMilliseconds/1000) + " sec ") 
	}
    $oResult = New-Object -TypeName PSObject -Property ([Ordered]@{
        "ExeFile"  = $sExeFile;
        "Args"     = $cArgs -join " ";
        "ExitCode" = $oProcess.ExitCode;
        "StdOut"   = $outText;
        "StdErr"   = $errText
    })

    return $oResult
}
function CallTestRunner ($test, $testSettings){

	$assemblyName = split-path $test -leaf -resolve
    $resultFile = [String]::Format("{0}-{1}.trx", [DateTime]::Now.ToString("MMddyyyyHHmmssfff"), [string]$assemblyName.Replace(".",""))
	$resultFile = "$baseDir\Output\$resultFile"
	if(IsMSTestRunner  ) {
		#prepare to call MSTest
		$arguments = [string[]]@(
			"-testcontainer:$test",
			"-resultsfile:$resultFile",
			"-category:!Broken",
			"-testsettings:$testSettings")
	}	
	else { #VSTest
		# No direct way to specify test results file, see http://stackoverflow.com/questions/14483837/specifying-results-filename-for-vstest-console-exe	
		#prepare to call VSTest.Console.exe
		$arguments = [string[]]@(
			"$test",
			"/Logger:trx",
			"/TestCaseFilter:""TestCategory!=Broken""")
	}	
    RunProcess -processPath $TestRunner -arguments $arguments
}

function Run-CS-Tests {
	param(
		[string]$BaseDir,
		[string[]]$TestProjects,
		[string]$TestProjectsBaseDir,
		[string]$BuildConfiguration,
		[string]$TestCategory,
		[string]$TestSettingsFileName,
		[string]$TestRunConfigFileName,
		[bool]$EnableCodeMetrics,
		[string]$TestCoverageExclusion,
		[string]$BuildPlatform,
		[string]$VisualStudioVersion
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

	foreach ($testProject in $TestProjects) {
		Write-Verbose "Running tests for project $testProject"

		foreach ($testDllFileName in Get-ChildItem -Path "$TestProjectsBaseDir" -Recurse $testProject) {
			Write-Verbose "Running tests for DLL $testDllFileName"

			$trxFilePath = 'Output\TestResults\{0}.trx' -F $(Nicify-TestProjectDir $testDllFileName)
      
			if(Test-Path $trxFilePath){
				Remove-Item $trxFilePath
			}
      
			if ($EnableCodeMetrics -eq $true) {
				Write-Verbose "--- Running tests with coverage on $file ---"
				Run-CS-Tests-With-Coverage $testProjectDir $trxFilePath $msTestPath $(($testDllFileName).FullName) $TestCoverageExclusion $categoryArg $testSettingsArg $testRunConfigArg $VisualStudioVersion
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
	$supportedVersions = @( '14.0', '12.0', '11.0', '10.0' )
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
		[string]$VisualStudioVersion
	)
	
	$nicifiedName = Nicify-TestProjectDir $TestProjectDir
 
	Run-Coverage-Tool $TestProjectDir $nicifiedName $TrxFilePath $MsTestPath $TestDllFileName $TestCoverageExclusion $CategoryArg $TestSettingsArg $TestRunConfigArg $VisualStudioVersion
	
	Run-Report-Tool $nicifiedName
	
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
		[string]$VisualStudioVersion
	)
	
	if (!$TestCoverageExclusion) {
		$TestCoverageExclusion =  ''
	}
	
    Write-Debug "Running Test with coverage on $TestDllFileName"
	
	
	
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
	
	$coverageResult = (Start-Process -PassThru -FilePath BuildSystem/opencover/opencover.console.exe -Wait -NoNewWindow -ArgumentList $coverageArgs)

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
		[string]$nicifiedName
	)
	
	Write-Debug "Running Reporting"
	
	$reportArgs = @(
		"-reports:`"Output\Coverage\$nicifiedName.xml`""
		"-targetdir:`"Output\Coverage\$nicifiedName`""
		"-verbosity:Info"
	)

	$reportResult = (Start-Process -PassThru -FilePath BuildSystem/reportgenerator/bin/ReportGenerator.exe -Wait -NoNewWindow -ArgumentList $reportArgs)

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

		throw ("Error executing tests for: $TestProjectDir (TRX file has been opened in VS)")
	}
}

function XmlDocTransform($xml, $xdt)
{
    Write-Host "Transforming $xml with $xdt"
    $xml = ("$BaseDir\$xml")
    $xdt = ("$BaseDir\$xdt")

    if (!$xml -or !(Test-Path -path $xml -PathType Leaf)) {
        Write-Host "File not found. $xml";
        exit 1;
        #throw "File not found. $xml";
    }
    if (!$xdt -or !(Test-Path -path $xdt -PathType Leaf)) {
        Write-Host "File not found. $xdt";
        exit 1;
        #throw "File not found. $xdt";
    }

    #$scriptPath = (Get-Variable MyInvocation -Scope 2).Value.InvocationName | split-path -parent

    Add-Type -LiteralPath "$BaseDir\BuildSystem\bin\Microsoft.Web.XmlTransform.dll"

    $xmldoc = New-Object Microsoft.Web.XmlTransform.XmlTransformableDocument;
    $xmldoc.PreserveWhitespace = $true
    $xmldoc.Load($xml);

    $transf = New-Object Microsoft.Web.XmlTransform.XmlTransformation($xdt);
    if ($transf.Apply($xmldoc) -eq $false)
    {
        throw "Transformation failed."
    }
    $xmldoc.Save($xml);
}

function TransformTSA($tenant)
{

    $TSA = "main\TSA\Applications\WebjetTsa\"
    $webConfig = "Web.config"
    $transformConfig = "Web.{0}.config"

    XmlDocTransform ($TSA + $webConfig)  ($TSA + $transformConfig -f $tenant)

}