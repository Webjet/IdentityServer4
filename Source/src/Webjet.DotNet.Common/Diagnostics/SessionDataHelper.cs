using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace Webjet.DotNet.Common
{
    //[Serializable]
    //[XmlRoot("SessionDataHelper")]
    public static class SessionDataHelper
    {
        /// <summary>
        /// This method tries to retrieve the customer information from the current session state if it exists in the current context
        /// In which an event is being raised...
        /// </summary>
        /// <returns></returns>
        public static string GetTraceableObjectDetailsFromSession()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine();
            result.AppendLine("--ITraceable Trace Data--");
            bool traceAvailable = false;
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session.Count > 0)
                {
                    //  AppState astate = AppState.GetInstance();
                    // ITraceable t = (ITraceable)astate[AppState.State.Customer];
                    IEnumerator en = HttpContext.Current.Session.Contents.GetEnumerator();
                    while (en.MoveNext())
                    {
                        ITraceable traceableObject = HttpContext.Current.Session[en.Current.ToString()] as ITraceable;
                        if (traceableObject == null)
                            continue;

                        result.AppendLine("--");
                        result.AppendLine(traceableObject.GetTraceData());
                        result.AppendLine("--");
                        traceAvailable = true;
                    }
                }
            }
            catch (Exception exc)
            {
                // If this occurs, it means that the current place that we are logging does not have access to a http session.
                // So we cannot get any customer information at this point in time.
                Debug.Assert(false, exc.ToString());
            }
            if (!traceAvailable)
                result.AppendLine("No ITraceable objects found in session for this event.");
            return result.ToString();
        }

        public static string ItemsAsString(this HttpSessionStateBase session)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(" SessionID={0} \n", session.SessionID);
            sb.AppendFormat(" Keys.Count {0} \n", session.Keys.Count);
            foreach (string text1 in session.Keys)
            {
                sb.AppendFormat("Session({0})={1} \n", text1, session[text1]);
            }
            return sb.ToString();
        }
        public static string SessionStateAsString(this HttpSessionState myCol)
        {
            return (new HttpSessionStateWrapper(myCol)).ItemsAsString();
        }

    }
}
