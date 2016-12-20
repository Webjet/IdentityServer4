
function Internal-LoadWebAdministration
{
    #Write-Host "Loading Microsoft.Web.Administration assembly..."
    
    $webAdminAssemlyPath = "C:\windows\system32\inetsrv\Microsoft.Web.Administration.dll"
    if(!(Test-Path $webAdminAssemlyPath))
    {
        throw "Web administration assembly does not exist at : $webAdminAssemlyPath"
    }

    [System.Reflection.Assembly]::LoadFrom($webAdminAssemlyPath) | Out-Null

    #Write-Host "Microsoft.Web.Administration assembly loaded"
}

function Internal-SetServersState
{
    [CmdletBinding()]
    param(
    [Parameter(Position=0,Mandatory=1)] [Microsoft.Web.Administration.ConfigurationElementCollection]$ServerCollection,
    [Parameter(Position=1,Mandatory=1)] [PSCustomObject]$ServersJson
    )
    
    foreach($serverJson in $ServersJson)
    {
        foreach($server in $ServerCollection)
        {
            $address = $server.GetAttributeValue("address")
            if($address -eq $serverJson.Address)
            {
                Write-Host "Found server address $address in farm"
                Internal-SetServerState -Server $server -ServerJson $serverJson
            }
        }
    }
}

function Internal-SetServerState
{
    [CmdletBinding()]
    param(
    [Parameter(Position=0,Mandatory=1)] [Microsoft.Web.Administration.ConfigurationElement]$Server,
    [Parameter(Position=1,Mandatory=1)] [PSCustomObject]$ServerJson
    )
    
    $servername = $Server.GetAttributeValue("address")
    

    $arr = $Server.GetChildElement("applicationRequestRouting")
    $serverCounters = $arr.GetChildElement("counters")
    $currentState = $serverCounters.GetAttributeValue("state")
    
    Write-Host "Server to update : $servername State : $currentState"
                    
    $method = $arr.Methods["SetState"]
    $methodInstance = $method.CreateInstance()

    $methodValue = -1
    $methodLabel = ""
    switch($ServerJson.State.ToLower())
    {
        "start" 
        { 
            $methodValue = 0
            $methodLabel = "start"
        }
        "drain" 
        { 
            $methodValue = 1
            $methodLabel = "drain"
        }
        "gracefulstop" 
        { 
            $methodValue = 2
            $methodLabel = "gracefulstop"
        }
        "forcefulstop" 
        { 
            $methodValue = 3
            $methodLabel = "forcefulstop"
        }
        
        default { throw "Server state needs to be specified correctly!"}
    }

    if($methodValue -eq -1)
    {
        throw "Server state needs to be specified correctly!"
    }
    else
    {
        $satisfy = 0
        
        if($ServerJson.StateConditions -ne "" -and $ServerJson.StateConditions -ne $null)
        {
            $conditionMet = $false
            
            $retryCount = 0
            $retryCounter = 0
            $retrySleepInSec = 60

            if($ServerJson.StateConditionRetry -ne "" -and $ServerJson.StateConditionRetry -ne $null)
            {
                $retrySleepInSec = $ServerJson.StateConditionRetry.SleepTimeBetweenRetriesInSec
            }

            do
            {
                Write-Host "Checking conditions..."
                
                $satisfy = 0
                $conditionCount = 0
                
                foreach($condition in $ServerJson.StateConditions)
                {
                    $conditionCount = $conditionCount + 1
                    $counterValue = $serverCounters.GetAttributeValue($condition.Counter)

                    if($counterValue -eq $condition.Value)
                    {
                       $satisfy = $satisfy + 1
                    }
                }

                #ensure all conditions = satisfied
                if($conditionCount -ne $satisfy)
                {
                    $satisfy = 0 #force not satisfied!
                }
                else
                {
                    $conditionMet = $true
                }

                if(-not $conditionMet)
                {
                   Write-Host "Condition not met, sleeping $retrySleepInSec seconds..."
                   Start-Sleep $retrySleepInSec
                }

            }

            while (-not $conditionMet)
            
        }
        else
        {
            $satisfy = 1
        }

        if($satisfy -gt 0)
        {
            Write-Host "Setting server state to $methodLabel ..."

            $methodInstance.Input.Attributes[0].Value = $methodValue
            $methodInstance.Execute()

            Write-Host "Server state updated!"
        }
        else
        {
            Write-Host "No conditions met, not setting state"
        }

       
    }
  
    <# Get Methods you can call
    foreach($method in $arr.Methods)
    {
        $method
    }
    #>

    <# See counters of server
        $serverCounters = $arr.GetChildElement("counters")
        $serverCounters.Attributes | Format-List
    #>
}

<#
C:\Windows\System32\inetsrv\config\schema\arr_schema
Send an Array of ARR server farms where you would like to set a state on servers
*************************************************************************************
Sample JSON:
[
  {
    "Name": "SecurePay.Gateway",
    "Servers": [
      {
        "Address": "192.168.43.5",
        "State": "Drain"
      }
    ]
  },
  {
    "Name": "SecurePay.Tokeniser",
    "Servers": [
      {
        "Address": "192.168.43.5",
        "State": "Drain"
      }
    ]
  }
]
#>
function Set-IISARRServerFarmsState
{

    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject
    )

    switch ($PsCmdlet.ParameterSetName) { "File"  { $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; break} } 
    $ErrorActionPreference = "Stop"

    Internal-LoadWebAdministration

    $mgr = new-object Microsoft.Web.Administration.ServerManager
    $conf = $mgr.GetApplicationHostConfiguration()
    
    $section = $conf.GetSection("webFarms")
    $webFarms = $section.GetCollection()

    $serverFarmsJson = $ConfigurationJSONObject

    foreach($serverFarmJson in $serverFarmsJson)
    {
        #Write-Host "Loading ARR farm and servers to configure on farm $($serverFarmJson.Name)..."

        foreach($farm in $webFarms)
        {
            $farmname = $farm.GetAttributeValue("name")
            if($farmname -eq $serverFarmJson.Name)
            {
                Write-Host "Found web farm $farmName"
                $servers = $farm.GetCollection()
                $serversJson = $serverFarmJson.Servers

                Internal-SetServersState -ServerCollection $servers -ServersJson $serversJson
            }
        }
    }
}

<#
C:\Windows\System32\inetsrv\config\schema\arr_schema
Send an Array of ARR servers where you would like to set a state for
*************************************************************************************
Sample JSON:
[
  {
    "Name": "SecurePay.Gateway",
    "Servers": [
      {
        "Address": "192.168.43.5",
        "State": "Drain"
      }
    ]
  },
  {
    "Name": "SecurePay.Tokeniser",
    "Servers": [
      {
        "Address": "192.168.43.5",
        "State": "Drain"
      }
    ]
  }
]
#>
function Set-IISARRServersState
{
    [CmdletBinding()]
     param(
        [Parameter(Position=0,Mandatory=1,ParameterSetName='File')] [string]$ConfigurationJSONPath,
        [Parameter(Position=0,Mandatory=1,ParameterSetName='Content')] [PSCustomObject]$ConfigurationJSONObject
    )

    switch ($PsCmdlet.ParameterSetName) { "File"  { $ConfigurationJSONObject = Get-Content -Raw -Path $ConfigurationJSONPath | ConvertFrom-Json; break} } 

    $ErrorActionPreference = "Stop"

    Internal-LoadWebAdministration

    #Write-Host "Loading management objects..."
    $mgr = new-object Microsoft.Web.Administration.ServerManager
    $conf = $mgr.GetApplicationHostConfiguration()

    #Write-Host "Loading web farm..."
    $section = $conf.GetSection("webFarms")
    $webFarms = $section.GetCollection()

    $serversJson = $ConfigurationJSONObject

    foreach($serverJson in $serversJson)
    {
        #Write-Host "Loading ARR servers to configure for server address $($serverJson.Address)..."

        foreach($farm in $webFarms)
        {
            $servers = $farm.GetCollection()
            $farmName = $farm.GetAttributeValue("name")
            foreach($server in $servers)
            {
                $address = $server.GetAttributeValue("address")
                if($address -eq $serverJson.Address)
                {
                    Write-Host "Found server $address in farm $farmName"
                    Internal-SetServerState -Server $server -ServerJson $serverJson
                }
            }
        }
    }
}

###########################################################################################
$drainjson = @"
[
    {
        "Address": "192.168.43.5",
        "State": "Drain"
    }
]
"@ | ConvertFrom-Json 

$graceFulStopJson = @"
[
    {
        "Address": "192.168.43.5",
        "State": "GracefulStop",
        "StateConditions" : 
        [
            {
                "Counter" : "currentRequests",
                "Value" : 0
            },
            {
                "Counter" : "state",
                "Value" : 1
            }
        ],
        "StateConditionRetry" : 
        {
            "SleepTimeBetweenRetriesInSec" : 10
        }
    }
]
"@ | ConvertFrom-Json


$stopJson = @"
[
    {
        "Address": "192.168.43.5",
        "State": "ForcefulStop",
        "StateConditions" : 
        [
            {
                "Counter" : "currentRequests",
                "Value" : 0
            },
            {
                "Counter" : "state",
                "Value" : 2
            }
        ],
        "StateConditionRetry" : 
        {
            "SleepTimeBetweenRetriesInSec" : 10
        }
    }
]
"@ | ConvertFrom-Json

cls

Write-Host "Draining..."

Set-IISARRServersState -ConfigurationJSONObject $drainjson

Write-Host "Waiting to drain..."
Start-Sleep 30
Write-Host "Drained!"

Write-Host "Graceful stopping..."

Set-IISARRServersState -ConfigurationJSONObject $graceFulStopJson

Write-Host "Waiting for graceful stop..."
Start-Sleep 30
Write-Host "Graceful stopped!"

Write-Host "Force unavailable if inactive and 0 request..."

Set-IISARRServersState -ConfigurationJSONObject $stopJson

Write-Host "Done!"



 
