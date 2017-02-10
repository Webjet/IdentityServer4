using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.Common.Debugging;
using AdminPortal.BusinessServices.LandingPage;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using NLog.Common;
using Webjet.DotNet.Common;

namespace AdminPortal.BusinessServices
{
    public class LandingPageLayoutLoader
    {
        private string _filepath = "";
        private IConfigurationRoot _config;
        private string DefaultConfigFilePath
        {
            get
            {
                //DebugEnvironmentSettings.DebugApplicationFolderPath();
                
                //DI configuration
                var relPath = _config["LandingPageLayoutRelativePath"]?? "";
                return Path.Combine(WebApplicationHelper.WebApplicationRootDirectory(), relPath);
                
            }
        }

        private string _regionIndicatorFilepath = Directory.GetCurrentDirectory() + @"\config\RegionIndicatorList.xml"; //HostingEnvironment.ApplicationPhysicalPath + "config\\RegionIndicatorList.xml";

        private static readonly NLog.ILogger StaticLogger = LogManager.GetCurrentClassLogger();

        //TODO: Change to Serilog
        private readonly NLog.ILogger _logger;

        public LandingPageLayoutLoader(IConfigurationRoot config=null, string filepath = null, ILogger logger = null, string regionIndicatorFilePath = null)
        {
            _config = config;
           
            _filepath = !string.IsNullOrEmpty(filepath) ? filepath : DefaultConfigFilePath;

            if (!string.IsNullOrEmpty(regionIndicatorFilePath))
            {
                _regionIndicatorFilepath = regionIndicatorFilePath;
            }
            _logger = logger ?? StaticLogger;
        }


        public UiLinks GetUiLinks()
        {
           
            string xml = StreamHelper.FileToString(_filepath);
            return xml.ParseXml<UiLinks>();
          

        }
        
        public RegionIndicatorList GetRegionIndicators()
        {
            try
            {
                string xml = StreamHelper.FileToString(_regionIndicatorFilepath);
                return xml.ParseXml<RegionIndicatorList>();

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex, "Error in  " + _regionIndicatorFilepath);
            }
            return null;
        }
        
    }

}
