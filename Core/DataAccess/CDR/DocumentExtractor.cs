using System;
using System.Xml;
using System.Xml.Serialization;

using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects;

namespace GateKeeper.DataAccess.CDR
{
    [XmlInclude(typeof(SummaryExtractor))]
    abstract public class DocumentExtractor
    {
        /// <summary>
        /// Extracts document metadata from the input XML document.
        /// </summary>
        /// <param name="xmlDoc">The XML doc.</param>
        /// <param name="document">An object of a concrete Document type.</param>
        /// <param name="xPath">DocumentXPathManager object containing the xPaths for all
        /// data points to be extracted.</param>
        /// <param name="targetedDevice">The targeted device.</param>
        /// <remarks>Extract must be run to determine what devices are valid for a document.
        /// The caller is responsible for verifying that the device is supported for the
        /// currently targetted device before proceeding with document processing.
        /// </remarks>
        abstract public void Extract(XmlDocument xmlDoc, Document document, DocumentXPathManager xPath, TargetedDevice targetedDevice);
    }
}
