using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace GateKeeper
{
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException()
            : base()
        {
        }

        public InvalidRequestException(string message)
            : base(message)
        {
        }

        public InvalidRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
