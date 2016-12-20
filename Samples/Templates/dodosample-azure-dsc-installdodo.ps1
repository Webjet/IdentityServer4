Configuration Installs 
{
	Import-DscResource -ModuleName 'PSDesiredStateConfiguration'
    Node "localhost"
    { 
		File Directory
		{
			Ensure = "Present"
			Type = "Directory"
			DestinationPath = "C:\DevOps\DSC"
		}
		
        Script DownloadInstalls 
        { 
			SetScript = { 
				#Invoke-Command -ScriptBlock { . "C:\temp\SampleExternal.ps1" -TestString "test" }
				Invoke-WebRequest -uri "https://devopsteststore.blob.core.windows.net/installs/dodo.exe/dodo.exe" -outfile "C:\DevOps\DSC\dodo.exe"
				#Unblock-File -Path "C:\DevOps\DSC\DSCPackage.zip"
			}
			TestScript = { 
				return $false 
			}
			GetScript = { 
			}
			
			DependsOn = "[File]Directory"
        }
		<#
		Archive ExtractInstalls
		{
			Ensure = "Present"
			Path = "C:\DevOps\DSC\DSCPackage.zip"
			Destination = "C:\DevOps\DSC\"
			Force = $true
			DependsOn = "[Script]DownloadInstalls"
		}
		
		Script InstallDODO 
        { 
			SetScript = { 
				Invoke-Command -ScriptBlock { . "C:\DevOps\DSC\DSCPackage\dodo\install.ps1" }
			}
			TestScript = { 
				return $false 
			}
			GetScript = { 
			}
			
			DependsOn = "[Archive]ExtractInstalls"
        }#>
    } 
}