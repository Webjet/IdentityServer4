using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
#if NET461
using ServiceStack.Text;
#endif // NET461

namespace Webjet.DotNet.Common
{
    public static class TraceOutputExtensions
    {
        /// <summary>
        /// Dumps the public properties. ServiceStack.Text  C# .NET Extension method: T.Dump(); http://www.servicestack.net/mythz_blog/?p=202
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize">The object to serialize.If parameter is null, the function returns null</param>
        /// <returns></returns>
        /// 
#if NET461
        public static string DumpPublicProperties<T>(this T objectToSerialize)
        {
            string str = "";
            try
            {
                str = objectToSerialize.Dump();
            }
            catch (Exception exc)
            {
                str = objectToSerialize.ToString() + exc;
                Debug.Assert(false, "Investigate why ?" + exc);
            }
            return str;
        }
#endif // NET461
        ///// <summary>
        /////  Dump object to HTML string 
        ///// http://stackoverflow.com/questions/6032908/is-there-a-library-that-provides-a-formatted-dump-function-like-linqpad/6035014#6035014
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="objectToSerialize"></param>
        ///// <returns></returns>
        //public static string DumpToHtmlString<T>(this T objectToSerialize)
        //{
        //   // return objectToSerialize.DumpPublicProperties<T>();
        //    //Replaced 32 linqpad with ServiceStack.Text  C# .NET Extension method: T.Dump(); http://www.servicestack.net/mythz_blog/?p=202
        //    string strHTML = "";
        //    try
        //    {
        //        //Replaced 32 linqpad with ServiceStack.Text  C# .NET Extension method: T.Dump(); http://www.servicestack.net/mythz_blog/?p=202
        //        var writer = LINQPad.Util.CreateXhtmlWriter(true);
        //        writer.Write(objectToSerialize);
        //        strHTML = writer.ToString();
        //    }
        //    catch (Exception exc)
        //    {
        //        Debug.Assert(false, "Investigate why ?" + exc);
        //    }

        //    return strHTML;
        //}


        /// <summary>
        ///  Usually required [Serializable] attribute(e.g for LinqToSql generated entities] 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        public static string ToXmlString<T>(this T objectToSerialize)
        {
            string strXml = "";
            if (null == objectToSerialize)
            {
                return "null";
            }
            bool bRet = XmlSerializationHelper.TryDataContractSerializeToXml(objectToSerialize, out strXml);
            if (bRet == false)
            {
                Debug.Assert(false, String.Format("Investigate why failed TryDataContractSerializeToXml for {0} {1}  ?.", objectToSerialize, strXml));//exception
                strXml = "";
            }
            if (String.IsNullOrEmpty(strXml))
            {
                //Debug.Assert(false, "Investigate why DataContractSerialize not working?-ignore if empty.");
                bRet = XmlSerializationHelper.TryXmlSerialize(objectToSerialize, out strXml);
                if (bRet == false)
                {
                    Debug.Assert(false, "Investigate why ?." + strXml);
                }
            }
            return strXml;
        }
        //If possible, use 
        public static string ToXmlString(object objectToSerialize)
        {
            string strXml = "";
            //TODO move to separate class
            MessageContractAttribute[] attribs = objectToSerialize.GetType().GetCustomAttributes<MessageContractAttribute>(false) as MessageContractAttribute[];
            
            if (!attribs.IsNullOrEmpty())
            {
                bool bRet = XmlSerializationHelper.TryDataContractSerializeToXml(objectToSerialize, out strXml);
                if (bRet == false)
                {
                    Debug.Assert(false, "Investigate why ?." + strXml);//exception
                    strXml = "";
                }
                ////similar to XmlSerializationHelper.XmlSerialize()
                //DataContractSerializer serializer = new DataContractSerializer(objectToSerialize.GetType());
                //System.IO.MemoryStream aMemStr = new System.IO.MemoryStream();
                //System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(aMemStr, null);
                //serializer.WriteObject(writer, objectToSerialize);
                //strXml = System.Text.Encoding.UTF8.GetString(aMemStr.ToArray());
            }
            if (String.IsNullOrEmpty(strXml))
            {
                bool bRet = objectToSerialize.TryXmlSerialize(out strXml);
                if (bRet == false)
                {
                    Debug.Assert(false, "Investigate why ?." + strXml);
                }
            }
            return strXml;
        }

        public static string DictionaryAsString(this IDictionary dict)
        {
            return TraceOutputHelper.NotEmptyDictionaryAsString(dict, "");
        }
    }
}
