#Approved Verbs for Windows PowerShell Commands
#https://msdn.microsoft.com/en-us/library/windows/desktop/ms714428(v=vs.85).aspx

function Publish-DODOSlackMessage
{
	[CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject,
        [Parameter(Position=1,Mandatory=0)] [string]$ContainerName,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='File')] [string]$ParametersJSONPath,
        [Parameter(Position=2,Mandatory=0,ParameterSetName='Content')] [PSCustomObject]$ParametersJSONObject
    )
	
    Write-Host "Executing Publish-DODOSlackMessage"

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
        $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "SlackMessage" -and $_.Name -eq $ContainerName  }
    }
    else
    {
	    $json = $ConfigurationJSONObject.Containers | where { $_.Type -eq "SlackMessage" }
    }

    if($json -eq $NULL)
    {
	    throw "SlackMessage container not found in json" + $ContainerName
    }
    #endregion
	
    foreach($container in $json)
    {
        $payload = @{
            channel = $container.Attributes.Properties.Channel;
            username = $container.Attributes.Properties.Username;
            icon_url = $container.Attributes.Properties.IconUrl;
            attachments = @(
                @{
                    fallback = $container.Attributes.Properties.Fallback;
                    color = $container.Attributes.Properties.Color;
                    fields = @(
                        @{
                        title = $container.Attributes.Properties.Title;
                        value = $container.Attributes.Properties.Message;
                        });
                };
            );
        }

        $hook = $container.Attributes.SlackApi
        Invoke-Restmethod -Method POST -Body ($payload | ConvertTo-Json -Depth 4) -Uri $hook
    }

    Write-Host "Done executing  Publish-DODOSlackMessage"
}

Export-ModuleMember -Function 'Publish-DODOSlackMessage'


