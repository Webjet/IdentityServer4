#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualBasic;

#endregion
namespace Webjet.DotNet.Common.Strings
{

    public static class StringHelper
    {
        // See also Westwind.Utilities.StringUtils	
        //'See also FxLib Author: Kamal Patel, Rick Hodder
        //	'Find the first entry of sToFind and returns the string after it
        //	'See also FxLib StringExtract (and StuffString)
        // Methods
        /// <summary>
        /// From http://thomasfreudenberg.com/blog/archive/2008/01/25/string-isnullorempty-as-extension-method.aspx
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this String s)
        {
            return String.IsNullOrEmpty(s);
        }
        /// <summary>
        /// From http://stackoverflow.com/questions/647643/string-isnullorblank-extension-method
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNullOrBlank(this String text)
        {
            return text == null || text.Trim().Length == 0;
        }
        public static string IfNullSetEmpty(this string value)
        {
            if (value == null)
            {
                return "";
            }
            return value;
        }
        /// <summary>
        /// From http://stackoverflow.com/questions/7438957/check-if-string-is-empty-or-all-spaces-in-c-sharp
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmptyOrWhiteSpace(this string value)
        {
            return value.All(char.IsWhiteSpace);
        }
        #region "Find/Replace Functions"
        public static string LeftBefore(this string str, string sToFind)
        {
            return LeftBefore(str, sToFind, false);
        }
        //		'if sToFind not found, then the full string should be returned 
        //		'if sToFind is empty, all string should be returned
        public static string LeftBefore(this string str, string sToFind, bool EmptyIfNotFound)
        {
            StringBuilder builder1 = new StringBuilder(str);
            if (sToFind.Length > 0)
            {
                int num1 = str.IndexOf(sToFind);
                if (num1 < 0)
                {
                    if (EmptyIfNotFound == true) return "";
                    else return str;// 6/7/2005 full string should be returned
                }
                builder1.Remove(num1, builder1.Length - num1);
            }
            return builder1.ToString();
        }
        //		'if sToFind not found, then original string should be returned
        public static string LeftBeforeLast(this string str, string sToFind)
        {
            StringBuilder builder1 = new StringBuilder(str);
            if (sToFind.Length > 0)
            {
                int num1 = str.LastIndexOf(sToFind);
                if (num1 < 0)
                {
                    return str;
                }
                builder1.Remove(num1, builder1.Length - num1);
            }
            return builder1.ToString();
        }
        public static string LeftBeforeLast(this string str, string sToFind, int FindNumberTimes)//6/1/2006
        {
            for (int i = 0; i < FindNumberTimes; i++)
            {
                str = LeftBeforeLast(str, sToFind);
            }
            return str;
        }

        //		'if sBefore not found, then string from the beginning should be returned
        //		'if sAfter not found, then string up to the end should be returned
        public static string MidBetween(this string str, string sBefore, string sAfter)
        {
            return MidBetween(str, sBefore, sAfter, false);
        }
        public static string MidBetween(this string str, string sBefore, string sAfter, bool EmptyIfNotFound)
        {
            string text2 = RightAfter(str, sBefore, EmptyIfNotFound);
            return LeftBefore(text2, sAfter, EmptyIfNotFound);
        }

        /// <summary>
        ///if sToFind not found, then original string should be returned 
        ///if sToFind is empty, all string should be returned
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sToFind"></param>
        public static string RightAfter(this string str, string sToFind)
        {
            return RightAfter(str, sToFind, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sToFind"></param>
        /// <param name="EmptyIfNotFound"></param>
        /// <returns></returns>
        public static string RightAfter(this string str, string sToFind, bool EmptyIfNotFound)
        {
            StringBuilder builder1 = new StringBuilder(str);
            int num1 = str.IndexOf(sToFind);
            if (num1 < 0)
            {
                if (EmptyIfNotFound == true) return "";
                else return str;
            }
            builder1.Remove(0, num1 + sToFind.Length);
            return builder1.ToString();
        }

        public static string RightSubstringOrEmpty(this string thisString, int length)
        {
            if (String.IsNullOrEmpty(thisString))
            {
                return String.Empty;
            }

            int indexForSubstring = (thisString.Length - length < 0) ? 0 : thisString.Length - length;

            return thisString.Substring(indexForSubstring);
        }

        //
        /// <summary>
        ///if sToFind not found, then original string should be returned 
        /// Otherwise removeBefore
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sToFind"></param>
        /// <returns></returns>
        public static string RemoveBefore(this string str, string sToFind)
        {
            int num1 = str.IndexOf(sToFind);
            if (num1 > 0)
            {
                return str.Remove(0, num1);
            }
            else
            {
                return str;
            }

        }

        //'Find the last entry of sToFind and returns the string after it
        //if sToFind not found, then original string should be returned ' 6/7/2005 change
        //if sToFind is empty, the original string should be returned
        public static string RightAfterLast(this string str, string sToFind)
        {
            StringBuilder builder1 = new StringBuilder(str);
            int num1 = str.LastIndexOf(sToFind);
            if (num1 < 0)
            {
                return str;
            }
            builder1.Remove(0, num1 + sToFind.Length);
            return builder1.ToString();
        }
        public static string RightAfterLast(this string str, char chToFind, int nOccurencesNumber)//6/1/2006
        { //C++ solutions to find n'th occurence see http://www.codecomments.com/forum272/message731385.html 
            string[] aStr = str.Split(chToFind);
            StringBuilder sb = new StringBuilder();
            int nPosInSplit = aStr.Length - nOccurencesNumber;
            for (int i = nPosInSplit; i < aStr.Length; i++)
            {
                sb.Append(aStr[i]);
                sb.Append(chToFind);
            }
            sb.Remove(sb.Length - 1, 1);//1 for char
            return sb.ToString();
        }
        //'Removes the start part of the string, if it is matchs, otherwise leave string unchanged
        public static string TrimStart(this string str, string sStartValue)
        {
            if (!String.IsNullOrWhiteSpace(str) && str.StartsWith(sStartValue))
            {
                str = str.Remove(0, sStartValue.Length);
            }
            return str;
        }
        //		'Removes the end part of the string, if it is matchs, otherwise leave string unchanged
        public static string TrimEnd(this string str, string sEndValue, bool ignoreCase = true)
        {
            if (str == null) return str;
            if (str.EndsWith(sEndValue, ignoreCase, CultureInfo.CurrentCulture))
            {
                str = str.Remove(str.Length - sEndValue.Length, sEndValue.Length);
            }
            return str;
        }

        /// <summary>
        /// If lenght of the string is greater than max allowed, remove the end
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string TrimLength(this string str, int maxLength)
        {
            if (str == null)
            {
                return str;
            }
            if (str.Length > maxLength)
            {
                str = str.Remove(maxLength);
            }
            return str;
        }
        //from http://mennan.kagitkalem.com/CommentView,guid,d8e01e32-49f3-4450-994a-990c4fa0a437.aspx 
        //use the most efficient
        public static int OccurencesCount(string str, string sToFind)
        {
            string copyOrginal = String.Copy(str);
            int place = 0;
            int numberOfOccurances = 0;
            place = copyOrginal.IndexOf(sToFind, StringComparison.Ordinal);
            while (place != -1)
            {
                copyOrginal = copyOrginal.Substring(place + 1);
                place = copyOrginal.IndexOf(sToFind, StringComparison.Ordinal);
                numberOfOccurances++;
            }
            return numberOfOccurances;
        }



        /// <summary>
        /// Replaces the string instances.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="instanceStartFrom">The instance start from 0-based.</param>
        /// <param name="numberOfInstances">The number of instances (1 or more)</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <returns></returns>
        static public string ReplaceStringInstances(string original, string pattern, string replacement, int instanceStartFrom, int numberOfInstances, StringComparison comparisonType)//, int stringBuilderInitialSize)
        {
            //    based on http://www.west-wind.com/weblog/posts/2007/Apr/30/How-many-ways-to-do-a-String-Replace and http://www.codeproject.com/cs/samples/fastestcscaseinsstringrep.asp?msg=1835929#xx1835929xx
            if (original.IsNullOrEmpty()) return original;
            if (String.IsNullOrEmpty(pattern)) return original;
            if (replacement == null) return original;
            if (numberOfInstances <= 0) return original;
            if (instanceStartFrom < 0) instanceStartFrom = 0;

            int posCurrent = 0;
            int lenPattern = pattern.Length;
            int idxNext = original.IndexOf(pattern, comparisonType);
            StringBuilder result = new StringBuilder(Math.Min(4096, original.Length));

            int instanceLastNumberExcluded = instanceStartFrom + numberOfInstances;
            int instance = 0;

            while (idxNext >= 0)
            {
                result.Append(original, posCurrent, idxNext - posCurrent);

                if (instance < instanceStartFrom)
                {
                    result.Append(pattern);
                    posCurrent = idxNext + lenPattern;
                }
                else if (instance < instanceLastNumberExcluded)
                {
                    result.Append(replacement);
                    posCurrent = idxNext + lenPattern;
                }
                idxNext = original.IndexOf(pattern, posCurrent, comparisonType);
                instance++;
                if (instance >= instanceLastNumberExcluded)
                {
                    break;
                }
            }

            result.Append(original, posCurrent, original.Length - posCurrent);

            return result.ToString();
        }
        ///// <summary>
        ///// String replace function that support
        ///// </summary>
        ///// <param name="origString">Original input string</param>
        ///// <param name="findString">The string that is to be replaced</param>
        ///// <param name="replaceWith">The replacement string</param>
        ///// <param name="instanceStartFrom">The instance start from.</param>
        ///// <param name="numberOfInstances">The number of instances.</param>
        ///// <param name="comparisonType">Type of the comparison.</param>
        ///// <returns>
        ///// updated string or original string if no matches
        ///// </returns>
        //public static string ReplaceStringInstances(string origString, string findString,
        //                                            string replaceWith, int instanceStartFrom,
        //                                            int numberOfInstances, StringComparison comparisonType)
        //{



        //case-incensitive replace from http://www.codeproject.com/cs/samples/fastestcscaseinsstringrep.asp?msg=1835929#xx1835929xx
        static public string Replace(this string original, string pattern, string replacement, StringComparison comparisonType)
        {
            //TODO: consider to use http://stackoverflow.com/questions/6275980/string-replace-ignoring-case instead
            if (original == null)
            {
                return null;
            }

            if (String.IsNullOrEmpty(pattern))
            {
                return original;
            }

            int lenPattern = pattern.Length;
            int idxPattern = -1;
            int idxLast = 0;

            StringBuilder result = new StringBuilder();

            while (true)
            {
                idxPattern = original.IndexOf(pattern, idxPattern + 1, comparisonType);

                if (idxPattern < 0)
                {
                    result.Append(original, idxLast, original.Length - idxLast);

                    break;
                }

                result.Append(original, idxLast, idxPattern - idxLast);
                result.Append(replacement);

                idxLast = idxPattern + lenPattern;
            }

            return result.ToString();
        }
        /// <summary>
        /// Uses regex '\b' as suggested in //http://stackoverflow.com/questions/6143642/way-to-have-string-replace-only-hit-whole-words
        /// </summary>
        /// <param name="original"></param>
        /// <param name="wordToFind"></param>
        /// <param name="replacement"></param>
        /// <param name="regexOptions">e.g. RegexOptions.IgnoreCase</param>
        /// <returns></returns>
        static public string ReplaceWholeWord(this string original, string wordToFind, string replacement, RegexOptions regexOptions = RegexOptions.None)
        {

            string pattern = String.Format(@"\b{0}\b", wordToFind);
            string ret = Regex.Replace(original, pattern, replacement, regexOptions);
            return ret;
        }
        /// <summary>
        /// Find the last entry of sToFind and replace it with sToReplace string 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sToFind">if sToFind not found, then original string should be returned.
        /// if sToFind is empty, the original string should be returned</param>
        /// <param name="sToReplace"></param>
        /// <returns></returns>
        public static string ReplaceLast(this string str, string sToFind, string sToReplace)
        {
            StringBuilder builder1 = new StringBuilder(str);
            int num1 = str.LastIndexOf(sToFind);
            if (num1 < 0)
            {
                return str;
            }
            builder1.Replace(sToFind, sToReplace, num1, sToFind.Length);
            return builder1.ToString();
        }
        public static string IfNotEmptyEnsureEndsWith(string str, string sEndValue)
        {
            if (String.IsNullOrEmpty(str)) return str; //21/10/2005
            if (!str.EndsWith(sEndValue))
            {
                str = str + sEndValue;
            }
            return str;
        }
        public static string EnsureEndsWith(this string str, string sEndValue, bool ignoreCase = true)
        {
            if (!str.EndsWith(sEndValue, ignoreCase, CultureInfo.InvariantCulture))
            {
                str = str + sEndValue;
            }
            return str;
        }
        //converted from http://stackoverflow.com/questions/1250514/find-length-of-initial-segment-matching-mask-on-arrays
        public static string LongestCommonPrefix(string str1, string str2)
        {
            int minLen = Math.Min(str1.Length, str2.Length);
            for (int i = 0; i < minLen; i++)
            {
                if (str1[i] != str2[i])
                {
                    return str1.Substring(0, i);
                }
            }
            return str1.Substring(0, minLen);
        }
        /// <summary>
        ///  Adds Prefix, if it is not exist in the string, case sensitive
        /// </summary>
        /// <param name="str">if null, returns prefix</param>
        /// <param name="sPrefix">if null or empty, returns original string</param>
        /// <returns></returns>
        public static string EnsureStartsWith(this string str, string sPrefix)
        {
            if (str == null)
            { //throw new ArgumentNullException("str"); 
                return sPrefix;
            }
            if (!String.IsNullOrEmpty(sPrefix))
            {
                if (!str.StartsWith(sPrefix))
                {
                    str = sPrefix + str;
                }
            }
            return str;
        }
        public static string AppendWithDelimeter(string str, string sToAppend, string delimeter)
        {
            if ((!str.EndsWith(delimeter) & !String.IsNullOrEmpty(str)) & !String.IsNullOrEmpty(sToAppend))
            {
                str = str + delimeter;
            }
            str = str + sToAppend;
            return str;
        }
        public static string AppendIfNotContains(string str, string sToAppend, string delimeter)
        {
            if (!str.Contains(sToAppend))
            {
                str = AppendWithDelimeter(str, sToAppend, delimeter);
            }
            return str;
        }

        public static string AppendIfNotEmpty(this string str, string prefixBeforeAppend, string valueToAppend, string suffixAfterAppend = "")
        {
            if (!String.IsNullOrEmpty(valueToAppend))
            {
                str += prefixBeforeAppend + valueToAppend + suffixAfterAppend;
            }
            return str;
        }
        // TODO : Need to move this function to Microsoft\SDC\Common\StringHelper.cs
        public static string AppendIfObjectIsNull(this string errorMsg, object obj, string mssageToAppend)
        {
            if (obj == null)
            {
                errorMsg += mssageToAppend;
            }

            return errorMsg;
        }
        //from http://66.102.7.104/search?q=cache:DSw2bnf_FlMJ:blogs.msdn.com/brada/archive/2004/02/16/73535.aspx+%22EndsWith+char+%22+string+C%23&hl=en
        //" internal bool EndsWith(char value) in String class. Why it would be internal? Also, there is no bool StartsWith(char value). "
        public static bool EndsWith(string str, char value)
        {
            int num1 = str.Length;
            if ((num1 != 0) && (str[num1 - 1] == value))
            {
                return true;
            }
            return false;
        }
        public static bool StartsWith(string str, char value)
        {
            if ((str.Length != 0) && (str[0] == value))
            {
                return true;
            }
            return false;
        }
        #region Case conversions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <remarks>
        /// from  http://west-wind.com/weblog/posts/361.aspx
        /// use TextInfo.ToTitleCase(mText.ToLower());
        /// Alternative see From http://aspcode.net/propercase-function-in-c/ 
        ///	</remarks>
        public static string ToTitleCase(this string input)
        {
            input = input.ToLower();
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(input);

        }
        public static string ToCamelCase(this string s)
        {
            var sb = new StringBuilder();
            char[] ca = s.ToLower().ToCharArray();
            for (int i = 0; i < ca.Length; i++)
            {
                char c = ca[i];
                if (i == 0 || Char.IsSeparator(ca[i - 1]))
                {
                    c = Char.ToUpper(c);
                }
                sb.Append(c);
            }

            return sb.ToString();
        }
        /// <summary>
        /// Convert string from pascal case to human readable string
        /// <example>pascalCaseExample => Pascal Case Example</example>
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>human readable string</returns>
        public static string ToHumanFromPascal(string s)
        {
            //TODO:See similar 
            //            http://www.mehdi-khalili.com/bdd-simply-with-bddify
            //public static string CreateSentenceFromCamelName(string name)
            //    {
            //        return Regex.Replace(name, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
            //    }

            StringBuilder sb = new StringBuilder();
            char[] ca = s.ToCharArray();
            sb.Append(ca[0]);
            for (int i = 1; i < ca.Length - 1; i++)
            {
                char c = ca[i];
                if (Char.IsUpper(c) && (Char.IsLower(ca[i + 1]) || Char.IsLower(ca[i - 1])))
                {
                    sb.Append(" ");
                }
                sb.Append(c);
            }
            sb.Append(ca[ca.Length - 1]);

            return sb.ToString();
        }

        #endregion //Case conversions
        //Alternatively see http://weblogs.asp.net/sushilasb/archive/2006/08/03/How-to-extract-numbers-from-string.aspx
        public static string GetStartingNumericFromString(string itmName)
        {
            string safeNumericString = "";
            foreach (char s in itmName)
            {
                if (s.CompareTo('0') < 0 || s.CompareTo('9') > 0)
                {
                    break;
                }
                safeNumericString += s.ToString();
            }
            return safeNumericString;
        }

        public static string GetEndingNumericFromString(string itmName)
        {
            Regex regex = new Regex(@"[$](?<Amount>[\d.]+)[\s\S]*?");
            Match match = regex.Match(itmName);
            if (match.Success)
            {
                itmName = match.Groups["Amount"].Value;
                return itmName;
            }

            return itmName;

        }
        /// <summary>
        /// method for removing all whitespace from a given string
        /// </summary>
        /// <param name="str">string to strip</param>
        /// <returns></returns>
        public static string RemoveAllWhitespace(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                Regex reg = new Regex(@"\s*");
                str = reg.Replace(str, "");
            }
            return str;
        }

        /*From http://bytes.com/topic/c-sharp/answers/253519-using-regex-create-sqls-like-like-function
         * Ex:
*
* bool isMatch =
* IsSqlLikeMatch("abcdef", "[az]_%[^qz]ef");
*
* should return true.
*/
        /// <summary>
        /// Note that it could be very serious performance hit, if the pattern is started with %.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool IsSqlLikeMatch(this string input, string pattern)
        {
            /* Turn "off" all regular expression related syntax in
            * the pattern string. */
            pattern = Regex.Escape(pattern);

            /* Replace the SQL LIKE wildcard metacharacters with the
            * equivalent regular expression metacharacters. */
            pattern = pattern.Replace("%", ".*?").Replace("_", ".");

            /* The previous call to Regex.Escape actually turned off
            * too many metacharacters, i.e. those which are recognized by
            * both the regular expression engine and the SQL LIKE
            * statement ([...] and [^...]). Those metacharacters have
            * to be manually unescaped here. */
            pattern = pattern.Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\^", "^");

            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        /// <summary>
        /// Determines whether [contains] [the specified source] with string comparison.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="toCheck">To check.</param>
        /// <param name="comp">The comp.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified source]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {   //From http://stackoverflow.com/questions/444798/case-insensitive-containsstring/444818#444818
            return source.IndexOf(toCheck, comp) >= 0;
        }


        #endregion //"String Functions"

        #region "String Array Functions"
        //TODO: move to 
        public static string[] ToLower(string[] sArray)
        {
            for (int i = 0; i < sArray.Length; i++)
            {
                sArray[i] = sArray[i].ToLower();
            }
            return sArray;
        }
        //see also a few methods in http://www.codeproject.com/csharp/StringBuilder_vs_String.asp 
        public static string Join(string separator, params string[] list)
        {
            return String.Join(separator, list);
        }
        #endregion //"String Array Functions"


        #region "String Brackets Functions"
        //		
        /// <summary>
        /// 'StripBrackets checks that starts from sStart and ends with sEnd (case sensitive).
        ///		'If yes, than removes sStart and sEnd.
        ///		'Otherwise returns full string unchanges
        ///		'See also MidBetween
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sStart"></param>
        /// <param name="sEnd"></param>
        /// <returns></returns>
        public static string StripBrackets(this string str, string sStart, string sEnd)
        {
            if (CheckBrackets(str, sStart, sEnd))
            {
                str = str.Substring(sStart.Length, (str.Length - sStart.Length) - sEnd.Length);
            }
            return str;
        }
        public static bool CheckBrackets(string str, string sStart, string sEnd)
        {
            bool flag1 = (str != null) && (str.StartsWith(sStart) && str.EndsWith(sEnd));
            return flag1;
        }

        public static string WrapBrackets(string str, string sStartBracket, string sEndBracket)
        {
            StringBuilder builder1 = new StringBuilder(sStartBracket);
            builder1.Append(str);
            builder1.Append(sEndBracket);
            return builder1.ToString();
        }
        //    'Concatenates a specified separator String between each element of a specified String array wrapping each element, yielding a single concatenated string
        public static string JoinWrapBrackets(string[] aStr, string sDelimeter, string sStartBracket, string sEndBracket)
        {
            StringBuilder builder1 = new StringBuilder();
            string[] textArray1 = aStr;
            for (int num1 = 0; num1 < textArray1.Length; num1++)
            {
                string text2 = textArray1[num1];
                builder1.Append(WrapBrackets(text2, sStartBracket, sEndBracket));
                builder1.Append(sDelimeter);
            }
            return TrimEnd(builder1.ToString(), sDelimeter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisString"></param>
        /// <param name="openTag"></param>
        /// <param name="closeTag"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        /// <example>
        /// 	// mask <AccountNumber>XXXXX4488</AccountNumber>
        ///requestAsString  = requestAsString.ReplaceBetweenTags("<AccountNumber>", "</AccountNumber>", CreditCard.MaskedCardNumber);
        ///mask cvv
        ///requestAsString = requestAsString.ReplaceBetweenTags("<FieldName>CC::VerificationCode</FieldName><FieldValue>", "</FieldValue>", cvv=>"XXX");
        /// </example>
        public static string ReplaceBetweenTags(this string thisString, string openTag, string closeTag, Func<string, string> transform)
        {
            //See also http://stackoverflow.com/questions/1359412/c-sharp-remove-text-in-between-delimiters-in-a-string-regex
            string sRet = thisString;
            string between = thisString.MidBetween(openTag, closeTag, true);
            if (!String.IsNullOrEmpty(between))
                sRet = thisString.Replace(openTag + between + closeTag, openTag + transform(between) + closeTag);
            return sRet;
        }
        public static string ReplaceBetweenTags(this string thisString, string openTag, string closeTag, string newValue)
        {
            //See also http://stackoverflow.com/questions/1359412/c-sharp-remove-text-in-between-delimiters-in-a-string-regex
            string sRet = thisString;
            string between = thisString.MidBetween(openTag, closeTag, true);
            if (!String.IsNullOrEmpty(between))
                sRet = thisString.Replace(openTag + between + closeTag, openTag + newValue + closeTag);
            return sRet;
        }

        //		' Quote the arguments, in case they have a space in them.
        public static string QuotePath(string sPath)
        {
            return ("\"" + sPath + "\"");
        }
        public static string DblQuoted(string sWord)
        {
            sWord = Microsoft.VisualBasic.Strings.Replace(sWord, "\"", "\"\"", 1, -1, CompareMethod.Binary);
            return ("\"" + sWord + "\"");
        }
        #endregion //"String Brackets Functions"

        /// <summary>
        /// Returns true, if  string contains any of substring from the list (case insensitive)
        /// See similar (with SqlLikeMatch support) in ResponseMessagePatternsCache
        /// </summary>
        /// <returns></returns>
        public static bool IsStringContainsAnyFromList(this string stringToSearch, List<String> stringsToFind)
        {
            //TODO: create overloads with exact match  or case sencitive
            if (stringsToFind.IsNullOrEmpty()) { return false; }
            else
            {
                stringToSearch = stringToSearch.ToUpper();
                return stringsToFind.Any(pattern => stringToSearch.Contains(pattern.ToUpper()));
            }
        }

        /// <summary>
        /// http://stackoverflow.com/questions/2420125/coalesce-for-empty-string
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        /// <example>string result = Coalesce(s.SiteNumber, s.AltSiteNumber, "No Number");</example>
        public static string Coalesce(params string[] strings)
        {
            return strings.FirstOrDefault(s => !String.IsNullOrEmpty(s));
        }

        /// <summary>
        /// Removes the diacritics.
        /// This will replace non english characters to english equivalent
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this string text)
        {
            string ret = text;
            if (text != null)
            {
                ret =
                    String.Concat(
                        text.Normalize(NormalizationForm.FormD)
                           .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark))
                          .Normalize(NormalizationForm.FormC);
            }
            return ret;
        }


    }
}
