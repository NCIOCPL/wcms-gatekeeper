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

                    // Get the audience this link is intended for.
                    AudienceType audience = GetMediaLinkAudienceType(mediaLinkIter.Current);

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
                                glossaryTermDoc.GlossaryTermTranslationMap[lang].MediaLinkList.Add(audience, mediaLink);
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
                            glossaryTermDoc.GlossaryTermTranslationMap[lang].MediaLinkList.Add(audience, mediaLink);
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

        /// <summary>
        /// Retrieves the intended audience for a MediaLink element.  If no audience is specified, default to Patient.
        /// </summary>
        /// <param name="element">Reference to a MedialLink element.</param>
        /// <returns>A value of AudienceType.  If no audience is specified, AudienceType.Patient is returned.</returns>
        private AudienceType GetMediaLinkAudienceType(XPathNavigator element)
        {
            string audienceTmp = DocumentHelper.GetAttribute(element, xPathManager.GetXPath(GlossaryTermXPath.MediaAudience));

            // Convert string to MediaLink.AudienceType.  If the value is not recognized, default to Patient.
            // The text values used in the MediaLink XML element are not compatible with GateKeeper's AudienceType.
            // Using MediaLink.AudienceType enum as an intermediary allows us to avoid writing a string comparison.
            MediaLink.AudienceType intermediary = ConvertEnum<MediaLink.AudienceType>.Convert(audienceTmp, MediaLink.AudienceType.Patient);

            // Convert to GateKeeper AudienceType before returning.
            return intermediary == MediaLink.AudienceType.Patient ? AudienceType.Patient : AudienceType.HealthProfessional;
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
                    // Look up the list of languages to use.
                    string useWith = DocumentHelper.GetAttribute(definitionNav, xPathManager.GetXPath(RelatedInformationXPath.UseWith));
                    Language[] applyToLanguage = DecodeLanguageToUse(DocumentHelper.GetAttribute(definitionNav, xPathManager.GetXPath(RelatedInformationXPath.UseWith)));

                    string name = definitionNav.Value;
                    RelatedInformationLink.RelatedLinkType linkType = GetRelatedLinkType(definitionNav.Name);

                    if (linkType == RelatedInformationLink.RelatedLinkType.External)
                    {
                        String url = DocumentHelper.GetAttribute(definitionNav, xPathManager.GetXPath(RelatedInformationXPath.XRef));
                        linkType = RelatedInformationLink.RelatedLinkType.External;
                        StoreRelatedExternalLink(glossaryTermDoc, name, url, applyToLanguage);
                    }
                    else if (linkType == RelatedInformationLink.RelatedLinkType.DrugInfoSummary ||
                        linkType == RelatedInformationLink.RelatedLinkType.Summary)
                    {
                        StoreRelatedPDQLinkSimple(glossaryTermDoc, definitionNav, name, linkType, applyToLanguage);
                    }
                    else if(linkType == RelatedInformationLink.RelatedLinkType.GlossaryTerm)
                    {
                        StoreRelatedGlossaryTerm(glossaryTermDoc, definitionNav, applyToLanguage);
                    }
                }
            }
            catch (Exception e)
            {
                //swallow this exception Glossary Term should be allowed to process even
                //if related information fails
                glossaryTermDoc.WarningWriter("Extraction Error for RelatedInformation: Extracting " + path + " failed.  Document CDRID=" + _documentID.ToString());
                glossaryTermDoc.WarningWriter(e.ToString());
            }
        }

        /// <summary>
        /// Implements the extraction of related glossary terms.
        /// </summary>
        /// <param name="glossaryTermDoc">The parent glossary term</param>
        /// <param name="linkNav">An XmlNavigator pointing to the node containing the related item's reference.</param>
        /// <param name="applyToLanguage">The list of languages the link with which the link is marked to appear.
        /// This differ from the list of languages supported by both the parent and the related terms.</param>
        private void StoreRelatedGlossaryTerm(GlossaryTermDocument glossaryTermDoc,
            XPathNavigator linkNav, Language[] applyToLanguage)
        {
            // Use the document id to look up the prettyurl
            string cdrId = DocumentHelper.GetAttribute(linkNav, xPathManager.GetXPath(RelatedInformationXPath.HRef));
            int documentId = 0;

            if (!Int32.TryParse(CDRHelper.ExtractCDRID(cdrId), out documentId))
                throw new Exception("Failed to parse" + linkNav.Name + " document id from value: " + cdrId);

            // Fetch the referenced glossary term so we can determine audiences, languages, etc.
            GlossaryTermSimple relatedTerm = getRelatedGlossaryTerm(documentId, glossaryTermDoc.WarningWriter);

            // The related glossary term only appears in the translations where it supports the language of that
            // translation.  It also only appears when it has the same target audience as the definition.
            // What ends up happening is that we check through the list of languages
            foreach (Language lang in applyToLanguage)
            {
                // Make sure we're using the right version of the term name.
                String termName = lang == Language.English ? relatedTerm.Name : relatedTerm.SpanishName;

                if (glossaryTermDoc.GlossaryTermTranslationMap.ContainsKey(lang))
                {
                    GlossaryTermTranslation trans = glossaryTermDoc.GlossaryTermTranslationMap[lang];

                    // The related glossary term only appears with definitions where the
                    // related term supports the definition's targeted audience.
                    foreach (GlossaryTermDefinition def in trans.DefinitionList)
                    {
                        RelatedInformationLink relInfoLink = null;

                        // It is assumed that each definition has exactly 1 audience.
                        AudienceType targetAudience = def.AudienceTypeList[0];
                        switch (targetAudience)
                        {
                            case AudienceType.HealthProfessional:
                                if (relatedTerm.HasHealthProfessionalDefinition)
                                {
                                    // Health Professional is always English, always /geneticsdictionary.
                                    String url = String.Format("/geneticsdictionary?cdrid={0}", documentId);
                                    relInfoLink = new RelatedInformationLink(termName, url, Language.English, RelatedInformationLink.RelatedLinkType.GlossaryTerm);
                                }
                                break;
                            case AudienceType.Patient:
                                if (relatedTerm.HasPatientDefinition)
                                {
                                    String url = null;

                                    // Patient always has an English term, the Spanish term is optional
                                    if (lang == Language.English)
                                        url = String.Format("/dictionary?CdrID={0}", documentId);
                                    else if (lang == Language.Spanish && relatedTerm.HasSpanishTerm)
                                        url = String.Format("/diccionario?CdrID={0}", documentId);

                                    // Check whether there is a related link for the current language.
                                    if (url != null)
                                        relInfoLink = new RelatedInformationLink(termName, url, lang, RelatedInformationLink.RelatedLinkType.GlossaryTerm);
                                }
                                break;
                        }

                        // Was a related link created for the current definition?
                        if (relInfoLink != null)
                            def.RelatedInformationList.Add(relInfoLink);
                    }
                }
            }
        }

        /// <summary>
        /// Implements the extraction of related links to summaries and drug info summaries.
        /// </summary>
        /// <param name="glossaryTermDoc">The parent glossary term</param>
        /// <param name="linkNav">An XmlNavigator pointing to the node containing the related item's reference.</param>
        /// <param name="name">The name to use when displaying the link to the related item.</param>
        /// <param name="linkType">The type of document in the related link.</param>
        /// <param name="applyToLanguage">The list of languages the link with which the link is marked to appear.</param>
        private void StoreRelatedPDQLinkSimple(GlossaryTermDocument glossaryTermDoc,
            XPathNavigator linkNav, String name, RelatedInformationLink.RelatedLinkType linkType, Language[] applyToLanguage)
        {
            String url = String.Empty;

            // Use the document id to look up the prettyurl
            string cdrId = DocumentHelper.GetAttribute(linkNav, xPathManager.GetXPath(RelatedInformationXPath.HRef));
            int documentId = 0;

            if (!Int32.TryParse(CDRHelper.ExtractCDRID(cdrId), out documentId))
                throw new Exception("Failed to parse" + linkNav.Name + " document id from value: " + cdrId);

            switch (linkType)
            {
                case RelatedInformationLink.RelatedLinkType.Summary:
                    url = getRelatedSummaryUrl(documentId);
                    break;
                case RelatedInformationLink.RelatedLinkType.DrugInfoSummary:
                    url = getRelatedDrugInfoSummaryUrl(documentId);
                    break;
                default:
                    throw new Exception("Error in Extract: Unexpected document type: " + linkType.ToString());
            }

            // Trim any leading hostname (e.g. http://cancer.gov/foo becomes /foo)
            if (!string.IsNullOrEmpty(url))
            {
                url = url.Trim().ToLower();

                if (url.IndexOf("http://") > -1)
                    url = url.Substring(url.IndexOf("/", "http://".Length));
            }
            else
                throw new Exception("Could not find document url for " + linkNav.Name + " document id from value: " + cdrId);

            // Special condition: RelatedDrugSummaryRef are English-only because there are Spanish drug summaries
            // Force these ref to only be applied to English.
            if (linkType == RelatedInformationLink.RelatedLinkType.DrugInfoSummary)
                applyToLanguage = new Language[] { Language.English };


            foreach (Language lang in applyToLanguage)
            {
                if (glossaryTermDoc.GlossaryTermTranslationMap.ContainsKey(lang))
                {
                    GlossaryTermTranslation trans = glossaryTermDoc.GlossaryTermTranslationMap[lang];
                    RelatedInformationLink relInfoLink = new RelatedInformationLink(name, url, lang, RelatedInformationLink.RelatedLinkType.External);

                    // External links are applied to all audiences.
                    foreach (GlossaryTermDefinition def in trans.DefinitionList)
                    {
                        def.RelatedInformationList.Add(relInfoLink);
                    }
                }
            }
        }

        /// <summary>
        /// Implements the extraction of related external links.
        /// </summary>
        /// <param name="glossaryTermDoc">The parent glossary term</param>
        /// <param name="name">The term name.</param>
        /// <param name="url">The external link</param>
        /// <param name="applyToLanguage">The list of languages the link with which the link is marked to appear.</param>
        private void StoreRelatedExternalLink(GlossaryTermDocument glossaryTermDoc, String name, String url, Language[] applyToLanguage)
        {
            foreach (Language lang in applyToLanguage)
            {
                if (glossaryTermDoc.GlossaryTermTranslationMap.ContainsKey(lang))
                {
                    GlossaryTermTranslation trans = glossaryTermDoc.GlossaryTermTranslationMap[lang];
                    RelatedInformationLink relInfoLink = new RelatedInformationLink(name, url, lang, RelatedInformationLink.RelatedLinkType.External);

                    // External links are applied to all audiences.
                    foreach (GlossaryTermDefinition def in trans.DefinitionList)
                    {
                        def.RelatedInformationList.Add(relInfoLink);
                    }
                }
            }
        }

        /// <summary>
        /// Examines a relationship reference name and find the RelatedInformationLink.RelatedLinkType enum value
        /// matching that relationship type.
        /// </summary>
        /// <param name="linkTypeName">String containing the relationship refernce name.</param>
        /// <returns>A RelatedInformationLink.RelatedLinkType value.  Throws an exception if the type is not known.</returns>
        RelatedInformationLink.RelatedLinkType GetRelatedLinkType(String linkTypeName)
        {
            RelatedInformationLink.RelatedLinkType linkType;

            if (string.Compare("RelatedExternalRef", linkTypeName) == 0)
                linkType = RelatedInformationLink.RelatedLinkType.External;
            else if (string.Compare("RelatedDrugSummaryRef", linkTypeName) == 0)
                linkType = RelatedInformationLink.RelatedLinkType.DrugInfoSummary;
            else if (string.Compare("RelatedSummaryRef", linkTypeName) == 0)
                linkType = RelatedInformationLink.RelatedLinkType.Summary;
            else if (string.Compare("RelatedGlossaryTermRef", linkTypeName) == 0)
                linkType = RelatedInformationLink.RelatedLinkType.GlossaryTerm;
            else
            {
                // This really should be something derived from Exception, but this part of the system
                // isn't architected that way. :-/
                throw new Exception("Extraction Error for RelatedInformation: Unknown link type: " + linkTypeName);
            }

            return linkType;
        }

        /// <summary>
        /// Converts a (possibly empty) two-character language code into an array of Language values.
        /// A blank language code is interpreted as "All known languages."
        /// </summary>
        /// <param name="languageCode">The two-character language code</param>
        /// <returns>An array of Language values.</returns>
        private Language[] DecodeLanguageToUse(String languageCode)
        {
            List<Language> languageList = new List<Language>();
            languageCode = languageCode.Trim();

            // A blank language code is interpreted as "All known languages."
            if (string.IsNullOrEmpty(languageCode))
            {
                languageList.Add(Language.English);
                languageList.Add(Language.Spanish);
            }
            else if (string.Compare("en", languageCode) == 0)
                languageList.Add(Language.English);
            else if (string.Compare("es", languageCode) == 0)
                languageList.Add(Language.Spanish);

            return languageList.ToArray();
        }

        /// <summary>
        /// Helper method to look up the pretty URL for a related summary.
        /// </summary>
        /// <param name="documentId">The document's CDR ID.</param>
        /// <returns>A string containing the pretty URL of the specified document.</returns>
        private String getRelatedSummaryUrl(int documentId)
        {
            String url = string.Empty;

            SummaryQuery docQuery = new SummaryQuery();
            SummaryDocument summaryDoc = docQuery.GetDocumentData(documentId);
            if (summaryDoc != null)
                url = summaryDoc.PrettyURL;

            return url;
        }

        /// <summary>
        /// Helper method to look up the pretty URL for a related drug info summary.
        /// </summary>
        /// <param name="documentId">The document's CDR ID.</param>
        /// <returns>A string containing the pretty URL of the specified document.</returns>
        private String getRelatedDrugInfoSummaryUrl(int documentId)
        {
            String url = String.Empty;
            DrugInfoSummaryQuery docQuery = new DrugInfoSummaryQuery();
            DrugInfoSummaryDocument drugDoc = docQuery.GetDocumentData(documentId);
            if (drugDoc != null)
                url = drugDoc.PrettyURL;

            return url;
        }

        /// <summary>
        /// Helper method to look up the pretty URLs and names for a related glossary term.
        /// (Looks up both English and Spanish names.)
        /// Yes, the out params are less than ideal, but creating a new type for this
        /// single method, for a single extract, is even less ideal.
        /// </summary>
        /// <param name="documentId">The term's CDR ID.</param>
        /// <param name="englishName">Term's english name [Out].</param>
        /// <param name="englishUrl">Dictionary URL for the english term [Out].</param>
        /// <param name="spanishName">Term's spanish name [Out].</param>
        /// <param name="spanishUrl">Dictionary URL for the english term [Out].</param>
        private GlossaryTermSimple getRelatedGlossaryTerm(int documentId, HistoryEntryWriter logWriter)
        {
            GlossaryTermSimple term =null;
            try
            {
                GlossaryTermQuery query = new GlossaryTermQuery();
                term = query.GetGlossaryTerm(documentId);
            }
            catch (Exception ex)
            {
                // Swallow the exception.  Not finding the related term shouldn't be fatal.
                logWriter("HistoryEntryWriter.getRelatedGlossaryTerm(). Error retrieveing documentId = " + documentId + ". " + ex.ToString());
            }
            return term;
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
