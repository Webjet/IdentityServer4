
<#
******************D.O.D.O****************
*****Dev.Ops.Deployment.Orchestrator*****
*****************************************
#>

function Run-DODO
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
		[Parameter(Position=2,Mandatory=0)] [string]$Command,
		[Parameter(Position=3,Mandatory=0)] [string]$Arguments,
		[Parameter(Position=4,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=4,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject

    )
	Write-Host "     ______ __"
	Write-Host "   {-_-_= '. `'."
	Write-Host "    {=_=_-  \   \"
	Write-Host "     {_-_   |   /"
	Write-Host "      '-.   |  /    .===,"
	Write-Host "   .--.__\  |_(_,==`  ( o)'-."
	Write-Host "  `---.=_ `     ;      `/    \"
	Write-Host "      `,-_       ;    .'--') /"
	Write-Host "        {=_       ;=~  `  `""
	Write-Host "         `//__,-=~"
	Write-Host "         <<__ \\__"
	Write-Host "         /`)))/`)))"
	
	Write-Host "Executing Run-DODO $($PsCmdlet.ParameterSetName)"
	 
    switch ($PsCmdlet.ParameterSetName) 
	{ 	
		"File"  
		{ 
			$ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json;
			if($ParametersJSONPath -ne "" -and $ParametersJSONPath -ne $NULL)
			{
				$ParametersJSONObject = Get-Content -Raw -Path $ParametersJSONPath | ConvertFrom-Json;
			}
			break
		} 
	}  
	
    $ConfigurationJSONObject = Set-InternalDODOVariables -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
    
	#if a command is passed in, a container must be passed in
	if(($Command -ne $NULL -and $Command -ne "") -and ($ContainerName -eq $NULL -or $ContainerName -eq "") ){ throw "Command requires ContainerName parameter to run explicit commands on containers. Please pass in a ContainerName"}

    if($ContainerName -ne $NULL -and $ContainerName -ne "")
    {
		Write-Host "Running dodo on container $($ContainerName)"
        $json = $ConfigurationJSONObject.Containers | where { $_.Name -eq $ContainerName  }

		#command can only be run on a single container
		if($Command -ne $NULL -and $Command -ne "")
		{
			Write-Host "Processing command $($Command) on container $($ContainerName)"

			foreach($container in $json)
			{	
				switch($Command)
				{
					"Invoke-DODOAzureWebsiteWarmup"
					{
						Invoke-DODOAzureWebsiteWarmup -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $ContainerName -ParametersJSONObject $ParametersJSONObject
						break;
					}
					"Switch-DODOAzureWebApp"
					{
						Switch-DODOAzureWebApp -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $ContainerName -ParametersJSONObject $ParametersJSONObject
						break;
					}
					"Publish-DODOAzureWebApp"
					{
						if($Arguments -eq "" -or $Arguments -eq $null) { throw "The given command $($Command) requires arguments to run. See the WIKI for examples"}
						$args = $Arguments | ConvertFrom-Json
						Write-Host "Arguments.PackagePath - $($args.PackagePath)"
						Write-Host "Arguments.SetParametersPath - $($args.SetParametersPath)"

						$params = @{
							ConfigurationJSONObject = $ConfigurationJSONObject
							ContainerName = $ContainerName
							PackagePath = $args.PackagePath
							SetParametersPath = $args.SetParametersPath
							ParametersJSONObject = $ParametersJSONObject
						}
						 
						Publish-DODOAzureWebApp @params
						 
						break;
					}
				}
			}
			
			Write-Host "Command processed, exiting call"
			return;
		}
    }
    else
    {
		Write-Host "Running dodo on all containers in JSON"
	    $json = $ConfigurationJSONObject.Containers
    }

    if($json -eq $NULL)
    {
	    throw "Unable to find JSON to execute"
    }
	
    foreach($container in $json)
    {	
		Write-Host "Processing container Name: $($container.Name) Type: $($container.Type)..."
		switch ($container.Type) 
		{
			"AzureActiveDirectory"
			{
				Publish-DODOAzureActiveDirectory -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureAppServiceEnvironment"
			{
				Publish-DODOAzureAppServiceEnvironment -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureAutomationAccount"
			{
				Publish-DODOAzureAutomationAccount -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			} 
			"AzureAvailabilitySet"
			{
				Publish-DODOAzureAvailabilitySet -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureClassicReservedIP"
			{
				Publish-DODOAzureReservedIp -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureCloudService"
			{
				Publish-DODOAzureCloudService -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureContainerService"
			{
				Publish-DODOAzureContainerService -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureKeyVault"
			{
				Publish-DODOAzureKeyVault -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureLoadBalancer"
			{
				Publish-DODOAzureLoadBalancer -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"DCOSMesosService"
			{
				Publish-DODODCOSMesosService -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureApiManagement"
			{
				Publish-DODOAzureApiManagement -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzurePublicIp"
			{
				Publish-DODOAzurePublicIp -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureRedisCache"
			{
				Publish-DODOAzureRedis -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureSQLServer"
			{
				Publish-DODOAzureSQLServer -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureStorageAccount"
			{
				Publish-DODOAzureStorageAccount -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureTrafficManager"
			{
				Publish-DODOAzureTrafficManager -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureVirtualNetwork"
			{
				Publish-DODOAzureVNet -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureVirtualNetworkGateway"
			{
				Publish-DODOAzureVirtualNetworkGateway -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureVirtualNetworkGatewayConnection"
			{
				Publish-DODOAzureVirtualNetworkGatewayConnection -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureVM"
			{
				Publish-DODOAzureVM -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureVmDSC"
			{
				Publish-DODOAzureVMDSC -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureVmScaleSet"
			{
				Publish-DODOAzureVMScaleSet -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureWebApp"
			{
				Publish-DODOAzureWebApp -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureWebjob"
			{
				Publish-DODOAzureWebjob -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"IIS-ARR"
			{
				Publish-DODOIISARR -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"IISWebApplication"
			{
				Publish-DODOIISWebApplication -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"SlackMessage"
			{
				Publish-DODOSlackMessage -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
			"AzureSearch"
			{
				Publish-DODOAzureSearch -ConfigurationJSONObject $ConfigurationJSONObject -ContainerName $($container.Name) -ParametersJSONObject $ParametersJSONObject
				break;
			}
		}
	}
}

<# Variable Injection Module #>
function Set-InternalDODOVariables 
{
	[CmdletBinding()]
		param(
		[Parameter(Position=0,Mandatory=1)] [PSCustomObject]$ConfigurationJSONObject,
		[Parameter(Position=1,Mandatory=0)] [PSCustomObject]$ParametersJSONObject
	)
	
	Write-Host "Executing Internal-DODOSetVariables"
	
	if($ConfigurationJSONObject -eq "" -or $ConfigurationJSONObject -eq $NULL)
	{
		throw "Configuration is blank and not specified!"
	}
	elseif($ConfigurationJSONObject.Containers -eq "" -or $ConfigurationJSONObject.Containers -eq $NULL)
	{
		throw "Configuration container is blank and not specified!"
	}
	elseif($ConfigurationJSONObject.Variables -eq "" -or $ConfigurationJSONObject.Variables -eq $NULL)
	{
		Write-Host "No Variables specified, moving on!"
	}
	else 
	{
		$ConfigurationJSONObject = Set-InternalDODOParameters -ConfigurationJSONObject $ConfigurationJSONObject -ParametersJSONObject $ParametersJSONObject
		
		$jsonString = ($ConfigurationJSONObject | ConvertTo-Json -Depth 9999999).Replace("\u0027","'")
		
		$variables = Get-Member -InputObject $($ConfigurationJSONObject.Variables) -MemberType NoteProperty
		foreach($variable in $variables)
		{
			$propValue= $($ConfigurationJSONObject.Variables) | Select-Object -ExpandProperty $variable.Name
			$propValue = $propValue.Replace("\","\\")
			Write-Host "Injecting -Variable $($variable.Name)..."
			
			$jsonString = $jsonString.Replace("[variables('$($variable.Name)')]", $propValue)
		}
		
		$ConfigurationJSONObject = $jsonString | ConvertFrom-Json
	}
	
	Write-Host "Done executing  Internal-DODOSetVariables"
	return $ConfigurationJSONObject
}

function Set-InternalDODOParameters 
{
	[CmdletBinding()]
		param(
		[Parameter(Position=0,Mandatory=1)] [PSCustomObject]$ConfigurationJSONObject,
		[Parameter(Position=1,Mandatory=0)] [PSCustomObject]$ParametersJSONObject
	)

	Write-Host "Executing Set-InternalDODOParameters "

	if($ParametersJSONObject -eq "" -or $ParametersJSONObject -eq $NULL)
	{
		Write-Host "No Parameters specified, moving on!"
	}
	else 
	{
		$variables = Get-Member -InputObject $($ConfigurationJSONObject.Variables) -MemberType NoteProperty
		$parameters = Get-Member -InputObject $($ParametersJSONObject.Parameters) -MemberType NoteProperty

		foreach($parameter in $parameters)
		{
			$paramValue= $($ParametersJSONObject.Parameters) | Select-Object -ExpandProperty $parameter.Name
			Write-Host "Searching -Parameter: $($parameter.Name)"

			$found = $false
			foreach($variable in $variables)
			{
				if($variable.Name -eq $parameter.Name)
				{
					$found = $true
					Write-Host "Updating variable with parameter: $($parameter.Name)"
					Add-Member -InputObject $ConfigurationJSONObject.Variables -MemberType "NoteProperty" -Name $parameter.Name -Value $paramValue -Force
				}
			}

			if(!$found)
			{
				Write-Host "Adding variable with parameter: $($parameter.Name)"
				Add-Member -InputObject $ConfigurationJSONObject.Variables -MemberType "NoteProperty" -Name $parameter.Name -Value $paramValue -Force
			}
		}
	}

	return $ConfigurationJSONObject  
}

Export-ModuleMember -Function "Run-DODO"
Export-ModuleMember -Function "Set-InternalDODOVariables"