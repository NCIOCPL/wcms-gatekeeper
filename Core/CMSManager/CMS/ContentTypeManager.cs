using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.PercussionWebSvc;

namespace NCI.WCM.CMSManager.CMS
{

    public class ContentTypeManager
    {
        /// <summary>
        /// Danger! This is a deliberately static member. This dictionary
        /// is shared across *all* instances of TemplateNameManager.  It is
        /// only updated the first time the constructor is executed.
        /// </summary>
        private static Dictionary<string, PercussionGuid> _contentTypeMap = null;

        private object lockObject = new object();

        private contentSOAP _contentService = null;

        #region Constructor and Initialization

        internal ContentTypeManager(contentSOAP contentService)
        {
            _contentService = contentService;

            if (_contentTypeMap == null)
            {
                // Lock the container for possible updating.
                lock (lockObject)
                {
                    // Was the map loaded while we waited for the lock?
                    if (_contentTypeMap == null)
                    {
                        LoadTemplateIDMap();
                    }
                }
            }
        }

        private void LoadTemplateIDMap()
        {
            PSContentTypeSummary[] contentTypeData = PSWSUtils.LoadContentTypes(_contentService);

            _contentTypeMap = new Dictionary<string, PercussionGuid>();
            Array.ForEach(contentTypeData, contentType =>
            {
                if (!_contentTypeMap.ContainsKey(contentType.name))
                {
                    _contentTypeMap.Add(contentType.name, new PercussionGuid(contentType.id));
                }
            });
        }

        #endregion

        /// <summary>
        /// Find the ID matching a given content type name.
        /// </summary>
        /// <param name="key">Content type name to search for.  Case sensitive. (rffNavon != rffnavon)</param>
        /// <returns></returns>
        public PercussionGuid this[string key]
        {
            get
            {
                if (!_contentTypeMap.ContainsKey(key))
                    throw new CMSMissingContentTypeException(string.Format("Unknown contentType name: {0}.", key));

                return _contentTypeMap[key];
            }
        }

        /// <summary>
        /// Performs a reverse look up for a content type name, based on its ID.
        /// Returns the first content type with that ID.
        /// Note that searching for an ID from a name is much faster.
        /// </summary>
        /// <param name="reverseKey"></param>
        /// <returns></returns>
        public string this[PercussionGuid reverseKey]
        {
            get
            {
                if(!_contentTypeMap.ContainsValue(reverseKey))
                    throw new CMSMissingContentTypeException(string.Format("Unknown contentType name: {0}.", reverseKey));

                return _contentTypeMap.First(kvp => kvp.Value == reverseKey).Key;
            }
        }
    }
}
