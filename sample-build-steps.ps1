Include '.\BuildSystem\psake_ext.ps1'

properties {
  $base_dir = resolve-path .
  $sln = "$base_dir\YOUR_SOLUTION.sln"
  $testProjectsBaseDir = $base_dir
  $unit_test_projects = @("NAME_OF_THE_UNIT_TESTS_PROJECT")
  $js_test_projects = @("NAME_OF_THE_JSUNIT_TESTS_PROJECT")
  $test_settings_file = $null
  $test_run_config = $null
  $enable_code_metrics = $false
  $testCoverage_exclusions = $null
  $configuration = "Release"
  $platform = "Any CPU"
}

task default -depends Test

task Test -description "Runs all unit tests in solution" -depends CsTest, JsTest

task CsTest -description "Runs all CSharp unit tests in solution" -depends Compile {
  Run-CS-Tests $unit_test_projects $testProjectsBaseDir $test_categories $test_settings_file $test_run_config $enable_code_metrics $testCoverage_exclusions
}

task JsTest -description "Runs all jasmine unit tests in solution" {
  Run-JS-Tests $js_test_projects $enable_code_metrics
}

task Compile -description "Compiles all projects in solution" -depends Clean { 
  exec { msbuild $sln /p:Configuration=$configuration /p:Platform=$platform }
}

task Clean -description "Deletes all compiled files from solution" { 
  exec { msbuild $sln /target:clean /p:Configuration=$configuration /p:Platform=$platform }
}

task Publish -description "Builds and Publishes the solution" -depends Compile {
	exec { msbuild $sln /t:publish }
}

task ? -description "Lists all build tasks" -alias "Help" {
	WriteDocumentation
}