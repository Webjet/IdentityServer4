Configuration IISInstall 
{
	Import-DscResource -ModuleName 'PSDesiredStateConfiguration'
    Node "localhost"
    { 
        WindowsFeature IIS 
        { 
            Ensure = "Present"
            Name = "Web-Server"                       
        } 
    } 
}