using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.Common
{
    public static class ExceptionBuilder
    {
        public static Exception InvalidValue(string name, object value)
        {
            return InvalidValue(name, value, null);
        }

        public static Exception InvalidValue(string name, object value, Exception innerException)
        {
            string format = "{0} - Invalid value ({1}).";
            string message = string.Format(format, name, value);
            return new Exception(message, innerException);
        }
    }
}
