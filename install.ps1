
param (
	[switch]$InstallDependencies
)

function Install-DODOAzureDependancies 
{
    #################SETTINGS##############################
    $azureMSI = "azure-powershell.1.4.0.msi"
    $downloadUrl = "https://github.com/Azure/azure-powershell/releases/download/v1.4.0-May2016/azure-powershell.1.4.0.msi"
    $version =  @{ "Major" = 1; "Minor" = 4; "Build" = 0}
    #######################################################
    
    Write-Host "`r`n Checking Windows Azure PowerShell Installed version..." -ForegroundColor 'Yellow';

    $found = $false;
    $modules = (Get-Module -ListAvailable | Where-Object{ $_.Name -eq 'Azure' })
    foreach($module in $modules)
    {
        #Require 1.3.0 or newer!
        if($module.Version.Major -ge $version.Major)
        {
            if($module.Version.Minor -ge $version.Minor)
            {
                if($module.Version.Build -ge $version.Build)
                {
                    $found = $true;
                }
            }
        }
    }

    if($found){ 
        Write-Host "Azure PowerShell $($version.Major).$($version.Minor).$($version.Build) installed!" -ForegroundColor 'Green';
    }
	else{ 

		if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")){    
			throw "Installing Azure Modules automatically requires the executing account to have local administrative priveleges"
		}

		Write-Host "Azure PowerShell $($version.Major).$($version.Minor).$($version.Build) is not installed. Downloading..." -ForegroundColor 'Yellow';
		
        if(!(Test-Path "$PSScriptRoot\$azureMSI"))
        {

            Write-Host "Downloading Azure PowerShell..."
            Invoke-WebRequest -uri $downloadUrl -outfile $azureMSI
        
            #test the download was successful
            if(!(Test-Path "$PSScriptRoot\$azureMSI") )
            {
                throw "Please check that the $azureMSI file had downloaded successfully"
            }
        
            Write-Host "Download complete."
        }
        else
        {
            Write-Host "$azureMSI is already downloaded"
        }

        Write-Host "Installing $azureMSI..."
        $process = Start-Process -FilePath msiexec -ArgumentList /i, $azureMSI, /quiet -Wait -PassThru

        if ($process.ExitCode -ne 0)
        {
            Write-Host "Installer returned exit code"$($process.ExitCode)
            throw "Error occurred. Installation aborted"
        }
		
		Write-Host "loading WindowsAzurePowershell module..."
		Import-Module "C:\Program Files (x86)\Microsoft SDKs\Azure\PowerShell\ServiceManagement\Azure\Azure.psd1"
		Write-Host "module loaded! You may need to restart the shell executable in order to run the Azure modules"
	}
}
 
if($InstallDependencies.IsPresent)
{
	Install-DODOAzureDependancies	
} 

$modulePathRoot = "C:\Program Files\WindowsPowerShell\Modules\dodo"

Write-Host "Installing Dev.Ops.Deployment.Orchestrator D.O.D.O"

Write-Host "Importing PSD1 schema..."
Import-LocalizedData -BaseDirectory $PSScriptRoot -FileName dodo.psd1 -BindingVariable moduleManifest

$moduleVersion = $moduleManifest.ModuleVersion;
Write-Host "Importing PSD1 schema done. ModuleVersion = $moduleVersion"
$moduleVersionPath = "$modulePathRoot\$moduleVersion"

if(!(Test-Path "$moduleVersionPath"))
{
	Write-Host "Module path does not exist, creating : $moduleVersionPath\dodo"
	md "$moduleVersionPath\dodo"
}

Write-Host "Cleanup module folder $moduleVersionPath ..."
Remove-Item -Recurse "$moduleVersionPath\dodo\**" | Where { ! $_.PSIsContainer }
Write-Host "Deploying module files to $moduleVersionPath ..."
Copy-Item $PSScriptRoot\*.psm1 "$moduleVersionPath\dodo" -Force
Copy-Item $PSScriptRoot\*.psd1 "$moduleVersionPath\dodo" -Force


Write-Host "Adding module version to PSModulePath env variable"
$p = [Environment]::GetEnvironmentVariable("PSModulePath",  [EnvironmentVariableTarget]::Machine).ToString()

if ($p.Contains($moduleVersionPath))
{
    Write-Host "Module path exists in the PSModulePath variable"
}
else
{
    $p += ";$moduleVersionPath\"
    [Environment]::SetEnvironmentVariable("PSModulePath",$p,  [EnvironmentVariableTarget]::Machine)
    Write-Host "PSModulePath updated!"
}

Write-Host "Installation complete"