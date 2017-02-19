using System.Diagnostics;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NLog;

// install-package Microsoft.IdentityModel.Clients.ActiveDirectory

namespace WebFormsOpenIdConnectAzureAD.AAD
{

    public class AdalLoggerCallback : IAdalLogCallback
    {
        public void Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel level, string message)
        {
            // platform-specific code goes here
            Debug.WriteLine("AdalLoggerCallback " + level+ ": "+ message);
            //TODO Nlog 
        }

       
    }
}