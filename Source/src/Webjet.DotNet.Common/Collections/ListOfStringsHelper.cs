#region  Namespace Imports

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Webjet.DotNet.Common.Strings;
#endregion  //Namespace Imports

namespace Webjet.DotNet.Common.Collections
{
//    using System.Collections;
    //See also StringArrayHelper.cs, CollectionsHelper.cs
    public static class ListOfStringsHelper
    {
        /// <summary>
        /// TODO: See also StringHelper.IsStringContainsAnyFromList -is it the same
        /// </summary>
        /// <param name="patternsList"></param>
        /// <param name="sMsg"></param>
        /// <returns></returns>
        public static bool StringContainsAnyFromList(List<string> patternsList, string sMsg)
        {
            bool bFound = patternsList.Exists(
                sMsg.Contains
                );
            return bFound;
        }
        public static string FindFirstPatternContainedInString(List<string> patternsList, string sMsg)
        {
            string sFound = patternsList.Find(
                sMsg.Contains
               );
            return sFound;
        }
        /// <summary>
        /// If no patters match the string, return empty
        /// </summary>
        /// <param name="patternsList"></param>
        /// <param name="sMsg"></param>
        /// <returns></returns>
        public static string FindFirstPatternContainedInString(string[] patternsList, string sMsg)
        {
            foreach (string pattern in patternsList)
            {
                if (sMsg.Contains(pattern))
                {
                   // Debug.Assert(false, "Investigate why not?.");
                    // bValid = false;
                    return pattern;
                }

                break;
            }
            return "";
        }
		public static bool ContainsString(this IEnumerable<string> collection, string toFind, bool ignoreCase = true,bool trimStrings=false)
		{
		    if (trimStrings)
		    {
		        collection = collection.Select(s => s.Trim());
		        toFind = toFind.Trim();
		    }
			return collection.Contains(toFind, StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase));
		}


    	/// <summary>
    	/// Returns true, if any string in list contains the substring(case insensitive)
    	/// </summary>
    	/// <param name="list"></param>
    	/// <param name="substringToFind"></param>
    	/// <returns></returns>
    	/// <remarks><seealso cref="FindFirstContainingSubstring" /></remarks>
    	public static bool IsListContainsSubstring(this IEnumerable<string> list, string substringToFind)
    	{
    		//TODO: create overloads with exact match  or case sencitive
    		list = list.ToList();
    		if (list.IsNullOrEmptySequence())
    		{ return false; }
    		else
    		{
    			substringToFind = substringToFind.ToUpper();
    			return list.Any(remark => remark.ToUpper().Contains(substringToFind));
    		}
    	}

    	/// <summary>
    	/// Returns first string in list,that contains the substring(case insensitive)
    	/// </summary>
    	/// <param name="list"></param>
    	/// <param name="substringToFind"></param>
    	/// <returns>NullOrEmpty if not found</returns>
    	public static string FindFirstContainingSubstring(this List<string> list, string substringToFind)
    	{
    		//TODO: create overloads with exact match  or case sencitive
    		if (list.IsNullOrEmpty())
    		{ return ""; }
    		else
    		{
    			substringToFind = substringToFind.ToUpper();
    			return list.FirstOrDefault(remark => remark.ToUpper().Contains(substringToFind));
    		}
    	}

    	/// <summary>
    	/// Returns first string in list,that StartsWith the substring(case insensitive)
    	/// </summary>
    	/// <param name="list"></param>
    	/// <param name="substringToFind"></param>
    	/// <returns>NullOrEmpty if not found</returns>
    	public static string FindFirstStartingWithSubstring(this List<string> list, string substringToFind)
    	{
    		//TODO: create overloads with exact match  or case sencitive
    		if (list.IsNullOrEmpty())
    		{ return ""; }
    		else
    		{
    			substringToFind = substringToFind.ToUpper();
    			return list.FirstOrDefault(remark => remark.ToUpper().StartsWith(substringToFind));
    		}
    	}
        public static IEnumerable<string> TrimEnds(this IEnumerable<string> collection, string sEndValue, bool ignoreCase = true)
        {
            var retCollection = collection.Select(s => s.TrimEnd(sEndValue, ignoreCase));
            return retCollection;
        }
        public static string ToCSVString(this List<String> inputList)
    	{
    		return inputList.ToString<string>(",", "");
    		//string [] arrStrs=inputList.ToArray();
    		//return string.Join(",", arrStrs);
    	}
        public static IList<string> MergeWith(this IList<string> source, IList<string> listToMerge, string joinerString)
        {
            if (source == null && listToMerge != null)
                return listToMerge;

            if (listToMerge == null)
                return source;

            var minIndex = Math.Min(source.Count, listToMerge.Count);

            var returnValue = source;

            for (var i = 0; i < minIndex; i++)
            {
                returnValue[i] = string.Join(joinerString, new[] { source[i], listToMerge[i] }.Where(x => !string.IsNullOrEmpty(x)));
            }

            if (listToMerge.Count > source.Count)
            {
                for (var i = minIndex; i < listToMerge.Count; i++)
                {
                    returnValue.Add(listToMerge[i]);
                }
            }
            return returnValue;
        }
    }//end of class
}
