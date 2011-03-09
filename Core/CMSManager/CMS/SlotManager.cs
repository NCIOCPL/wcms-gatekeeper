 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.PercussionWebSvc;

namespace NCI.WCM.CMSManager.CMS
{

    public class SlotManager
    {
        /// <summary>
        /// Danger! This is a deliberately static member. This dictionary
        /// is shared across *all* instances of SlotManager.  It is
        /// only updated the first time the constructor is executed.
        /// </summary>
        private static Dictionary<string, SlotInfo> _slotMap = null;

        private object lockObject = new object();

        private assemblySOAP _assemblyService = null;

        #region Constructor and Initialization

        internal SlotManager(assemblySOAP assemblyService,
            ContentTypeManager contentTypeManager,
            TemplateNameManager templateNameManager)
        {
            _assemblyService = assemblyService;

            if (_slotMap == null)
            {
                // Lock the container for possible updating.
                lock (lockObject)
                {
                    // Was the map loaded while we waited for the lock?
                    if (_slotMap == null)
                    {
                        LoadSlotInfoMap(contentTypeManager, templateNameManager);
                    }
                }
            }
        }

        private void LoadSlotInfoMap(ContentTypeManager contentTypeManager,
                                TemplateNameManager templateNameManager)
        {
            PSTemplateSlot[] slotData = PSWSUtils.LoadSlots(_assemblyService);

            _slotMap = new Dictionary<string, SlotInfo>();
            foreach (PSTemplateSlot slot in slotData)
            {
                SlotInfo info = new SlotInfo(slot.name, new PercussionGuid(slot.id));
                foreach(PSTemplateSlotContent idPair in slot.AllowedContent)
                {
                    PercussionGuid contentTypeID = new PercussionGuid(idPair.contentTypeId);
                    PercussionGuid templateID = new PercussionGuid(idPair.templateId);
                    string contentTypeName = contentTypeManager[contentTypeID];
                    string templateName = templateNameManager[templateID];
                    ContentTypeToTemplateInfo pairData
                        = new ContentTypeToTemplateInfo(contentTypeName, contentTypeID, templateName, templateID);
                    info.AllowedContentTemplatePairs.Add(pairData);
                }

                if (!_slotMap.ContainsKey(slot.name))
                {
                    _slotMap.Add(slot.name, info);
                }
            }
        }

        #endregion

        public SlotInfo this[string key]
        {
            get
            {
                if (!_slotMap.ContainsKey(key))
                    throw new CMSMissingSlotException(string.Format("Unknown slot name: {0}.", key));

                return _slotMap[key];
            }
        }

    }
}