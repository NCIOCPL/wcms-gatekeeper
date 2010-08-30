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
            string html = string.Empty;

            string xml = Regex.Replace(content.OuterXml, "xmlns=\"(.+?)\"", "", RegexOptions.Singleline | RegexOptions.Compiled);

            // First check if the xml is well-formed or not. Return error if catch one.
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            try
            {
                document.LoadXml(xml);
            }
            catch (Exception e)
            {
                return "CDRPreview web service error: XML document is not well-formed. " + e.ToString();
            }

            // Verify the document with DTD
            string docURI = document.DocumentElement.Name.Trim();
            try
            {
                this.ValidateWithDTD(xml, docURI);
            }
            catch (Exception e)
            {
                return "CDRPreview web service error: XML is DTD validation failed. " + e.ToString();
            }

            if (errorMsg.Length > 0)
                return "CDRPreview web service error: " + errorMsg;


            // Switch to different document class calls based on the document type
            PreviewTypes documentType = GetDocumentType(template_type);
            switch (documentType)
            {
                case PreviewTypes.Summary:
                    html = RenderSummaryHTML(document);
                    break;
                case PreviewTypes.Protocol_HP:
                    html = RenderProtocolHTML(document, PreviewTypes.Protocol_HP);
                    break;
                case PreviewTypes.Protocol_Patient:
                    html = RenderProtocolHTML(document, PreviewTypes.Protocol_Patient);
                    break;
                case PreviewTypes.CTGovProtocol:
                    html = RenderCTGovProtocolHTML(document);
                    break;
                case PreviewTypes.GlossaryTerm:
                    html = RenderGlossaryTermHTML(document);
                    break;
                case PreviewTypes.DrugInfoSummary:
                    html = RenderDrugInfoSummaryHTML(document);
                    break;
                case PreviewTypes.GeneticsProfessional:
                    html = RenderGeneticsProfessional(document);
                    break;
                default:
                    html = "The document type + " + template_type + " is not supported.";
                    break;
            }

            // Added image server location
            html = html.Replace("/Common/PopUps/popImage.aspx?", GetServerURL() + "/Common/PopUps/popImage.aspx?");
            html = html.Replace("/images/spacer.gif", GetServerURL() + "/images/spacer.gif");
            html = html.Replace("/images/gray_spacer.gif", GetServerURL() + "/images/gray_spacer.gif");
            html = html.Replace("/images/backtotop_red.gif", GetServerURL() + "/images/backtotop_red.gif");
            html = html.Replace("/images/new_search_red.gif", GetServerURL() + "/images/new_search_red.gif");  

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
        /// This method calls Summary extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">Summary XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderSummaryHTML(XmlDocument document)
        {
           string html = string.Empty;
           DocumentXPathManager xPathManager = new DocumentXPathManager();

           SummaryDocument summary = new SummaryDocument();
           SummaryExtractor extractor= new SummaryExtractor();
           extractor.PrepareXml(document, xPathManager);
           extractor.Extract(document, summary);
           SummaryRenderer render = new SummaryRenderer();
           render.Render(summary);
           GenerateCDRPreview.SummaryPreview(ref html, summary);

           return html;
        }

        // <summary>
        /// This method calls Protocol extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">Protocol XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderProtocolHTML(XmlDocument document, PreviewTypes docType)
        {
            string html = string.Empty;
            DocumentXPathManager xPathManager = new DocumentXPathManager();

            ProtocolDocument protocol = new ProtocolDocument();
            ProtocolExtractor pe = new ProtocolExtractor(xPathManager);
            pe.Extract(document, protocol);
            ProtocolRenderer render = new ProtocolRenderer();
            render.Render(protocol);
            GenerateCDRPreview.ProtocolPreview(ref html, protocol, docType);

            return html;
        }

        // <summary>
        /// This method calls CTGovProtocol extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">CTGovProtocol XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderCTGovProtocolHTML(XmlDocument document)
        {
            string html = string.Empty;
            DocumentXPathManager xPathManager = new DocumentXPathManager();

            ProtocolDocument protocol = new ProtocolDocument();
            CTGovProtocolExtractor extractor = new CTGovProtocolExtractor(xPathManager);
            extractor.Extract(document, protocol);
            ProtocolRenderer render = new ProtocolRenderer();
            render.Render(protocol);
            GenerateCDRPreview.CTGovProtocolPreview(ref html, protocol);

            return html;
        }

        // <summary>
        /// This method calls GlossaryTerm extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">GlossaryTerm XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderGlossaryTermHTML(XmlDocument document)
        {
            string html = string.Empty;
            GlossaryTermDocument glossary = new GlossaryTermDocument();
            GlossaryTermExtractor extractor = new GlossaryTermExtractor();
            extractor.Extract(document, glossary, xPathManager);
            GlossaryTermRenderer gtRender = new GlossaryTermRenderer();
            gtRender.Render(glossary);
            GenerateCDRPreview.GlossaryPreview(ref html, glossary);
   
            return html;
        }

        /// <summary>
        /// This method calls DrugInfoSummary extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">DrugInfoSummary XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderDrugInfoSummaryHTML(XmlDocument document)
        {
            string html = string.Empty;
            DrugInfoSummaryDocument drug = new DrugInfoSummaryDocument();
            DrugInfoSummaryExtractor extractor = new DrugInfoSummaryExtractor();
            extractor.Extract(document, drug, xPathManager);
            DrugInfoSummaryRenderer render = new DrugInfoSummaryRenderer();
            render.Render(drug);
            GenerateCDRPreview.DrugInfoSummaryPreview(ref html, drug);

            return html;
        }

        /// <summary>
        /// This method calls the GeneticsProfessional extract/render/preview methods to obtain the final HTML for display
        /// </summary>
        /// <param name="document">GeneticsProfessional XmlDocument</param>
        /// <returns>HTML in string</returns>
        private string RenderGeneticsProfessional(XmlDocument document)
        {
            string html = string.Empty;

            GeneticsProfessionalDocument genProf = new GeneticsProfessionalDocument();
            GeneticsProfessionalExtractor extractor = new GeneticsProfessionalExtractor();
            extractor.Extract(document, genProf, xPathManager);
            GeneticsProfessionalRenderer render = new GeneticsProfessionalRenderer();
            render.Render(genProf);
            GenerateCDRPreview.GeneticsProfessionalPreview(ref html, genProf);

            return html;
        }

        // <summary>
        /// This method converts document template type from string to enum
        /// </summary>
        /// <param name="type">document type</param>
        /// <returns>document TemplateType</returns>
        private PreviewTypes GetDocumentType(string type)
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

            try {
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

			
        #endregion private methods
    }
}
