using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.Logging;
using GateKeeper.DataAccess.GateKeeper;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Helper class to extract GeneticsProfessionalDocuments from source XML.
    /// </summary>
    public class GeneticsProfessionalExtractor
    {
        #region Fields
        private int _documentID;
        private DocumentXPathManager xPathManager;
        #endregion

        #region Private Methods
        /// <summary>
        /// Extracts genetics professional name, degrees and specialties.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="genProf"></param>
        private void ExtractMetadata(XPathNavigator xNav, GeneticsProfessionalDocument genProf)
        {
            // This variable is set for error tracking purpose
            string path = string.Empty;
            try {
                path = xPathManager.GetXPath(GenProfXPath.FirstName);
                genProf.FirstName = DocumentHelper.StripControlCharacters(xNav.SelectSingleNode(path).Value);
                path = xPathManager.GetXPath(GenProfXPath.LastName);
                genProf.LastName = DocumentHelper.StripControlCharacters(xNav.SelectSingleNode(path).Value);
                path = xPathManager.GetXPath(GenProfXPath.ShortName);
                genProf.ShortName = DocumentHelper.StripControlCharacters(xNav.SelectSingleNode(path).Value);
                path = xPathManager.GetXPath(GenProfXPath.Suffix);
                XPathNavigator suffixNav = xNav.SelectSingleNode(path);
                if (suffixNav != null)
                {
                    genProf.Suffix = DocumentHelper.StripControlCharacters(suffixNav.Value);
                }

                path = xPathManager.GetXPath(GenProfXPath.Degree);
                XPathNodeIterator degreeIter = xNav.Select(path);
                while (degreeIter.MoveNext())
                {
                    genProf.Degrees.Add(DocumentHelper.StripControlCharacters(degreeIter.Current.Value));
                }

                path = xPathManager.GetXPath(GenProfXPath.Specialty);
                XPathNodeIterator specialtyIter = xNav.Select(path);
                while (specialtyIter.MoveNext())
                {
                    genProf.Specialties.Add(DocumentHelper.StripControlCharacters(specialtyIter.Current.Value));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract genetic professional practice locations
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="genProf"></param>
        private void ExtractPracticeLocations(XPathNavigator xNav, GeneticsProfessionalDocument genProf)
        {
            string path = xPathManager.GetXPath(GenProfXPath.Location);
            //try {
                XPathNodeIterator practiceLocationIter = xNav.Select(path);
                while (practiceLocationIter.MoveNext())
                {
                    path = xPathManager.GetXPath(GenProfXPath.State);
                    string state = string.Empty;
                    if (practiceLocationIter.Current.SelectSingleNode(path) != null)
                        state = DocumentHelper.StripControlCharacters(practiceLocationIter.Current.SelectSingleNode(path).Value);

                    path = xPathManager.GetXPath(GenProfXPath.Zip);
                    string zip = string.Empty;
                    if (practiceLocationIter.Current.SelectSingleNode(path) != null)
                         zip = DocumentHelper.StripControlCharacters(practiceLocationIter.Current.SelectSingleNode(path).Value);

                    path = xPathManager.GetXPath(GenProfXPath.Country);
                    string country = string.Empty;
                    if (practiceLocationIter.Current.SelectSingleNode(path) != null)
                        country = DocumentHelper.StripControlCharacters(practiceLocationIter.Current.SelectSingleNode(path).Value);

                    path = xPathManager.GetXPath(GenProfXPath.City);
                    string city = string.Empty;
                    if (practiceLocationIter.Current.SelectSingleNode(path) != null)
                        city = DocumentHelper.StripControlCharacters(practiceLocationIter.Current.SelectSingleNode(path).Value);

                    genProf.PracticeLocations.Add(new PracticeLocation(city, state, zip, country));
                }
           // }
           // catch (Exception e)
           // {
           //     throw new Exception("Extraction Error: Extracting practice location " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
           // }
        }

        /// <summary>
        /// Method to extract family cancer syndrome and the cancer 
        /// types (with the associated cancer sites).
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="genProf"></param>
        private void ExtractFamilyCancerSyndrome(XPathNavigator xNav, GeneticsProfessionalDocument genProf)
        {
            string path = xPathManager.GetXPath(GenProfXPath.FamilySyndrome);
            try {
                XPathNodeIterator familySyndromeIter = xNav.Select(path);
                while (familySyndromeIter.MoveNext())
                {
                    path = xPathManager.GetXPath(GenProfXPath.SyndromeName);
                    FamilyCancerSyndrome syndrome = new FamilyCancerSyndrome();
                    syndrome.SyndromeName =
                        DocumentHelper.StripControlCharacters(
                        familySyndromeIter.Current.SelectSingleNode(path).Value);

                    // Process syndrome's cancer types and cancer sites...
                    path = xPathManager.GetXPath(GenProfXPath.CancerType);
                    XPathNodeIterator cancerTypeIter = familySyndromeIter.Current.Select(path);
                    while (cancerTypeIter.MoveNext())
                    {
                        path = xPathManager.GetXPath(GenProfXPath.CancerTypeName);
                        string cancerTypeName =
                            DocumentHelper.StripControlCharacters(
                            cancerTypeIter.Current.SelectSingleNode(path).Value);

                        path = xPathManager.GetXPath(GenProfXPath.CancerSite);
                        XPathNodeIterator cancerSiteIter = cancerTypeIter.Current.Select(path);
                        while (cancerSiteIter.MoveNext())
                        {
                            // Concatenate the cancer type name and the cancer site
                            syndrome.CancerTypeSites.Add(cancerTypeName + ": " +
                                DocumentHelper.StripControlCharacters(cancerSiteIter.Current.Value));
                        }
                    }
                    genProf.FamilyCancerSyndromes.Add(syndrome);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting family cancer syndrome " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }
        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Modifies the document XML, so that subsequent processing is based on
        /// ideal input.
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void PrepareXml(XmlDocument xmlDoc)
        {
            // TODO: Add code to "prepare" xml (fix problems with data) 
        }

        /// <summary>
        /// Extracts the genetics professional metadata from the input XML document.
        /// </summary>
        /// <param name="xmlDoc">Genetics professional XML</param>
        /// <param name="genProf">Genetics professional document object</param>
        public void Extract(XmlDocument xmlDoc, GeneticsProfessionalDocument genProf, DocumentXPathManager xPath)
        {
            try
            {
                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                if (CDRHelper.ExtractCDRID(xNav, xPath.GetXPath(CommonXPath.CDRID), out _documentID))
                {
                    genProf.DocumentID = _documentID;
                }
                else
                {
                    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in Genetics Professional document!");
                }

                DocumentHelper.CopyXml(xmlDoc, genProf);

                // Get document xpath
                xPathManager = xPath;

                // Set document type to be terminology
                genProf.DocumentType = DocumentType.GENETICSPROFESSIONAL;

                // Extract misc metadata...
                ExtractMetadata(xNav, genProf);

                // Extract practice locations
                ExtractPracticeLocations(xNav, genProf);

                // Extract family cancer syndromes
                ExtractFamilyCancerSyndrome(xNav, genProf);
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Failed to extract genetic professional document", e);
            }
        }
        #endregion
    }
}
