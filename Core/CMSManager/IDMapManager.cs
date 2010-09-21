using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSManager
{
    class IDMapManager
    {
        private IDMapQuery m_query = new IDMapQuery();

        public bool InsertCdrIDMapping(int cdrid, long cmsid, string prettyURL)
        {
            return m_query.InsertCdrIDMapping(cdrid, cmsid, prettyURL);
        }
    }
}
