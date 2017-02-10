using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Webjet.DotNet.Common
{
   public static class XmlSerializationHelper
    {
        /// <summary>
        /// Serialize an object into XML
        /// </summary>
        /// <param name="serializableObject">Object that can be serialized</param>
        /// <returns>Serial XML representation</returns>
        /// <remarks>Consider to call TraceOutputExtensions.ToXmlString(responseDetails); instead</remarks>
        public static string XmlSerialize(object serializableObject)
        {
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            //serializer.
            System.IO.MemoryStream aMemStr = new System.IO.MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(aMemStr, null);
            writer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, serializableObject);
            string strXml = System.Text.Encoding.UTF8.GetString(aMemStr.ToArray());
            return strXml;
        }
        /// <summary>
        /// Serialize an object into XML. Doesn't throw exception, but returns exc.ToString.
        /// </summary>
        /// <param name="serializableObject">Object that can be serialized</param>
        /// <returns>Serial XML representation</returns>
        /// <remarks>If you need detailed output,  <seealso cref="TraceOutputExtensions.ToXmlString"/> </remarks>
        public static string XmlSerializeSafe(this object objectToSerialize)
        {
            bool bRet = true;
            string strXml;
            try
            {
                strXml = XmlSerialize(objectToSerialize);
            }
            catch (Exception exc)
            {
                //	bRet = false;
                strXml = exc.ToString();
            }
            return strXml;
        }
        /// <summary>
        /// Serialize an object into XML
        /// </summary>
        /// <param name="serializableObject">Object that can be serialized</param>
        /// <returns>Serial XML representation</returns>
        /// <remarks>If you need detailed output,  <seealso cref="TraceOutputExtensions.ToXmlString"/> </remarks>
        public static bool TryXmlSerialize(this object objectToSerialize, out string strXml)
        {
            bool bRet = true;
            try
            {
                strXml = XmlSerialize(objectToSerialize);
            }
            catch (Exception exc)
            {
                bRet = false;
                strXml = exc.ToString();
            }
            return bRet;
        }

        ///// <summary>
        ///// Restore (Deserialize) an object, given an XML string
        ///// </summary>
        ///// <param name="xmlString">XML</param>
        ///// <param name="serializableObject">Object to restore as</param>
        //public static object XmlDeSerialize(string xmlString, object serializableObject)
        //{
        //  XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());      
        //  System.IO.MemoryStream aStream = new
        //    System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlString));

        //  return serializer.Deserialize(aStream);
        //}

        /// <summary>
        /// Restore (Deserialize) an object, given an XML string
        /// </summary>
        /// <param name="xmlString">XML</param>
        /// <param name="serializableObject">Type of object to restore as</param>
        public static object XmlDeSerialize(string xmlString, Type objectType)
        {
            XmlSerializer serializer = new XmlSerializer(objectType);
            System.IO.MemoryStream aStream = new
              System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlString));

            return serializer.Deserialize(aStream);
        }

        public static T DeserializeXml<T>(string xml)
        {
            return (T)XmlDeSerialize(xml, typeof(T));
        }
        //public static string SerializeToXmlElement(object serializableObject)
        //{
        //    string sXml=XmlSerialize(serializableObject);
        //    XmlElement elmt=new XmlElement();
        //    elmt.InnerXml= 
        //    return strXml;
        //}
        /// <summary>
        /// Serialize an object into XML
        /// See "Types Supported by the Data Contract Serializer" http://msdn.microsoft.com/en-us/library/ms731923.aspx
        /// Usually required [Serializable] attribute(e.g for LinqToSql generated entities] 
        /// </summary>
        /// <param name="objectToSerialize">Object that can be serialized</param>
        /// <param name="strXml">In case of error -returns exception</param>
        /// <returns>Serial XML representation</returns>
        /// <remarks>If possible, use   <seealso cref="TraceOutputExtensions.ToXmlString"/> </remarks>
        /// <remarks> To be XML serializable, types which inherit from IEnumerable must have an implementation of Add(System.Object) at all levels of their inheritance hierarchy. /// Required to convert ToList()
        /// System.Linq.Enumerable+WhereEnumerableIterator`1[[TSA.BusinessEntities.DataModels.ConfigAutoRetry, TSA.BusinessEntities, Version=2.0.0.0, Culture=neutral, PublicKeyToken=5965ee7fa386c1b9]] does not implement Add(System.Object).
        ///</remarks>
        public static bool TryDataContractSerializeToXml<T>(T objectToSerialize, out string strXml)
        {
            bool bRet = true;
            try
            {
                strXml = DataContractSerializeToXml(objectToSerialize);
            }
            catch (Exception exc)
            {
                bRet = false;
                strXml = exc.ToString();
            }
            return bRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static string DataContractSerializeToXml<T>(T obj)
        {
            string strXml = "";
            //from http://billrob.com/archive/2010/02/09/datacontractserializer-converting-objects-to-xml-string.aspx
            var serializer = new DataContractSerializer(obj.GetType());
            using (var backing = new StringWriter())
            using (var writer = new XmlTextWriter(backing))
            {
                serializer.WriteObject(writer, obj);
                strXml = backing.ToString();
            }
            return strXml;
        }


        public static T DataContractDeserializeFromXml<T>(string xmlInput)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(xmlInput);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var deserializer = new DataContractSerializer(typeof(T));
                return (T)deserializer.ReadObject(stream);
            }
        }
        /// <summary>
        /// See "Types Supported by the Data Contract Serializer" http://msdn.microsoft.com/en-us/library/ms731923.aspx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string DataContractSerialize<T>(T obj)
        {
            Debug.Assert(false, "NOT working, use ToXmlString<T>, investigate why?");
            //seems not working for LTS generated BackgroundProcess -TODo investigate
            //From http://stackoverflow.com/questions/1077747/persist-a-datacontract-as-xml-in-a-database
            StringBuilder sb = new StringBuilder();
            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            ser.WriteObject(XmlWriter.Create(sb), obj);
            string sRet = sb.ToString();
            if (String.IsNullOrEmpty(sRet))
            {
                sRet = obj.ToXmlString();
            }
            return sRet;
        }
        public static T LoadFromXml<T>(string filepath) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            if (File.Exists(filepath))
            {
                System.IO.Stream aStream = null;
                try
                {
                    aStream = File.Open(filepath, FileMode.Open, FileAccess.Read);
                    return (T)serializer.Deserialize(aStream);

                }
                finally
                {
                    if (aStream != null)
                    {
                        aStream.Dispose();
                    }
                }
            }
            else
            {
                return null;// new T();
            }
        }
        /// <summary>
        /// Used in tests to check that no data lost after serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T SerializeAndRestore<T>(T source)
        {
            MemoryStream ms = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(typeof(T));
            xs.Serialize(ms, source);

            ms.Position = 0;
            return (T)xs.Deserialize(ms);
        }
    }
}
