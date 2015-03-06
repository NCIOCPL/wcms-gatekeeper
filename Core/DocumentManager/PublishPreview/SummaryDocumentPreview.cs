using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Summary;
using GKManagers.BusinessObjects;
using GateKeeper.ContentRendering;
using GateKeeper.DataAccess.CDR;
using GKManagers.CMSDocumentProcessing;
using NCI.WCM.CMSManager.CMS;

namespace GKPreviews
{
    /// <summary>
    /// This class provides functions which process a summary document for preview purposes.
    /// </summary>
    public class SummaryDocumentPreview : DocumentPreviewBase
    {
        public SummaryDocumentPreview(XmlDocument documentData, string userName)
            : base(documentData, userName)
        {

        }

        /// <summary>
        /// A concrete implementation of the base class method of the same name. This
        /// method is used to preform all the pre and post processing steps of a document before 
        /// the document submitted to the preview processor.
        /// </summary>
        protected override void ProcessPreview(ref string contentHtml, ref string headerContent)
        {
            contentHtml = string.Empty;

            SummaryDocument summary = new SummaryDocument();

            SummaryPreviewExtractor extractor = new SummaryPreviewExtractor();
            extractor.Extract(DocumentData, summary, DocXPathManager, TargetedDevice.screen);

            SummaryRenderer render = new SummaryRenderer();
            render.Render(summary);

            PercussionGuid contentItemGuid = null;

            // Save summary data into the Percussion CMS.
            using (CancerInfoSummaryPreviewProcessor processor = new CancerInfoSummaryPreviewProcessor(WriteHistoryWarningEntry, WriteHistoryInformationEntry))
            {
                try
                {
                    processor.ProcessDocument(summary, ref contentItemGuid);

                    // Generate the CMS HTML rendering of the content item
                    contentHtml = processor.ProcessCMSPreview(summary, contentItemGuid);

                    //NVCG update- Add dates to the bottom of the page and remove them from the header content
                    string updatedText = "Updated";
                    string date = String.Format("{0:MMMM dd, yyyy}", summary.LastModifiedDate);

                    if (summary.Language == Language.Spanish)
                    {
                        updatedText = "Actualización";
                        date = String.Format("{0:MMMM dd, yyyy}", CovertToSpanishFormat(summary.LastModifiedDate));
                    }

                    contentHtml += "<div id=\"cgvDate\"><div class=\"document-dates horizontal\"><div class=\"document-dates horizontal\"><ul class=\"clearfix\">";
                    if (summary.LastModifiedDate != DateTime.MinValue && summary.LastModifiedDate != null)
                        contentHtml += string.Format("<li><strong>{0}:</strong> {1}</li>", updatedText, date);
                    contentHtml += "</ul></div></div></div>";

                    headerContent = createHeaderZoneContent(summary);

                }
                catch (Exception ex)
                {
                    throw new Exception(" Preview generation failed for document Id:" + summary.DocumentID.ToString(), ex);                
                }
                finally 
                {
                    //// For Pub Preview the document will be removed from CMS once 
                    //// the job of generating the preview html is complete.
                    processor.DeleteContentItem(contentItemGuid);
                }
            }

        }

        #region Private Members
        private string createHeaderZoneContent(SummaryDocument document)
        {
            string html = "";
        //    // Create the content header html content
        //    string html = string.Format("<div id=\"cgvcontentheader\"><div class=\"document-title-block\" style=\"background-color:#d4d9d9\" >" +
        //"<img src=\"/PublishedContent/Images/SharedItems/ContentHeaders/{0}\" alt=\"\" style=\"border-width:0px;\" />" +
        //"<h1>{1}</h1></div></div>", "title_cancertopics.jpg", document.Title);

        //    // Create the cgvLanguageDate html content
        //    string audience = "Health Professional";
        //    if (document.Language == Language.Spanish)
        //        audience = "Profesionales de salud";

        //    if (document.AudienceType == "Patients")
        //    {
        //        audience = "Patient";
        //        if (document.Language == Language.Spanish)
        //            audience = "Pacientes";
        //    }

            //string updatedText = "Last Modified";
            //string date = String.Format("{0:MM/dd/yyyy}", document.LastModifiedDate);

            //if (document.Language == Language.Spanish)
            //{
            //    updatedText = "Actualizado";
            //    date = String.Format("{0:MM/dd/yyyy}", CovertToSpanishFormat(document.LastModifiedDate));
            //}


            //html += string.Format("<div id=\"cgvLanguageDate\"><div class=\"language-dates\"><div class=\"version-language\">" +
            //     "<ul><li class=\"one active\">{0}</li></ul></div>", audience);

            ////html += "<div class=\"document-dates\"><ul>";
            ////if (document.LastModifiedDate != DateTime.MinValue && document.LastModifiedDate != null)
            ////    html += string.Format("<li><strong>{0}:</strong> {1}</li>", updatedText, date);
            ////html += "</ul></div></div></div>";

            //html += "</div></div>";

            return html;
        }

        
        private string CovertToSpanishFormat(DateTime Date)
        {
            string spanishDate = string.Empty;

            spanishDate = Date.Day + " de " + GetSpanishMonth(Date.Month) + " de " + Date.Year;

            return spanishDate;
        }
        private string GetSpanishMonth(int month)
        {
            string spanishMonth = string.Empty;
            switch (month)
            {
                case 1:
                    spanishMonth = "enero";
                    break;
                case 2:
                    spanishMonth = "febrero";
                    break;
                case 3:
                    spanishMonth = "marzo";
                    break;
                case 4:
                    spanishMonth = "abril";
                    break;
                case 5:
                    spanishMonth = "mayo";
                    break;
                case 6:
                    spanishMonth = "junio";
                    break;
                case 7:
                    spanishMonth = "julio";
                    break;
                case 8:
                    spanishMonth = "agosto";
                    break;
                case 9:
                    spanishMonth = "septiembre";
                    break;
                case 10:
                    spanishMonth = "octubre";
                    break;
                case 11:
                    spanishMonth = "noviembre";
                    break;
                case 12:
                    spanishMonth = "diciembre";
                    break;
                default:
                    spanishMonth = "";
                    break;
            }

            return spanishMonth;
        }
        #endregion
    }

}
