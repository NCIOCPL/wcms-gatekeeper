using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

using GateKeeper.Common;
using GateKeeper.DataAccess;
using GateKeeper.ContentRendering;
using GateKeeper.DocumentObjects;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DocumentObjects.Summary;
using GateKeeper.DocumentObjects.Protocol;
using GateKeeper.DocumentObjects.GeneticsProfessional;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.DrugInfoSummary;
using GKManagers.BusinessObjects;
using GKManagers;
using GKPreviews;
using GKManagers.CMSManager.Configuration;
using CDRPreviewWS.Common;
using System.Xml.XPath;
using GateKeeper.Common.XPathKeys;

namespace CDRPreviewWS
{

    /// <summary>
    /// CDR Preview Web Service allows previewing individual documents before they are sent to 
    /// Gatekeeper Web Service and published.
    /// </summary>
    [WebService(Namespace = "http://gatekeeper.cancer.gov/CDRPreview/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class CDRPreview : System.Web.Services.WebService
    {
        #region Private variables
        private DocumentXPathManager xPathManager = new DocumentXPathManager();
        private string errorMsg = string.Empty;
        #endregion
        //OCEPROJECT - 3197 - language gets updated for Spanish Summaries
        public string currentLanguage = "en";
        #region Public methods
        public CDRPreview()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();
        }

        // <summary>
        /// this web method receive xml data, process the data based on the document typ,  
        /// and returns HTML for preview page rendering
        /// </summary>
        /// <param name="content">xml document</param>
        /// <param name="template_type">xml document type</param>
        /// <returns>HTML response</returns>
        [WebMethod(Description = "Return document HTML based on the XML input")]
        public string ReturnXML(XmlNode content, string template_type)
        {
            string pageHtml = string.Empty;

            // Set the critical section 
            string headerHtml = string.Empty;
            // Generate the HTML for the document data 
            string contentHtml = ReturnHTML(content, template_type, ref headerHtml);
            pageHtml = GeneratePageHtml(contentHtml, headerHtml);
            return pageHtml;
        }

        // <summary>
        /// this web method receive xml data, process the data based on the document typ,  
        /// and returns HTML for preview page rendering
        /// <param name="content">xml document</param>
        /// <param name="template_type">xml document type</param>
        /// <returns>HTML response</returns>
        public string ReturnHTML(XmlNode content, string template_type, ref string headerHtml)
        {
            string html = string.Empty;
            string xml = String.Empty;
            if (!string.IsNullOrEmpty(content.NamespaceURI))
            {
                Regex regex = new Regex("xmlns=\"(.+?)\"", RegexOptions.Singleline);
                string xml1 = regex.Replace(content.OuterXml, "");
                xml = Regex.Replace(content.OuterXml, "xmlns=\"(.+?)\"", "", RegexOptions.Singleline | RegexOptions.Compiled);
            }
            else
                xml = content.OuterXml.Replace("xmlns=\"\"", String.Empty);

            // First check if the xml is well-formed or not. Return error if catch one.
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            try
            {
                document.LoadXml(xml);
            }
            catch (Exception e)
            {
                errorMsg = "CDRPreview web service error: XML document is not well-formed.";
                GKCDRPreviewLogBuilder.Instance.CreateError(this.GetType(), errorMsg, e);
                return errorMsg + e.ToString();
            }

            // Verify the document with DTD
            string docURI = document.DocumentElement.Name.Trim();
            try
            {
                this.ValidateWithDTD(xml, docURI);
            }
            catch (Exception e)
            {
                errorMsg = "CDRPreview web service error: XML DTD validation failed. ";
                GKCDRPreviewLogBuilder.Instance.CreateError(this.GetType(), errorMsg, e);
                return errorMsg + e.ToString();
            }

            if (errorMsg.Length > 0)
            {
                errorMsg = "CDRPreview web service error: Xml data validation error," + errorMsg;
                GKCDRPreviewLogBuilder.Instance.CreateError(this.GetType(), "Xml data validation error", errorMsg);
                return errorMsg;
            }


            // Switch to different document class calls based on the document type
            PreviewTypes documentType = GetDocumentType(template_type);

            try
            {
                switch (documentType)
                {
                    case PreviewTypes.Summary:
                        //OCEPROJECT - 3197 - language gets updated for Spanish Summaries
                        XPathNavigator xNav = document.CreateNavigator();
                        xNav.MoveToFirstChild();
                        Language summaryLanguage;
                        string path = xPathManager.GetXPath(SummaryXPath.Lang);
                        summaryLanguage = DocumentHelper.DetermineLanguageString(DocumentHelper.GetXmlDocumentValue(xNav, path));
                        if (summaryLanguage == Language.Spanish)
                            currentLanguage = "es";

                        html = RenderSummaryHTML(document, ref headerHtml);
                        break;
                    case PreviewTypes.Protocol_HP:
                        html = RenderProtocolHTML(document, ref headerHtml, PreviewTypes.Protocol_HP);
                        break;
                    case PreviewTypes.Protocol_Patient:
                        html = RenderProtocolHTML(document, ref headerHtml, PreviewTypes.Protocol_Patient);
                        break;
                    case PreviewTypes.CTGovProtocol:
                        html = RenderCTGovProtocolHTML(document, ref headerHtml);
                        break;
                    case PreviewTypes.GlossaryTerm:
                        html = RenderGlossaryTermHTML(document, ref headerHtml);
                        break;
                    case PreviewTypes.DrugInfoSummary:
                        html = RenderDrugInfoSummaryHTML(document, ref headerHtml);
                        break;
                    case PreviewTypes.GeneticsProfessional:
                        html = RenderGeneticsProfessional(document, ref headerHtml);
                        break;
                    default:
                        errorMsg = "CDRPreview web service error: The document type " + template_type + " is not supported.";
                        return errorMsg;
                }
            }
            catch (Exception ex)
            {
                string message = "CDRPreview web service error:There was error generating Preview html for template type " + template_type + " at " + DateTime.Now.ToLongTimeString();
                message += " Error Message: " + ex.Message;
                GKCDRPreviewLogBuilder.Instance.CreateError(this.GetType(), "ReturnXML", message, ex);
                throw new Exception(message, ex);
            }

            string currentHost = this.Context.Request.Url.GetLeftPart(UriPartial.Authority);

            // make all image urls absolute. 
            if (!string.IsNullOrEmpty(headerHtml))
            {
                headerHtml = headerHtml.Replace("/images/", GetServerURL() + "/images/");
                headerHtml = headerHtml.Replace("/PublishedContent/Images/SharedItems/ContentHeaders/", GetServerURL() + "/PublishedContent/Images/SharedItems/ContentHeaders/");
            }

            // Added image server location
            html = html.Replace("/Common/PopUps/popImage.aspx?", GetServerURL() + "/Common/PopUps/popImage.aspx?");
            html = html.Replace("/images/spacer.gif", GetServerURL() + "/images/spacer.gif");
            html = html.Replace("/images/gray_spacer.gif", GetServerURL() + "/images/gray_spacer.gif");
                      
            html = html.Replace("/images/audio-icon.gif", "images/audio-icon.gif");
            html = html.Replace("images/audio-icon.gif", GetServerURL() + "/images/audio-icon.gif");

            html = html.Replace("/images/tabs-beginning.gif", GetServerURL() + "/images/tabs-beginning.gif");
            html = html.Replace("/images/health-professional-on.gif", GetServerURL() + "/images/health-professional-on.gif");
            html = html.Replace("/images/tabs-end-white.gif", GetServerURL() + "/images/tabs-end-white.gif");
            html = html.Replace("/images/patient-version-on.gif", GetServerURL() + "/images/patient-version-on.gif");

            html = html.Replace("/images/tabs-transition-white-gray.gif", GetServerURL() + "/images/tabs-transition-white-gray.gif");
            html = html.Replace("/images/tabs-end-gray.gif", GetServerURL() + "/images/tabs-end-gray.gif");

            string imageContentLocation = ConfigurationManager.AppSettings["PreviewEnlargeImageContentLocation"];
            html = html.Replace("/PublishedContent/MediaLinks", currentHost + "/CDRPreviewWS/" + imageContentLocation);

            // For Image media the image url references should be pointing to CDR server
            string imagePath = ConfigurationManager.AppSettings["ImageLocation"];
            html = html.Replace("/images/cdr/live/", imagePath);

            html = html.Replace("/Common/PopUps/popDefinition.aspx?", GetServerURL() + "/Common/PopUps/popDefinition.aspx?");

            return html;
        }

        #endregion

        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region private methods

        // <summary>
        /// This method calls Protocol extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">Protocol XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderProtocolHTML(XmlDocument document, ref string headerHtml, PreviewTypes docType)
        {
            string html = string.Empty;
            DocumentXPathManager xPathManager = new DocumentXPathManager();

            ProtocolDocument protocol = new ProtocolDocument();
            ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
            pe.Extract(document, protocol);
            ProtocolRenderer render = new ProtocolRenderer();
            render.Render(protocol);
            GenerateCDRPreview.ProtocolPreview(ref html, ref headerHtml, protocol, docType);

            return html;
        }

        // <summary>
        /// This method calls CTGovProtocol extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">CTGovProtocol XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderCTGovProtocolHTML(XmlDocument document, ref string headerHtml)
        {
            string html = string.Empty;
            DocumentXPathManager xPathManager = new DocumentXPathManager();

            ProtocolDocument protocol = new ProtocolDocument();
            CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
            extractor.Extract(document, protocol);
            ProtocolRenderer render = new ProtocolRenderer();
            render.Render(protocol);
            GenerateCDRPreview.CTGovProtocolPreview(ref html, ref headerHtml, protocol);

            return html;
        }

        // <summary>
        /// This method calls GlossaryTerm extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">GlossaryTerm XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderGlossaryTermHTML(XmlDocument document, ref string headerHtml)
        {
            string html = string.Empty;
            GlossaryTermDocument glossary = new GlossaryTermDocument();
            GlossaryTermExtractor extractor = new GlossaryTermExtractor();
            extractor.Extract(document, glossary, xPathManager);
            GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
            gtRender.Render(glossary);
            GenerateCDRPreview.GlossaryPreview(ref html, ref headerHtml, glossary);

            return html;
        }

        // <summary>
        /// This method calls Summary extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">Summary XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderSummaryHTML(XmlDocument document, ref string headerHtml)
        {
            string contentHtml = string.Empty;
            headerHtml = string.Empty;
            SummaryDocumentPreview summaryPreview = new SummaryDocumentPreview(document, "");
            summaryPreview.Preview(ref contentHtml, ref headerHtml);
            return contentHtml;
        }

        /// <summary>
        /// This method calls DrugInfoSummary extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">DrugInfoSummary XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderDrugInfoSummaryHTML(XmlDocument document, ref string headerHtml)
        {
            string contentHtml = string.Empty;
            headerHtml = string.Empty;
            DrugInfoSummaryDocumentPreview drugPreview = new DrugInfoSummaryDocumentPreview(document, "");
            drugPreview.Preview(ref contentHtml, ref headerHtml);
            return contentHtml;
        }

        /// <summary>
        /// This method calls the GeneticsProfessional extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">GeneticsProfessional XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderGeneticsProfessional(XmlDocument document, ref string headerHtml)
        {
            string html = string.Empty;

            GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
            GeneticsProfessionalExtractor extractor = new GeneticsProfessionalExtractor();
            extractor.Extract(document, genProf, xPathManager);
            GeneticsProfessionalRenderer render = new GeneticsProfessionalRenderer();
            render.Render(genProf);
            GenerateCDRPreview.GeneticsProfessionalPreview(ref html, ref headerHtml, genProf);

            return html;
        }

        // <summary>
        /// This method converts document template type from string to enum
        /// </summary>
        /// <param name="type">document type</param>
        /// <returns>document TemplateType</returns>
        public PreviewTypes GetDocumentType(string type)
        {
            // Rather than maintain a list of if-else checks, we'll let the framework
            // do the heavy lifting of converting the string to an enum value.

            PreviewTypes previewType;

            try
            {
                previewType = (PreviewTypes)Enum.Parse(typeof(PreviewTypes), type, true);
            }
            catch (Exception)
            {
                previewType = PreviewTypes.None;
            }

            return previewType;
        }


        /// <summary>
        /// Validate XML against DTD
        /// </summary>
        /// <param name="document"></param>
        /// <param name="docType"></param>
        /// <return></return>
        private void ValidateWithDTD(string document, string docType)
        {
            XmlReader reader = null;
            StringReader xmlStr = null;

            try
            {
                // Get the dtd location
                string dtdURI = ConfigurationManager.AppSettings["DTDLocation"];

                document = Regex.Replace(document, "xmlns=\"(.+?)\"", "", RegexOptions.Singleline | RegexOptions.Compiled);
                document = "<!DOCTYPE " + docType + " SYSTEM \"" + dtdURI + "\">\n" + document;
                xmlStr = new StringReader(document);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ProhibitDtd = false;
                settings.ValidationType = ValidationType.DTD;
                settings.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);

                reader = XmlReader.Create(xmlStr, settings);
                while (reader.Read()) { }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (xmlStr != null)
                {
                    xmlStr.Close();
                    xmlStr.Dispose();
                }
            }
        }

        /// <summary>
        /// Display any validation errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <return></return>
        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            errorMsg = args.Message;

            if (args.Severity == XmlSeverityType.Warning)
            {
                errorMsg += "No schema found to enforce validation.";
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                errorMsg += "Validation error occurred when validating the instance document.";
            }

            if (args.Exception != null) // XSD schema validation error
            {
                errorMsg += args.Exception.SourceUri + "," + args.Exception.LinePosition + "," + args.Exception.LineNumber;
            }
        }

        /// <summary>
        /// Retrieve web service server url from configuration file
        /// </summary>
        /// <param></param>
        /// <returns>Return server url</returns>
        private string GetServerURL()
        {
            return ConfigurationManager.AppSettings["ServerURL"];
        }

        /// <summary>
        /// Generate the complete HTML including the frame html alonf with the content html and 
        /// the content header
        /// </summary>
        /// <returns>The string which contains the complete page html.</returns>
        private string GeneratePageHtml(string contentHtml, string headerHtml)
        {
            // Generate the outer or skeleton HTML so document HTML can be embedded.
            // To generate the outer html a .net aspx page which contains the outer html is requested or processed. 
            // using .apsx page is ideal since it can be updated quickly instead of hard coding html in 
            // the code.
            String response = string.Empty;
            PercussionConfig percussionConfig = (PercussionConfig)System.Configuration.ConfigurationManager.GetSection("PercussionConfig");

            try
            {
                Uri uri = this.Context.Request.Url;
                //string url = uri.Scheme + "://" + uri.Host + ":" + uri.Port.ToString() + "/CDRPreviewWS/CGovHtml.aspx";
                string url = percussionConfig.PreviewSettings.FrameHtmlPage.Value;
                WebClient webClient = new WebClient();
                using (Stream stream = webClient.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        response = reader.ReadToEnd();
                        response = response.Replace("CONTENT HTML", contentHtml);
                        response = response.Replace("HEADER HTML", headerHtml);
                        //OCEPROJECT - 3197 - pass the current language back to the page
                        //the language gets updated for Spanish Summaries
                        response = response.Replace("CURRENT LANGUAGE", currentLanguage);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                {
                    switch (((HttpWebResponse)ex.Response).StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            response = "The page used to generate the frame or outer html was not found:" + percussionConfig.PreviewSettings.FrameHtmlPage.Value;
                            break;

                        default:
                            throw ex;
                    }
                }
            }

            // When there is exception or error , blank the "CONTENT HTML" & "HEADER HTML"
            response = response.Replace("CONTENT HTML", string.Empty);
            response = response.Replace("HEADER HTML", string.Empty);

            return response;
        }

        #endregion private methods
    }
}
