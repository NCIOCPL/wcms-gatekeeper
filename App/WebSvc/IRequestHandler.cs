using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GateKeeper
{
    public interface IRequestHandler
    {
        XmlDocument ProcessRequest();
    }
}
