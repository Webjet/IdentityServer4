using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webjet.DotNet.Common
{
    /// <summary>
	/// See also DictionariesHelper.cs
	/// from http://serialization.codebetter.com/blogs/brendan.tompkins/archive/2005/03/01/56244.aspx
	/// </summary>
	/// <example>
	/// <code>
	///</code>
	/// </example> 
	public static class CollectionsHelper
    {
        /*When C# extensions will be available( promised in C# 3, add this keyword to parameter 
		 * public static bool IsNullOrEmpty(this ICollection c)
				//from http://diditwith.net/PermaLink,guid,b743c8bc-d40f-4e8c-bb3b-7a5af93db109.aspx
		 */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this ICollection c)
        {
            return (c == null || c.Count == 0);
        }
        public static bool IsNullOrEmptySequence<T>(this IEnumerable<T> c)
        {
            return (c == null || !c.Any());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myArr"> </param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this Object[] myArr)
        {
            return (myArr == null || myArr.Length == 0);
        }
        public static bool IsNullOrEmpty<T>(this T[] myArr)
        {
            return (myArr == null || myArr.Length == 0);
        }
        public static bool IsNullOrEmpty(this StringDictionary c)
        {
            return (c == null || c.Count == 0);
        }
        public static Object LastElement(Object[] myArr)
        {
            if (IsNullOrEmpty(myArr))
            {
                return null;
            }
            return myArr[myArr.Length - 1];
        }
        public static void AddIfNotNull<T>(this IList<T> coll, T newItem) where T : class
        {
            if (newItem != null)
            {
                coll.Add(newItem);
            }
        }

        public static void AddRangeIfNotNullOrEmpty<T>(this List<T> coll, IEnumerable<T> newItems) where T : class
        {
            if (!newItems.IsNullOrEmptySequence())
            {
                coll.AddRange(newItems);
            }
        }
        //created based on http://www.kirupa.com/net/removingDuplicates2.htm
        //Why it is not in .Net Framework yet? Why HashSet<T> is only in Orcas(http://blogs.msdn.com/bclteam/archive/2006/11/09/introducing-hashset-t-kim-hamilton.aspx)
        //Seems obsolete, use LINQ Distinct() instead
        public static List<GenericType> RemoveDuplicates<GenericType>(List<GenericType> inputList)
        {
            List<GenericType> finalList = new List<GenericType>();
            if (!IsNullOrEmpty(inputList))
            {
                Dictionary<GenericType, int> uniqueStore = new Dictionary<GenericType, int>();
                foreach (GenericType currValue in inputList)
                {
                    if (!uniqueStore.ContainsKey(currValue))
                    {
                        uniqueStore.Add(currValue, 0);
                        finalList.Add(currValue);
                    }
                }
            }
            return finalList;
        }
        //Why it is not in .Net Framework yet? Why HashSet<T> is only in Orcas(http://blogs.msdn.com/bclteam/archive/2006/11/09/introducing-hashset-t-kim-hamilton.aspx)
        public static bool AreValuesUnique<GenericType>(List<GenericType> inputList)
        {
            foreach (GenericType currValue in inputList)
            {
                if (inputList.IndexOf(currValue) != inputList.LastIndexOf(currValue))
                    return false;
            }
            return true;
        }
        //Why it is not in .Net Framework yet? Why HashSet<T> is only in Orcas(http://blogs.msdn.com/bclteam/archive/2006/11/09/introducing-hashset-t-kim-hamilton.aspx)
        public static List<GenericType> FindDuplicates<GenericType>(List<GenericType> inputList)
        {
            Dictionary<GenericType, int> uniqueStore = new Dictionary<GenericType, int>();
            List<GenericType> finalList = new List<GenericType>();

            foreach (GenericType currValue in inputList)
            {
                if (uniqueStore.ContainsKey(currValue))
                {
                    finalList.Add(currValue);
                }
                else
                {
                    uniqueStore.Add(currValue, 0);
                }
            }
            return finalList;
        }
        /// <summary>
        /// Returns new line separatad objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messages"></param>
        /// <returns></returns>
        /// <remarks>If you need detailed output,  <seealso cref="TraceOutputExtensions.ToXmlString"/> </remarks>
        public static string ToString<T>(this IEnumerable<T> messages)
        {//, string sComment
            return ToString<T>(messages, Environment.NewLine, "");
        }

        /// <summary>
        /// Type specific parameter is not required, consider to call EnumerableToString directly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messages"></param>
        /// <param name="separator"></param>
        /// <param name="sComment"></param>
        /// <returns></returns>
        /// See also http://www.codemeit.com/linq/c-array-delimited-tostring.html
        public static string ToString<T>(this IEnumerable<T> messages, string separator, string sComment)
        {
            return EnumerableToString(messages, separator, sComment);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="separator">By </param>
        /// <param name="sComment"></param>
        /// <returns></returns>
        public static string EnumerableToString(this IEnumerable items, string separator = null, string sComment = null)
        {
            if (items == null)
            {
                return ("null");
            }
            StringBuilder sb = new StringBuilder(sComment);
            if (items is string) //string is IEnumerable, so required special treatment
            {
                sb.Append(items as string);
                return sb.ToString();
            }

            if (string.IsNullOrEmpty(separator))
            {
                separator = Environment.NewLine;
            }
            foreach (var item in items)
            {
                if (item != null)
                {
                    string strItem = "";
                    try
                    {
                        strItem = item.ToString();
                    }
                    catch (Exception exc)
                    {
                        strItem = exc.ToString();
                    }
                    sb.Append(strItem);
                    sb.Append(separator);
                }
            }
            string sRet = sb.ToString().TrimEnd(separator);
            return sRet;
        }

        /// <summary>
        /// Convert each element of collection to string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static IEnumerable<string> ToStrings<T>(this IEnumerable<T> objects)
        {//from http://www.c-sharpcorner.com/Blogs/997/using-linq-to-convert-an-array-from-one-type-to-another.aspx
            return objects.Select(en => en.ToString());
        }


        /// <summary>
        /// May be the same as ToString<T>
        /// </summary>
        /// <param name="list"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string ToDelimitedString(this IEnumerable<string> list, string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in list)
            {
                if (sb.Length > 0)
                {
                    sb.Append(delimiter);
                }
                sb.Append(item);
            }
            return sb.ToString();
        }

        public static string ToCSVString<GenericType>(List<GenericType> inputList)
        {
            return inputList.ToString<GenericType>(",", "");
            //StringBuilder sb = new StringBuilder();
            //foreach (GenericType currValue in inputList)
            //{
            //    sb.AppendFormat("{0},",currValue.ToString());
            //}
            //return sb.ToString().TrimEnd(',');
        }
        public static List<string> CSVStringAsList(string sValue)
        {
            List<string> resultList = new List<string>();

            if (!string.IsNullOrEmpty(sValue))
            {
                string[] arrOfStrings = sValue.Split(',');
                List<string> list = new List<string>(arrOfStrings);
                list.ForEach(str => resultList.Add(str.Trim()));
            }

            return resultList;

        }

        public static List<GenericType> ArrayListToGenericList<GenericType>(ArrayList arrayList)
        {
            //From http://www.codeproject.com/Tips/68291/Convert-ArrayList-to-a-Generic-List
            var typedList = arrayList.OfType<GenericType>().ToList();
            return typedList;
        }
        /// <summary>
        /// Determines whether a System.Collections.Generic.HashSet<T> object is a subset of the specified collection.
        /// http://stackoverflow.com/questions/332973/linq-check-whether-an-array-is-a-subset-of-another
        /// </summary>
        /// <param name="coll1">Instance</param>
        /// <param name="coll2"> other should be bigger to return true </param>
        /// <returns></returns>
        public static bool IsSubsetOf<T>(this List<T> coll1, List<T> coll2)
        {
            bool isSubset = !coll1.Except(coll2).Any();
            return isSubset;
        }
    }// class CollectionsHelper
}
