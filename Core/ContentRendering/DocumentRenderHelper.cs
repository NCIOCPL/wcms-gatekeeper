using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using GateKeeper.Common;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;

namespace GateKeeper.ContentRendering
{
    /// <summary>
    /// Common utility functions for rendering.
    /// </summary>
    internal sealed class DocumentRenderHelper
    {

        #region Constants
        const string vertSpacer = "<td><img src=\"/images/spacer.gif\" alt=\"\" width=\"11\" height=\"1\" /></td>";
        const string horSpacer = " <tr><td><img src=\"/images/spacer.gif\" alt=\"\" width=\"1\" height=\"5\" /></td></tr> ";
        const string botSpacer = " <tr><td><img src=\"/images/spacer.gif\" alt=\"\" width=\"1\" height=\"10\" /></td></tr> ";

        #endregion

        #region Public Static Methods

        static public string ProcessMediaLink(MediaLink mediaLink, bool bGlossary, bool bInTable)
        {
            // Check if the alt and image info is available 
            if (mediaLink.ReferencedCdrID == 0)
                throw new Exception("Media Extraction Error: No ref found in the MediaLink tag.  Exception occurred in ProcessMediaLink");

            string type = mediaLink.Type;

            // mime type information is not always present for a image media. If it is not present 
            // we are making an assumption that it should be image media. 
            type = string.IsNullOrEmpty(type) ? "image" : type;

            if (type.Contains("image"))
            {
                if (string.IsNullOrEmpty(mediaLink.Alt))
                    throw new Exception("Media Extraction Error: No ref or alt attribute found in the MediaLink tag.  Exception occurred in ProcessMediaLink");
                return DocumentRenderHelper.ProcessImageMediaLink(mediaLink, bGlossary, bInTable);
            }
            else if (type.Contains("audio"))
                return DocumentRenderHelper.ProcessAudioMediaLink(mediaLink);
            else
                return String.Empty;
        }

        static string ProcessAudioMediaLink(MediaLink mediaLink)
        {
            string audioMediaName = mediaLink.ReferencedCdrID + mediaLink.Extension;
            string audioMediaHtml = "<a href=\"[_audioMediaLocation]/{0}\" id=\"audioLink{1}\" class=\"CDR_audiofile\" shape=\"rect\"><img src=\"/images/audio-icon.gif\" alt=\"listen\" border=\"0\" height=\"16\" width=\"16\" /></a>";
            audioMediaHtml = string.Format(audioMediaHtml, audioMediaName, mediaLink.ReferencedCdrID.ToString());
            return audioMediaHtml;
        }

        /// <summary>
        /// This method generates html required to render the image for a glossary term.
        /// </summary>
        /// <param name="mediaLink">The medialink object</param>
        /// <param name="bGlossary"></param>
        /// <param name="bInTable"></param>
        /// <returns></returns>
        static public string ProcessImageMediaLink(MediaLink mediaLink, bool bGlossary, bool bInTable)
        {
            /*
            // Captions will not be displayed in the dictionary page, but will be displayed in the pop-up

            //If the size is not specified, the default for glossary is 179 and for summary is 274
            int width = MediaLink.DeterminePixelSize(mediaLink.Size, (bGlossary ? 179 : 274));

            bool bThumb = mediaLink.IsThumb;
            if (mediaLink.Size.Equals("not-set") && bGlossary)
                bThumb = true;
            else if (mediaLink.MinWidth > width && !mediaLink.Size.Equals("as-is"))
                bThumb = true;
            */

            string imName = string.Empty;
            string imEnlarge = string.Empty;
            string enlargeHtml = string.Empty;
            string enlarge = string.Empty;
            string captionHtml = string.Empty;
            string caption = string.Empty;
            string imHtml = string.Empty;
            string imLoc = "[__imagelocation]";

            /*
            // For as-is image, the image name is CDR#.jpg
            if (mediaLink.Size.Equals("as-is"))
            {
                imName = imLoc + "CDR" + mediaLink.ReferencedCdrID + ".jpg";
            }
            // For other size image, the image name is CDR#-width.jpg
            else
                imName = imLoc + "CDR" + mediaLink.ReferencedCdrID + "-" + width + ".jpg";

            */
            
            // Image name for normal image
            imName = imLoc + "CDR" + mediaLink.ReferencedCdrID + "-571.jpg";
 
            // Image name for enlarged image
            imEnlarge = imLoc + "CDR" + mediaLink.ReferencedCdrID + "-750.jpg";

            // Set HTML for image itself
            imHtml = "<img src=\"" + imName + "\" alt=\"" + mediaLink.Alt + "\">";

            // Set language for Enlarge text
            if (mediaLink.Language == Language.English)
                enlarge = "Enlarge";
            else
                enlarge = "Ampliar";

            //Write HTML
            string langBuf = string.Empty;

            langBuf = "<figure class=\"image-left-medium\">";
            
            enlargeHtml = "<a href=\"" + imEnlarge + "\" " +
                    "target=\"_blank\" class=\"article-image-enlarge no-resize\">" +
                    enlarge + "</a>";

            langBuf += enlargeHtml + imHtml;

            // If there is a caption, display caption
            if (mediaLink.Caption.Length > 0)
            {
                caption = mediaLink.Caption;
                captionHtml = "<figcaption><div class=\"caption-container no-resize\"><p>" +
                    caption + "</p></div></figcaption>";
                langBuf += captionHtml;
            }

            langBuf += "</figure>";

            /*
            string idTag = string.Empty;
            if (mediaLink.Id != "")
            {
                langBuf = "<a name=\"Section" + mediaLink.Id + "\"/>";
                idTag = " __id=\"" + mediaLink.Id + "\" ";
            }

            // If the inline = "Yes", the image will be displayed in-line without a table around it.
            // MinWidth, Thumb, Captions are ignored
            if (mediaLink.Inline)
            {
                langBuf += "<img " + idTag + " alt=\"" + mediaLink.Alt + "\" border=\"0\" src=\"" + imName + "\"/>";
            }
            // If inline = "No", MinWidth, Thumb, Captions will be considered
            else
            {
                string vertSpacer = "<td><img src=\"/images/spacer.gif\" alt=\"\" width=\"11\" height=\"1\"/></td>";
                string horSpacer = " <tr><td><img src=\"/images/spacer.gif\" alt=\"\" width=\"1\" height=\"5\"/></td></tr> ";

                int table_wdt = width;
                if (!mediaLink.Size.Equals("full"))
                {
                    if ((!bGlossary && !bInTable) || (bGlossary))
                        table_wdt += 11;
                }
                if (bGlossary)
                {
                    langBuf += " <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"" + table_wdt + "\" align=\"left\">";
                    langBuf += horSpacer;
                }
                else
                {
                    langBuf += " <p><table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"" + table_wdt + "\" >";
                }

                string imPopup = "/Common/PopUps/popImage.aspx?imageName=" + imLoc + "CDR" + mediaLink.ReferencedCdrID + "-750.jpg";
                if (mediaLink.Caption.Length > 0)
                {
                    string caption = string.Empty;
                    caption = mediaLink.Caption;
                    imPopup = imPopup + "&amps;caption=" + HttpUtility.HtmlEncode(caption.Replace("’", "'").Replace("'", "\\'"));
                }
                if ((bThumb && bGlossary) || (bThumb && !bGlossary && !bInTable))
                {
                    langBuf += " <tr><td align=\"right\"><A HREF=\"javascript:dynPopWindow('" + imPopup + "','popup','width=780,height=630,scrollbars=1,resizable=1,menubar=0,location=0,status=0,toolbar=0')\">";
                    if (mediaLink.Language == Language.English)
                        langBuf += "Enlarge</A></td>";
                    else
                        langBuf += "Ampliar</A></td>";

                    langBuf += "</tr>";
                }

                //Summary: for display grey line above the caption
                if (!bGlossary && mediaLink.Caption.Length > 0)
                    langBuf += "<tr><td><div class=\"caption-image\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\">";

                langBuf += " <tr><td align=\"center\" > ";

                //for clickable of image if it is thrumbnail
                if (bThumb)
                    langBuf += "<A HREF=\"javascript:dynPopWindow('" + imPopup + "','popup','width=780,height=630,scrollbars=1,resizable=1,menubar=0,location=0,status=0,toolbar=0')\">";

                langBuf += " <img " + idTag + " alt=\"" + mediaLink.Alt + "\" border=\"0\" ";
                langBuf += " src=\"" + imName + "\"/>";
                if (bThumb)
                    langBuf += "</A>";

                string capHtml = "<tr><td class=\"caption\" align=\"left\"> " + mediaLink.Caption + "</td></tr>";
                if (bGlossary)
                {
                    langBuf += "</td>";
                    if (!mediaLink.Size.Equals("full"))
                        langBuf += vertSpacer;
                    langBuf += "</tr>";
                    langBuf += "</table>";
                }
                else
                {
                    langBuf += "</td></tr>";

                    //for display grey line above the caption
                    if (mediaLink.Caption.Length > 0)
                    {
                        langBuf += "<tr><td valign=\"top\"><img src=\"/images/spacer.gif\" alt=\"\" width=\"12\" height=\"10\" border=\"0\"/></td></tr></table></div></td></tr>";
                        langBuf += "<tr><td valign=\"top\"><img src=\"/images/spacer.gif\" alt=\"\" width=\"12\" height=\"3\" border=\"0\"/></td></tr>";
                        // Insert captions in the dictionary if it is from summary page
                        langBuf += capHtml;
                    }
                    langBuf += "</table></p>";

                }
            }*/

            return langBuf.ToString();
        }


        /// <summary>
        /// Common method to render media links.
        /// </summary>
        /// <param name="languageMap"></param>
        /// <param name="mediaLink"></param>
        /// <param name="determineDefaultSize"></param>
        /// <param name="isGlossary"></param>
        /// <param name="isInList"></param>
        /// <param name="isInTable"></param>
        public static void ProcessMediaLink(ref Dictionary<string, string> languageMap, MediaLink mediaLink, bool determineDefaultSize, bool isGlossary, bool isInTable, bool isInList)
        {
            ProcessMediaLink(ref languageMap, mediaLink, determineDefaultSize, isGlossary, isInTable, isInList);
        }

        /// <summary>
        /// This function is called before the XML is rendered
        /// it takes in the original summary XML, which contains several reference links
        /// that are listed as 1,2,3,5,6,10,11,12,14,16 and should be converted to
        /// [1-3,5,6,10-12,14,16]
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string FormatReferences(string strInput)
        {
            string strOutput = string.Empty;
            int iPos;
            string strNextElement;
            bool bLastIsRefTag = false;
            bool bNeedToCloseRefTag = false;
            string strLastRefElement = string.Empty;
            int iLastIndex = -5;
            int iNewIndex;
            int iPreviousIndex = 0;
            int iFirstIndex = 0;

            Regex regexRefIndex = new Regex("<Reference\\s+?refidx\\s*?=\\s*?\"(?<refidx>[0-9]+?)\"\\s*?/>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Regex regexRemoveLeadingSpace = new Regex("^\\s+?<Reference", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            while (strInput.Length > 0)
            {
                // remove spaces between reference tags
                if (bLastIsRefTag)
                {
                    strInput = regexRemoveLeadingSpace.Replace(strInput, "<Reference");
                }

                // get next element
                if (strInput.StartsWith("<"))
                {
                    // next element is a tag
                    iPos = strInput.IndexOf(">");
                    if (iPos == -1)
                    {
                        iPos = strInput.Length - 1;
                    }
                    strNextElement = strInput.Substring(0, iPos + 1);
                    strInput = strInput.Substring(iPos + 1);
                }
                else
                {
                    // next element is text
                    iPos = strInput.IndexOf("<");
                    if (iPos == -1)
                    {
                        // end of file, not more tags
                        iPos = strInput.Length;
                    }
                    strNextElement = strInput.Substring(0, iPos);
                    strInput = strInput.Substring(iPos);
                }

                // if it's not a reference tag, just display it
                if (!strNextElement.ToLower().StartsWith("<reference "))
                {
                    // not a reference tag
                    if (bNeedToCloseRefTag)
                    {
                        if (iPreviousIndex == 1)
                        {
                            strOutput += "," + strLastRefElement;
                            iPreviousIndex = 0;
                        }
                        else
                        {
                            if (iPreviousIndex == iLastIndex - iFirstIndex)
                            {
                                strOutput += "-" + strLastRefElement;
                                iPreviousIndex = 0;
                            }
                            else
                            {
                                strOutput += "-" + strLastRefElement;
                            }
                        }
                        bNeedToCloseRefTag = false;
                    }

                    if (bLastIsRefTag)
                    {
                        // close out the last reference
                        strOutput += "] ";
                        bLastIsRefTag = false;
                    }

                    strOutput += strNextElement;
                }
                else
                {
                    // this is a ref tag, handle appropriately
                    if (!bLastIsRefTag)
                    {
                        // last was not a ref tag
                        bLastIsRefTag = true;
                        strOutput += "[" + strNextElement;
                        Match mtIndex = regexRefIndex.Match(strNextElement);
                        iLastIndex = Int32.Parse(mtIndex.Groups["refidx"].Value);
                        iFirstIndex = Int32.Parse(mtIndex.Groups["refidx"].Value);
                    }
                    else
                    {
                        // last was a ref tag
                        Match mtIndex = regexRefIndex.Match(strNextElement);
                        iNewIndex = Int32.Parse(mtIndex.Groups["refidx"].Value);

                        if (iNewIndex == iLastIndex + 1)
                        {
                            iPreviousIndex++;
                            bNeedToCloseRefTag = true;
                            strLastRefElement = strNextElement;
                        }
                        else
                        {

                            if (bNeedToCloseRefTag)
                            {
                                if (iPreviousIndex == 1)
                                {
                                    strOutput += "," + strLastRefElement + "," + strNextElement;
                                    //iFirstIndex = iNewIndex;
                                }
                                else
                                {
                                    if (iPreviousIndex == iLastIndex - iFirstIndex)
                                    {
                                        strOutput += "-" + strLastRefElement + "," + strNextElement;
                                        //iFirstIndex = iNewIndex;
                                    }
                                    else
                                    {
                                        strOutput += "-" + strNextElement;
                                    }
                                }
                                iPreviousIndex = 0;
                                bNeedToCloseRefTag = false;

                            }
                            else
                            {
                                strOutput += "," + strNextElement;
                            }
                            iFirstIndex = iNewIndex;
                        }
                        iLastIndex = iNewIndex;

                    } // if (! bLastIsRefTag) // else
                } // if (! strNextElement.ToLower().StartsWith("<reference ")) // else
            } // while (strInput.Length > 0) 

            return strOutput;
        }

        #endregion

        #region Private methods
        private static string ReplaceCaptionUnicode(string caption)
        {
            caption = caption.Replace("á", "&#225;");
            caption = caption.Replace("é", "&#233;");
            caption = caption.Replace("ó", "&#243;");
            caption = caption.Replace("í", "&#237;");
            caption = caption.Replace("ñ", "&#241;");
            caption = caption.Replace("ú", "&#250;");
            return caption;
        }

        #endregion
    }
}
