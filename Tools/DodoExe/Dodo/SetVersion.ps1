

#Get the module path
$path = (Get-Item $PSScriptRoot).Parent.Parent.Parent
#Import the module manifest to get version
Import-LocalizedData -BaseDirectory $path.FullName -FileName dodo.psd1 -BindingVariable moduleManifest
$moduleVersion = $moduleManifest.ModuleVersion;

#Write the version in the AssemblyInfo.cs
if(Test-Path "$PSScriptRoot\Properties\AssemblyInfo.cs" )
{
	#$content = [System.IO.File]::ReadAllText("$PSScriptRoot\Properties\AssemblyInfo.cs")
    $NewVersion = 'AssemblyVersion("' + $moduleVersion + '")';
    $NewFileVersion = 'AssemblyFileVersion("' + $moduleVersion + '")';

    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'

    (Get-Content "$PSScriptRoot\Properties\AssemblyInfo.cs") | ForEach-Object  { 
           % {$_ -replace $assemblyVersionPattern, $NewVersion } |
           % {$_ -replace $fileVersionPattern, $NewFileVersion }
        } | Out-File "$PSScriptRoot\Properties\AssemblyInfo.cs" -encoding UTF8 -force
}