
#http://stackoverflow.com/questions/885349/how-to-write-a-powershell-script-that-accepts-pipeline-input/887406#887406
function StripComments{
	[CmdletBinding()]
    param(  
    [Parameter(Position=0,Mandatory=$true,ValueFromPipeline=$true,ValueFromPipelineByPropertyName=$true)][String]$JsonWithComments
    ) 
   $validJson=[JSonMinify]::StripComments($JsonWithComments)
   return $validJson
}
#http://stackoverflow.com/questions/24868273/run-a-c-sharp-cs-file-from-a-powershell-script
$source = @"
using System;
using System.Text.RegularExpressions;
public class JsonMinify
{
//http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
  public static string StripComments(string  input)
  {
var blockComments = @"/\*(.*?)\*/";
var lineComments = @"//(.*?)\r?\n";
var strings = @"""((\\[^\n]|[^""\n])*)""";
var verbatimStrings = @"@(""[^""]*"")+";
  
       string noComments = Regex.Replace(input,
	    blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
		me => {
	        if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
	            return me.Value.StartsWith("//") ? Environment.NewLine : "";
	        // Keep the literal strings
	        return me.Value;
	    },
	    RegexOptions.Singleline);
	  return noComments;
    }

}
"@
if (-not ([System.Management.Automation.PSTypeName]'JsonMinify').Type)
{
    Add-Type -TypeDefinition $source
}



# $validJson=[JSonMinify]::StripComments($testData)
$localDebug=$true # true only for local debugging, change to false before commit
if ($localDebug){
cls
$testData=@"
{
  "Parameters": {
// remove whole line comments   "Subscription": "Development",
    "SubscriptionID": "xxxxxxxxx" // remove partial line comments
  }
}
"@

 $result= $testData | StripComments | ConvertFrom-Json 
 Write-Output $result
if($false)  #step by step output
 { 
	 $validJson=[JSonMinify]::StripComments($testData)
	 Write-Output $validJson
	 $result=   ConvertFrom-Json $validJson
	 Write-Output $result
	 $validJson= StripComments($testData)
	 Write-Output $validJson
	 $result=   ConvertFrom-Json $validJson
	 Write-Output $result
 }
}