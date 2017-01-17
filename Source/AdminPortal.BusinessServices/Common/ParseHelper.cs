using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Json;

namespace AdminPortal.BusinessServices
{
    //TODO: Move to Microsoft.SDC.Common OR AdminPortal.Common

    #region Summary
    // Convert XML string to Object
    //Reference URL - http://stackoverflow.com/questions/3187444/convert-xml-string-to-object/19613953#19613953
    #endregion
    public static class ParseHelper
    {

        //TODO: Not tested nor refered in solution either. Commenting below code
        //public static T ParseXmlFromPath<T>(string xmlFilePath) where T : class
        //{
        //    string xml = File.ReadAllText(xmlFilePath);
        //    var reader = XmlReader.Create(xmlFilePath, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });

        //    return new XmlSerializer(typeof(T)).Deserialize(reader) as T;

        //}

        //TODO: Move to Microsoft.SDC.Common
        /// <summary>
        ///  Extention method for Stream
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static Stream ToStream(this string xmlString)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(xmlString);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T ParseXml<T>(this string xmlString) where T : class
        {
            var reader = XmlReader.Create(xmlString.Trim().ToStream(), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });
            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }
#if INCLUDE_NOT_COVERED_BY_TESTS
        //TODO: Not tested nor refered in solution either. Commenting below code
        public static T ParseJSON<T>(this string @this) where T : class
        {
            return JsonParser.Deserialize<T>(@this.Trim());
        }
#endif
    }
}
