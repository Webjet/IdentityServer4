using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualBasic;
using Webjet.DotNet.Common.Strings;
using VBStrings = Microsoft.VisualBasic.Strings;

namespace Webjet.DotNet.Common
{   /// <summary>
    /// Summary description for UriHelper.
    /// See also QueryStringHelper
    /// See also Understanding Paths in ASP.NET http://www.informit.com/articles/article.aspx?p=101145 and http://delicious.com/save?url=http%3A%2F%2Fwest-wind.com%2Fweblog%2Fposts%2F132081.aspx&title=Making%20Sense%20of%20ASP.NET%20Paths%20-%20Rick%20Strahl's%20Web%20Log&v=5&jump=yes
    /// </summary>
    //There are helper classes available with some functionality that can be useful
    //http://codeproject.com/aspnet/SimpleQueryString.asp "A small Class for simplifying the Work with URL Parameters"
    //http://www.codeproject.com/aspnet/UrlBuilder.asp
    // http://webdevel.blogspot.com/2004/09/url-manipulation-v2-c.html
    public static class UriHelper
    {
        public static string GetDomainFromUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.Authority; //localhost OR FULL DOMAIN NAME
        }
        public static string RemoveSchemeFromUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.Authority + uri.PathAndQuery;
        }
        public static string RemoveQueryStringFromUrl(string url)
        {
            //similar to string qs = QueryStringHelper.QueryStringFromUrl(url);
            string urlPath = url.LeftBefore("?");
            return urlPath;
        }
        public static string SchemeWithAuthority(string url)
        { //e.g returns 	"http://www.lexisnexis.com/"
            return SchemeWithAuthority(new Uri(url));
        }
        public static string SchemeWithAuthority(Uri uri)
        { //e.g returns 	"http://www.lexisnexis.com/"
            return uri.Scheme + Uri.SchemeDelimiter + uri.Authority;
        }
        public static string CombineUrl(string letfPart, string rightPart)
        { //see also System.Uri.private  CombineUri
            char DirectorySeparatorChar = '/';
            string sRet = letfPart;
            if (!StringHelper.EndsWith(letfPart, DirectorySeparatorChar) && !StringHelper.StartsWith(rightPart, DirectorySeparatorChar))
                sRet += DirectorySeparatorChar + rightPart;
            else if (StringHelper.EndsWith(letfPart, DirectorySeparatorChar) && StringHelper.StartsWith(rightPart, DirectorySeparatorChar))
                sRet += rightPart.Remove(0, 1);
            else
                sRet += rightPart;
            return sRet;
        }
        public static string CombineWithBase(string sBaseUrl, string url)
        {
            return CombineWithBase(sBaseUrl, url, false);
        }
        //bReplaceAuthority is not used at the moment
        /// <summary>
        /// If the passing url is Relative,adds BaseUrl as prefix 
        /// If the passing url is RelativeRoot(begins with / -a slash), adds shhema with domain of sBaseUrl as prefix
        /// if the passing url is Absolute and bReplaceAuthority=false, url is unchanged.
        /// </summary>
        /// <param name="sBaseUrl"></param>
        /// <param name="url"></param>
        /// <param name="bReplaceAuthority"></param>
        /// <returns></returns>
		public static string CombineWithBase(string sBaseUrl, string url, bool bReplaceAuthority)
        {
            string sRet = "";
            if (UriHelper.IsRelativeUrl(url))
            {
                if (UriHelper.IsRelativeRootUrl(url))
                {
                    sBaseUrl = SchemeWithAuthority(sBaseUrl);// e.g. http://www.lexisnexis.com/
                }
                sRet = UriHelper.CombineUrl(sBaseUrl, url);
            }
            else
            {//TODO check if absolute URL requires replacement
                DebugOutputHelper.TracedLine("absolute URL  sHref=" + url);
                sRet = url;
            }
            return sRet;
        }

        /// <summary>
        /// from http://stackoverflow.com/questions/4002692/c-sharp-determine-if-absolute-or-relative-url
        ///  Relative URLs http://www.webreference.com/html/tutorial2/3.html
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsRelativeUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Relative, out result);

            //            //The function is differen to  mono  UrlUtils.IsRelativeUrl(url);UrlUtils, but made it public		
            //            //it consideres Rooted urls as relative, when mono' IsRelativeUrl excluded Rooted from relative
            //            bool bRet=true;
            ////			  'is the value relative URL? from http://www.motobit.com/tips/detpg_replace-relative-links/ 
            //            if ((url.IndexOf("://") >= 0) || (url.IndexOf("mailto:")>=0) || (url.IndexOf("javascript:")>=0))
            //            {
            //                bRet=false;
            //            }
            //return bRet; 
        }
        /// <summary>
        /// from http://stackoverflow.com/questions/4002692/c-sharp-determine-if-absolute-or-relative-url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }

        //A relative URL that begins with / (a slash) always replaces the entire pathname of the base URL. 
        //from Relative URLs http://www.webreference.com/html/tutorial2/3.html
        public static bool IsRelativeRootUrl(string url)
        {
            return UrlUtils.IsRooted(url);//use mono UrlUtils, but made it public		
                                          //			bool bRet=false;
                                          //			//			  'is the value relative URL? from http://www.motobit.com/tips/detpg_replace-relative-links/ 
                                          //			if ((url.IndexOf("/") == 0 ) &&  (url.IndexOf("//") != 0 ) )
                                          //			{
                                          //				bRet=true;
                                          //			}
                                          //			return bRet; 
        }
        public static string BaseUrl(string url)
        {   //see http://www.webreference.com/html/tutorial2/3.html
            //e.g returns 	"http://www.lexisnexis.com/dir/subdir"
            //Code stolen from private uri.BasePath
            Uri uri = new Uri(url);
            string sPath = uri.AbsolutePath;
            int num1 = 0;
            int num2 = sPath.LastIndexOf('/');
            if (uri.IsUnc && (sPath.Length > 1))
            {
                num1 = sPath.IndexOf('/', 1);
                if (num1 < 0)
                {
                    num1 = 0;
                }
            }
            return SchemeWithAuthority(uri) + sPath.Substring(num1, (num2 - num1) + 1);
        }
        //Previously was in System
        public static string URLProtocolPrefix(string strURL)
        {//based on  DNNLibrary\Components\Shared\Globals.vb:AddHTTP
            //        'TODO try to use URI.Scheme 
            strURL = strURL.ToLower();
            string sRet = "";
            int nSchemeEnd = strURL.IndexOf("://");
            if (nSchemeEnd > 0)
            {
                int nHash = strURL.IndexOf("#");
                //there are special cases in e-library like www.xxx.com#http://www.xxx.com#"
                if ((nHash > 0) && (nHash < nSchemeEnd)) return sRet;
                sRet = VBStrings.Left(strURL, nSchemeEnd);
            }
            else
            {
                if (strURL.StartsWith("mailto:"))
                    return "mailto:";
            }
            return sRet;
        }
        public static string AddDefaultProtocolIfRequired(string strURL)
        { //overload
            return AddDefaultProtocolIfRequired(strURL, "http://");
        }
        //sDefaultProtocol can be "https://" ,"http://"  or anything else
        public static string AddDefaultProtocolIfRequired(string strURL, string sDefaultProtocol)
        {//based on  DNNLibrary\Components\Shared\Globals.vb:AddHTTP
            string sRet = strURL;
            if (URLProtocolPrefix(strURL).Length == 0)
            { //do not support "~") 
              //TODO add validation c:\ etc
                if (strURL.StartsWith(@"\\"))
                {
                    sDefaultProtocol = "file://";
                }
                sRet = sDefaultProtocol + strURL;
            }
            return sRet;
        }
        /// <summary>
        /// MS has internal IsAppRelativePath(string path)(http://weblogs.asp.net/doubinski/archive/2003/07/03/9640.aspx) 
        /// to check for '~' to refer to relative to root url paths
        /// </summary>
        /// <param name="strURL"></param>
        /// <returns></returns>
        public static bool IsAppRelativeUrl(string sURL)
        {  //baseb on part of DotNetNuke DNNLibrary\Components\Shared\Globals.vb ResolveUrl
            bool bRet = false;
            if (!String.IsNullOrEmpty(sURL))
            {
                bRet = sURL.StartsWith("~");
            }
            return bRet;
        }

        [Conditional("DEBUG")]
        public static void PrintUri(Uri uri)
        {
            //System.Uri samples http://www.geekpedia.com/tutorial68_Using-the-URI-Class.html
            StringBuilder txtResult = new StringBuilder();
            txtResult.Append("Absolute URI: " + uri.AbsoluteUri + "\r\n");
            txtResult.Append("Absolute Path: " + uri.AbsolutePath + "\r\n");
            txtResult.Append("Local path: " + uri.LocalPath + "\r\n");
            txtResult.Append("Scheme: " + uri.Scheme + "\r\n");
            txtResult.Append("Authority: " + uri.Authority + "\r\n");
            txtResult.Append("Host: " + uri.Host + "\r\n");
            txtResult.Append("Port: " + uri.Port + "\r\n");
            txtResult.Append("Fragment: " + uri.Fragment + "\r\n");
            txtResult.Append("Query: " + uri.Query + "\r\n");
            Debug.WriteLine(txtResult.ToString());
        }
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request">HttpContext.Current.Request can be passed</param>
        /// <returns></returns>
		public static string ApplicationPathUrl(HttpRequest Request)
        {
            String sUrl = CombineWithBase(SchemeWithAuthority(Request.Url), Request.ApplicationPath);
            return sUrl;
        }
    }//UriHelper
}
