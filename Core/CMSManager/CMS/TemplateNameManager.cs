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

        public TemplateNameManager(assemblySOAP assemblyService)
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
                        _templateMap = new Dictionary<string, PercussionGuid>();
                        LoadTemplateIDMap();
                    }
                }
            }
        }

        private void LoadTemplateIDMap()
        {
            PSAssemblyTemplate[] templateData = PSWSUtils.LoadAssemblyTemplates(_assemblyService);

            Array.ForEach(templateData, template =>
            {
                _templateMap.Add(template.name, new PercussionGuid(template.id));
            });
        }

        #endregion

        public PercussionGuid this[string key]
        {
            get { return _templateMap[key]; }
        }
    }
}
