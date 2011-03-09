using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.PercussionWebSvc;

namespace NCI.WCM.CMSManager.CMS
{

    public class TemplateNameManager
    {
        /// <summary>
        /// Danger! This is a deliberately static member. This dictionary
        /// is shared across *all* instances of TemplateNameManager.  It is
        /// only updated the first time the constructor is executed.
        /// </summary>
        private static Dictionary<string, PercussionGuid> _templateMap = null;

        private object lockObject = new object();

        private assemblySOAP _assemblyService = null;

        #region Constructor and Initialization

        internal TemplateNameManager(assemblySOAP assemblyService)
        {
            _assemblyService = assemblyService;

            if (_templateMap == null)
            {
                // Lock the container for possible updating.
                lock (lockObject)
                {
                    // Was the map loaded while we waited for the lock?
                    if (_templateMap == null)
                    {
                        LoadTemplateIDMap();
                    }
                }
            }
        }

        private void LoadTemplateIDMap()
        {
            PSAssemblyTemplate[] templateData = PSWSUtils.LoadAssemblyTemplates(_assemblyService);

            _templateMap = new Dictionary<string, PercussionGuid>();
            Array.ForEach(templateData, template =>
            {
                if (!_templateMap.ContainsKey(template.name))
                {
                    _templateMap.Add(template.name, new PercussionGuid(template.id));
                }
            });
        }

        #endregion

        /// <summary>
        /// Find the ID matching a given template name.
        /// </summary>
        /// <param name="key">Content type name to search for.  Case sensitive. (rffNavon != rffnavon)</param>
        /// <returns></returns>
        public PercussionGuid this[string key]
        {
            get
            {
                if (!_templateMap.ContainsKey(key))
                    throw new CMSMissingTemplateException(string.Format("Unknown template name: {0}.", key));

                return _templateMap[key];
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
                if (!_templateMap.ContainsValue(reverseKey))
                    throw new CMSMissingContentTypeException(string.Format("Unknown template name: {0}.", reverseKey));

                return _templateMap.First(kvp => kvp.Value == reverseKey).Key;
            }
        }

    }
}
