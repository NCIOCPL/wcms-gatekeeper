using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.Common
{
    public static class ConvertEnum<T>
    {
        /// <summary>
        /// Convert a string embedded in an Object to an enumerated value.  If a null object
        /// or an empty string is passed, an InvalidCastException is thrown.
        /// </summary>
        /// <param name="obj">an Object encapsulating the string representation of
        /// an enumerated value.</param>
        /// <returns>An enumerated value of type T</returns>
        public static T Convert(Object obj)
        {
            if (obj == null)
            {
                throw new InvalidCastException("Cannot cast null to an enumerated type.");
            }
            else if ((string)obj == "")
            {
                throw new InvalidCastException("Cannot cast an empty string to an enumerated type.");
            }
            return (T)Enum.Parse(typeof(T), (string)obj, true);
        }

        /// <summary>
        /// Convert a string embedded in an Object to an enumerated value.  If a null object
        /// or an empty string is passed, defaultValue is returned.  Likewise, invalid values are also
        /// converted to defaultValue.
        /// </summary>
        /// <param name="obj">an Object encapsulating the string representation of
        /// an enumerated value.</param>
        /// <param name="defaultValue">The value to return if obj is null or contains an empty string.</param>
        /// <returns></returns>
        public static T Convert(Object obj, T defaultValue)
        {
            T result = defaultValue;
            if (obj != null && (string)obj != "")
            {
                try
                {
                    result = (T)Enum.Parse(typeof(T), (string)obj, true);
                }
                catch
                {
                    result = defaultValue;
                }
            }
            return result;
        }
    }

    public class ConvertGeneral<T>
    {
        /// <summary>
        /// Convert a string embedded in an Object to an enumerated value.  If a null object
        /// or an empty string is passed, defaultValue is returned.
        /// </summary>
        /// <param name="obj">an Object encapsulating the string representation of
        /// a value of type T.</param>
        /// <param name="defaultValue">The value to return if obj is null or contains an empty string.</param>
        /// <returns></returns>
        public static T Convert(Object obj, T defaultValue)
        {
            T result = defaultValue;
            if (obj != null && (string)obj != "")
            {
                result = (T)obj;
            }
            return result;
        }

        /// <summary>
        /// Converts an array of values of type T into an array of strings containing comma-delimited
        /// lists of the values.  It is the caller's responsibility to guarantee that type T provides
        /// a meaningful implementation of ToString().
        /// </summary>
        /// <param name="values">An array of values</param>
        /// <param name="maxStringSize">The maximum character count in a single string.
        /// (Note: Values larger than maxStringSize are not split across multiple strings.</param>
        /// <returns>An array of comma-delimited strings</returns>
        public static string[] ArrayToString(T[] values, int maxStringSize)
        {
            // Convert list of numbers to one or more strings containing comma-delimited values.
            StringBuilder buffer = new StringBuilder();
            List<string> valueList = new List<string>();

            bool isFirst = true;
            foreach (T curValue in values)
            {
                string str = curValue.ToString();
                if (buffer.Length + str.Length + 1 < maxStringSize)
                {
                    if (!isFirst)
                        buffer.Append(",");
                    else
                        isFirst = false;
                    buffer.Append(str);
                }
                else
                {
                    valueList.Add(buffer.ToString());
                    buffer = new StringBuilder(str);
                }
            }
            valueList.Add(buffer.ToString());

            return valueList.ToArray();
        }
    }

}
