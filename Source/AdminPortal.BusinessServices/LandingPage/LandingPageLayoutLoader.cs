using Microsoft.SDC.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.LandingPage;
using NLog.Common;

namespace AdminPortal.BusinessServices
{
    public class LandingPageLayoutLoader
    {
        private string _filepath ="";

        private string DefaultConfigFilePath
        {
            get
            {
                //TODO: DI configuration after Core
                var relPath=WebConfigurationManager.AppSettings["LandingPageLayoutRelativePath"] ??"";
                return Path.Combine(WebApplicationHelper.WebApplicationRootDirectory(), relPath);
            }
        }

        private string _regionIndicatorFilepath = HostingEnvironment.ApplicationPhysicalPath + "config\\RegionIndicatorList.xml";

        private static readonly NLog.ILogger StaticLogger = LogManager.GetCurrentClassLogger();
        private readonly NLog.ILogger _logger;

        public LandingPageLayoutLoader(string filepath = null, ILogger logger = null, string regionIndicatorFilePath = null)
        {
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
