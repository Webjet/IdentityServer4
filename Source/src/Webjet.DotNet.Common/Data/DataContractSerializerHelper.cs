using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Webjet.DotNet.Common
{
    public class DataContractSerializerHelper
    {
        public static T DeepCopy<T>(T src)
        {
            T oClone;
            var dcs = new DataContractSerializer(typeof(T));

            using (var ms = new MemoryStream())
            {
                dcs.WriteObject(ms, src);
                ms.Position = 0;
                oClone = (T)dcs.ReadObject(ms);
            }

            return oClone;
        }
    }
}
