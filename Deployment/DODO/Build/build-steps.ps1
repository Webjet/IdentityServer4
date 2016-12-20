properties {
  $basedir = resolve-path $PSScriptRoot
  $homeDir = "$((Get-Item $baseDir).Parent.FullName)"
  $sln = "$homeDir\Tools\DodoExe\Dodo.sln"
  $configuration = "Release"
  $platform = "Any CPU"
  $nuGet = "$homeDir\Tools\NuGet\Nuget.exe"
  $7za = "$homeDir\Tools\7za.exe"
}

task default -depends Publish

task Compile -description "Compiles all projects in solution" -depends Clean { 
  exec {
    Write-Host "Running Nuget restore..."
    & $nuGet restore $sln
    msbuild $sln /p:Configuration=$configuration /p:Platform=$platform 
  }
}

task Clean -description "Deletes all compiled files from solution" { 
  exec { msbuild $sln /target:clean /p:Configuration=$configuration /p:Platform=$platform }
}

task Publish -description "Builds and Publishes the solution" -depends Compile {
	Write-Host "Publishing Python DODO..."

  Invoke-Expression "$7za -y -r -q $homeDir\Py\dodo-py.zip $homeDir\Py\"
  #Invoke-Expression "$7za a -ttar $homeDir\Py\dodo-py.tar $homeDir\Py\"
  #Invoke-Expression "$7za a -tgzip $homeDir\Py\dodo-py.tgz $homeDir\Py\dodo-py.tar"
  
}

task Test -description "Runs DODO regression on DODO exe" -depends Compile {
  $exe = "$homeDir\Exe\dodo.exe" 
  & $exe "$homeDir\Samples\Templates\dodosample-azure-vm-scaleset.json"
}
