using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    // Provides a mechanism for storing a collection of content
    // items in memory and loading additional ones with an unknown status.
    public class ItemCache
    {
        Dictionary<PercussionGuid, PSItem> _itemStore = new Dictionary<PercussionGuid, PSItem>();
        CMSController _cmsController;

        public ItemCache(CMSController controller)
        {
            _cmsController = controller;
        }

        public void Preload(PercussionGuid[] itemList)
        {
            PSItem[] result = _cmsController.LoadContentItems(itemList);
            for (int i = 0; i < itemList.Length; i++)
            {
                _itemStore[itemList[i]] = result[i];
            }
        }

        public PSItem LoadContentItem(PercussionGuid itemID)
        {
            if (!_itemStore.ContainsKey(itemID))
            {
                PSItem[] result = _cmsController.LoadContentItems(new PercussionGuid[] { itemID });
                _itemStore[itemID] = result[0];
            }
            return _itemStore[itemID];
        }

        public void RemoveContentItem(PercussionGuid itemID)
        {
            _itemStore.Remove(itemID);
        }
    }
}
