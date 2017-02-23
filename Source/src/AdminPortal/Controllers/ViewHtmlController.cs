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
            htmlPath = _config?["GoogleBigQueryItineraryDirectoryPath"];

            if(htmlPath.IsNullOrBlank())
            {
                htmlPath = Directory.GetCurrentDirectory() + @"\Views\ViewHtml\GoogleBigQueryItinerary\index.html";
            }
           
            googleBigQueryHostUrl = _config["GoogleBigQueryHostUrl"];
            string htmlContent = System.IO.File.ReadAllText(htmlPath);
            ViewData["HtmlContent"] = htmlContent;
            ViewData["GoogleBigQueryHostUrl"] = googleBigQueryHostUrl;


            //ViewData["GoogleBigQueryHostUrl"] = _config.GetValue<string>("AppSettings:GoogleBigQueryHostUrl"); 
            //ConfigurationManager.AppSettings["GoogleBigQueryHostUrl"];

            return View();
        }
        

        [ResourceAuthorize("GenerateRandomNumber")]
        [HttpGet]
        public ActionResult GenerateRandomNumber()
        {
            string htmlPath;
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

        // GET: AnalyticsScripts
        [ResourceAuthorize("GoogleBigQueryItinerary")]
        [HttpGet]
        public ActionResult GoogleBigQueryItineraryCiaran()
        {
            string htmlPath;
            string googleBigQueryHostUrl;
            htmlPath = _config?["GoogleBigQueryItineraryDirectoryPath"];

            if (htmlPath.IsNullOrBlank())
            {
                htmlPath = Directory.GetCurrentDirectory() + @"\Views\ViewHtml\GoogleBigQueryItinerary\index.html";
            }

            googleBigQueryHostUrl = _config["GoogleBigQueryHostUrl_Ciaran"];
            string htmlContent = System.IO.File.ReadAllText(htmlPath);
            ViewData["HtmlContent"] = htmlContent;
            ViewData["GoogleBigQueryHostUrl"] = googleBigQueryHostUrl;


            //ViewData["GoogleBigQueryHostUrl"] = _config.GetValue<string>("AppSettings:GoogleBigQueryHostUrl"); 
            //ConfigurationManager.AppSettings["GoogleBigQueryHostUrl"];

            return View(viewName: "GoogleBigQueryItinerary");
        }

    }
}