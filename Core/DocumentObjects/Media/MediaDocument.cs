using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GateKeeper.DocumentObjects.Media
{
    /// <summary>
    /// Class to represent a media document.
    /// </summary>
    [Serializable]
    public class MediaDocument : Document
    {
        #region Fields

        private int _width = 0;
        private int _height = 0;
        private Image _image = null;
        private string _caption = string.Empty;
        private Language _language = Language.English;
        private string _id = string.Empty;
        private bool _isThumb = false;
        private string _mimeType = string.Empty;
        private string _imageExtension = "jpeg";
        #endregion

        #region Public Properties

        /// <summary>
        /// Raw image data.
        /// </summary>
        public System.Drawing.Image Image
        {
            get { return _image; }
            internal set { _image = value; }
        }
        
        /// <summary>
        /// Image height.
        /// </summary>
        public int Height
        {
            get { return _height; }
            internal set { _height = value; }
        }
        
        /// <summary>
        /// Image width.
        /// </summary>
        public int Width
        {
            get { return _width; }
            internal set { _width = value; }
        }

        /// <summary>
        /// Image caption.
        /// </summary>
        public string Caption
        {
            get { return _caption; }
            internal set { _caption = value; }
        }

        /// <summary>
        /// Language of the caption.
        /// </summary>
        public Language Language
        {
            get { return _language; }
            internal set { _language = value; }
        }

        /// <summary>
        /// Internal document id.
        /// </summary>
        public string Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        /// <summary>
        /// Flag to indicate that the image is a thumbnail.
        /// </summary>
        public bool IsThumb
        {
            get { return _isThumb; }
            internal set { _isThumb = value; }
        }

        /// <summary>
        /// Image file extension.
        /// </summary>
        /// <example>jpeg</example>
        public string ImageExtension
        {
            get { return _imageExtension; }
            internal set { _imageExtension = value; }
        }

        /// <summary>
        /// Image mime type.
        /// </summary>
        /// <example>image/jpeg</example>
        public string MimeType
        {
            get { return _mimeType; }
            internal set
            {
                _mimeType = value;
                if (_mimeType != null && _mimeType.Length > 0)
                {
                    _imageExtension = _mimeType.Remove(0, _mimeType.IndexOf("/") + 1);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the MediaDocument.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" Height = {0} Width = {1} Caption = {2} Language = {3} Id = {4} IsThumb = {5} ", 
                Height, Width, Caption, Language, Id, IsThumb));

            return sb.ToString();
        }

        #endregion
    }
}
