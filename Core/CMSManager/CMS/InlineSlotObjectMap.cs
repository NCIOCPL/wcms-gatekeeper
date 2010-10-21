using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using GateKeeper.DocumentObjects.Summary;

namespace GKManagers.CMSManager
{
    internal class InlineSlotObjectMap
    {
        Dictionary<string, long> _map = new Dictionary<string, long>();

        public InlineSlotObjectMap()
        {
        }

        public void AddSection(string sectionID, long itemID)
        {
                _map.Add(sectionID, itemID);
        }

        public long this[string key]
        {
            get { return _map[key]; }
        }
    }
}
