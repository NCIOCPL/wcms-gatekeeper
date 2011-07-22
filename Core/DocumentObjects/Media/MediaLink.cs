using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using GateKeeper.DocumentObjects;

namespace GateKeeper.DocumentObjects.Media
{
    /// <summary>
    /// Class to represent a content MediaLink.
    /// </summary>
    [Serializable]
    public class MediaLink
    {
        #region Fields

        private string _reference = string.Empty;
        private int _referencedCdrDocumentID = 0;
        private string _alt = string.Empty;
        private string _size = "not-set";
        private string _html = string.Empty;
        private string _caption = string.Empty;
        private string _ID = string.Empty;
        private XmlDocument _xml = null;
        private Language _language = Language.English;
        private bool _isThumb = false;
        private bool _isInline = false;
        private bool _displayEnlargeLink = true;
        private int _mediaDocumentID = 0;
        private long _minWidth = 0;
        private string _type = String.Empty;

        #endregion

        #region Public Properties
        /// <summary>
        /// Reference for the mediaLink.
        /// </summary>
        public string Reference
        {
            get { return _reference; }
            internal set { _reference = value; }
        }

        
        public int ReferencedCdrID
        {
            get { return _referencedCdrDocumentID; }
            internal set { _referencedCdrDocumentID = value; }
        }

        /// <summary>
        /// Alt text for the medialink
        /// </summary>
        public string Alt
        {
            get { return _alt; }
            internal set { _alt = value; }
        }

        /// <summary>
        /// Inline (Yes/No) for the medialink.
        /// </summary>
        public bool Inline
        {
            get { return _isInline; }
            private set { _isInline = value; }
        }

        /// <summary>
        /// Reports whether the media link is intended to display
        /// with an "Enlarge" link.
        /// </summary>
        public bool DisplayEnlargeLink
        {
            get { return _displayEnlargeLink; }
            private set { _displayEnlargeLink = value; }
        }

        /// <summary>
        /// Minimun width for the medialink image
        /// </summary>
        public long MinWidth
        {
            get { return _minWidth; }
            internal set { _minWidth = value; }
        }

        /// <summary>
        /// Size for the medialink image
        /// </summary>
        public string Size
        {
            get { return _size; }
            internal set { _size = value; }
        }
        
        /// <summary>
        /// Document ID for the target of the mediaLink.
        /// </summary>
        public int MediaDocumentId
        {
            get { return _mediaDocumentID; }
            internal set { _mediaDocumentID = value; }
        }

        /// <summary>
        /// Internal document id.
        /// </summary>
        public string Id
        {
            get { return _ID; }
            internal set { _ID = value; }
        }

        /// <summary>
        /// Language of the caption text.
        /// </summary>
        public Language Language
        {
            get { return _language; }
            internal set { _language = value; }
        }

        /// <summary>
        /// Image caption.
        /// </summary>
        public string Caption
        {
            get { return _caption; }
            set { _caption = value; }
        }

        /// <summary>
        /// Flag to indicate that the image is a thumbnail.
        /// </summary>
        public bool IsThumb
        {
            get { return _isThumb; }
            set { _isThumb = value; }
        }

        /// <summary>
        /// Raw XML of the MediaLink element.
        /// </summary>
        public XmlDocument Xml
        {
            get { return _xml; }
            internal set { _xml = value; }
        }

        /// <summary>
        /// Pre-rendered HTML.
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }


        /// <summary>
        /// URL for the size-specific version of the image file to use for inline references.
        /// The URL is relative to the base location where Media documents are stored.
        /// </summary>
        public string InlineImageUrl
        {
            get
            {
                string url;
                int width = DeterminePixelSize();

                if (_size.Equals("as-is"))
                    url = string.Format("CDR{0}.jpg", ReferencedCdrID);
                else
                    url = string.Format("CDR{0}-{1}.jpg", ReferencedCdrID, width);

                return url;
            }
        }

        /// <summary>
        /// Gets the width of the inline image.
        /// </summary>
        /// <value>The width of the inline image.</value>
        public int InlineImageWidth
        {
            get { return DeterminePixelSize(); }
        }

        /// <summary>
        /// URL to use when referencing the image file from within a pop-up.
        /// The URL is relative to the base location where Media documents are stored.
        /// </summary>
        public string PopupImageUrl
        {
            get { return string.Format("CDR{0}-750.jpg", ReferencedCdrID); }
        }

        /// <summary>
        /// Gets the width of the popup image.
        /// </summary>
        /// <value>The width of the popup image.</value>
        public int PopupImageWidth
        {
            get { return 750; }
        }
        
        /// <summary>
        /// Gets or Sets the MimeType of the media.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
        
         public string Extension
         {
             get
             {
                 string ext = string.Empty;
                 string mime = _type.ToLower();
                 switch (mime)
                 {
                     case "image/jpeg":
                         ext = ".jpg";
                         break;
                     case "audio/mpeg":
                         ext = ".mp3";
                         break;
                 }
                 return ext;
             }
         }
        #endregion

        #region Constructors

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="caption"></param>
        /// <param name="mediaDocumentID"></param>
        /// <param name="language"></param>
        /// <param name="isThumb"></param>
        /// <param name="xml"></param>
        public MediaLink(string reference, int cdrID, string alt, bool isInline, bool displayEnlargeLink, long minWidth, string size, string ID, string caption, int mediaDocumentID, Language language, bool isThumb, string type, XmlDocument xml)
        {
            this._reference = reference;
            this._referencedCdrDocumentID = cdrID;
            this._alt = alt;
            this._isInline = isInline;
            this._displayEnlargeLink = displayEnlargeLink;
            this._minWidth = minWidth;
            this._size = size;
            this._ID = ID;
            this._caption = caption;
            this._mediaDocumentID = mediaDocumentID;
            this._language = language;
            this._isThumb = isThumb;
            this._xml = xml;
            this._type = type;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a System.String representation of the MediaLink.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" Id = {0} MediaDocumentId = {1} IsThumb = {2} Language = {3} Caption = {4} \n",
                Id, MediaDocumentId, IsThumb, Language, Caption));

            sb.Append(string.Format("Xml = {0} \n Html = {1} ",
                Xml.OuterXml, Html));

            return sb.ToString();
        }


        /// <summary>
        /// Translates a user-friendly image size text into a pixel size for use in HTML.
        /// </summary>
        /// <param name="imSize">One of the allowed image size values: full, three-quarters, half,
        /// thrd, quarter, fifth, sixth, as-is, or not-set.</param>
        /// <param name="defaultSz">Pixel size to use if no value is set.</param>
        /// <returns>The image width in pixels to use when displaying the image. (Determines
        /// which version of the image will be used.)</returns>
        /// <remarks>For MediaLink elements in glossary terms, a default size of 179 pixels (third) should be used.
        /// for MediaLink elements in summary documents, a default size of 274 pixels (half) is used.</remarks>
        [Obsolete("The static implemention of DeterminePixelSize is deprecated. New code should use the instance variation which has no default values.")]
        static public int DeterminePixelSize(string imSize, int defaultSz)
        {
            int width;
            switch (imSize)
            {
                case "full": width = 571; break;
                case "three-quarters": width = 429; break;
                case "half": width = 274; break;
                case "third": width = 179; break;
                case "quarter": width = 131; break;
                case "fifth": width = 103; break;
                case "sixth": width = 84; break;
                case "as-is": width = 571; break;
                case "not-set": width = defaultSz; break;
                default: width = defaultSz; break;
            }
            return width;
        }

        /// <summary>
        /// Translates the media item's user-friendly image size text into a pixel size for use in HTML.
        /// </summary>
        /// <returns>The image width in pixels to use when displaying the image. (Determines
        /// which version of the image will be used.)</returns>
        public int DeterminePixelSize()
        {
            int width;
            switch (_size)
            {
                case "full": width = 571; break;
                case "three-quarters": width = 429; break;
                case "half": width = 274; break;
                case "third": width = 179; break;
                case "quarter": width = 131; break;
                case "fifth": width = 103; break;
                case "sixth": width = 84; break;
                case "as-is": width = 571; break;
                default:
                    throw new MediaLinkSizeException(string.Format("Unknown MediaLink size value: {0}.", _size));
                    ;
            }
            return width;
        }

        #endregion

    }
}
