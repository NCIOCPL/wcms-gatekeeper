using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.PercussionWebSvc;

namespace NCI.WCM.CMSManager.CMS
{

    public class SlotNameManager
    {
        /// <summary>
        /// Danger! This is a deliberately static member. This dictionary
        /// is shared across *all* instances of SlotNameManager.  It is
        /// only updated the first time the constructor is executed.
        /// </summary>
        private static Dictionary<string, PercussionGuid> _slotMap = null;

        private object lockObject = new object();

        private assemblySOAP _assemblyService = null;

        #region Constructor and Initialization

        internal SlotNameManager(assemblySOAP assemblyService)
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
                        LoadSlotIDMap();
                    }
                }
            }
        }

        private void LoadSlotIDMap()
        {
            PSTemplateSlot[] slotData = PSWSUtils.LoadSlots(_assemblyService);

            _slotMap = new Dictionary<string, PercussionGuid>();
            Array.ForEach(slotData, slot =>
            {
                _slotMap.Add(slot.name, new PercussionGuid(slot.id));
            });
        }

        #endregion

        public PercussionGuid this[string key]
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
