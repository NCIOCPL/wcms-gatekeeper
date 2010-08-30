using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.Common
{
    public class Strings
    {

        /// <summary>
        /// Converts string integer to Int32
        /// </summary>
        /// <param name="intString"></param>
        /// <returns>int, -1</returns>
        public static int ToInt(string intString)
        {
            int i = -1;

            try
            {
                i = Convert.ToInt32(intString.Trim());
            }
            catch (Exception)
            {
            }

            return i;
        }

        public static int ToInt(string intString, int defaultValue)
        {
            int i = defaultValue;

            try
            {
                i = Convert.ToInt32(intString.Trim());
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts object to Int32
        /// </summary>
        /// <param name="intObject">Object to be converted</param>
        /// <returns>int, -1</returns>
        public static int ToInt(object intObject)
        {
            int i = -1;

            try
            {
                i = Convert.ToInt32(intObject);
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts object to Int32
        /// </summary>
        /// <param name="intObject">Object to be converted</param>
        /// <param name="defaultValue">Default value to be used</param>
        /// <returns>int, defaultValue</returns>
        public static int ToInt(object intObject, int defaultValue)
        {
            int i = defaultValue;

            try
            {
                i = Convert.ToInt32(intObject);
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts string integer to UInt16
        /// </summary>
        /// <param name="intString"></param>
        /// <returns>UInt16, 0</returns>
        public static UInt16 ToUInt16(string uInt16String)
        {
            UInt16 i = 0;

            try
            {
                i = Convert.ToUInt16(uInt16String.Trim());
            }
            catch (Exception)
            {
            }

            return i;
        }

        public static UInt16 ToUInt16(string uInt16String, UInt16 defaultValue)
        {
            UInt16 i = defaultValue;

            try
            {
                i = Convert.ToUInt16(uInt16String.Trim());
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts string integer to UInt16
        /// </summary>
        /// <param name="intString"></param>
        /// <returns>UInt16, 0</returns>
        public static uint ToUInt(string uIntString)
        {
            uint i = 0;

            try
            {
                i = uint.Parse(uIntString);
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts string integer to UInt16
        /// </summary>
        /// <param name="intString"></param>
        /// <returns>UInt16, 0</returns>
        public static uint ToUInt(object obj)
        {
            uint i = 0;

            try
            {
                i = uint.Parse(obj.ToString());
            }
            catch (Exception)
            {
            }

            return i;
        }

        public static uint ToUInt(string uIntString, uint defaultValue)
        {
            uint i = defaultValue;

            try
            {
                i = uint.Parse(uIntString);
            }
            catch (Exception)
            {
            }

            return i;
        }

        public static uint ToUInt(object obj, uint defaultValue)
        {
            uint i = defaultValue;

            try
            {
                i = uint.Parse(obj.ToString());
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts string integer to Long
        /// </summary>
        /// <param name="longString"></param>
        /// <returns>long, -1</returns>
        public static long ToLong(string longString)
        {
            long i = -1;

            try
            {
                i = Convert.ToInt64(longString.Trim());
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts object to Long
        /// </summary>
        /// <param name="longObject">Object to be converted</param>
        /// <returns>long, -1</returns>
        public static long ToLong(object longObject)
        {
            long i = -1;

            try
            {
                i = Convert.ToInt64(longObject);
            }
            catch (Exception)
            {
            }

            return i;
        }

        public static long ToLong(string longString, int defaultValue)
        {
            long i = defaultValue;

            try
            {
                i = Convert.ToInt64(longString.Trim());
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts object to Long
        /// </summary>
        /// <param name="longObject">Object to be converted</param>
        /// <returns>long, -1</returns>
        public static long ToLong(object longObject, long defaultValue)
        {
            long i = defaultValue;

            try
            {
                i = Convert.ToInt64(longObject);
            }
            catch (Exception)
            {
            }

            return i;
        }

        /// <summary>
        /// Converts string boolean to type bool
        /// </summary>
        /// <param name="boolString"></param>
        /// <returns>bool, false</returns>
        /// <summary>
        /// Converts string boolean to type bool
        /// </summary>
        /// <param name="boolString"></param>
        /// <returns>bool, false</returns>
        public static bool ToBoolean(object obj)
        {
            bool b = false;

            try
            {
                b = ToBoolean(obj.ToString());
            }
            catch (Exception)
            {

            }

            return b;
        }

        /// <summary>
        /// Converts string boolean to type bool
        /// </summary>
        /// <param name="boolString"></param>
        /// <returns>bool, false</returns>
        /// <summary>
        /// Converts string boolean to type bool
        /// </summary>
        /// <param name="boolString"></param>
        /// <returns>bool, false</returns>
        public static bool ToBoolean(string boolString)
        {
            bool b = false;

            if (boolString == null)
            {
                return false;
            }

            if (boolString.ToUpper() == "YES")
            {
                return true;
            }

            if (boolString.ToUpper() == "NO")
            {
                return false;
            }

            if (boolString.ToUpper() == "Y")
            {
                return true;
            }

            if (boolString.ToUpper() == "N")
            {
                return false;
            }

            if (boolString.ToUpper() == "1")
            {
                return true;
            }

            if (boolString.ToUpper() == "0")
            {
                return false;
            }


            try
            {
                b = Convert.ToBoolean(boolString.Trim());
            }
            catch (Exception)
            {
            }

            return b;
        }



        /// <summary>
        /// Converts string date to type DateTime
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns>DateTime, 00/00/0000</returns>
        public static DateTime ToDateTime(string dateTimeString)
        {
            DateTime dt = new DateTime(0);

            try
            {
                dt = Convert.ToDateTime(dateTimeString.Trim());
            }
            catch (Exception)
            {
            }

            return dt;
        }

        /// <summary>
        /// Converts string guid to type Guid
        /// </summary>
        /// <param name="guidString"></param>
        /// <returns>Guid, null</returns>
        public static Guid ToGuid(string guidString)
        {
            Guid g;

            try
            {
                g = new Guid(guidString.Trim());
            }
            catch (Exception)
            {
                g = System.Guid.Empty;
            }

            return g;
        }

        /// <summary>
        /// Converts string guid to type Guid
        /// </summary>
        /// <param name="guidString"></param>
        /// <returns>Guid, null</returns>
        public static Guid ToGuid(object guidObj)
        {
            string guidString = null;

            try
            {
                guidString = guidObj.ToString();
            }
            catch (Exception)
            {
                guidString = null;
            }

            return ToGuid(guidString);
        }


        /// <summary>
        /// Trims string, returns null for both null and zero length strings
        /// </summary>
        /// <param name="val"></param>
        /// <returns>null or trimmed string</returns>
        public static string Clean(string val)
        {
            if (val == null || val.Trim() == String.Empty)
            {
                return null;
            }
            else
            {
                return val.Trim();
            }
        }

        /// <summary>
        /// Trims string, returns null for both null and zero length strings
        /// </summary>
        /// <param name="val"></param>
        /// <returns>null or trimmed string</returns>
        public static string Clean(object val)
        {
            string str = null;

            if ((val != null) && (val.ToString().Trim() != String.Empty))
            {
                str = val.ToString().Trim();
            }

            return str;
        }

        /// <summary>
        /// Trims string, returns null for both null and zero length strings
        /// </summary>
        /// <param name="val"></param>
        /// <returns>null or trimmed string</returns>
        public static string Clean(object val, string defaultValue)
        {
            string str = defaultValue;

            if ((val != null) && (val.ToString().Trim() != String.Empty))
            {
                str = val.ToString().Trim();
            }

            return str;
        }


        /// <summary>
        /// Substitutes default string for a null string
        /// </summary>
        /// <param name="val">Test string</param>
        /// <param name="valDefault">Default string</param>
        /// <returns>Test string or default string</returns>
        public static string IfNull(string val, string valDefault)
        {
            if (val == null)
            {
                return valDefault;
            }
            else
            {
                return val;
            }
        }


        public static ArrayList ToArrayListOfInts(string val, char separator)
        {
            ArrayList alList = new ArrayList();

            if (val != null)
            {
                foreach (string strItem in val.Split(separator))
                {
                    alList.Add(Strings.ToInt(strItem));
                }
            }

            return alList;
        }

        public static ArrayList ToArrayListOfStrings(string val, char separator)
        {

            ArrayList alList = new ArrayList();

            if (val != null)
            {
                foreach (string strItem in val.Split(separator))
                {
                    alList.Add(strItem);
                }
            }

            return alList;
        }

        public static long[] ToListOfLongs(string val, char separator)
        {
            List<long> list = new List<long>();

            if (val != null)
            {
                foreach (string strItem in val.Split(separator))
                {
                    long tmp = Strings.ToLong(strItem);
                    if (tmp != -1)
                        list.Add(tmp);
                }
            }

            return list.ToArray();
        }

        public static string GetNumericSuffixFormat(string num)
        {
            int tens = 0;
            string suffix = "";

            try
            {
                tens = Convert.ToInt16(num.Substring(num.Length - 2, 1));
            }
            catch
            {
            }

            if (tens == 1)
            {
                suffix = "\\t\\h";
            }
            else
            {
                switch (num.Substring(num.Length - 1, 1))
                {
                    case "0":
                        suffix = "\\t\\h";
                        break;
                    case "1":
                        suffix = "\\s\\t";
                        break;
                    case "2":
                        suffix = "\\n\\d";
                        break;
                    case "3":
                        suffix = "\\r\\d";
                        break;
                    case "4":
                        suffix = "\\t\\h";
                        break;
                    case "5":
                        suffix = "\\t\\h";
                        break;
                    case "6":
                        suffix = "\\t\\h";
                        break;
                    case "7":
                        suffix = "\\t\\h";
                        break;
                    case "8":
                        suffix = "\\t\\h";
                        break;
                    case "9":
                        suffix = "\\t\\h";
                        break;
                }
            }
            return suffix;
        }

        public static string Wrap(string source, int charWidth, string wrapChar)
        {
            int index = charWidth;

            while (index < source.Length)
            {
                source = source.Insert(index, wrapChar);
                index += charWidth + wrapChar.Length;
            }

            return source;
        }
    }
}
