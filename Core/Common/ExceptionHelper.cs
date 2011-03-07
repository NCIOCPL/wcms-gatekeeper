using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.Common
{
    public static class ExceptionHelper
    {
        public static string RetrieveMessage(Exception ex)
        {
            string innerMessage = "";
            if (ex != null && ex.InnerException != null)
                innerMessage = RetrieveMessage(ex.InnerException);
            return string.Format("{0}\n{1}", ex.Message, innerMessage);
        }

        public static string RetreiveInnermostMessage(Exception ex)
        {
            if (ex != null && ex.InnerException != null)
                return RetreiveInnermostMessage(ex.InnerException);
            return ex.Message;
        }
    }
}
