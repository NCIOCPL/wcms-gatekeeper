using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers.CMSManager
{
    /// <summary>
    /// Simple container class representing a single mapping between a
    /// CDRID, its corresponding ID in the CMS, and the pretty URL used
    /// to identify it on the web site.
    /// </summary>
    class CMSIDMapping
    {
        public int CdrID { get; private set; }
        public long CmsID { get; private set; }
        public string PrettyURL { get; private set; }


        /// <summary>
        /// Initializes a new instance of the CMSIDMapping class.
        /// </summary>
        /// <param name="cdrid">The CDR ID.</param>
        /// <param name="cmsid">The CMS ID.</param>
        /// <param name="prettyURL">The pretty URL.</param>
        public CMSIDMapping(int cdrid, long cmsid, string prettyURL)
        {
            CdrID = cdrid;
            CmsID = cmsid;
            PrettyURL = prettyURL;
        }
    }
}
