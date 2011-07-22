using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.Logging;
namespace GateKeeper.ContentRendering
{

    /// <summary>
    /// This class resize the media image and save them to the staging server location
    /// </summary>
    public class MediaRenderer : DocumentRenderer
    {
        #region Fields
        string imagePath = string.Empty;
        #endregion

        #region Public methods
        public MediaRenderer()
        {
            //  Get image path on staging machine
            imagePath = ConfigurationManager.AppSettings["ImageStaging"];
        }

        public override void Render(Document document)
        {
            // We are not supporting other enconding other than base64
            string encodingType = ((MediaDocument)document).EncodingType;
            if (!string.IsNullOrEmpty(encodingType) && encodingType.ToLower() == "base64")
            { }
            else
                throw new Exception("Rendering Error: Unknown EncodingType:" + encodingType + " for document id " + document.DocumentID.ToString());

            // Different processing needed for document object based on the 
            // MimeType.
            string mimeType = ((MediaDocument)document).MimeType;

            if (!string.IsNullOrEmpty(mimeType))
            {
                mimeType = mimeType.ToLower();
                if (mimeType.Contains("image"))
                    RenderImage(document);
                else if (mimeType.Contains("audio"))
                    RenderAudio(document);
                else
                    throw new Exception("Rendering Error: Unknown mimetype:" + mimeType + " for document id " + document.DocumentID.ToString());
            }
            else
                throw new Exception("Rendering Error: mimetype not specified" + document.DocumentID.ToString());

        }

        private void RenderAudio(Document document)
        {
            // Does not do anything for audio document.
        }

        private void RenderImage(Document document)
        {
            try
            {

                int[] arrImageWidth = { 0, 750, 571, 429, 274, 179, 131, 103, 84 };
                double heightRatio = 0.000d;
                string destination = string.Empty;

                MediaDocument media = (MediaDocument)document;
                MemoryStream imgMS = new MemoryStream(System.Convert.FromBase64String(media.EncodedData));
                Image ImageMedia = System.Drawing.Image.FromStream(imgMS);

                Image origImage = ImageMedia;

                // Set the first image to be the original image size
                arrImageWidth[0] = origImage.Width;
                heightRatio = (double)origImage.Height / origImage.Width;


                // Create destination directory if it is not exist
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);

                // Loop through the size scales to resize image
                for (int i = 0; i < arrImageWidth.Length; i++)
                {
                    // Set the image destination locations
                    if (i == 0)
                        destination = imagePath + "CDR" + media.DocumentID.ToString() + ".jpg";
                    else
                        destination = imagePath + "CDR" + media.DocumentID.ToString() + "-" + arrImageWidth[i] + ".jpg";

                    // Set the new image's width and height
                    Size imageSize = new Size();
                    // TODO: this logic is copied from original version, need to find out why we do this
                    if (origImage.Width < arrImageWidth[i])
                    {
                        imageSize.Width = origImage.Width;
                        imageSize.Height = origImage.Height;
                    }
                    else
                    {
                        imageSize.Width = arrImageWidth[i];
                        imageSize.Height = Convert.ToInt32(arrImageWidth[i] * heightRatio);
                    }

                    ImageResize(origImage, imageSize, destination);
                }

                origImage.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Resizing media image failed. Document CDRID=" + document.DocumentID.ToString(), e);
            }
        }
        #endregion  // End public methods

        #region private methods
        /// <summary>
        /// Resize media image and save it to server location
        /// </summary>
        /// <param name="GlossaryTermDocument"></param>
        void ImageResize(Image sourceImage, Size newSize, string destination)
        {
            try
            {
                // Load the image into bitmap object
                Bitmap bitmap = new Bitmap(newSize.Width, newSize.Height, sourceImage.PixelFormat);
                // Create graphic object
                Rectangle rect = new Rectangle(0, 0, newSize.Width, newSize.Height);
                SolidBrush brush = new SolidBrush(Color.White);
                Graphics graph = null;
                try
                {
                    // get the graphics object from the bitmap
                    graph = Graphics.FromImage(bitmap);
                    graph.FillRectangle(brush, rect);
                    graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graph.DrawImage(sourceImage, 0, 0, bitmap.Width, bitmap.Height);
                }
                catch (Exception)
                {
                    // You cannot create a Graphics object from an image with an indexed pixel format < 16.
                    //  To get around this problem: size the new bitmap to the new bitmaps dimensions.
                    // Draw the old bitmaps contents to the new bitmap. Paint the entire region of the old bitmap to the 
                    // new bitmap. Use the rectangle type to select area of the source image
                    Bitmap bmpNew = new Bitmap(newSize.Width, newSize.Width);
                    graph = Graphics.FromImage(bmpNew);
                    graph.FillRectangle(brush, rect);
                    graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graph.DrawImage(sourceImage, new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel);
                    bitmap = bmpNew;
                }
                //Save the image to the destionation location
                bitmap.Save(destination, System.Drawing.Imaging.ImageFormat.Jpeg);
                bitmap.Dispose();
                graph.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception("Rendering Error: Resizing media image failed.", e);
            }
        }
        #endregion // End private methods
    }
}
