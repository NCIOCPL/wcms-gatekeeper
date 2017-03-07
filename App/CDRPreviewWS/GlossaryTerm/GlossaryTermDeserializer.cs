using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using GateKeeper.DocumentObjects.GlossaryTerm;
using GateKeeper.DocumentObjects.Dictionary;
using CDRPreviewWS.GlossaryTerm.BusinessObjects;
using Newtonsoft.Json;
using System.Configuration;

namespace CDRPreviewWS.GlossaryTerm
{
    public class GlossaryTermDeserializer
    {
        public string GenerateGlossaryTermPreview(GlossaryTermDocument glossary)
        {
            string mediaPath = ConfigurationManager.AppSettings["MediaLocation"];
            string imagePath = ConfigurationManager.AppSettings["ImageLocation"];

            StringBuilder glossaryTermHtml = new StringBuilder();
            if (glossary != null && glossary.Dictionary != null)
            {
                DictionaryTerm dictionaryTerm = null;
                
                foreach (GeneralDictionaryEntry item in glossary.Dictionary)
                {
                    try
                    {
                        //deserialize term details as returned by the service layer 
                        dictionaryTerm = JsonConvert.DeserializeObject<DictionaryTerm>(item.Object);

                        if (dictionaryTerm != null)
                        {
                            //Stinky code - put together the entire html in a giant StringBuilder
                            glossaryTermHtml.Append("<div class=\"results clearfix\">");
                            glossaryTermHtml.Append("<dl class=\"dictionary-list\"><dt>");
                            glossaryTermHtml.Append("<dfn>" + dictionaryTerm.Term + "</dfn>");

                            if (dictionaryTerm.HasPronunciation)
                            {
                                glossaryTermHtml.Append("<dd class=\"pronunciation\">");

                                if (dictionaryTerm.Pronunciation.HasAudio)
                                {
                                    string audioIcon = dictionaryTerm.Pronunciation.Audio.Replace("[__audiolocation]", mediaPath + "/");

                                    glossaryTermHtml.Append("<a href=\"" + audioIcon);
                                    glossaryTermHtml.Append("\" class=\"CDR_audiofile\"><span class=\"hidden\">listen</span></a>");
                                }
                                if (dictionaryTerm.Pronunciation.HasKey)
                                {
                                    glossaryTermHtml.Append("<span>" + dictionaryTerm.Pronunciation.Key + "</span>");
                                }

                                glossaryTermHtml.Append("</dd>");
                            }

                            glossaryTermHtml.Append("<dd class=\"definition\">" + dictionaryTerm.Definition.Text + "<br/>");
                            
                            //display the related information 
                            //when atleast one of the related items exists
                            if (dictionaryTerm.Related.Term.Length > 0 ||
                                dictionaryTerm.Related.Summary.Length > 0 ||
                                dictionaryTerm.Related.DrugSummary.Length > 0 ||
                                dictionaryTerm.Related.External.Length > 0 ||
                                dictionaryTerm.Images.Length > 0 ||
                                dictionaryTerm.HasVideos )
                            {
                                string moreInformationText = "More Information";
                                if (item.Language == GateKeeper.DocumentObjects.Language.Spanish)
                                    moreInformationText = "M&aacute;s informaci&oacute;n";
                                glossaryTermHtml.Append("<div id=\"pnlRelatedInfo\"><br/><div class=\"related-resources\">");

                                //don't display more information text when only images are being displayed
                                if (dictionaryTerm.Related.Term.Length > 0 ||
                                dictionaryTerm.Related.Summary.Length > 0 ||
                                dictionaryTerm.Related.DrugSummary.Length > 0 ||
                                dictionaryTerm.Related.External.Length > 0)
                                    glossaryTermHtml.AppendFormat("<h6>{0}</h6>", moreInformationText);

                                //external links
                                if (dictionaryTerm.Related.External.Length > 0)
                                {
                                    glossaryTermHtml.Append("<ul class=\"no-bullets\">");
                                    foreach (RelatedExternalLink externalLink in dictionaryTerm.Related.External)
                                    {
                                        glossaryTermHtml.AppendFormat("<li><a href=\"{0}\">{1}", externalLink.Url, externalLink.Text);
                                        glossaryTermHtml.Append("</a></li>");
                                    }


                                    glossaryTermHtml.Append("</ul>");
                                }

                                //summary refs
                                if (dictionaryTerm.Related.Summary.Length > 0)
                                {
                                    glossaryTermHtml.Append("<ul class=\"no-bullets\">");
                                    foreach (RelatedSummary summaryRef in dictionaryTerm.Related.Summary)
                                    {
                                        glossaryTermHtml.AppendFormat("<li><a href=\"{0}\">{1}", summaryRef.url, summaryRef.Text);
                                        glossaryTermHtml.Append("</a></li>");
                                    }

                                    glossaryTermHtml.Append("</ul>");
                                }

                                //drug info summaries
                                if (dictionaryTerm.Related.DrugSummary.Length > 0)
                                {
                                    glossaryTermHtml.Append("<ul class=\"no-bullets\">");
                                    foreach (RelatedDrugSummary drugSummary in dictionaryTerm.Related.DrugSummary)
                                    {
                                        glossaryTermHtml.AppendFormat("<li><a href=\"{0}\">{1}", drugSummary.url, drugSummary.Text);
                                        glossaryTermHtml.Append("</a></li>");
                                    }
                                    glossaryTermHtml.Append("</ul>");
                                }

                                //related terms
                                if (dictionaryTerm.Related.Term.Length > 0)
                                {
                                    string glossaryRelatedTermDefintionLabel = "Definition of: ";
                                    if (item.Language == GateKeeper.DocumentObjects.Language.Spanish)
                                        glossaryRelatedTermDefintionLabel = "Definici&oacute;n de: ";

                                    glossaryTermHtml.AppendFormat("<p><span class=\"related-definition-label\">{0}</span>", glossaryRelatedTermDefintionLabel);

                                    for (int i = 0; i < dictionaryTerm.Related.Term.Length; i++)
                                    {
                                        RelatedTerm relatedTerm = dictionaryTerm.Related.Term[i];

                                        if (i > 0 && i < dictionaryTerm.Related.Term.Length)
                                            glossaryTermHtml.Append(", ");
                                        glossaryTermHtml.AppendFormat("<a href=\"CdrId={0}\">{1}</a>", relatedTerm.Termid, relatedTerm.Text);

                                    }

                                    glossaryTermHtml.Append("</p>");
                                }

                                //related images
                                if (dictionaryTerm.Images.Length > 0)
                                {
                                    foreach (ImageReference imageDetails in dictionaryTerm.Images)
                                    {
                                        glossaryTermHtml.Append(" <figure class=\"image-left-medium\">");
                                        string imageFileName = "";
                                        string termImageSrc = "";
                                        string termEnlargeImageHref = "";
                                        string termEnlargeImageInnerText = "";
                                        if (!string.IsNullOrEmpty(imageDetails.Filename))
                                            imageFileName = imageDetails.Filename.Replace("[__imagelocation]", imagePath);


                                        //if either the regular image size or the enlarge image size is not in the config file
                                        //default to the full image in the database
                                        if (string.IsNullOrEmpty(ConfigurationSettings.AppSettings["CDRImageRegular"]) || string.IsNullOrEmpty(ConfigurationSettings.AppSettings["CDRImageEnlarge"]))
                                        {
                                            termImageSrc = imageFileName;
                                            termEnlargeImageHref = imageFileName;
                                            termEnlargeImageInnerText = item.Language == GateKeeper.DocumentObjects.Language.Spanish ? "Ampliar" : "Enlarge";

                                        }
                                        else
                                        {
                                            string[] regularTermImage = imageDetails.Filename.Split('.');
                                            if (regularTermImage.Length == 2)
                                            {
                                                //termImage image size is 571
                                                //example format CDR526538-571.jpg
                                                termImageSrc = regularTermImage[0] + "-" + ConfigurationSettings.AppSettings["CDRImageRegular"] + "." + regularTermImage[1];
                                                termImageSrc = termImageSrc.Replace("[__imagelocation]", imagePath);

                                                //enlarge image size is 750
                                                //example format CDR526538-750.jpg
                                                termEnlargeImageHref = regularTermImage[0] + "-" + ConfigurationSettings.AppSettings["CDRImageEnlarge"] + "." + regularTermImage[1];
                                                termEnlargeImageHref = termEnlargeImageHref.Replace("[__imagelocation]", imagePath);
                                                termEnlargeImageInnerText = item.Language == GateKeeper.DocumentObjects.Language.Spanish ? "Ampliar" : "Enlarge";

                                            }
                                        }
                                        glossaryTermHtml.AppendFormat("<a target=\"_blank\" class=\"article-image-enlarge no-resize\" href=\"{0}\">{1}</a>", termEnlargeImageHref, termEnlargeImageInnerText);
                                        glossaryTermHtml.AppendFormat("<img src=\"{0}\" alt=\"{1}\" />", termImageSrc, imageDetails.AltText);
                                        glossaryTermHtml.Append("<figcaption>");
                                        glossaryTermHtml.Append("<div class=\"caption-container no-resize\">");
                                        glossaryTermHtml.AppendFormat("<p>{0}</p>", imageDetails.Caption);
                                        glossaryTermHtml.Append("</div>");
                                        glossaryTermHtml.Append("</figcaption>");
                                        glossaryTermHtml.Append("</figure>");

                                    }
                                }

                                // Output any videos
                                if (dictionaryTerm.HasVideos)
                                {
                                    foreach (VideoReference video in dictionaryTerm.Videos)
                                    {
                                        String classes = GetVideoClassesFromTemplateName(video.Template);
                                        glossaryTermHtml.AppendFormat("<figure class=\"{0}\">", classes);

                                        if (!video.Template.ToLowerInvariant().Contains("notitle") && !String.IsNullOrEmpty(video.Title))
                                        {
                                            glossaryTermHtml.AppendFormat("<h4>{0}</h4>", video.Title);
                                        }

                                        glossaryTermHtml.AppendFormat("<div id=\"ytplayer-{0}\" class=\"flex-video widescreen\" data-video-id=\"{0}\" data-video-title=\"{1}\">",
                                            video.UniqueID,
                                            video.Title
                                            );

                                        glossaryTermHtml.Append("<noscript><p>");
                                        glossaryTermHtml.AppendFormat("<a href=\"https://www.youtube.com/watch?v={0}\" target=\"_blank\" title=\"{1}\">View this video on YouTube.</a>",
                                            video.UniqueID, video.Title);
                                        glossaryTermHtml.Append("</p></noscript>");

                                        glossaryTermHtml.Append("</div>");


                                        // TODO Caption

                                        glossaryTermHtml.Append("</figure>");
                                    }
                                }

                                glossaryTermHtml.Append("</div></div>");

                            }

                            glossaryTermHtml.Append("</dl></div>");
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        throw ex;
                    }
                                                            
                }
            }

          
            return glossaryTermHtml.ToString();
        }

        private string GetVideoClassesFromTemplateName(string templateName)
        {
            string classList;

            // These are the templates allowed by the CDR's DTD for GlossaryTerm Embedded videos.
            // Others do exist in Percussion, but are deprecated per OCECDR-3558.
            switch (templateName.ToLowerInvariant())
            {
                case "video100notitle":
                case "video100title":
                    classList = "video center size100";
                    break;

                case "video50notitle":
                    classList = "video center size50";
                    break;

                case "video50notitleright":
                case "video50titleright":
                    classList = "video right size50";
                    break;

                case "video75notitle":
                case "video75title":
                    classList = "video center size75";
                    break;

                default:
                    classList = "video center size100";
                    break;
            }

            return classList;
        }

    }
}
