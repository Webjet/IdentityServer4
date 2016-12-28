
 function Publish-DODOAzureStorageAccount
 {
	[CmdletBinding()]
     param(
		[Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )

	Write-Host "Executing Publish-DODOAzureStorageAccount" 
	
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
        $storageJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureStorageAccount" -and $_.Name -eq $ContainerName }
    }
    else
    {
	    $storageJson = $ConfigurationJSONObject.Containers | where { $_.Type -eq "AzureStorageAccount" }
    }

    if($storageJson -eq $NULL)
    {
	    throw "AzureStorageAccount container not found in json" + $ContainerName
    }

	
	foreach($storageContainer in $storageJson)
	{
		$subscriptionName = $storageContainer.Attributes.Properties.Subscription
		$subscriptionId = $storageContainer.Attributes.Properties.SubscriptionID
        $tenantID = $storageContainer.Attributes.Properties.TenantID
		$resourceGroupName = $storageContainer.Attributes.ResourceGroup.Name
		$resourceGroupLocation = $storageContainer.Attributes.ResourceGroup.Location
		$storageAccountName = $storageContainer.Attributes.Properties.Name
        $storageAccountLocation = $storageContainer.Attributes.Properties.Location
		$storageType = $storageContainer.Attributes.Properties.Type
		$isClassic = [System.Convert]::ToBoolean($storageContainer.Attributes.Properties.IsClassic)

		Set-DODOAzureRmAuthentication -ConfigurationJSONObject $ConfigurationJSONObject -SubscriptionId $subscriptionID -TenantId $tenantID
		
		
		if($isClassic)
		{
			Write-Host "Running classic commands for classic storage"
            #Set-DODOAzureAuthentication -SubscriptionName $subscriptionName -SubscriptionId $subscriptionId
			Internal-CreateStorageAccountClassic -Name $storageAccountName -Location $resourceGroupLocation
		}
		else
		{
			Write-Host "Running RM commands for non classic storage"
			Internal-CreateStorageAccountRm -Name $storageAccountName -ResourceGroupName $resourceGroupName -ResourceGroupLocation $resourceGroupLocation -Type $storageType -Location $storageAccountLocation
		}
		
		$blobcontainerJson = $storageContainer.Attributes.Containers
		
		if($blobcontainerJson -ne "" -and $blobcontainerJson -ne $null)
		{
            Write-Host "Connecting to blob service..."
			
			$accountKey = $NULL
			
			if($isClassic)
			{
				$accountKey = (Get-AzureStorageKey -StorageAccountName "devopssamplelogs").Primary
			}
			else
			{
				$accountKey = (Get-AzureRmStorageAccountKey -Name $storageAccountName -ResourceGroupName $resourceGroupName)[0].Value
			}
			
            $context = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $accountKey
            Write-Host "Connected to blob service!"
           
			foreach($blobContainer in $blobcontainerJson)
			{
                Write-Host "Checking $($blobContainer.Name) blob container..." 

                $container = Get-AzureStorageContainer -Context $context | Where-Object { $_.Name -like $blobContainer.Name }

                if($container)
                {
                    Write-Host "Blob container exists"
                }
                else
                {
                    $permission = "Off"

                    if($blobContainer.Permission -ne "" -and $blobContainer.Permission -ne $NULL)
                    {
                        $permission = $blobContainer.Permission
                    }

                    Write-Host "Blob container not found, creating..."
                    New-AzureStorageContainer -Name $blobContainer.Name -Context $context -Permission $permission -ErrorAction Stop
                    Write-Host "Blob container created!"
                }

                Write-Host "Uploading data to $($blobContainer.Name) blob container..."

                foreach($blobItem in $blobContainer.Blobs)
                {
                    if($blobItem -ne "" -and $blobItem -ne $null)
                    {
                        
                       if($($blobItem.Source).Contains("`$PSScriptRoot"))
                       {
                            $Invocation = (Get-Variable MyInvocation -Scope 2).Value;
                            $InvocationPath =  Split-Path $Invocation.MyCommand.Path
                            $blobItem.Source = $($blobItem.Source).Replace("`$PSScriptRoot", $InvocationPath )
                       }

                       Internal-UploadFilesBlob -StorageContainer $blobContainer.Name -Source $blobItem.Source -Target $blobItem.Target -Context $context
                    }
                }
			}
            
             
            
		}
        
        $tableStorageJson = $storageContainer.Attributes.Tables
        
        if($tableStorageJson -ne "" -and $tableStorageJson -ne $null)
        {
            Write-Host "Connecting to table service..."
            
            $accountKey = $NULL
			
			if($isClassic)
			{
				$accountKey = (Get-AzureStorageKey -StorageAccountName $storageAccountName).Primary
			}
			else
			{
				$accountKey = (Get-AzureRmStorageAccountKey -Name $storageAccountName -ResourceGroupName $resourceGroupName)[0].Value
			}
			
            $context = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $accountKey
            
            Write-Host "Connected to table service!"
            
            foreach($table in $tableStorageJson)
            {
                Write-Host "Checking $($table.Name) table.." 

                $tableItem = Get-AzureStorageTable -Context $context | Where-Object { $_.Name -like $table.Name }

                if($tableItem)
                {
                    Write-Host "Table exists"
                }
                else
                {
                    Write-Host "Table not found, creating..."
                    New-AzureStorageTable -Name $table.Name -Context $context -ErrorAction Stop
                    Write-Host "Table created!"
                }
            }
        }
	}
	
	Write-Host "Done executing Publish-DODOAzureStorageAccount" 
	
 }
 
 function Internal-UploadFilesBlob
{
    param(
        # The name of the storage container to copy files to.
        [Parameter(Mandatory = $true)]
        [string]$StorageContainer,

        [Parameter(Mandatory = $true)]
        [string]$Source,
       
        # The target folder of the blob where to copy the files
        [Parameter(Mandatory = $false)]
        [string]$Target,
        
        [Parameter(Mandatory = $true)]
        $Context
    )
    
    $isDirectory = (Get-Item $Source) -is [System.IO.DirectoryInfo]

    if($isDirectory)
    {
        $files = ls -Path $source -File -Recurse

        foreach($file in $files)
        {
            $blobFileName = $file.FullName.Replace($Source, "")
			
			if($Target -ne $NULL -and $Target -ne "")
			{
				$blobFileName = Join-Path -Path $Target -ChildPath $blobFileName
			}
			
			if($blobFileName.StartsWith("\")){ $blobFileName = $blobFileName.Substring(1)}
			
            Write-Host "Uploading $file Target : $blobFileName"

            Set-AzureStorageBlobContent -Container $StorageContainer `
                        -File $file.FullName -Blob $blobFileName `
                        -Context $Context `
                        -ConcurrentTaskCount 5 `
                        -Force
        }
    }
    else
    {
        #single file!

        $blobFileName = Split-Path -Path $Source -Leaf

        if($Target -ne $null -and $Target -ne "")
        {
            $blobFileName = Join-Path -Path $Target -ChildPath $blobFileName
        }
		
		if($blobFileName.StartsWith("\")){ $blobFileName = $blobFileName.Substring(1)}
		
        Write-Host "Uploading $Source Target : $blobFileName"

        Set-AzureStorageBlobContent -Container $StorageContainer `
                    -File $Source -Blob $blobFileName `
                    -Context $Context `
                    -ConcurrentTaskCount 5 `
                    -Force
        
    }
}


 function Internal-CreateStorageAccountRm
 {
	param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
       
      
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupLocation,

		[Parameter(Mandatory = $true)]
        [string]$Type,

        [Parameter(Mandatory = $false)]
        [string]$Location
    )
	
	Write-Host "Checking storage account $Name under resource group $ResourceGroupName ..."
	$storage = Find-AzureRmResource -ResourceNameContains $Name -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Storage/storageAccounts -ApiVersion 2015-11-01	

	if ($storage -eq "" -or $storage -eq $null)
    {
		Write-Host "Storage account not found, setting up resources..."
		Create-DODOAzureResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation 
			
		Write-Host "Creating storage account..."

        if($Location -eq "" -or $Location -eq $null)
        {
            $Location = $ResourceGroupLocation
        }

		New-AzureRmStorageAccount -Location $Location -Name $Name -ResourceGroupName $ResourceGroupName -Type $Type
		Write-Host "Storage account created!"
	}
    else
	{
		Write-Host "Storage account already exists!"
	}
 }
 
 function Internal-CreateStorageAccountClassic
 {
	param(
        # The name of the storage container to copy files to.
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Location
    )
	
	$storage = Get-AzureStorageAccount -StorageAccountName $Name -ErrorAction SilentlyContinue -ErrorVariable e

	if($storage -ne $null)
	{
		Write-Host "Storage account already exists!"
	}
	else 
	{
		if ($e[0] -ne $null)
		{
			Write-Host "Storage account not found, creating..."
			New-AzureStorageAccount -StorageAccountName $Name -Location $Location
			Write-Host "Storage account created!"
		}
		else
		{
			throw "Unable to find storage account -" + $e
		}
	}
 }
 
 function New-DODOAzureStorageSASUrl
 {
     [CmdletBinding()]
     param(
       
        [Parameter(Position=0,Mandatory=1)] [string]$StorageAccountName,
        [Parameter(Position=1,Mandatory=1)] [string]$BlobContainerName,
        [Parameter(Position=2,Mandatory=1)] [string]$ResourceGroupName
    )
    
    Write-Host "Executing New-DODOAzureStorageSASUrl..."  
    Write-Host "Setting up blob sas connection..."
  
    Write-Host "storage acc: $StorageAccountName"
    Write-Host "container: $BlobContainerName"
    Write-Host "resource group: $ResourceGroupName"

    $accountKeys = (Get-AzureRmStorageAccountKey -Name $StorageAccountName -ResourceGroupName $ResourceGroupName)
    $accountKey = $accountKeys[0].Value
    Write-Host "Setting up blob sas connection - creating storage context..."
    $storageContext = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $accountKey

    Write-Host "Setting up blob sas connection - setting up container..."
    $container = Get-AzureStorageContainer -Context $storageContext | Where-Object { $_.Name -like $BlobContainerName }

    if($container)
    {
        Write-Host "Blob container exists"
    }
    else
    {
        Write-Host "Blob container not found, creating..."
            $container = New-AzureStorageContainer -Context $storageContext -Name $BlobContainerName
        Write-Host "Blob container created!"
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
    Add-Type -AssemblyName "System.Web"
    $qs = [System.Web.HttpUtility]::ParseQueryString($url)
    $newUrl = $url.Substring(0,$url.IndexOf("st="))
    $newUrl = $newUrl + "st=" + $qs["st"] + "&se=" + $qs["se"]
    $newUrl = $newUrl + "&sp=rwdl"

    Write-Host "Generated URL : $newUrl"
    return $newUrl;
 }
 
Export-ModuleMember -Function 'Publish-DODOAzureStorageAccount'
Export-ModuleMember -Function 'New-DODOAzureStorageSASUrl'

