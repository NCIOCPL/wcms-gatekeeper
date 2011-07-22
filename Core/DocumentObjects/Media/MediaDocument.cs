using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GateKeeper.DocumentObjects.Media
{
    /// <summary>
    /// Class to represent a media document.
    /// Prasad(05/24) : Updated the class to include additional property 
    /// for handling media document like audio. Eliminated unsed properties( Caption, IsThumb,
    /// ImageExtension) 
    /// and methods(ToSring, )
    /// </summary>
    [Serializable]
    public class MediaDocument : Document
    {
        #region Fields

        private string _encodedData = null;
        private string _id = string.Empty;
        private string _mimeType = string.Empty;
        private string _encodingType = string.Empty;
        #endregion

        #region Public Properties

        /// <summary>
        /// Raw data of the media.
        /// </summary>
        public string EncodedData
        {
            get { return _encodedData; }
            internal set { _encodedData = value; }
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
        /// The mimetype extension of the media.
        /// </summary>
        /// <example>jpeg</example>
        public string Extension
        {
            get
            {
                string ext = string.Empty;
                string mime = MimeType.ToLower();
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
            }
        }

        /// <summary>
        /// Encoding type used for the data stored in EncodedData property
        /// For example Base64
        /// </summary>
        public string EncodingType
        {
            get { return _encodingType; }
            set { _encodingType = value; }
        }

        /// <summary>
        /// The size of the encoded data.
        /// </summary>
        public string Size
        { get; set; }

        #endregion
    }
}
