Configuration Installs 
{
	Import-DscResource -ModuleName 'PSDesiredStateConfiguration'
    Node "localhost"
    { 
		Script InstallDODO 
        { 
			SetScript = { 
				#region Load up DODO!
				$dodoExe = "C:\DevOps\DSC\dodo.exe"
				& $dodoExe --export
				Import-Module -Name "C:\DevOps\DSC\DODO\dodo.psd1" -Verbose -Force -Scope Global
				#endregion

				#load up an ARR template
				$arr = @"
{
    "Containers": [
        {
            "Name": "My ARR",
            "Type": "IIS-ARR",
            "Attributes": {
                "Properties": {
                    "WebPlatformInstaller": {
                        "HTTPUri": "http://download.microsoft.com/download/7/0/4/704CEB4C-9F42-4962-A2B0-5C84B0682C7A/WebPlatformInstaller_amd64_en-US.msi"
                    },
                    "DownloadFolder": "C:\\Temp",
                    "WindowsFeatures": [
                        {
                            "Name": "Web-Server"
                        }
                    ],
                    "ServerFarms": [
                        {
                            "Name": "TestServerFarm",
                            "Servers": [
                                {
                                    "Name": "App server 01",
                                    "Address": "192.168.43.5"
                                },
                                {
                                    "Name": "App server 02",
                                    "Address": "192.168.43.6"
                                }
                            ],
                            "HealthTest": {
                                "Url": "http://server/healthcheck/SecurePay.Gateway.html",
                                "Interval": "00:00:10",
                                "ResponseMatch": "up"
                            }
                        }
                    ],
                    "RoutingRules": [
                        {
                            "Name": "ARR_Https_Redirect",
                            "Description": "Rule to route HTTP to HTTPS",
                            "Enabled": "false",
                            "PatternSyntax": "Wildcard",
                            "StopProcessing": "True",
                            "Match": {
                                "Name": "url",
                                "Value": "*"
                            },
                            "Actions": [
                                {
                                    "Type": "Redirect",
                                    "Url": "https://{HTTP_HOST}/{R:0}"
                                }
                            ],
                            "Conditions": [
                                {
                                    "Input": "{HTTPS}",
                                    "Pattern": "off"
                                }
                            ]
                        },
                        {
                            "Name": "ARR_RouteTo_GateWay",
                            "Description": "Rule to route traffic to gateway application",
                            "Enabled": "true",
                            "PatternSyntax": "Wildcard",
                            "StopProcessing": "True",
                            "Match": {
                                "Name": "url",
                                "Value": "*"
                            },
                            "Actions": [
                                {
                                    "Type": "Rewrite",
                                    "Url": "http://SecurePay.Gateway/SecurePay.Gateway/{R:0}"
                                }
                            ],
                            "ConditionAttributes" : 
                            {
                                "LogicalGrouping" : "MatchAny",
                                "TrackAllCaptures" : "false"
                            },
                            "Conditions": [
                                {
                                    "Input": "{HTTP_HOST}",
                                    "Pattern": "uatgateway.*"
                                }
                            ]
                        },
                        {
                            "Name": "ARR_Rule2",
                            "Description": "Rule sample to showcase condition MatchAll",
                            "Enabled": "true",
                            "PatternSyntax": "Wildcard",
                            "StopProcessing": "True",
                            "Match": {
                                "Name": "url",
                                "Value": "*"
                            },
                            "Actions": [
                                {
                                    "Type": "Rewrite",
                                    "Url": "http://SecurePay.Gateway/SecurePay.Gateway/{R:0}"
                                }
                            ],
                            "ConditionAttributes" : 
                            {
                                "LogicalGrouping" : "MatchAlll",
                                "TrackAllCaptures" : "false"
                            },
                            "Conditions": [
                                {
                                    "Input": "{HTTP_HOST}",
                                    "Pattern": "uatgateway.*"
                                }
                            ]
                        }
                    ]
                }
            }
        }
    ]
}
"@ | ConvertFrom-Json;

				Run-DODO -ConfigurationJSONObject $arr

			}
			TestScript = { 
				return $false 
			}
			GetScript = { 
			}
        }
    } 
}