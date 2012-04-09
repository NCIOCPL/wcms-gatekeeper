using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// This derived class is used by Preview to change the behaviour of the 
    /// Extract method slightly to suit the needs of the publish preview document.
    /// The pubpreview document will create document in unique folder. 
    /// </summary>
    public class SummaryPreviewExtractor : SummaryExtractor
    {
        /// <summary>
        /// This override will do the following.
        /// 1) Calls the base class Extract method.  
        /// 2) Replace document.BasePrettyUrl with a URL where the path is 
        /// the current time expressed as milliseconds since 1-1-1970.  
        /// </summary>
        /// <param name="xmlDoc">Summary XML</param>
        /// <param name="summary">Summary document object</param>
        override public void Extract(XmlDocument xmlDoc, Document document, DocumentXPathManager xPathManager, TargetedDevice targetedDevice)
        {
            // invoke the base class method to extract information from xml document.
            base.Extract(xmlDoc, document, xPathManager, TargetedDevice.screen);

            // create unique identifier using current time expressed as milliseconds since 1-1-1970.   
            // append this unique to the base url
            string uniqueId = Math.Round((decimal)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks / 1000000).ToString();

            ((SummaryDocument)document).BasePrettyURL = "/" + uniqueId + "/summary";
        }
    }
}
