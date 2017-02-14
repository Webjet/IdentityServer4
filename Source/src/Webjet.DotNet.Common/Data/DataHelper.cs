using System;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using VBStrings=Microsoft.VisualBasic.Strings;

namespace Webjet.DotNet.Common.Data
{
    public static class DataHelper
    {
        #region "DbNull,Null and Empty"
        //		'TODO there are too many similar function 'review and re-factor them
        //'Generic method to set not NULL values, similar to SQL Coalesce
        //It is similar to SafeRef class, but also consider DBNull
        //USE ToString_EmptyIfNull if convert to String is required
        //For VBStrings see StringHelper.Coalesce
        public static object Nz(object oItemValue, object oDefault)
        {
            if (oItemValue == null)
            {
                return oDefault;
            }
            if (Information.IsDBNull(oItemValue))
            {
                return oDefault;
            }
            //'TODO add DateTime = Nothing as in IsDBNullOrEmpty
            return oItemValue;
        }
        /// <summary>
        /// Method similar to Nz, but empty Strings also replaced with Default
        /// </summary>
        /// <param name="oItemValue"></param>
        /// <param name="oDefault"></param>
        /// <returns></returns>
        public static object DefaultIfDBNullOrEmpty(object oItemValue, object oDefault)
        {
            if (DataHelper.IsDBNullOrEmpty(oItemValue))
            {
                return oDefault;
            }
            return oItemValue;
        }
        /// <summary>
        /// returns true if DBNull Or Empty String or nothing
        /// </summary>
        /// <param name="oItemValue"></param>
        /// <returns></returns>

        public static bool IsDBNullOrEmpty(object oItemValue)
        {
            bool bRet = false;
            if (oItemValue == null)
            {
                return true;
            }
            if (Information.IsDBNull(oItemValue))
            {
                return true;
            }
            if (oItemValue.GetType() == typeof(System.DateTime))
            {
                if (ObjectType.ObjTst(oItemValue, null, false) == 0)
                {
                    bRet = true;
                }
                return bRet;
            }
            if (StringType.StrCmp(oItemValue.ToString(), "", false) == 0)
            {
                bRet = true;
            }
            return bRet;
        }
        public static bool IsEmpty(DateTime dt)
        {
            return (dt == DateTime.MinValue);
        }
        public static bool SetDBNullIfEmpty(ref object LValue)
        {
            bool bRet = false;
            if (DataHelper.IsDBNullOrEmpty(LValue))
            {
                LValue = DBNull.Value;
                bRet = true;
            }
            return bRet;
        }
        /// <summary>
        /// Use instead of Nz to return string
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static string ToString_EmptyIfNull(object Value)
        {
            if (DataHelper.IsDBNullOrEmpty(Value))
            {
                return "";
            }
            return Value.ToString();
        }
        public static object DBNullIfEmpty(object Value)
        {
            //bool bRet = false;
            if (DataHelper.IsDBNullOrEmpty(Value))
            {
                Value = DBNull.Value;
            }
            return Value;
        }
        public static object NothingIfEmpty(object Value)
        {
            //bool bRet = false;
            if (DataHelper.IsDBNullOrEmpty(Value))
            {
                Value = null;
            }
            return Value;
        }
        public static string GetNullIfStringEmpty(string str)
        {
            if (str == "")
                return null;
            else return str;
        }


        public static bool SetIfNotEmpty(ref object LValue, object Value)
        {
            DebugOutputHelper.TracedLine("not sure that it returns valid ref");
            bool bRet = false;
            if (!DataHelper.IsDBNullOrEmpty(Value))
            {
                LValue = Value;
                bRet = true;
            }
            return bRet;
        }
        //(Guid)TypeDescriptor.GetConverter(typeof(Guid)).ConvertFromString(sClassId) doesn't convert empty string
        public static Guid GetGUIDOrNullIfEmpty(string Value)
        {
            Guid oRet = new Guid();// default guid struct
            if (!DataHelper.IsDBNullOrEmpty(Value))
            {
                //			   if (Value is Guid) 
                //				   return Value;
                //			   else
                {
                    oRet = new Guid(Value);
                }
            }
            return oRet;
        }
        public static string StringFromObject(object Value)
        {
            string sRet = null;
            if (Value != null)
            {
                //special case for Guid because is is not supported by Microsoft.VisualBasic.CompilerServices.StringType.FromObject 
                if (Value.GetType() == typeof(Guid))
                    sRet = Value.ToString();
                else
                    sRet = StringType.FromObject(Value);
            }
            return sRet;
        }
        /// <summary>
        /// from http://smellegantcode.wordpress.com/2008/12/11/the-maybe-monad-in-c/
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="v"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        /// <example>return Music.GetCompany("4ad.com").IfNotNull(company => company.GetBand("Pixies")).IfNotNull(band => band.GetMember("David"))</example>
        public static TOut IfNotNull<TIn, TOut>(this TIn v, Func<TIn, TOut> f) where TIn : class where TOut : class
        {
            if (v == null)
                return null;
            return f(v);
        }
        #endregion "DbNull,Nothing and Empty"

        //import manually as in http://lists.ximian.com/pipermail/mono-patches/2004-March/031645.html 

        #region   " SQL String Manipulations "


        public static string Quoted(char arg)
        {
            return DataHelper.Quoted(StringType.FromChar(arg));
        }
        //		'To use in SQL queries
        public static string Quoted(string arg)
        {
            return ("'" + VBStrings.Replace(arg, "'", "''", 1, -1, CompareMethod.Binary) + "'");
        }
        //    'Used in some cases when Quoted is not appropriate
        public static string EscapeSQL(string arg)
        {
            return VBStrings.Replace(arg, "'", "''", 1, -1, CompareMethod.Binary);
        }
        //		'Should be called inside Quoted, eg " LIKE " + Quoted(SafeLikeLiteral(sInput) + "%")
        public static string SafeLikeLiteral(string inputSQL)
        {
            //        ' Make the following replacements:
            //        ' [  becomes  [[]
            //        ' %  becomes  [%]
            //        ' _  becomes  [_]
            //        ' s = inputSQL.Replace("'", "''")' ' '  becomes  ''
            string text1 = inputSQL;
            text1 = text1.Replace("[", "[[]");
            text1 = text1.Replace("%", "[%]");
            return text1.Replace("_", "[_]");
        }
        //		'Sample code
        //		'   sWhere = sSQLAppendWhereOrAnd(sWhere)
        //		'   sWhere &= " IdentityKey <> " & ExcludeIdentityKey

        public static string sSQLAppendWhereOrAnd(string strWhere)
        {
            if (VBStrings.Len(strWhere) == 0)
            {
                strWhere = " Where ";
                return strWhere;
            }
            strWhere = strWhere + " and ";
            return strWhere;
        }
        //		'Not very relyable, because it can be 'where' in the data, not as keyword
        public static string PrefixWhereIfRequired(string strWhere)
        {
            if ((VBStrings.Len(strWhere.Trim()) > 0) && (VBStrings.InStr(strWhere, "Where", CompareMethod.Text) <= 0))
            {
                strWhere = " Where " + strWhere;
            }
            return strWhere;
        }
        /// <summary>
        /// Example                 sRet=DataHelper.SQLAppendAndIfNotEmpty(sRet);
        /// </summary>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public static string SQLAppendAndIfNotEmpty(string strWhere)
        {
            if (VBStrings.Len(strWhere) > 0)
                strWhere = strWhere + " and ";
            return strWhere;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strWhere">existing Where string</param>
        /// <param name="Condition">additional "and" condition</param>
        /// <returns></returns>
        public static string SQLAppendCondition(string strWhere, string Condition)
        {
            if (VBStrings.Len(strWhere) > 0)
            {
                strWhere = strWhere + " and ";
            }
            strWhere += Condition;
            return strWhere;
        }
        public static string SQLBetweenClause(string Field, string Fro, string Tos)
        {
            return Field + " BETWEEN " + Quoted(Fro) + " AND " + Quoted(Tos);
            //SQLBetweenClause = Field + " >= " + Quoted(Fro) + " AND " + Field + " <= " + Quoted(Tos); 
        }

        public static string WrapSqlWildCards(string criteria)
        {
            string newCriteria = criteria;

            if (newCriteria.Length > 0)
            {

                if (!newCriteria.StartsWith("%"))
                {
                    newCriteria = "%" + newCriteria;
                }

                if (!newCriteria.EndsWith("%"))
                {
                    newCriteria = newCriteria + "%";
                }
            }
            return newCriteria;
        }
        #endregion  // " SQL String Manipulations "

        /// <summary>
        /// invalid string will throw FormatException -see Boolean.Parse method
        /// </summary>
        /// <param name="oValue"></param>
        /// <returns>false if oValue is null</returns>
        public static bool BoolFromSQLReturn(object oValue)
        {
            //TODO try BooleanType.FromObject, but unlikely 0 or 1 will be considered
            //return "select 1 from.."
            //expected to check result "1" or "0" or "true" or "false"
            bool bRet = false;
            //			if ((oValue is int) &&( (int)oValue==1)) bRet=true;
            if (Microsoft.VisualBasic.Information.IsNumeric(oValue))
            {
                double dValue = Convert.ToDouble(oValue);
                if (dValue == 1) bRet = true;
            }
            else if (oValue is string)
            {
                if (String.IsNullOrEmpty((string)oValue)) bRet = false;
                else bRet = Boolean.Parse((string)oValue);
            }
            else if (oValue is bool)
            {
                bRet = (bool)oValue;
            }
            return bRet;
        }


    }
}
