using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Webjet.DotNet.Common
{
    public static class ParseHelper
    {

        //TODO: Not tested nor refered in solution either. Commenting below code
        //public static T ParseXmlFromPath<T>(string xmlFilePath) where T : class
        //{
        //    string xml = File.ReadAllText(xmlFilePath);
        //    var reader = XmlReader.Create(xmlFilePath, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });

        //    return new XmlSerializer(typeof(T)).Deserialize(reader) as T;

        //}

        
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
