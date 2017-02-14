using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace WebFormsOpenIdConnectAzureAD.AAD.Common
{
    public static class StringsCollectionExtensions
    {
        public static IEnumerable<string> TrimEnds(this IEnumerable<string> collection, string sEndValue, bool ignoreCase = true)
        {
            var retCollection = collection.Select(s => s.TrimEnd(sEndValue, ignoreCase));
            return retCollection;
        }
     //from   C:\GitRepos\TSA\main\Microsoft\SDC\Common\Strings\StringHelper.cs
    //		'Removes the end part of the string, if it is matchs, otherwise leave string unchanged
    public static string TrimEnd(this string str, string sEndValue,bool ignoreCase=true)
    {
        if (str == null) { throw new NullReferenceException("str is null"); }
        if (str.EndsWith(sEndValue, ignoreCase, CultureInfo.CurrentCulture))
        {
            str = str.Remove(str.Length - sEndValue.Length, sEndValue.Length);
        }
        return str;
    }
}
}