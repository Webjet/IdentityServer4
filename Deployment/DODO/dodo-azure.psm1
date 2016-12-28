#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Login-DODOAzureClassic
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1)] [string]$SubscriptionId,
        [Parameter(Position=1,Mandatory=1)] [string]$SubscriptionName,
        [Parameter(Position=2,Mandatory=1,ParameterSetName='CetrificateFilePath')] [string]$CertificateFilePath,
        [Parameter(Position=2,Mandatory=1,ParameterSetName='CetrificateCertStorePath')] [string]$CertificateStorePath,
        [Parameter(Position=3,Mandatory=1,ParameterSetName='CetrificateCertStorePath')] [string]$CnName,
        [Parameter(Position=2,Mandatory=1,ParameterSetName='CertificateObj')] [System.Security.Cryptography.X509CertificateS.X509Certificate2]$CertificateObj,
        [Parameter(Position=2,Mandatory=1,ParameterSetName='CertificateStore')] [switch]$CertificateStore,
        [Parameter(Position=3,Mandatory=0,ParameterSetName='CertificateStore')] [string]$CertificateVersion
    )
	
	Write-Host "Login-DODOAzureClassic starting..."
    
    $certificate = $null;

    switch ($PsCmdlet.ParameterSetName) 
    {
        "CetrificateFilePath"
        {
            Write-Host "Classic Login - CetrificateFilePath"
            
		    Write-Host "Certificate file path supplied :" $CertificateFilePath
		    $certFromFile = Get-Item $CertificateFilePath
            $certificate = New-Object -TypeName System.Security.Cryptography.X509CertificateS.X509Certificate2 -ArgumentList $certFromFile
            break
        } 
        "CetrificateCertStorePath"  
        {
            Write-Host "Classic Login - CetrificateCertStorePath" 

            Write-Host "Certificate store path : $CertificateStorePath and CN : $CnName" 
            $certFromStore = Get-ChildItem -Path $CertificateStorePath | ? { $_.subject -eq "CN=$CnName" }
            if($certFromStore -eq $NULL)
            {
                throw "Certificate not found by give path and CN name"
            }

            $certificate = New-Object -TypeName System.Security.Cryptography.X509CertificateS.X509Certificate2 -ArgumentList $certFromStore
            break
        } 
        "CertificateObj"  
        { 
            Write-Host "Classic Login - CertificateObj"
            $certificate = $CertificateObj
            break
        }
        "CertificateStore"
        {
            Write-Host "Classic Login - CertificateStore"

            Write-Host "Performing certificate store search..."
            $subCN = $SubscriptionName.Replace(" ","")
            $certCN = "CN=DevOpsAzureDeployment-$($subCN)"     
        
            if($CertificateVersion -ne $NULL -and $CertificateVersion -ne "")
            {
                #append a version if exists!
                $certCN = $certCN + "-" + $CertificateVersion
            }
        
            Write-Host "Using CN : $($certCN) ..."    

            Write-Host "searching stores... LocalMachine\TrustedPublisher"
            $certObj = $null;
            
            $certObj = Get-ChildItem -Path cert:\LocalMachine\TrustedPublisher\ | ? { $_.subject -eq $certCN }
            if($certObj -eq $NULL)
            {
	            Write-Host "Certificate not found in LocalMachine TrustedPublisher checking... CurrentUser\My"
	            $certObj = Get-ChildItem -Path cert:\CurrentUser\My\ | ? { $_.subject -eq $certCN }
		
	            if($certObj -eq $NULL)
	            {
		            Write-Host "Certificate not found in Personal Store for Current User... checking LocalMachine\My"
		            $certObj = Get-ChildItem -Path cert:\LocalMachine\My\ | ? { $_.subject -eq $certCN }

		            if($certObj -eq $NULL)
		            {
			            #still not found :(
			            throw "Certificate path not supplied and not found and searched stores, please supply certificate file or upload to CurrentUser\My or LocalMachine\My cert stores"
		            }
		            else
		            {
			            Write-Host "Certificate found in LocalMachine\My"
		            }
	            }
	            else
	            {
		            Write-Host "Certificate found in CurrentUser\My"
	            }
            }
            else
            {
                Write-Host "Certificate found in LocalMachine\TrustedPublisher"
            }
		    
            Write-Host "Certificate found in certificate store"
            $certificate = New-Object -TypeName System.Security.Cryptography.X509CertificateS.X509Certificate2 -ArgumentList $certObj
        }
    }

    if($certificate -eq $NULL -and $certificate -eq "")
    {
        throw "There was a problem loading up the specified Azure certificate"
    }

    Write-Host 'Authenticating...Clearing Azure profile...'
    Get-AzureSubscription | ForEach-Object { Remove-AzureSubscription -SubscriptionName $_.SubscriptionName -WarningAction SilentlyContinue -Force } # -Force }
    Write-Host 'Authenticating...Azure profile cleared!'

	Write-Host 'Authenticating...Loading Azure Management Certificate'
	write $certificate
	 
	Write-Host "Setting Subscription to: "$SubscriptionName
	Write-Host "Subsription ID : "$SubscriptionId
 
	Write-Host "Setting Subscription..."
	Set-AzureSubscription -SubscriptionName $SubscriptionName -SubscriptionId $SubscriptionId -Certificate $certificate
	Select-AzureSubscription -SubscriptionName $SubscriptionName 
	Write-Host "Azure Authentication Complete!"

}

function Set-DODOAzureAuthentication
{
    [CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1)] [string]$SubscriptionName,
		 [Parameter(Position=1,Mandatory=1)] [string]$SubscriptionId,
		 [Parameter(Position=2,Mandatory=0)] [string]$certPath,
         [Parameter(Position=3,Mandatory=0)] [string]$CertVersion

     )
	 
	$AzureCert = $NULL
    #remove spaces from subscriptionName
    #$SubscriptionName = $SubscriptionName.Replace(" ","")

	if($certPath -eq $NULL -or $certPath -eq "")
	{
        $subCN = $SubscriptionName.Replace(" ","")
        $certCN = "CN=DevOpsAzureDeployment-$($subCN)"     
        
        if($CertVersion -ne $NULL -and $CertVersion -ne "")
        {
            #append a version if exists!
            $certCN = $certCN + "-" + $CertVersion
        }
        
        Write-Host "Using CN : $($certCN) ..."    

        Write-Host "Certificate path not supplied... searching stores... LocalMachine\TrustedPublisher"
		$certObj = Get-ChildItem -Path cert:\LocalMachine\TrustedPublisher\ | ? { $_.subject -eq $certCN }
        if($certObj -eq $NULL)
		{
		    Write-Host "Certificate path not supplied... searching stores... CurrentUser\My"
		    $certObj = Get-ChildItem -Path cert:\CurrentUser\My\ | ? { $_.subject -eq $certCN }
		
		    if($certObj -eq $NULL)
		    {
			    Write-Host "Certificate not found in Personal Store for Current User... checking LocalMachine\My"
			    $certObj = Get-ChildItem -Path cert:\LocalMachine\My\ | ? { $_.subject -eq $certCN }

			    if($certObj -eq $NULL)
			    {
				    #still not found :(
				    throw "Certificate path not supplied and not found and searched stores, please supply certificate file or upload to CurrentUser\My or LocalMachine\My cert stores"
			    }
			    else
			    {
				    Write-Host "Certificate found in LocalMachine\My"
			    }
		    }
		    else
		    {
			    Write-Host "Certificate found in CurrentUser\My"
		    }
        }
		
		Write-Host "Certificate found in certificate store"
		$AzureCert = $certObj
	}
	else
	{
		#Certificate file path supplied!
		Write-Host "Certificate file path supplied :" $certPath
		$AzureCert = Get-Item $certPath
	}
    Write-Host "AzureCert..."
    $AzureCert

    Write-Host 'Authenticating...Clearing Azure profile...'
    Get-AzureSubscription | ForEach-Object { Remove-AzureSubscription -SubscriptionName $_.SubscriptionName -WarningAction SilentlyContinue -Force } # -Force }
    Write-Host 'Authenticating...Azure profile cleared!'

	Write-Host 'Authenticating...Loading Azure Management Certificate'
	write $AzureCert
	$cert = New-Object -TypeName System.Security.Cryptography.X509CertificateS.X509Certificate2 -ArgumentList $AzureCert

	Write-Host "Setting Subscription to: "$SubscriptionName
	Write-Host "Subsription ID : "$SubscriptionId
 
	Write-Host "Setting Subscription..."
	Set-AzureSubscription -SubscriptionName $SubscriptionName -SubscriptionId $SubscriptionId -Certificate $cert
	Select-AzureSubscription -SubscriptionName $SubscriptionName 
	Write-Host "Azure Authentication Complete!"
}

function Set-DODOAzureRmAuthentication
{
	[CmdletBinding(DefaultParameterSetName="NoJSON")]
     param(
        [Parameter(Position=0,Mandatory=0,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$SubscriptionID,
        [Parameter(Position=2,Mandatory=0)] [string]$TenantID,
        [Parameter(Position=3,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Set-DODOAzureRmAuthentication"

    $isAuthenticated = Test-IsAzureAuthenticated
    Write-Host "Is Authenticated : $($isAuthenticated)"
    if(!$isAuthenticated)
    {
        switch ($PsCmdlet.ParameterSetName) { "File"  { $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; break} }  
        if($ConfigurationJSONObject -ne "" -and $ConfigurationJSONObject -ne $null){
            $ConfigurationJSONObject = Set-InternalDODOVariables -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
        }
        
        $container = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureCredentials" }
        
        $U = ""
        $P = ""

        if($container -eq "" -or $container -eq $null)
        {
            Write-Warning "Set-DODOAzureRmAuthentication - AzureCredentials container not supplied in JSON, you will be prompted for login"
        }
        else 
        {
            #Use JSON if the SubscriptionID and TenantID not specified
            if($SubscriptionID -eq "" -or $SubscriptionID -eq $null)
            {
                $SubscriptionID = $container.Attributes.SubscriptionID
            }
            if($TenantID -eq "" -or $TenantID -eq $null)
            {
                $TenantID = $container.Attributes.TenantID
            }

            Write-Host "AzureCredentials container supplied... processing auth type: $($container.Attributes.AuthType)"
            
            switch($container.Attributes.AuthType)
            {
                "OrgUserCredentials"
                {
                   
                    if($container.Attributes.Password -ne $null -and $container.Attributes.Password -ne "")
                    {
                        Write-Host "Setting credentials..."
                        $P = ConvertTo-SecureString $($container.Attributes.Password ) -AsPlainText -Force
                        $U = $container.Attributes.Username
                        Write-Host "Credentials set"
                    }
                    break;
                }
                "OrgUserPSCredential"
                {
                    Write-Warning "OrgUserPSCredential not supported yet"
                    break;
                }
                "OrgUserPSCredentialPath"
                {
                    Write-Host "Locating credential..."
                    if(!(Test-Path $container.Attributes.Password))
                    {
                        throw "Credentials do not exist at : $($container.Attributes.Password)"
                    }

                    if(!(Test-Path $container.Attributes.Username))
                    {
                        throw "Username do not exist at : $($container.Attributes.Username)"
                    }

                    $P = Get-Content $container.Attributes.Password  | ConvertTo-SecureString
                    $U = Get-Content $container.Attributes.Username
                    Write-Host "Credentials set"
                    break;
                }
                 "Prompt"
                {
                    Write-Host "You will be prompted for login..."
                    break;
                }
                "ServicePrincipal"
                {
                    Write-Warning "ServicePrincipal not supported yet"
                    break;
                }
            }    
        }

        Write-Host "Subsription ID : "$SubscriptionID
        Write-Host "Tenant ID : "$TenantID

        if(($U -eq "" -or $U -eq $NULL -or $P -eq "" -or $P -eq $NULL) -and $container.Attributes.AuthType -ne "Prompt" ) 
        { 
            Write-Warning "Credentials could not be translated from auth type, check your JSON. You will be prompted for login"
            Login-AzureRmAccount
        }
        elseif ($container.Attributes.AuthType -eq "Prompt")
        {
            Login-AzureRmAccount
        }
        else 
        {
            $credentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $U,$P
            
            Write-Host 'Authenticating with RM...'
            Login-AzureRmAccount -Credential $credentials -SubscriptionId $SubscriptionID -TenantId $TenantID -ErrorAction SilentlyContinue -ErrorVariable e

             #Temporary workaround for interactive login into RM
            if ($e[0] -ne $null)
            {
                Write-Host "Login-AzureRmAccount did not work - Running interactive login..."
                Write-Host "Login-AzureRmAccount details: " + $e
                Login-AzureRmAccount
                Write-Host "Interactive login complete!"
            }
            else
            {
                Write-Host "Login success!"
            }   
        }

        Select-AzureRmSubscription -SubscriptionId $SubscriptionID
    }
    
    Write-Host "Ensure subscription is selected: $($SubscriptionID)..."
    Select-AzureRmSubscription -SubscriptionId $SubscriptionID -ErrorAction "Stop"
	
    Write-Host "Azure Authentication Complete!"
	Write-Host "Done executing Set-DODOAzureRmAuthentication"
}

function Create-DODOAzureSecureCredential
{
	[CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1)] [string]$OutputPath
     )
	 
	Write-Host "Setting up credential paths"
	if(!(Test-Path $OutputPath))
	{
		  md $OutputPath
	}
	 
	Read-Host -Prompt "Please enter your Azure Username:" | Out-File "$OutputPath\AzureSecureCredentialUsername.txt"
	 
	Read-Host -Prompt "Please enter your Azure password:" -AsSecureString | ConvertFrom-SecureString | Out-File "$OutputPath\AzureSecureCredential.txt"
	
	Write-Host "Azure credentials created!"
}

function Create-DODOAzureResourceGroup
{
	[CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1)] [string]$Name,
		 [Parameter(Position=1,Mandatory=1)] [string]$Location
     )
	
	Write-Host "Executing Create-DODOAzureResourceGroup"
	Write-Host "Checking resource group: $Name  at location: $Location"
	$found = $false
    $rgs = Find-AzureRmResourceGroup
    foreach ($rg in $rgs)
    {
        if($rg.Name -eq $Name)
        {
            $found = $true
        }
    }
    
	if (!$found)
	{
        Write-Host "Creating a new resource group named $($Name)..."
        New-AzureRmResourceGroup -Name $Name -Location $Location
        Write-Host "Resource group created!"
    }
    else
    {
        Write-Host "Resource group already exists"
    }
	
	Write-Host "Done executing Create-DODOAzureResourceGroup"
}

function Create-DODOAzureAppServicePlan
{
	[CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1)] [string]$Name,
		 [Parameter(Position=1,Mandatory=1)] [string]$Location,
		 [Parameter(Position=2,Mandatory=1)] [string]$ResourceGroupName,
		 [Parameter(Position=3,Mandatory=1)] [string]$Tier
		 
     )
	
	Write-Host "Executing Create-DODOAzureAppServicePlan"
	Write-Host "Checking appservice plan: $Name  at location: $Location on resourcegroup: $ResourceGroupName"
	$appServicePlan = Get-AzureRMAppServicePlan -Name $Name -ResourceGroupName $ResourceGroupName -ErrorVariable a -ErrorAction SilentlyContinue

	if ($a[0] -ne $null)
	{
		if($a[0] -Match "could not be found" -or $a[0] -Match "not found")
		{
			Write-Host "AppService plan not found... Creating a new app service plan..."
			New-AzureRMAppServicePlan -Name $Name -Location $Location -ResourceGroupName $ResourceGroupName -Tier $Tier
			Write-Host "AppService plan created!"
		}
		else
		{
			throw "Unable to create AppService plan -" + $a
		}
	}
    else
    {
        Write-Host "AppService plan already exists"
    }
	
	Write-Host "Done executing Create-DODOAzureAppServicePlan"
}

<#
This function makes an Azure call to see if the session is authenticated
Azure does not provide a mechanism to check this so we do it be calling azure with Get-AzureRmResourceGroup
and exception handling.
#>
function Test-IsAzureAuthenticated
{
    #Make an Azure Call to check authentication
    try {
        $test = Get-AzureRmResourceGroup -Name "test" -Location "SouthEast Asia" -ErrorVariable a -ErrorAction SilentlyContinue
    }
    catch [System.Exception] {
        
    }

    if ($a -ne $NULL -and $a[0] -ne $null)
    {
        Write-Host $a[0]
        if($a[0] -Match "Login-AzureRmAccount")
        {
            Write-Host "You are not logged in"
            return $false
        }
        else
        {
            Write-Host "You are already logged in"
            return $true
        }
    }
    else
    {
        Write-Host "You are already logged in"
        return $true
    }
    
    return $false
}

Export-ModuleMember -Function 'Login-DODOAzureClassic'
Export-ModuleMember -Function 'Set-DODOAzureAuthentication'
Export-ModuleMember -Function 'Set-DODOAzureRmAuthentication'
Export-ModuleMember -Function 'Create-DODOAzureSecureCredential'
Export-ModuleMember -Function 'Create-DODOAzureResourceGroup'
Export-ModuleMember -Function 'Create-DODOAzureAppServicePlan'
