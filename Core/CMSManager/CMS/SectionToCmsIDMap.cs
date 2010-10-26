using System;
using System.Collections.Generic;


namespace NCI.WCM.CMSManager.CMS
{
    /// <summary>
    /// Map for section placeholder values to their CMS IDs.
    /// </summary>
    public class SectionToCmsIDMap
    {
        Dictionary<string, long> _map = new Dictionary<string, long>();

        public SectionToCmsIDMap()
        {
        }

        public void AddSection(string sectionID, long itemID)
        {
                _map.Add(sectionID, itemID);
        }

        public long this[string sectionKey]
        {
            get { return _map[sectionKey]; }
        }

        public bool ContainsSectionKey(string sectionKey)
        {
            return _map.ContainsKey(sectionKey);
        }
    }
}
