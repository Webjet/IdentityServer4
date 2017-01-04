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
using NLog.Common;

namespace AdminPortal.BusinessServices
{
    public class LandingPageLayoutLoader
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\UILinksMapping.xml";
        private static readonly NLog.ILogger StaticLogger = LogManager.GetCurrentClassLogger();
        private readonly NLog.ILogger _logger; 

        public LandingPageLayoutLoader(string filepath = null, ILogger logger = null)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                _filepath = filepath;
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

    }

}
