using Microsoft.SDC.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;
using AdminPortal.BusinessServices.LandingPage;
using NLog.Common;

namespace AdminPortal.BusinessServices
{
    public class LandingPageLayoutLoader
    {
        private string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\UILinksMapping.xml";
        private string _regionIndicatorFilepath = HostingEnvironment.ApplicationPhysicalPath + "config\\RegionIndicatorList.xml";

        private static readonly NLog.ILogger StaticLogger = LogManager.GetCurrentClassLogger();
        private readonly NLog.ILogger _logger; 

        public LandingPageLayoutLoader(string filepath = null, ILogger logger = null, string regionIndicatorFilePath=null)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                _filepath = filepath;
            }
            if (!string.IsNullOrEmpty(regionIndicatorFilePath))
            {
                _regionIndicatorFilepath = regionIndicatorFilePath;
            }
            _logger = logger ?? StaticLogger;
        }


        public UiLinks GetParsedXmlToObject()
        {
            try
            {
                string xml = StreamHelper.FileToString(_filepath);
                return xml.ParseXml<UiLinks>();

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex,
                    "Error parsing xml in RoleBasedMenuItemMap.xml ");
            }
            return null;
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

        //public Rootobject GetParsedJsonToObject()
        //{
        //    try
        //    {
        //        _filepath += "RegionIndicatorList.json";
        //        string json = StreamHelper.FileToString(_filepath);
        //        return json.ParseJSON<Rootobject>();

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log(LogLevel.Warn, ex,
        //            "Error parsing xml in RoleBasedMenuItemMap.xml ");
        //    }
        //    return null;

        //}
    }

}
