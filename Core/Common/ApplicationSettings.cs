using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace GateKeeper.Common
{
    public static class ApplicationSettings
    {
        private const int DEFAULT_COMMAND_TIMEOUT = 60;
        private const int DEFAULT_VERY_LONG_COMMAND_TIMEOUT = 1200;

        public static int CommandTimeout
        {
            get
            {
                int timeout = RetrieveIntSetting("CommandTimeout", DEFAULT_COMMAND_TIMEOUT);
                if (timeout == 0)
                    timeout = DEFAULT_COMMAND_TIMEOUT;
                return timeout;
            }
        }


        public static int VeryLongProcessCommandTimeout
        {
            get
            {
                int timeout = RetrieveIntSetting("VeryLongProcessCommandTimeout", DEFAULT_VERY_LONG_COMMAND_TIMEOUT);
                if (timeout == 0)
                    timeout = DEFAULT_VERY_LONG_COMMAND_TIMEOUT;
                return timeout;
            }
        }

        #region Helpers

        private static int RetrieveIntSetting(string name, int defaultValue)
        {
            int result = defaultValue;
            string temp = ConfigurationManager.AppSettings[name];
            try
            {
                // If value is missing or invalid, defaultValue will remain.
                if (!string.IsNullOrEmpty(temp))
                    result = int.Parse(temp);
            }
            catch
            {
                // Swallow the exception.
            }

            return result;
        }

        #endregion

    }
}
