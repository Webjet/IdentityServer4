#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx
#https://azure.microsoft.com/en-us/documentation/articles/container-service-mesos-marathon-rest/

function Publish-DODODCOSMesosService
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
	Write-Host "Executing Publish-DODODCOSMesosService"
	
    #region Read JSON 
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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "DCOSMesosService" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "DCOSMesosService" }
    }

    if($json -eq $NULL)
    {
	    throw "DCOSMesosService container not found in json" + $ContainerName
    }
	#endregion

    foreach($container in $json)
    {
        #Properties
        $marathonTemplate = $container.Attributes.MarathonTemplate
        $marathonAPI = $container.Attributes.Properties.MarathonAPI
       
        if ($marathonAPI -eq $null -or $marathonAPI -eq "")
        {
                throw "Please supply a marathon API endpoint in your json template. See documentation for samples"
        }

        if ($marathonTemplate -eq $null -or $marathonTemplate -eq "")
        {
	         throw "Marathon template not specified in JSON, please check documentation and provide correctly formated DCOS marathon template"
        }
        
        Write-Host "Sending marathon template to DCOS API $($marathonAPI)..."
        $postBody = $marathonTemplate | ConvertTo-Json -Depth 999
        Invoke-WebRequest -Method Post -Uri "$($marathonAPI)/apps" -ContentType application/json -Body $postBody
        
    }

	Write-Host "Done executing  Publish-DODODCOSMesosService"
}

Export-ModuleMember -Function 'Publish-DODODCOSMesosService'