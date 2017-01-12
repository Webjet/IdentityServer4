The PowerShell Community Extensions (https://www.powershellgallery.com/packages/Pscx/3.2.2) from The PowerShell Gallery(https://msdn.microsoft.com/powershell/gallery/readme)
suppose to be installed using
Install-Module -Name Pscx -Scope CurrentUser

Installing items from the Gallery requires the latest version of the PowerShellGet module, which is available in PowerShell 5 in Windows 10, in Windows Management Framework (WMF) 5.0, or in the MSI-based installer (for PowerShell 3 and 4).
	* Get Windows 10,
	* Get WMF 5.0, or
	* Get MSI Installer

However  we have had complications with running PowerShell 5 on windows server 2012 R2 in the past. 
So Pscx was imported manually on local macune and included as subfolder.
TODO when TeamCity will have  PowerShell 5+ supported, local modules subfolder(s) will not be required


