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
                        //deserialize term details as returned by the service layer (termret.term)
                        dictionaryTerm = JsonConvert.DeserializeObject<DictionaryTerm>(item.Object);

                        if (dictionaryTerm != null)
                        {
                            glossaryTermHtml.Append("<div class=\"results clearfix\">");
                            glossaryTermHtml.Append("<dl class=\"dictionary-list\"><dt>");
                            glossaryTermHtml.Append("<dfn>" + dictionaryTerm.Term + "</dfn>");

                            if (dictionaryTerm.HasPronunciation)
                            {
                                glossaryTermHtml.Append("<dd class=\"pronunciation\">");

                                if (dictionaryTerm.Pronunciation.HasAudio)
                                {
                                    string audioIcon = dictionaryTerm.Pronunciation.Audio.Replace("[__audiolocation]", mediaPath + "/");

                                    glossaryTermHtml.Append("<a href=\"" + audioIcon );
                                    glossaryTermHtml.Append("\" class=\"CDR_audiofile\"><span class=\"hidden\">listen</span></a>");
                                }
                                if (dictionaryTerm.Pronunciation.HasKey)
                                {
                                    glossaryTermHtml.Append("<span>" + dictionaryTerm.Pronunciation.Key + "</span>");
                                }

                                glossaryTermHtml.Append("</dd>");
                            }

                            glossaryTermHtml.Append("<dd class=\"definition\">" + dictionaryTerm.Definition.Text + "<br/>");

                            if (dictionaryTerm.HasRelatedItems)
                            {
                                string moreInformationText = "More Information";
                                if (item.Language == GateKeeper.DocumentObjects.Language.Spanish)
                                    moreInformationText = "M&aacute;s informaci&oacute;n";
                                glossaryTermHtml.Append("<div id=\"pnlRelatedInfo\"><br/><div class=\"related-resources\">");

                                glossaryTermHtml.AppendFormat("<h6>{0}</h6>", moreInformationText);

                                //extrenal links
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
                                                                      
                                    for (int i = 0; i < dictionaryTerm.Related.Term.Length; i++ )
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

                                }

                                glossaryTermHtml.Append("</div></div>");
                                
                            }

                            glossaryTermHtml.Append("</dl></div>");
                        }
                        //if (!first)
                        //    glossaryTermHtml.Append(',');
                        //first = false;
                        //glossaryTermHtml.AppendFormat(@"{{""id"" : ""{0}"", ""term"" : ""{1}"", ""dictionary"" : ""{2}"", ""language"" : ""{3}"", ""audience"" : ""{4}"", ""version"" : ""{5}"", ""object"" : {6} }}",
                        //    item.TermID, item.TermName, item.Dictionary, item.Language, item.Audience, item.ApiVersion, item.Object);
                    }
                    catch (JsonReaderException ex)
                    {
                        throw ex;// ("Error in Json string from service: " + ex.ToString());
                    }
                                                            
                }
            }

            //Audio HTML
            //<a href="[_audioMediaLocation]/703961.mp3" id="audioLink703961" class="CDR_audiofile"><span class="hidden">listen</span></a>

            //Media HTML 
            //<figure class="image-left-medium"><a href="[__imagelocation]CDR539773-750.jpg" target="_blank" class="article-image-enlarge no-resize">Enlarge</a><img src="[__imagelocation]CDR539773-571.jpg" alt="Intrathecal chemotherapy; drawing shows the cerebrospinal fluid (CSF) in the brain and spinal cord, and an Ommaya reservoir (a dome-shaped container that is placed under the scalp during surgery; it holds the drugs as they flow through a small tube into the brain). Top section shows a syringe and needle injecting anticancer drugs into the Ommaya reservoir. Bottom section shows a syringe and needle injecting anticancer drugs directly into the cerebrospinal fluid in the lower part of the spinal column."><figcaption><div class="caption-container no-resize"><p>Intrathecal chemotherapy. Anticancer drugs are injected into the intrathecal space, which is the space that holds the cerebrospinal fluid (CSF, shown in blue). There are two different ways to do this. One way, shown in the top part of the figure, is to inject the drugs into an Ommaya reservoir (a dome-shaped container that is placed under the scalp during surgery; it holds the drugs as they flow through a small tube into the brain). The other way, shown in the bottom part of the figure, is to inject the drugs directly into the CSF in the lower part of the spinal column, after a small area on the lower back is numbed.  </p></div></figcaption></figure>

            //foreach (Language lang in glossary.GlossaryTermTranslationMap.Keys)
            //{
            //    GlossaryTermTranslation trans = glossary.GlossaryTermTranslationMap[lang];


            //    foreach (GlossaryTermDefinition gtDef in trans.DefinitionList)
            //    {
            //        sb.Append("<div class=\"results clearfix\"><dl class=\"dictionary-list\">");


            //        // Retrieve rendered markup for audio.
            //        String pronunciation = "<dd class=\"pronunciation\">" + trans.GetAudioMarkup();
            //        if (!String.IsNullOrEmpty(pronunciation))
            //        {
            //            pronunciation = pronunciation.Replace("[_audioMediaLocation]", mediaPath);
            //            // Fix audio icon relative to pub-preview.
            //            pronunciation = pronunciation.Replace("src=\"/images/audio-icon.gif\"", "src=\"images/audio-icon.gif\"");
            //        }

            //        // Pronuncation key
            //        if (trans.Pronounciation != null && trans.Pronounciation.Trim().Length > 0)
            //        {
            //            pronunciation += "&nbsp;&nbsp;<span>" + trans.Pronounciation.Trim() + "</span></dd>";
            //        }

            //        sb.AppendFormat("<dt><dfn>{0}</dfn></dt>{1}", trans.TermName, pronunciation);

            //        // Definition
            //        string defHtml = gtDef.Html.Trim();
            //        using (GlossaryTermQuery gQuery = new GlossaryTermQuery())
            //        {
            //            if (defHtml.Contains("<SummaryRef"))
            //            {
            //                gQuery.BuildSummaryRefLink(ref defHtml, 1);
            //            }
            //        }
            //        sb.Append("<dd class=\"definition\">" + defHtml + "<br/>");

            //        // Use the GK rendered HTML for Related Links.
            //        if (gtDef.RelatedInformationHTML != String.Empty)
            //        {
            //            sb.Append("<div id=\"pnlRelatedInfo\"><br/>");
            //            sb.Append(gtDef.RelatedInformationHTML);
            //            sb.Append("</div>");
            //        }

            //        // Retrieve rendered markup for images.

            //        String imageHtml = trans.GetImageMarkup(gtDef.AudienceTypeList[0]);
            //        if (!String.IsNullOrEmpty(imageHtml))
            //        {
            //            imageHtml = imageHtml.Replace("[__imagelocation]", imagePath);
            //        }
            //        sb.Append(imageHtml);

            //        sb.Append("</dd>");

            //        sb.Append("</dl></div>");


            return glossaryTermHtml.ToString();
        }
    }
}
