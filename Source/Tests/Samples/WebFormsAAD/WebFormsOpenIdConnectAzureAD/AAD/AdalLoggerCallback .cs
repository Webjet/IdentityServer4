using System.Diagnostics;
using Microsoft.IdentityModel.Clients.ActiveDirectory; // install-package Microsoft.IdentityModel.Clients.ActiveDirectory

namespace WebFormsOpenIdConnectAzureAD.AAD
{

    public class AdalLoggerCallback : IAdalLogCallback
    {
        public void Log(LogLevel level, string message)
        {
            // platform-specific code goes here
            Debug.WriteLine("AdalLoggerCallback " + level+ ": "+ message);
            //TODO Nlog 
        }

       
    }
}