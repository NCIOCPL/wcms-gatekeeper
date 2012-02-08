using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using GateKeeper.Common;
using GateKeeper.Common.XPathKeys;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GateKeeper.DocumentObjects.Summary;

namespace GateKeeper.DataAccess.CDR
{
    /// <summary>
    /// Class to extract GlossaryTerm object from XML.
    /// </summary>
    public class GlossaryTermExtractor
    {
        #region Fields
        private int _documentID = 0;
        private DocumentXPathManager xPathManager;
        #endregion

        #region Private Methods

        /// <summary>
        /// Extract English term
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="glossaryTerm"></param>
        private void ExtractEnglishTerm(XPathNavigator xNav, GlossaryTermDocument glossaryTerm)
        {
            string path = xPathManager.GetXPath(GlossaryTermXPath.Name);
            try
            {
                XPathNavigator englishTermNameNav = xNav.SelectSingleNode(path);
                if (englishTermNameNav != null)
                {
                    // Handle term names
                    string termName = DocumentHelper.GetXmlDocumentValue(xNav, path);
                    path = xPathManager.GetXPath(GlossaryTermXPath.Pronunciation);
                    string pronunciation = DocumentHelper.GetXmlDocumentValue(xNav, path);
                    path = xPathManager.GetXPath(GlossaryTermXPath.Definition);
                    List<GlossaryTermDefinition> definitionList = ExtractDefinitions(xNav, path);

                    GlossaryTermTranslation term = new GlossaryTermTranslation(termName, pronunciation, Language.English, definitionList);

                    glossaryTerm.GlossaryTermTranslationMap.Add(Language.English, term);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Extract Spanish term
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="glossaryTerm"></param>
        private void ExtractSpanishTerm(XPathNavigator xNav, GlossaryTermDocument glossaryTerm)
        {
            string path = xPathManager.GetXPath(GlossaryTermXPath.SpanishName);
            try
            {
                XPathNavigator spanishTermNameNav = xNav.SelectSingleNode(path);
                if (spanishTermNameNav != null)
                {
                    // Handle term names
                    string termName = DocumentHelper.GetXmlDocumentValue(xNav, path);
                    path = xPathManager.GetXPath(GlossaryTermXPath.SpanishDef);
                    List<GlossaryTermDefinition> definitionList = ExtractDefinitions(xNav, path);
                    string pronunciation = string.Empty; // TODO: Add spanish pronounciation here if CDR group changes the document to publish one

                    GlossaryTermTranslation term =
                        new GlossaryTermTranslation(termName, null, Language.Spanish, definitionList);

                    glossaryTerm.GlossaryTermTranslationMap.Add(Language.Spanish, term);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        /// <summary>
        /// Method to extract definition list.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        private List<GlossaryTermDefinition> ExtractDefinitions(XPathNavigator xNav, string xpath)
        {
            List<GlossaryTermDefinition> definitionList = new List<GlossaryTermDefinition>();
            try
            {
                XPathNodeIterator definitionIter = xNav.Select(xpath);
                foreach (XPathNavigator definitionNav in definitionIter)
                {
                    string path = xPathManager.GetXPath(GlossaryTermXPath.DefText);
                    string text = DocumentHelper.GetXmlDocumentRawData(definitionNav, path);
                    path = xPathManager.GetXPath(GlossaryTermXPath.Dictionary);
                    List<string> dictionaryNameList = DocumentHelper.ExtractValueList(definitionNav, path);
                    path = xPathManager.GetXPath(GlossaryTermXPath.Audience);
                    List<AudienceType> audienceTypeList = ExtractAudience(definitionNav, path);
                    GlossaryTermDefinition definition = new GlossaryTermDefinition(text, audienceTypeList, dictionaryNameList);
                    definitionList.Add(definition);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting definition failed.  Document CDRID=" + _documentID.ToString(), e);
            }
            return definitionList;
        }

        /// <summary>
        /// Method to extract audience type.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        private List<AudienceType> ExtractAudience(XPathNavigator xNav, string xpath)
        {
            List<AudienceType> audienceTypeList = new List<AudienceType>();
            try
            {
                XPathNodeIterator audienceIter = xNav.Select(xpath);
                foreach (XPathNavigator audienceNav in audienceIter)
                {
                    AudienceType audience = DocumentHelper.DetermineAudienceType(audienceNav.Value);
                    if (!audienceTypeList.Contains(audience))
                        audienceTypeList.Add(DocumentHelper.DetermineAudienceType(audienceNav.Value));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + xpath + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
            return audienceTypeList;
        }

        /// <summary>
        /// Method to extract media links.
        /// </summary>
        /// <param name="xNav"></param>
        /// <param name="glossaryTerm"></param>
        private void ExtractMediaLinks(XPathNavigator xNav, GlossaryTermDocument glossaryTermDoc)
        {
            string path = xPathManager.GetXPath(GlossaryTermXPath.MediaLink);
            try
            {
                XPathNodeIterator mediaLinkIter = xNav.Select(path);

                while (mediaLinkIter.MoveNext())
                {
                    string tempMediaDocumentID = string.Empty;
                    int mediaDocumentID = 0;
                    tempMediaDocumentID = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaRef));
                    if (!Int32.TryParse(CDRHelper.ExtractCDRID(tempMediaDocumentID), out mediaDocumentID))
                    {
                        throw new Exception("Failed to parse media document id from value: " + tempMediaDocumentID);
                    }


                    string mediaLinkID = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaID));

                    // get the mime type
                    string type = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaType));
                    type = string.IsNullOrEmpty(type) ? String.Empty : type;

                    // Extract document reference and find the CDR document ID
                    string imgRef = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaRef));
                    int cdrId = Int32.Parse(Regex.Replace(imgRef, "^CDR(0*)", "", RegexOptions.Compiled));

                    string thumb = string.Empty;
                    string alt = string.Empty;
                    bool isThumb = false;
                    bool isInline = false;
                    bool showEnlargeLink = true; // Not used for MediaLink in glossary terms, but this is approximately the correct logic.
                    // Should really be a rendering decision based on parent node.
                    string inLine = String.Empty;
                    string width = String.Empty;
                    long minWidth = -1;
                    string size = String.Empty;

                    if (string.IsNullOrEmpty(type) || type.Contains("image"))
                    {
                        thumb = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaThumb));
                        isThumb = (thumb.ToUpper() == "YES") ? true : false;

                        alt = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaAlt));
                        isInline = false;

                        inLine = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaInline));
                        if (inLine.ToUpper().Equals("YES"))
                        {
                            isInline = true;
                            showEnlargeLink = false;
                        }

                        width = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaMidWidth));
                        if ((width != null) && (width.Length > 0))
                            minWidth = long.Parse(width);

                        // If size is not speicified, a width of one-third is used.
                        // Logic migrated from legacy rendering code.
                        size = DocumentHelper.GetAttribute(mediaLinkIter.Current, xPathManager.GetXPath(GlossaryTermXPath.MediaSize));
                        if (size.Equals(String.Empty))
                            size = "third";
                    }

                    XmlDocument mediaXml = new XmlDocument();
                    mediaXml.LoadXml(mediaLinkIter.Current.OuterXml);

                    // To handle the case that more than one Caption nodes under the same MediaLink node
                    Dictionary<Language, string> langCapMap = new Dictionary<Language, string>();

                    XmlNode mediaNode = ((IHasXmlNode)mediaLinkIter.Current).GetNode();

                    // This processing is only for image media
                    if (string.IsNullOrEmpty(type) || type.Contains("image"))
                    {
                        foreach (XmlNode node in mediaNode.ChildNodes)
                        {
                            if (node.Name.Equals(xPathManager.GetXPath(GlossaryTermXPath.MediaCaption)))
                            {
                                XmlAttributeCollection atts = node.Attributes;
                                XmlAttribute tmpLang = (XmlAttribute)atts.GetNamedItem(xPathManager.GetXPath(GlossaryTermXPath.MediaLanguage));
                                Language capLang = Language.English;
                                if (tmpLang != null)
                                    capLang = DocumentHelper.DetermineLanguageString(tmpLang.Value.ToString());
                                if (langCapMap.ContainsKey(capLang))
                                    langCapMap[capLang] += node.InnerText;
                                else
                                    langCapMap.Add(capLang, node.InnerText);
                            }
                        }

                        foreach (Language lang in langCapMap.Keys)
                        {
                            if (glossaryTermDoc.GlossaryTermTranslationMap.ContainsKey(lang))
                            {
                                MediaLink mediaLink = new MediaLink(imgRef, cdrId, alt, isInline, showEnlargeLink, minWidth, size, mediaLinkID, langCapMap[lang], mediaDocumentID, lang, isThumb, type, mediaXml);
                                glossaryTermDoc.GlossaryTermTranslationMap[lang].MediaLinkList.Add(mediaLink);
                            }
                            else
                            {
                                throw new Exception("Extraction Error: Error glossary term contains a media link that does not have top nodes associate with it. GlossaryTermExtractor:: ExtractMediaLinks()");
                            }
                        }

                    }
                    else if (type.Contains("audio"))
                    {
                        XmlAttributeCollection atts = mediaNode.Attributes;
                        XmlAttribute tmpLang = (XmlAttribute)atts.GetNamedItem(xPathManager.GetXPath(GlossaryTermXPath.MediaLanguage));
                        Language lang = Language.English;
                        if (tmpLang != null)
                            lang = DocumentHelper.DetermineLanguageString(tmpLang.Value.ToString());

                        if (glossaryTermDoc.GlossaryTermTranslationMap.ContainsKey(lang))
                        {
                            MediaLink mediaLink = new MediaLink(imgRef, cdrId, "", isInline, showEnlargeLink, minWidth, size, mediaLinkID, "", mediaDocumentID, lang, isThumb, type, mediaXml);
                            glossaryTermDoc.GlossaryTermTranslationMap[lang].MediaLinkList.Add(mediaLink);
                        }
                        else
                        {
                            throw new Exception("Extraction Error: Error glossary term contains a media link that does not have top nodes associate with it. GlossaryTermExtractor:: ExtractMediaLinks()");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        private void ExtractRelatedInformation(XPathNavigator xNav, GlossaryTermDocument glossaryTermDoc)
        {
            ExtractRelatedInformationByType(xNav, glossaryTermDoc, "./RelatedInformation/*");  
        }

        private void ExtractRelatedInformationByType(XPathNavigator xNav, GlossaryTermDocument glossaryTermDoc, string xpath)
        {
            string path = xpath;

            try
            {
                XPathNodeIterator relatedInformations = xNav.Select(path);
                foreach (XPathNavigator definitionNav in relatedInformations)
                {
                    string useWith = DocumentHelper.GetAttribute(definitionNav, xPathManager.GetXPath(RelatedInformationXPath.UseWith));
                    string url = string.Empty;
                    string name = definitionNav.Value;
                    RelatedInformationLink relInfoLink = null;

                    if (definitionNav.Name == "RelatedExternalRef")
                        url = DocumentHelper.GetAttribute(definitionNav, xPathManager.GetXPath(RelatedInformationXPath.XRef));
                    else
                    {
                        // For RelatedDrugSummaryRef & RelatedSummaryRef the href are the cdrids.
                        // The documents are stored in database, so obtain the prettyurl 
                        // using the documentid
                        string cdrId = DocumentHelper.GetAttribute(definitionNav, xPathManager.GetXPath(RelatedInformationXPath.HRef));
                        int documentId = 0;

                        if (!Int32.TryParse(CDRHelper.ExtractCDRID(cdrId), out documentId))
                        {
                            throw new Exception("Failed to parse" +  definitionNav.Name + " document id from value: " + cdrId);
                        }

                        if (definitionNav.Name == "RelatedDrugSummaryRef")
                        {
                            DrugInfoSummaryQuery docQuery = new DrugInfoSummaryQuery();
                            DrugInfoSummaryDocument drugDoc = docQuery.GetDocumentData(documentId);
                            if (drugDoc != null)
                                url = drugDoc.PrettyURL;
                        }
                        else if (definitionNav.Name == "RelatedSummaryRef")
                        {
                            SummaryQuery docQuery = new SummaryQuery();
                            SummaryDocument summaryDoc = docQuery.GetDocumentData(documentId);
                            if (summaryDoc != null)
                                url = summaryDoc.PrettyURL;
                        }

                        //Cleanup up the URL that will be replaced by CDE at runtime.
                        // 1. Url may have domain name example http://cancer.gov/
                        if (!string.IsNullOrEmpty(url))
                        {
                            url = url.Trim().ToLower();
                            if (url.IndexOf("http://") > -1)
                                url = url.Substring(url.IndexOf("/", "http://".Length));

                        }
                        else
                        {
                            throw new Exception("Could not find document in database for " + definitionNav.Name + " document id from value: " + cdrId);
                        }
                    }

                    // Special condition: useWith is not applicable to RelatedDrugSummaryRef and also the fact 
                    // there are no Spanish drug summaries, so this ref will be applied only to English language.
                    if (definitionNav.Name == "RelatedDrugSummaryRef")
                        useWith = "en";

                    if (string.IsNullOrEmpty(useWith) || useWith == "en" )
                    {
                        relInfoLink = new RelatedInformationLink(name, url, Language.English);
                        glossaryTermDoc.GlossaryTermTranslationMap[Language.English].RelatedInformationList.Add(relInfoLink);
                    }

                    if (string.IsNullOrEmpty(useWith) || useWith == "es")
                    {
                        relInfoLink = new RelatedInformationLink(name, url, Language.Spanish);
                        glossaryTermDoc.GlossaryTermTranslationMap[Language.Spanish].RelatedInformationList.Add(relInfoLink);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error for RelatedInformation: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString(), e);
            }
        }

        #endregion

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
        /// Main extraction method.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="glossaryTermDocument"></param>
        public void Extract(XmlDocument xmlDoc, GlossaryTermDocument glossaryTermDocument, DocumentXPathManager xPath)
        {
            try
            {
                XPathNavigator xNav = xmlDoc.CreateNavigator();

                // Extract the CDR ID...
                xNav.MoveToFirstChild();
                if (CDRHelper.ExtractCDRID(xNav, xPath.GetXPath(CommonXPath.CDRID), out _documentID))
                {
                    glossaryTermDocument.DocumentID = _documentID;
                }
                else
                {
                    throw new Exception("Extraction Error: Failed to extract document (CDR) ID in glossary term document!");
                }

                DocumentHelper.CopyXml(xmlDoc, glossaryTermDocument);

                // Get Glossary Term XPath
                xPathManager = xPath;

                ExtractEnglishTerm(xNav, glossaryTermDocument);
                ExtractSpanishTerm(xNav, glossaryTermDocument);
                ExtractMediaLinks(xNav, glossaryTermDocument);
                ExtractRelatedInformation(xNav, glossaryTermDocument);

                // Handle modified and published dates
                DocumentHelper.ExtractDates(xNav, glossaryTermDocument, xPathManager.GetXPath(CommonXPath.LastModifiedDate), xPathManager.GetXPath(CommonXPath.FirstPublishedDate));
            }
            catch (Exception e)
            {
                throw new Exception("Extraction Error: Failed to extract glossary term document", e);
            }
        }

        #endregion
    }
}
