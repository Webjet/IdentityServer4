using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace Webjet.DotNet.Common
{
    /// <summary>
    /// See also TraceOutputExtensions
    /// </summary>
    public static class DebugOutputHelper
    {

        // Methods

        public static void LogToFile(Exception exc, string path, bool append)
        {
            // string text1;
            StreamWriter writer1 = new StreamWriter(path, append);
            writer1.AutoFlush = true;
            Debug.WriteLine(exc.ToString());
            writer1.WriteLine(TraceOutputHelper.LineWithTrace(DateTime.Now.ToString()));
            writer1.WriteLine(TraceOutputHelper.LineWithTrace(exc.ToString()));
            writer1.Close();
            //return text1;
        }

        [Conditional("DEBUG")]
        public static void PrintChildren(System.Web.UI.Control container, string sComment /* = "" */)
        {
            Debug.WriteLine(TraceOutputHelper.ChildrenAsString(container, sComment));
        }

        [Conditional("DEBUG")]
        public static void PrintConstraints(DataTable tbl, string sComment /* = "" */)
        {
            PrintIfNotEmpty(sComment);
            Debug.WriteLine("Table: " + tbl.TableName);
            foreach (Constraint constraint1 in tbl.Constraints)
            {
                Debug.WriteLine(constraint1.ToString());
            }
        }
        [Conditional("DEBUG")]
        public static void PrintCookies(string url, CookieContainer container)
        {
            PrintCookies(new Uri(url), container);
        }
        [Conditional("DEBUG")]
        public static void PrintCookies(Uri uri, CookieContainer container)
        {
            if (container == null)
            {
                DebugOutputHelper.TracedLine("CookieContainer is null");
                return;
            }
            System.Net.CookieCollection cookies = container.GetCookies(uri);
            PrintCookies(cookies, "PrintCookies for Uri " + uri.ToString());
        }

        [Conditional("DEBUG")]
        public static void PrintCookies(CookieCollection cookies, string sComment)
        {
            PrintCookies(cookies, sComment, TraceOutputHelper.OutputLevel.Brief);
        }
        [Conditional("DEBUG")]
        public static void PrintCookies(CookieCollection cookies, string sComment, TraceOutputHelper.OutputLevel level)
        {
            Debug.WriteLine(TraceOutputHelper.CookieCollectionAsString(cookies, sComment, level));
        }

        //		coll=Request.ServerVariables; 
        [Conditional("DEBUG")]
        public static void PrintServerVariables(NameValueCollection coll, string sComment /* = "" */)
        {
            Debug.WriteLine(TraceOutputHelper.NameValueCollectionAsString(coll, sComment));
            //			if (!String.IsNullOrEmpty(sComment))
            //			{
            //				Debug.WriteLine(sComment);
            //			}
            //			int loop1, loop2;
            // 
            //			// Get names of all keys into a string array. 
            //			String[] arr1 = coll.AllKeys; 
            //			for (loop1 = 0; loop1 < arr1.Length; loop1++) 
            //			{
            //				Debug.WriteLine("Key: " + arr1[loop1] );
            //				String[] arr2=coll.GetValues(arr1[loop1]);
            //				for (loop2 = 0; loop2 < arr2.Length; loop2++) 
            //				{
            //					Debug.WriteLine("Value " + loop2 + ": " + arr2[loop2] );
            //				}
            //			}
        }


        [Conditional("DEBUG")]
        public static void PrintDataAdapter(DbDataAdapter da, string sComment /* = "" */)
        {
            PrintIfNotEmpty(sComment);
            Debug.WriteLine("DataAdapter SelectCommand " + da.SelectCommand.CommandText);
            if (da.InsertCommand != null)
            {
                Debug.WriteLine("DataAdapter InsertCommand " + da.InsertCommand.CommandText);
            }
            if (da.UpdateCommand != null)
            {
                Debug.WriteLine("DataAdapter UpdateCommand " + da.UpdateCommand.CommandText);
            }
            if (da.DeleteCommand != null)
            {
                Debug.WriteLine("DataAdapter DeleteCommand " + da.DeleteCommand.CommandText);
            }
        }
        [Conditional("DEBUG")]
        public static void PrintSqlCommandBuilder(DbCommandBuilder commandBuilder, string sComment /* = "" */)
        {
            PrintIfNotEmpty(sComment);
            PrintDataAdapter(commandBuilder.DataAdapter, "");
            Debug.WriteLine("SqlCommandBuilder InsertCommand " + commandBuilder.GetInsertCommand().CommandText);
            Debug.WriteLine("SqlCommandBuilder UpdateCommand " + commandBuilder.GetUpdateCommand().CommandText);
            Debug.WriteLine("SqlCommandBuilder DeleteCommand " + commandBuilder.GetDeleteCommand().CommandText);
        }

        [Conditional("DEBUG")]
        public static void PrintDataset(DataSet ds, string sComment /* = "" */)
        {
            Debug.WriteLine(TraceOutputHelper.DatasetAsString(ds, sComment));
            //PrintIfNotEmpty(sComment);
            //Debug.WriteLine("Tables Count: " + ds.Tables.Count.ToString());
            //foreach (DataTable table1 in ds.Tables)
            //{
            //    DebugOutputHelper.PrintTable(table1, "");
            //}
        }

        [Conditional("DEBUG")]
        public static void PrintDictionary(IDictionary coll, string sComment /* = "" */)
        {
            Debug.WriteLine(TraceOutputHelper.DictionaryAsString(coll, sComment));
        }
        [Conditional("DEBUG")]
        public static void PrintStringArray(ArrayList messages, string sComment)
        {
            Debug.WriteLine(TraceOutputHelper.StringArrayToString(messages, sComment));
            //PrintIfNotEmpty(sComment);
            //foreach (string error in messages)
            //{
            //    Debug.WriteLine(error);
            //}
        }

        //E.G Page.Request.Form includes __EVENTTARGET
        [Conditional("DEBUG")]
        public static void PrintNameObjectCollection(NameValueCollection coll, string sComment /* = "" */)
        {
            Debug.WriteLine(TraceOutputHelper.NameValueCollectionAsString(coll, sComment));
        }

        [Conditional("DEBUG")]
        public static void PrintPrimaryKeys(DataTable myTable, string sComment /* = "" */)
        {
            PrintIfNotEmpty(sComment);
            DataColumn[] columnArray1 = myTable.PrimaryKey; //        ' Create the array for the columns.
            Debug.WriteLine("Primary Keys Column Count: " + columnArray1.Length.ToString());
            int num2 = columnArray1.GetUpperBound(0);//        ' Get the number of elements in the array.
            for (int num1 = 0; num1 <= num2; num1++)
            {
                Debug.WriteLine(columnArray1[num1].ColumnName + ": " + columnArray1[num1].DataType.ToString());
            }
        }

        [Conditional("DEBUG")]
        public static void PrintRow(DataRow row, string sComment /* = "" */)
        {
            Debug.WriteLine(TraceOutputHelper.RowAsString(row, sComment));
        }
        //    ''TODO: see trace.axd- should be better
        [Conditional("DEBUG")]
        public static void PrintSessionCollection(HttpSessionState myCol, string sComment /* = "" */)
        {
            PrintIfNotEmpty(sComment);
            Debug.WriteLine(new HttpSessionStateWrapper(myCol).ItemsAsString());

        }

        [Conditional("DEBUG")]
        public static void PrintTable(DataTable tbl, string sComment /* = "" */)
        {
            Debug.WriteLine(TraceOutputHelper.TableAsString(tbl, sComment));
            //PrintIfNotEmpty(sComment);
            //if (tbl == null)
            //{
            //    Debug.WriteLine("Table is null");
            //    return;
            //}
            //Debug.WriteLine(String.Format("Table: '{0}' with {1} rows ", tbl.TableName, tbl.Rows.Count));
            //foreach (DataRow row1 in tbl.Rows)
            //{
            //    DebugOutputHelper.PrintRow(row1, "");
            //}
        }
        [Conditional("DEBUG")]
        public static void PrintView(DataView view, string sComment)
        {
            PrintIfNotEmpty(sComment);
            if (view == null)
            {
                Debug.WriteLine("view is null");
                return;
            }
            Debug.WriteLine(String.Format("view: '{0}' with {1} rows ", view.Table.TableName, view.Count));
            for (int i = 0; i < view.Count; i++)
            {
                DebugOutputHelper.PrintRow(view[i].Row, "");
            }
        }
        [Conditional("DEBUG")]
        public static void PrintAppDomain(string sComment)
        {
            PrintIfNotEmpty(sComment);
            AppDomain domain = AppDomain.CurrentDomain;

            // Write out application domain information
            Debug.WriteLine("Host domain: " + domain.FriendlyName);
            Debug.WriteLine("Configuration file is: " + domain.SetupInformation.ConfigurationFile);
            Debug.WriteLine("Application Base Directory is: " + domain.BaseDirectory);
            Debug.WriteLine("RelativeSearchPath is: " + domain.RelativeSearchPath);

        }
        private static void PrintIfNotEmpty(string sComment)
        {
            if (!String.IsNullOrEmpty(sComment))
            {
                Debug.WriteLine(sComment);
            }
        }
        [Conditional("DEBUG")]
        public static void TracedLine(string sComment)
        {
            string sRet = TraceOutputHelper.LineWithTrace(sComment);
            Debug.WriteLine(sRet);
        }
        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] arg)
        {
            Debug.WriteLine(string.Format(format, arg));
        }
        [Conditional("DEBUG")]
        public static void TracedLine(string format, params object[] arg)
        {
            string sRet = TraceOutputHelper.LineWithTrace(format, arg);
            Debug.WriteLine(sRet);
        }
        [Conditional("DEBUG")]
        public static void VariableNotNullAssert(object argument, string argumentName)
        { //Idea from http://blogs.msdn.com/brada/archive/2004/07/11/180315.aspx 
            if (argument == null)
            {           //		throw new ArgumentNullException(argumentName); 
                Debug.Assert(false, argumentName + " cannot be null");
            }
        }

        [Conditional("DEBUG")]
        public static void PrintRowErrors(DataSet myDataSet)
        {
            foreach (DataTable myTable in myDataSet.Tables)
            {
                foreach (DataRow myRow in myTable.Rows)
                {
                    if (myRow.HasErrors)
                    {
                        Debug.WriteLine(myRow.RowError);
                    }
                }
            }
        }
        //see ASP.NET Identity Matrix http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnnetsec/html/SecNetAP05.asp
        [Conditional("DEBUG")]
        public static void PrintAspNetIdentities()
        {
            bool bHttpContextAvailable = (System.Web.HttpContext.Current != null);
            if (bHttpContextAvailable)
            { Debug.WriteLine("HttpContext.Current.User=" + System.Web.HttpContext.Current.User.Identity.Name); }
            else
            { Debug.WriteLine("HttpContext.Current is not available"); }
            Debug.WriteLine("WindowsIdentity.GetCurrent()=" + System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            Debug.WriteLine("Thread.CurrentPrincipal =" + System.Threading.Thread.CurrentPrincipal.Identity.Name);
            string AuthenticationType = System.Security.Principal.WindowsIdentity.GetCurrent().AuthenticationType;
            Debug.WriteLine("		AuthenticationType			=" + AuthenticationType);
            if (bHttpContextAvailable)
            {
                NameValueCollection serverVariables = System.Web.HttpContext.Current.Request.ServerVariables;
                Debug.WriteLine("serverVariables[LOGON_USER]=" + serverVariables["LOGON_USER"]);
                Debug.WriteLine("serverVariables[AUTH_USER]=" + serverVariables["AUTH_USER"]);
                Debug.WriteLine("serverVariables[REMOTE_USER]=" + serverVariables["REMOTE_USER"]);
            }
        }
        public static string DateTimeInFileName(DateTime time, bool IncludeSecs, bool IncludeMilliSecs)
        {
            string format = "yyMMdd_HHmm";
            if (IncludeSecs == true) format += "ss";
            if (IncludeSecs == true) format += "fffffff";
            return time.ToString(format);
        }
        [Conditional("DEBUG")]
        public static void PrintHttpWebRequest(HttpWebRequest reqHttp, string sComment)
        {
            PrintIfNotEmpty(sComment);
            //http://www.codeproject.com/csharp/ClientTicket_MSNP9.asp 
            Debug.Write(" AllowAutoRedirect: " + reqHttp.AllowAutoRedirect);
            Debug.Write(" Pipelined is: " + reqHttp.Pipelined);
            Debug.Write(" KeepAlive is: " + reqHttp.KeepAlive);
            Debug.WriteLine(" ProtocolVersion is: " + reqHttp.ProtocolVersion);
        }
        [Conditional("DEBUG")]
        public static void PrintHttpWebResponse(HttpWebResponse respHttp, string sComment)
        {
            PrintIfNotEmpty(sComment);
            //http://www.codeproject.com/csharp/ClientTicket_MSNP9.asp 
            Debug.WriteLine(" StatusCode: " + respHttp.StatusCode);
            Debug.WriteLine(" ResponseUri: " + respHttp.ResponseUri);
            Debug.WriteLine(" Headers are: " + respHttp.Headers.ToString());
        }
#if DIRECTORYSERVICES_TRACES_REQUIRED //not used at the moment

            [Conditional("DEBUG")]
            public static void PrintDirectoryEntryProperties(System.DirectoryServices.DirectoryEntry entry, string sComment)
            { //similar to GetPropertyList function from http://www.c-sharpcorner.com/UploadFile/klaus_salchner@hotmail.com/LDAPIISWinNTDirectoryServices08242005032318AM/LDAPIISWinNTDirectoryServices.aspx?ArticleID=74360cb9-3d8e-49c2-9429-3d492857ac95
                PrintIfNotEmpty(sComment);
                // loop through all the properties and get the key for each
                foreach (string Key in entry.Properties.PropertyNames)
                {
                    string sPropertyValues = String.Empty;
                    // now loop through all the values in the property;
                    // can be a multi-value property
                    foreach (object Value in entry.Properties[Key])
                        sPropertyValues += Convert.ToString(Value) + ";";
                    // cut off the separator at the end of the value list
                    sPropertyValues = sPropertyValues.Substring(0, sPropertyValues.Length - 1);
                    // now add the property info to the property list
                    Debug.WriteLine(Key + "=" + sPropertyValues);
                }
            }
#endif// DIRECTORYSERVICES_TRACES_REQUIRED //not used at the moment

#if WINFORMS_TRACES_REQUIRED //not used at the moment

            [Conditional("DEBUG")]
            public static void Print(BindingMemberInfo bInfo,  string sComment /* = "" */)
            {
                Debug.WriteLine(TraceString(bInfo, sComment));
            }

            [Conditional("DEBUG")]
            public static void Print(ControlBindingsCollection bindings,  string sComment /* = "" */)
            {
                Debug.WriteLine(TraceString(bindings, sComment));
            }

            [Conditional("DEBUG")]
            public static void PrintBindingMemberInfo(System.Windows.Forms.Control container,  string sComment /* = "" */)
            {
                Debug.WriteLine(BindingMemberInfoAsString(container, sComment));
            }
            public static void PrintCurrentRow(BindingManagerBase bm,  string sComment /* = "" */)
            {
                PrintIfNotEmpty(sComment);
                //        ' Check the type of the Current object. If it is not a
                //        ' DataRowView, exit the method. 
                if (bm.Current.GetType() != typeof(DataRowView))
                {
                    return;
                }
                DataRowView drv = (DataRowView)bm.Current;
                DebugOutputHelper.PrintRow(drv.Row, "");
            }

#endif //WINFORMS_TRACES_REQUIRED //not used at the moment

    }//public class DebugOutputHelper 
}
