using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
//using NLog;
using LogLevel = Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel;

// install-package Microsoft.IdentityModel.Clients.ActiveDirectory

namespace AdminPortal.BusinessServices.Logging
{
    /// <summary>
    ///   From https://www.schaeflein.net/adal-v3-diagnostic-logging/
    /// set in Startup               LoggerCallbackHandler.Callback = new AdalLoggerCallback();
    /// </summary>
    public class AdalLoggerCallback : IAdalLogCallback
    {
        private readonly ILogger _logger;

         public AdalLoggerCallback(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(LogLevel level, string message)
        {
            // platform-specific code goes here
            var msg = "AdalLoggerCallback " + level+ ": "+ message;
            Debug.WriteLine(msg);
            _logger.LogTrace(msg);
        }
    
    }

}