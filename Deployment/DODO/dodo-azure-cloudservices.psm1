#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureCloudService
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
		[Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureCloudService"
	
	switch ($PsCmdlet.ParameterSetName) 
    { 
        "File"  
        { 
            $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; 
            if($ParametersJSONObject -ne $NULL -and $ParametersJSONObject -ne "")
            {
                $ParametersJSONObject = Get-Content -Raw -Path $ParametersJSONPath | ConvertFrom-Json;
            } 
            break 
        } 
    }  
    $ConfigurationJSONObject = Set-InternalDODOVariables -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
	
    if($ContainerName -ne $NULL -and $ContainerName -ne "")
    {
        $cloudServiceJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureCloudService" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $cloudServiceJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureCloudService" }
    }

    if($cloudServiceJson -eq $NULL)
	{
		throw "AzureCloudService container not found in json" + $ContainerName
	}
	
	foreach($cloudServiceContainer in $cloudServiceJson)
	{
		$subscriptionName = $cloudServiceContainer.Attributes.Properties.Subscription
		$subscriptionId = $cloudServiceContainer.Attributes.Properties.SubscriptionID
		
		$cloudServiceName = $cloudServiceContainer.Attributes.Properties.Name
		$slot  = $cloudServiceContainer.Attributes.Properties.Slot
		$package = $cloudServiceContainer.Attributes.Properties.Package
		$location = $cloudServiceContainer.Attributes.Properties.Location
		$description = $cloudServiceContainer.Attributes.Properties.Description
        $label = $cloudServiceContainer.Attributes.Properties.Label
		
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
		#Create the site and resource group if not exist!
		$cloudService = Get-AzureService -ServiceName $cloudServiceName -ErrorAction SilentlyContinue -ErrorVariable e
        
        if ($e[0] -ne $null)
        {
            if($e[0] -Match "ResourceNotFound")
            {
	            Write-Host "Cloud service $cloudServiceName does not exist, creating..."
	            New-AzureService -Location $location -ServiceName $cloudServiceName -Description $description -Label $label 
	            Write-Host "Cloud service created!"
            }
            else
            {
	            throw "Unable to create cloud service -" + $e
            }
        }
        else
        {
            Write-Host "Cloud service $cloudServiceName exists!"
        }
    }

	Write-Host "Done executing  Publish-DODOAzureCloudService"
}

function Switch-DODOAzureCloudService
{
	[CmdletBinding()]
	 param(
		[Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		[Parameter(Position=1,Mandatory=0)] [int]$Retries,
        [Parameter(Position=2,Mandatory=0)] [string]$ContainerName,
		[Parameter(Position=3,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=3,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
	 )
	 
	switch ($PsCmdlet.ParameterSetName) 
    { 
        "File"  
        { 
            $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; 
            if($ParametersJSONObject -ne $NULL -and $ParametersJSONObject -ne "")
            {
                $ParametersJSONObject = Get-Content -Raw -Path $ParametersJSONPath | ConvertFrom-Json;
            } 
            break 
        } 
    }  
    $ConfigurationJSONObject = Set-InternalDODOVariables -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
	
    if($ContainerName -ne $NULL -and $ContainerName -ne "")
    {
        $cloudServiceJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureCloudService" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $cloudServiceJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureCloudService" }
    }

    if($cloudServiceJson -eq $NULL)
	{
		throw "AzureCloudService container not found in json" + $ContainerName
	}

	
	foreach($cloudServiceContainer in $cloudServiceJson)
	{
<#
		$azureWebAppName = $webappContainer.Attributes.Properties.Name
		$deploymentSlot  = $webappContainer.Attributes.Properties.Slot
		
		$retryCount = 0
		$done = $false

		if($Retries -eq $NULL -or $Retries -eq 0){
			$Retries = 3
			Write-Host "Retry count not specified, defaulting to "$Retries
		}

		while(!$done){
			 
			try{

				if($Retries -eq $retryCount){
					$done = $true
					throw "Max retry count reached"
				}
				
				Write-Host "Swapping the $deploymentSlot slot. Attempt: "$retryCount
				Switch-AzureWebsiteSlot -Name $azureWebAppName -Slot1 'Production'  -Slot2 $deploymentSlot -Force -ErrorAction 'Stop'
				
				$done = $true
			}
			catch{
				Write-Host "Error : "$_ 
				$retryCount = $retryCount + 1
				
				if($Retries -eq $retryCount){
					throw "Max retry count reached"
				}
			}
		}

		Write-Host "Swap complete!" 
#>
	}
	
	Write-Host "Done executing Switch-DODOAzureWebsite"	
}

<#
function Internal-CreateAzureCloudServiceSlot
{
	param(
		[string]$AzureWebAppName,
		[string]$ResourceGroupName,
		[string]$Slot
	)
	
	Write-Host "Checking new $Slot slot"
	
	$deploymentSlot = Get-AzureRmWebAppSlot -Name $AzureWebAppName -Slot $Slot -ResourceGroupName $ResourceGroupName -ErrorVariable e -ErrorAction SilentlyContinue
	if ($e[0] -ne $null)
	{
		if($e[0] -Match "was not found" -or $e[0] -Match "could not be found")
		{
			Write-Host "Deployment slot does not exist, creating..."
			New-AzureRMWebAppSlot -Name $AzureWebAppName -Slot $Slot -ResourceGroupName $ResourceGroupName
			Write-Host "Deployment slot created!"
		}
		else
		{
			throw "Unable to create slot -" + $e
		}
	}
	else
	{
		Write-Host "$Slot slot exists!"
	}
	
}
#>

<#

function Internal-DeployCloudService
{
	[CmdletBinding()]
	 param(
		 [Parameter(Position=0,Mandatory=1)] [string]$Package,
		 [Parameter(Position=1,Mandatory=1)] [string]$Server, #Note: Remember to set WEBSITE_WEBDEPLOY_USE_SCM to false on Azure Management Portal Configuration settings under App Settings
		 [Parameter(Position=2,Mandatory=1)] [string]$IISSite,
		 [Parameter(Position=3,Mandatory=1)] [string]$SetParamsFile,
		 [Parameter(Position=4,Mandatory=1)] [string]$Username,
		 [Parameter(Position=5,Mandatory=1)] [string]$Password,
		 [Parameter(Position=6,Mandatory=0)] [string]$AzurePublishProfile
	 )
	 
	 
	$MSDeployKey = 'HKLM:\SOFTWARE\Microsoft\IIS Extensions\MSDeploy\3'
	if(!(Test-Path $MSDeployKey)) {
		throw "Could not find MSDeploy. Use Web Platform Installer to install the 'Web Deployment Tool' and re-run this command"
	}
	$InstallPath = (Get-ItemProperty $MSDeployKey).InstallPath
	if(!$InstallPath -or !(Test-Path $InstallPath)) {
		throw "Could not find MSDeploy. Use Web Platform Installer to install the 'Web Deployment Tool' and re-run this command"
	}

	$msdeploy = Join-Path $InstallPath "msdeploy.exe"
	if(!(Test-Path $MSDeploy)) {
		throw "Could not find MSDeploy. Use Web Platform Installer to install the 'Web Deployment Tool' and re-run this command"
	}

	#If the publish profile is present, use it as preference
	if($AzurePublishProfile -eq "" -or $AzurePublishProfile -eq $NULL){
	}else{ 
		#Get the correct run location
		Write-Host "Using PublishProfile: $AzurePublishProfile"

		if(!(Test-Path $AzurePublishProfile))
		{
			throw "PublishProfile does not exist"
		}

		[xml]$azureProfile = Get-Content $AzurePublishProfile
		$Server = $azureProfile.publishData.publishProfile.publishUrl[0]
		$Username = $azureProfile.publishData.publishProfile.userName[0]
		$Password = $azureProfile.publishData.publishProfile.userPWD[0]
		$IISSite = $azureProfile.publishData.publishProfile.msdeploySite
	}

	$PublishUrl = "https://$Server/MSDeploy.axd?site=$($IISSite)"
		
	# DEPLOY!
	Write-Host "Deploying package to $PublishUrl for site $IISSite"

	$arguments = [string[]]@(
		"-verb:sync",
		"-source:package='$Package'",
		"-dest:auto,computerName='$PublishUrl',userName='$($UserName)',password='$Password',authtype='Basic'",
		"-setParamFile:`"$SetParamsFile`"",
		"-skip:Directory=\\App_Data\\jobs", #SKIP WebJobs folder
		"-allowUntrusted",
		"-verbose")
		
	Write-Host $arguments
		
	#Start up the msdeploy process and read standard and error log output
	#Solution http://stackoverflow.com/questions/11531068/powershell-capturing-standard-out-and-error-with-process-object?lq=1
	$psi = New-object System.Diagnostics.ProcessStartInfo 
	$psi.CreateNoWindow = $true 
	$psi.UseShellExecute = $false 
	$psi.RedirectStandardOutput = $true 
	$psi.RedirectStandardError = $true 
	$psi.FileName = $msdeploy
	$psi.Arguments = $arguments
	$process = New-Object System.Diagnostics.Process 
	$process.StartInfo = $psi 
	$process.Start() | Out-Null
	$output = $process.StandardOutput.ReadToEnd() 
	$stderr = $process.StandardError.ReadToEnd()
	$process.WaitForExit() 
	$output
	$stderr
	if($process.ExitCode -ne 0){
		throw "MSDeploy threw an error, check the above output log for details"
	}
}

#>

Export-ModuleMember -Function 'Publish-DODOAzureCloudService'
Export-ModuleMember -Function 'Switch-DODOAzureCloudService'
