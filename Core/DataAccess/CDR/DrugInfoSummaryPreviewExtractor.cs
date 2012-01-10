using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Helper class to extract DrugInfoSummary from source XML.
    /// </summary>
    public class DrugInfoSummaryPreviewExtractor: DrugInfoSummaryExtractor
    {
        /// <summary>
        /// This override will do the following.
        /// 1) Calls the base class Extract method.  
        /// 2) Append to the prettyUrl the current time expressed as milliseconds 
        /// since 1-1-1970 to make it unique.
        /// </summary>
        /// <param name="xmlDoc">Drug Info Summary XML</param>
        /// <param name="summary">Drug Info Summary document object</param>
        override public void Extract(XmlDocument xmlDoc, DrugInfoSummaryDocument drugInfoSummary, DocumentXPathManager xPathManager)
        {
            // invoke the base class method to extract information from xml document.
            base.Extract(xmlDoc, drugInfoSummary, xPathManager);

            // create unique identifier using current time expressed as milliseconds since 1-1-1970.   
            // append this unique to the base url
            string uniqueId = Math.Round((decimal)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks / 1000000).ToString();

            drugInfoSummary.PrettyURL += "_" + uniqueId;
        }
    }
}
