#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOAzureWebsiteConfiguration
{
	[CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
         [Parameter(Position=1,Mandatory=0)] [string]$DeploymentSlot,
		 [Parameter(Position=2,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=3,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=3,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
     )

	Write-Host "Executing Publish-DODOAzureWebsiteConfiguration"
	
    #region Read Json 
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
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" }
    }

    if($webappJson -eq $NULL)
	{
		throw "AzureWebApp container not found in json" + $ContainerName
	}
    #endregion

	foreach($webAppContainers in $webappJson)
	{
		$AzureWebAppName = $webAppContainers.Attributes.Properties.Name
		$SubscriptionName = $webAppContainers.Attributes.Properties.Subscription
		$SubscriptionId = $webAppContainers.Attributes.Properties.SubscriptionID
		
		#Set-DODOAzureAuthentication -SubscriptionName $SubscriptionName -SubscriptionId $SubscriptionId

		$Appsettings = $webAppContainers.Attributes.AppSettings;
		$AppsettingHash = @{}
		$StickySettings = @()
		
		foreach($Appsetting in $Appsettings)
		{
			if($Appsetting.SlotSetting -eq "true")
			{
				$StickySettings += $Appsetting.Key
			}

			$AppsettingHash.Add($Appsetting.Key, $Appsetting.Value);
		}
        
        Write-Host "Updating $AzureWebAppName AppSettings $AppsettingHash"
        if($DeploymentSlot -eq $NULL -or $DeploymentSlot -eq "")
        {
            Set-AzureWebsite -Name $AzureWebAppName -AppSettings $AppsettingHash -SlotStickyAppSettingNames $StickySettings
        }
        else
        {
            Set-AzureWebsite -Name $AzureWebAppName -AppSettings $AppsettingHash -SlotStickyAppSettingNames $StickySettings -Slot $DeploymentSlot
        }

		Write-Host "AppSettings updated!"
	}
	 
	 
	 Write-Host "Done executing Publish-DODOAzureWebsiteConfiguration"
}

function Publish-DODOAzureWebAppConfiguration
{

	[CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
         [Parameter(Position=1,Mandatory=0)] [string]$DeploymentSlot,
		 [Parameter(Position=2,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=3,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=3,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
     )

	Write-Host "Executing Publish-DODOAzureWebAppConfiguration"


    #region Read Json 
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
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" }
    }

    if($webappJson -eq $NULL)
	{
		throw "AzureWebApp container not found in json " + $ContainerName
	}
    #endregion



	foreach($webAppContainers in $webappJson)
	{
        $subscriptionId = $webAppContainers.Attributes.Properties.SubscriptionID
		$tenantId = $webAppContainers.Attributes.Properties.TenantID
        
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionId -TenantId $tenantId

		$AzureWebAppName = $webAppContainers.Attributes.Properties.Name
		$SubscriptionName = $webAppContainers.Attributes.Properties.Subscription
		$SubscriptionId = $webAppContainers.Attributes.Properties.SubscriptionID
		$resourceGroupName = $webAppContainers.Attributes.ResourceGroup.Name
        $use32bit = $webAppContainers.Attributes.Properties.Use32Bit
        $configuration = $webAppContainers.Attributes.Properties.Configuration
        $failedRequestTracing = $webAppContainers.Attributes.Diagnostics.FailedRequestTracing
        $detailedErrorMessages = $webAppContainers.Attributes.Diagnostics.DetailedErrorMessages
        $applicationLoggingBlob = $webAppContainers.Attributes.Diagnostics.ApplicationLoggingBlob
        $applicationLoggingTable = $webAppContainers.Attributes.Diagnostics.ApplicationLoggingTable
        $diagnosticStorageAccountName = $webAppContainers.Attributes.Diagnostics.ApplicationLoggingBlob.StorageAccount.Name
        $diagnosticStorageContainerName = $webAppContainers.Attributes.Diagnostics.ApplicationLoggingBlob.StorageAccount.ContainerName
        $diagnosticStorageResourceGroupName = $webAppContainers.Attributes.Diagnostics.ApplicationLoggingBlob.StorageAccount.ResourceGroupName 

        $httpLoggingBlob = $webAppContainers.Attributes.Diagnostics.HTTPLoggingBlob
        $httpLoggingStorageAccountName = $webAppContainers.Attributes.Diagnostics.HTTPLoggingBlob.StorageAccount.Name
        $httpLoggingStorageContainerName = $webAppContainers.Attributes.Diagnostics.HTTPLoggingBlob.StorageAccount.ContainerName
        $httpLoggingStorageResourceGroupName = $webAppContainers.Attributes.Diagnostics.HTTPLoggingBlob.StorageAccount.ResourceGroupName 

		$Appsettings = $webAppContainers.Attributes.AppSettings;
        $connectionStrings = $webAppContainers.Attributes.ConnectionStrings;		 
        
        $connectionStringNames = $webAppContainers.Attributes.ConnectionStringNames;
        $appSettingNames = $webAppContainers.Attributes.AppSettingNames;

        $AppsettingHash = @{}
		foreach($Appsetting in $Appsettings)
		{
			$AppsettingHash.Add($Appsetting.Key, $Appsetting.Value);
		}
        

        #App Setting values update
        #
        Write-Host "Updating $AzureWebAppName AppSettings $AppsettingHash"
        
        #resource names to configue
        $loggingResourceName  = "$AzureWebAppName/logs"
        $webConfigurationResourceName = "$AzureWebAppName/web"

        #Resource types for configuration
        $configResourceType = "Microsoft.Web/sites/config"
        
        if($DeploymentSlot -eq $NULL -or $DeploymentSlot -eq "")
        {
           $DeploymentSlot = $webAppContainers.Attributes.Properties.Slot
        }

        if($DeploymentSlot -ne $NULL -and $DeploymentSlot -ne "")
        {
            #if we are deploying to a slot, update the resource type and name
            $configResourceType = "Microsoft.Web/sites/slots/config"
            $loggingResourceName  = "$($AzureWebAppName)/$($DeploymentSlot)/logs"
            $webConfigurationResourceName = "$AzureWebAppName/$($DeploymentSlot)/web"
        }

        Set-AzureRMWebAppSlot -Name $AzureWebAppName -ResourceGroupName $resourceGroupName -AppSettings $AppsettingHash -Slot $DeploymentSlot

		Write-Host "AppSettings updated!"
        
        if($appSettingNames -eq "" -or $appSettingNames -eq $NULL)
        {
            $appSettingNames = @()
        }

        if($connectionStringNames -eq "" -or $connectionStringNames -eq $NULL)
        {
            $connectionStringNames = @()
        }



        #Update slot config names
        Write-Host "Updating slot appsetting names ..."
        $slotConfigObj = @{
	        AppSettingNames = $appSettingNames;
            ConnectionStringNames = $connectionStringNames;
        }
        
        Set-AzureRmResource -PropertyObject $slotConfigObj -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName $("$AzureWebAppName/slotConfigNames") -ApiVersion 2015-08-01 -Force
        Write-Host "Slot config names updated!"
		

		#Connection strings update
        #
        if($connectionStrings -ne "" -and $connectionStrings -ne $NULL)
        {
            Write-Host "Updating $AzureWebAppName ConnectionStrings..."
            New-AzureRmResource -PropertyObject $connectionStrings -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName $("$AzureWebAppName/connectionstrings") -ApiVersion 2015-08-01 -Force
        }

        if($configuration -ne "" -and $configuration -ne $NULL)
        {
            Write-Host "Updating configuration..."
            Set-AzureRmResource -PropertyObject $configuration -ResourceGroupName $resourceGroupName -ResourceType $configResourceType -ResourceName $webConfigurationResourceName -ApiVersion 2015-08-01 -Force
        }

        #Diagnostics update
        #
        Write-Host "Updating Diagnostics..."

        
        #region Failed Request Tracing
        if($failedRequestTracing -eq "" -or $failedRequestTracing -eq $null)
        {
            $failedRequestTracing = $false
        }
        else
        {
            $failedRequestTracing = [System.Convert]::ToBoolean($failedRequestTracing)
        }
        #endregion

        #region Detailed Error Messages
        if($detailedErrorMessages -eq "" -or $detailedErrorMessages -eq $null)
        {
            $detailedErrorMessages = $false
        }
        else
        {
            $detailedErrorMessages = [System.Convert]::ToBoolean($detailedErrorMessages)
        }
        #endregion


        #region Application Logging Blob
        if($applicationLoggingBlob -eq "" -or $applicationLoggingBlob -eq $null)
        {
            $applicationLoggingBlob =  @{ Enabled = $false; Level = "Off";} #default to all off
        }
        else
        {
            $applicationLoggingBlob.Enabled = [System.Convert]::ToBoolean($applicationLoggingBlob.Enabled)
            if(!($applicationLoggingBlob.Enabled)){
                $applicationLoggingBlob =  @{ Enabled = $false; Level = "Off";} #turn it all off
            }
        }
        #endregion

        #region Application Logging Table
        if($applicationLoggingTable -eq "" -or $applicationLoggingTable -eq $null)
        {
            $applicationLoggingTable =  @{ Enabled = $false; Level = "Off"; TableSasUrl = ""} #default to false, off and no table url
        }
        else
        {
            $applicationLoggingTable.Enabled = [System.Convert]::ToBoolean($applicationLoggingTable.Enabled)
            if(!($applicationLoggingTable.Enabled)){
                $applicationLoggingTable =  @{ Enabled = $false; Level = "Off"; TableSasUrl = ""} #turn it off
            }
        }
        #endregion


        #region HTTP Logging Blob
        if($httpLoggingBlob -eq "" -or $httpLoggingBlob -eq $null)
        {
            $httpLoggingBlob =  @{ Mode = "Off";} #default to all off
        }

        #endregion

        
        $logging = (Get-AzureRmResource -ResourceGroupName $resourceGroupName -ResourceType $configResourceType -ResourceName $loggingResourceName -ApiVersion 2015-08-01)

        #FailedRequest Tracing
        #
        Write-Host "Setting FailedRequestsTracing to $failedRequestTracing ..."
        $logging.Properties.FailedRequestsTracing.Enabled = $failedRequestTracing;

        #Detailed Error Messages
        #
        Write-Host "Setting DetailedErrorMessages to $detailedErrorMessages ..."
        $logging.Properties.DetailedErrorMessages.Enabled = $detailedErrorMessages;

        #Application Logging Blob
        #
        Write-Host "Setting ApplicationLoggingBlob Enabled to $($applicationLoggingBlob.Enabled)..."
        $logging.Properties.ApplicationLogs.AzureBlobStorage.Level =  $applicationLoggingBlob.Level
        $logging.Properties.ApplicationLogs.AzureBlobStorage.RetentionInDays = 30

        if($applicationLoggingBlob.Enabled)
        {
            #Set-DODOAzureAuthentication -SubscriptionName  $SubscriptionName -SubscriptionId $SubscriptionId
            #add &comp=list&restype=container to view in browser
            $blobSasUrl = Internal-GetBlobSasUrl $SubscriptionName $SubscriptionId $diagnosticStorageAccountName $diagnosticStorageContainerName $diagnosticStorageResourceGroupName
            Write-Host "Retrieved URI $blobSasUrl"
            $logging.Properties.ApplicationLogs.AzureBlobStorage.SasUrl = $blobSasUrl
        }
        else
        {
            $logging.Properties.ApplicationLogs.AzureBlobStorage.SasUrl =  "" #turn it off
        }

        #HTTP Logging Blob
        #
        Write-Host "Setting HTTPLoggingBlob Mode to $($httpLoggingBlob.Mode)..."

        switch ($httpLoggingBlob.Mode) 
        { 
            "Off"  
            { 
                #turn all off
                $logging.Properties.HttpLogs.FileSystem.Enabled = $false;
                $logging.Properties.HttpLogs.AzureBlobStorage.Enabled = $false;
                break;
            }
            "FileSystem"
            {
                $logging.Properties.HttpLogs.FileSystem.Enabled = $true;
                $logging.Properties.HttpLogs.FileSystem.RetentionInDays = 30;
                $logging.Properties.HttpLogs.AzureBlobStorage.Enabled = $false;
                break;
            } 
            "Storage"
            {
                $logging.Properties.HttpLogs.FileSystem.Enabled = $false;
                $logging.Properties.HttpLogs.AzureBlobStorage.Enabled = $true;
                $logging.Properties.HttpLogs.AzureBlobStorage.RetentionInDays = 30;
				
				
                #Set-DODOAzureAuthentication -SubscriptionName  $SubscriptionName -SubscriptionId $SubscriptionId;
                $blobSasUrl = Internal-GetBlobSasUrl $SubscriptionName $SubscriptionId $httpLoggingStorageAccountName $httpLoggingStorageContainerName $httpLoggingStorageResourceGroupName

                Write-Host "Retrieved URI $blobSasUrl"
                $logging.Properties.HttpLogs.AzureBlobStorage.SasUrl = $blobSasUrl;
                 
                break;
            }
        }  

        #Update
        #
        Write-Host "Updating WebApp logging..."
        Set-AzureRmResource -PropertyObject $($logging.Properties) -ResourceGroupName $resourceGroupName -ResourceType $configResourceType -ResourceName $loggingResourceName -ApiVersion 2015-08-01 -Force
        Write-Host "Logging updated!"

	}
	 
	 
	 Write-Host "Done executing Publish-DODOAzureWebsiteConfiguration"

}

function Publish-DODOAzureWebsite
{
	[CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		 [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODOAzureWebsite"
	
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
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" }
    }

    if($webappJson -eq $NULL)
	{
		throw "AzureWebApp container not found in json" + $ContainerName
	}
	

	foreach($webappContainer in $webappJson)
	{
		$subscriptionName = $webappContainer.Attributes.Properties.Subscription
		$subscriptionId = $webappContainer.Attributes.Properties.SubscriptionID
		$tenantId = $webappContainer.Attributes.Properties.TenantID
		$azureWebAppName = $webappContainer.Attributes.Properties.Name
		$deploymentSlot  = $webappContainer.Attributes.Properties.Slot
		$package = $webappContainer.Attributes.Properties.Package
		$setParamsFile = $webappContainer.Attributes.Properties.ParameterFile
		$resourceGroupName = $webappContainer.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $webappContainer.Attributes.ResourceGroup.Location
        
		$appServicePlan = $webappContainer.Attributes.AppServicePlan.Name
		$appServiceTier = $webappContainer.Attributes.AppServicePlan.Tier
		
		Select-AzureRmSubscription -SubscriptionId $subscriptionId
		#Set-DODOAzureAuthentication -SubscriptionName $subscriptionName -SubscriptionId $subscriptionId
		
		#Create the site and resource group if not exist!
		$app = Get-AzureWebsite -Name $azureWebAppName

		if($app -eq $null){

			Write-Host "WebApp does not exist, setting up resources..."
			Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 
			
			Write-Host "Setting up service plan..."
			Create-DODOAzureAppServicePlan -Name $appServicePlan -Location $resourceGroupLocation -ResourceGroupName $resourceGroupName -Tier $appServiceTier
			
			Write-Host "Creating web app..."
			New-AzureRmWebApp -Location $resourceGroupLocation -Name $azureWebAppName -ResourceGroupName $resourceGroupName -AppServicePlan $appServicePlan
			
			Write-Host "Web app created... preconfiguring web deploy..."
			$appsettingHash = @{}
			$appsettingHash.Add("WEBSITE_WEBDEPLOY_USE_SCM", "false");

			Set-AzureWebsite -Name $azureWebAppName -AppSettings $appsettingHash 
			Write-Host "web app is ready for deployment!"
		}
		
		Internal-CreateAzureWebsiteSlot -AzureWebAppName $azureWebAppName -ResourceGroupName $resourceGroupName -Slot $deploymentSlot
		
		if($package -eq "" -or $package -eq $null -or $setParamsFile -eq "" -or $setParamsFile -eq $null)
		{
			Write-Host "No web app package and parameters provided... publish is complete"
		}
		else
		{
			Write-Host "Web app package and parameters supplied, performing a publish to web app: $azureWebAppName slot: $deploymentSlot ..."
			
			Write-Host "Fetching slot: $deploymentSlot for publish..."
			$site = (Get-AzureWebsite -Name $azureWebAppName -Slot $deploymentSlot)
			Write-Host $site; #log the site properties

			#Get the publish url
			Write-Host "Building publish URL from site object self link : " + $site.SelfLink
			$publishUri = [System.Uri]$site.SelfLink;
			$Server = $publishUri.Host.Replace("api","publish") + ":443"
			Write-Host "Publish URL : $Server"

			$slotSiteName = $azureWebAppName + "__$deploymentSlot" #This is how MSDeploy accepts the staging site name

			Internal-UpdateIISSiteNameInSetParameter -setParameterFilePath $setParamsFile -IISApplicationName $slotSiteName

			Write-Host "Stopping $deploymentSlot site for deployment..."
			Stop-AzureWebsite -Name $AzureWebAppName -Slot $deploymentSlot
			
			Internal-DeployAzureWebsite -Package $Package -Server $Server -IISSite $slotSiteName -SetParamsFile $setParamsFile -Username $site.PublishingUsername -Password $site.PublishingPassword
			
			Write-Host "Starting $deploymentSlot site"
			Start-AzureWebsite -Name $AzureWebAppName -Slot $deploymentSlot
			Write-Host "$deploymentSlot slot started!"
		}
	}
	
	Write-Host "Done executing Publish-DODOAzureWebsite"
}

function Publish-DODOAzureWebApp
{
    [CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		 [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=2,Mandatory=0)] [string]$PackagePath,
         [Parameter(Position=3,Mandatory=0)] [string]$SetParametersPath,
         [Parameter(Position=4,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=4,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )

    Write-Host "Executing Publish-DODOAzureWebApp"

    #region Read Json
    switch ($PsCmdlet.ParameterSetName) 
    { 
        "File"  
        { 
            $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; 
            $ParametersJSONObject = Get-Content -Raw -Path $ParametersJSONPath | ConvertFrom-Json;
            break; 
        } 
    }
      
    $ConfigurationJSONObject = Set-InternalDODOVariables -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
    
    if($ContainerName -ne $NULL -and $ContainerName -ne "")
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" }
    }

    if($webappJson -eq $NULL)
	{
		throw "AzureWebApp container not found in json" + $ContainerName
	}
    #endregion

    foreach($webappContainer in $webappJson)
	{
        $subscriptionName = $webappContainer.Attributes.Properties.Subscription
		$subscriptionId = $webappContainer.Attributes.Properties.SubscriptionID
		$tenantId = $webappContainer.Attributes.Properties.TenantID
        
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionId -TenantId $tenantId
		
        $azureWebAppName = $webappContainer.Attributes.Properties.Name
		$deploymentSlot  = $webappContainer.Attributes.Properties.Slot
       
        if($PackagePath -eq "" -or $PackagePath -eq $NULL)
        {
            Write-Host "Package path not supplied through input, reading json values..."
            $package = $webappContainer.Attributes.Properties.Package
		    $setParamsFile = $webappContainer.Attributes.Properties.ParameterFile
        }
        else
        {
            Write-Host "Package path supplied through CMDLET input!"
            $package = $PackagePath
		    $setParamsFile = $SetParametersPath
        }

        $webAppLocation = $webappContainer.Attributes.Properties.Location
		$resourceGroupName = $webappContainer.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $webappContainer.Attributes.ResourceGroup.Location

		$appServicePlan = $webappContainer.Attributes.AppServicePlan.Name
		$appServiceTier = $webappContainer.Attributes.AppServicePlan.Tier
        $appServicePlanLocation = $webappContainer.Attributes.AppServicePlan.Location

        if($webAppLocation -eq "" -or $webAppLocation -eq $null)
        {
            $webAppLocation = $resourceGroupLocation
        }

        if($appServicePlanLocation -eq "" -or $appServicePlanLocation -eq $null)
        {
            #fall back to resource group location if app service plan location is not supplied!
            $appServicePlanLocation = $resourceGroupLocation
        }

        $app = Find-AzureRmResource -ResourceNameContains $azureWebAppName  -ResourceGroupNameContains $resourceGroupName -ResourceType Microsoft.Web/sites -ApiVersion 2015-11-01
        
        if ($app -eq $NULL -or $app -eq "")
        {
	        Write-Host "Azure Web App $azureWebAppName does not exist, creating..."
            Create-DODOAzureResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation 
			
			Write-Host "Setting up service plan..."
			Create-DODOAzureAppServicePlan -Name $appServicePlan -Location $appServicePlanLocation -ResourceGroupName $resourceGroupName -Tier $appServiceTier
			
			Write-Host "Creating web app..."
			New-AzureRmWebApp -Location $webAppLocation -Name $azureWebAppName -ResourceGroupName $resourceGroupName -AppServicePlan $appServicePlan
			
			Write-Host "Web app created... preconfiguring web deploy..."
			$appsettingHash = @{}
			$appsettingHash.Add("WEBSITE_WEBDEPLOY_USE_SCM", "false");
                
            Set-AzureRMWebApp -Name $azureWebAppName -ResourceGroupName $resourceGroupName -AppSettings $appsettingHash 
            Write-Host "web app is ready for deployment!"
           
        }
        else
        {
            Write-Host "AzureWebApp $azureWebAppName exists!"
        }

        Internal-CreateAzureWebsiteSlot -AzureWebAppName $azureWebAppName -ResourceGroupName $resourceGroupName -Slot $deploymentSlot

        if($package -eq "" -or $package -eq $null -or $setParamsFile -eq "" -or $setParamsFile -eq $null)
		{
			Write-Host "No web app package and parameters provided... publish is complete"
		}
        else
        {
            Write-Host "Web app package and parameters supplied, performing a publish to web app: $azureWebAppName slot: $deploymentSlot ..."
			
			Write-Host "Fetching slot: $deploymentSlot for publish..."
            $resourceName = "$($azureWebAppName)/$($deploymentSlot)"
			$site = Get-AzureRmResource -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/slots -ResourceName $resourceName -ApiVersion 2015-08-01

			Write-Host $site; #log the site properties

			#Get the publish url
			Write-Host "Getting publish URL from Site Properties EnabledHostNames where SCM..." 
            $publishUri = ""
            if($site.Properties.EnabledHostNames -ne "" -and $site.Properties.EnabledHostNames -ne $NULL)
            {
                foreach($hostname in $site.Properties.EnabledHostNames)
                {
                    Write-Host "Checking hostname... $hostname "
                    if($hostname.Contains(".scm."))
                    {
                        $publishUri = $hostname
                    }
                }

                if($publishUri -eq "")
                {
                    throw "Cannot obtain site properties from enabled hostnames as it does not exist, please check RM explorer for SCM publish url"
                }
            }
            else
            {
                throw "Cannot obtain site properties from enabled hostnames, please check RM explorer for SCM publish url"
            }

			Write-Host "Publish URL : $publishUri"

            $publishCredentials = Invoke-AzureRmResourceAction -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/slots/config -ResourceName "$resourceName/publishingcredentials" -Action list -ApiVersion 2015-08-01 -Force
            
			$slotSiteName = $azureWebAppName + "__$deploymentSlot" #This is how MSDeploy accepts the staging site name

			Internal-UpdateIISSiteNameInSetParameter -setParameterFilePath $setParamsFile -IISApplicationName $slotSiteName

			Write-Host "Stopping $deploymentSlot site for deployment..."
            Invoke-AzureRmResourceAction -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/slots -ResourceName $("$AzureWebAppName/$deploymentSlot") -Action stop -ApiVersion 2015-08-01 -Force
           
            #Deploy package to slot!
			Internal-DeployAzureWebsite -Package $Package -Server $publishUri -IISSite $slotSiteName -SetParamsFile $setParamsFile -Username $publishCredentials.Properties.PublishingUsername -Password $publishCredentials.Properties.PublishingPassword
			
			Write-Host "Starting $deploymentSlot site"
            Invoke-AzureRmResourceAction -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/slots -ResourceName $("$AzureWebAppName/$deploymentSlot") -Action start -ApiVersion 2015-08-01 -Force		
           
			Write-Host "$deploymentSlot slot started!"

        }

    }

}

function Switch-DODOAzureWebApp
{
    [CmdletBinding()]
     param(
         [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=1,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		 [Parameter(Position=2,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )

    Write-Host "Executing Switch-DODOAzureWebApp"

    #region Read Container
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
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" }
    }

    if($webappJson -eq $NULL)
	{
		throw "AzureWebApp container not found in json" + $ContainerName
	}
    #endregion

    foreach($webappContainer in $webappJson)
	{
        $resourceGroupName = $webappContainer.Attributes.ResourceGroup.Name
        $azureWebAppName = $webappContainer.Attributes.Properties.Name
	    $deploymentSlot  = $webappContainer.Attributes.Properties.Slot
        $subscriptionId = $webappContainer.Attributes.Properties.SubscriptionID
        $tenantId = $webappContainer.Attributes.Properties.TenantID
        
		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionId -TenantId $tenantId

        # Action slotsswap
        $ParametersObject = @{
            targetSlot = "production"
            preserveVnet = "true"
        }

        Write-Host "Performing swap on web app : $azureWebAppName - Slot : $deploymentSlot ..."
        $resourceName = "$($azureWebAppName)/$($deploymentSlot)"
        Invoke-AzureRmResourceAction -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/slots -ResourceName $resourceName -Action slotsswap -Parameters $ParametersObject -ApiVersion 2015-08-01 -Force
        Write-Host "Swap complete"
        
    }
}

function Invoke-DODOAzureWebsiteWarmup
{
	[CmdletBinding()]
	 param(
		 [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
         [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
		 [Parameter(Position=1,Mandatory=0)] [string]$timeoutInsec,
		 [Parameter(Position=2,Mandatory=0)] [string]$ContainerName,
         [Parameter(Position=3,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
         [Parameter(Position=3,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
	 )
	
	Write-Host "Executing Invoke-DODOAzureWebsiteWarmup"
	
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
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" }
    }

    if($webappJson -eq $NULL)
	{
		throw "AzureWebApp container not found in json" + $ContainerName
	}
	
	foreach($webappContainer in $webappJson)
	{
		$siteName = $webappContainer.Attributes.Properties.Name
		
		Write-Host "Performing site warm up..."	
		if($timeoutInsec -eq "" -or $timeoutInsec -eq $NULL){ 
				#default to 5min
				$timeoutInsec = 300
				Write-Host "No timeout specified, default to $timeoutInsec seconds"
		}

		$guid = [guid]::NewGuid()
		$url = "http://" + $siteName + "-staging.azurewebsites.net?_=" + $guid

		Write-Host "Loading up Internet Explorer process..."
		$oIE= new-object -com internetexplorer.application
		$oIE.visible=$true;
		 
		# wait till browser is loaded
		while ($oIE.busy) {
			Write-Host "Awaiting IE load..."
			sleep -milliseconds 50
		}

		Write-Host "Internet Explorer process loaded. Navigating to "$url
		$oIE.navigate2($url);
		 
		# wait till url is loaded
		$sleepTotal = 0
		Write-Host "Waiting for URL to load..."
		while ($oIE.busy) {

			if($sleepTotal -eq $timeoutInsec){
				$oIE.Quit();
				throw "Timeout reached! Site did not load in the given timeout of " + $timeoutInsec + " seconds. Closing IE process"
			}

			$sleepTotal = $sleepTotal + 1
			sleep -Seconds 1   
		}

		Write-Host "Site took $sleepTotal seconds to become ready"
		$doc = $oIE.Document;
		Write-Host "Site state: "$doc.readyState;
		 
		Write-Host "Closing Internet Explorer process..."
		$oIE.Quit();
	}
	
	Write-Host "Done executing Invoke-DODOAzureWebsiteWarmup"	
}

function Switch-DODOAzureWebsite
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
	 
	Write-Host "Executing Switch-DODOAzureWebsite"
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
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" -and $_.Name -eq $ContainerName }
    }
    else
    {
        $webappJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureWebApp" }
    }

    if($webappJson -eq $NULL)
	{
		throw "AzureWebApp container not found in json" + $ContainerName
	}
	
	foreach($webappContainer in $webappJson)
	{        
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
	}
	
	Write-Host "Done executing Switch-DODOAzureWebsite"	
}

function Internal-UpdateIISSiteNameInSetParameter($setParameterFilePath, $IISApplicationName)
{
	Write-Host "Updating IIS Web Application Name in SetParameter to correct site and slot: $IISApplicationName"
	(get-content $setParameterFilePath) | foreach-object {$_ -replace "{{IISWebApplicationName}}", "$IISApplicationName"} | set-content $setParameterFilePath
	Write-Host "Updated IIS Web Application Name in SetParameter to correct deployment site and slot: $IISApplicationName"
}

function Internal-CreateAzureWebsiteSlot
{
	param(
		[string]$AzureWebAppName,
		[string]$ResourceGroupName,
		[string]$Slot
	)
	
	Write-Host "$($MyInvocation.MyCommand) : Checking new $Slot slot"
	
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

function Internal-DeployAzureWebsite
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

function Internal-GetBlobSasUrl($subscriptionName, $subscriptionId, $storageAccountName, $blobContainerName, $resourceGroupName )
{
    Write-Host "$($MyInvocation.MyCommand) : Setting up blob sas connection..."
    Write-Host "subscription: $subscriptionName"
    Write-Host "subscription id: $subscriptionId"
    Write-Host "storage AccountName: $storageAccountName"
    Write-Host "blob ContainerName: $blobContainerName"
    Write-Host "resource group: $resourceGroupName"
	
    Write-Debug "DebugPreference : $DebugPreference $(Get-CurrentFileName) $(Get-CurrentLineNumber) " #debug

    $accountKeys = (Get-AzureRmStorageAccountKey -Name $storageAccountName -ResourceGroupName $resourceGroupName)
	#By some reason response is different (why ??)
	# on local machine -just 2 properties Key1 and Key2
	#  on teamcity dictionary 
	#"keys": [{"keyName": "key1",       "value": "xxx"    },
    #         {"keyName": "key2",       "value": "yyy"   }
    $accountKey =$accountKeys[0].Value
    $accountKey =Coalesce $accountKey  $accountKeys.Key1 
	
	Write-Debug "Debug only accountKeys : $accountKeys "
	Write-Debug "debug accountKeys.Key1: $accountKeys.Key1 accountKeys[0].Value: $accountKeys[0].Value" #
	Write-Debug "debug accountKey: $accountKey" #debug

    Write-Host "Setting up blob sas connection - creating storage context..."
    $storageContext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $accountKey

    Write-Host "Setting up blob sas connection - setting up container..."
    $container = Get-AzureStorageContainer -Context $storageContext | Where-Object { $_.Name -like $blobContainerName }

    if($container)
    {
        Write-Host "Blob container exists"
    }
    else
    {
        Write-Host "Blob container not found, creating..."
         $container = New-AzureStorageContainer -Context $storageContext -Name $blobContainerName
      	Write-Debug "container $container  $(Get-CurrentFileName) $(Get-CurrentLineNumber) "     
		Write-Host "Blob container created $container.Name "
    }

    $cbc = $container.CloudBlobContainer

    Write-Host "Setting up blob sas connection - setting up SAS permissions..."
    
    $policy = new-object 'Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy'
    $policy.SharedAccessStartTime = $(Get-Date).ToUniversalTime().AddMinutes(-5)
    $policy.SharedAccessExpiryTime = $(Get-Date).ToUniversalTime().AddYears(10)
    $policy.Permissions = "Read,Write,List,Delete"

    $token = $cbc.GetSharedAccessSignature($policy)
    $url = [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0}{1}", $cbc.Uri, $token)

    #hack to remove HTTP encoding from dates :-/
    $qs = [System.Web.HttpUtility]::ParseQueryString($url)
    $newUrl = $url.Substring(0,$url.IndexOf("st="))
    $newUrl = $newUrl + "st=" + $qs["st"] + "&se=" + $qs["se"]
    $newUrl = $newUrl + "&sp=rwdl"

    Write-Host "Generated URL : $newUrl"
    return $newUrl;

}
function DODOAzureWebApp_GetVersion
{
	return "3.2.2.1"  #to verify is correct version is loaded, not necessary to update each time
}

Export-ModuleMember -Function 'Publish-DODOAzureWebsite'
Export-ModuleMember -Function 'Publish-DODOAzureWebsiteConfiguration'
Export-ModuleMember -Function 'Switch-DODOAzureWebsite'

Export-ModuleMember -Function 'Publish-DODOAzureWebApp'
Export-ModuleMember -Function 'Switch-DODOAzureWebApp'
Export-ModuleMember -Function 'Publish-DODOAzureWebAppConfiguration'

Export-ModuleMember -Function 'Invoke-DODOAzureWebsiteWarmup'

Export-ModuleMember -Function 'DODOAzureWebApp_GetVersion' 


