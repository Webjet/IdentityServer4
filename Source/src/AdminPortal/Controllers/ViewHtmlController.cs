using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.Extensions.Configuration;
using Webjet.Common;
using Webjet.Common.Strings;
using System.Text;

namespace AdminPortal.Controllers
{
    //http://stackoverflow.com/questions/30263681/asp-net-5-vnext-getting-a-configuration-setting

    public class ViewHtmlController : Controller
    {
        private IConfigurationRoot _config;
        public ViewHtmlController(IConfigurationRoot config)
        {
            this._config = config;
        }

        // GET: AnalyticsScripts
        [ResourceAuthorize("GoogleBigQueryItinerary")]
        [HttpGet]
        public ActionResult GoogleBigQueryItinerary()
        {
            string htmlPath;
            string googleBigQueryHostUrl;
            //Path assign from Unit Test Project
            htmlPath = _config?["GoogleBigQueryItineraryDirectoryPath"];
            htmlPath = Path.Combine(Directory.GetCurrentDirectory(), htmlPath);
           
            googleBigQueryHostUrl = _config["GoogleBigQueryHostUrl"];
            string htmlContent = System.IO.File.ReadAllText(htmlPath);
            ViewData["HtmlContent"] = this.ReplaceGoogleAnalyticsCustomerJourneyStaticUrls(htmlContent);
            ViewData["GoogleBigQueryHostUrl"] = googleBigQueryHostUrl;


            //ViewData["GoogleBigQueryHostUrl"] = _config.GetValue<string>("AppSettings:GoogleBigQueryHostUrl"); 
            //ConfigurationManager.AppSettings["GoogleBigQueryHostUrl"];

            return View();
        }
        
        private string ReplaceGoogleAnalyticsCustomerJourneyStaticUrls(string htmlContent)
        {
            var baseUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value+ 
                "/GoogleAnalyticsCustomerJourney/Static/";

            StringBuilder sb = new StringBuilder(htmlContent);
            sb.Replace("src=\"static/", "src=\"" + baseUrl);
            sb.Replace("href=\"static/", "href=\"" + baseUrl);
                        
            return sb.ToString();
        }

        [ResourceAuthorize("GenerateRandomNumber")]
        [HttpGet]
        public ActionResult GenerateRandomNumber()
        {
            string htmlPath;

            //Path assign from Unit Test Project
            htmlPath = _config?["GenerateRandomNumberDirectoryPath"];
            
            if (htmlPath.IsNullOrBlank())
            {
                 var key = "GenerateRandomNumber"; //TODO use reflection or Fody to generate
                 string pageName = key + ".html";
                 htmlPath = Directory.GetCurrentDirectory() + @"\Views\ViewHtml\" + pageName;
            }
            else
            {
                htmlPath=_config["GenerateRandomNumberDirectoryPath"];
            }

            string htmlContent = System.IO.File.ReadAllText(htmlPath);

            ViewData["HtmlContent"] = htmlContent;

            return View("Index");

            //var key = "GenerateRandomNumber";//TODO use reflection or Fody to generate
            //ViewData["PageName"] =key+".html";
            //return View("Index");
        }
        
    }
}