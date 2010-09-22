using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using NCI.Util;

namespace GKManagers.CMSManager
{
    class IDMapManager
    {
        private IDMapQuery m_query = new IDMapQuery();

        // <summary>
        // Stores a CDR to CMS ID mapping in the database.
        // </summary>
        /// <param name="mapping">A valid CDR to CMS ID mapping object.</param>
        /// <returns></returns>
        public int InsertCdrIDMapping(CMSIDMapping mapping)
        {
            return m_query.InsertCdrIDMapping(mapping.CdrID, mapping.CmsID, mapping.PrettyURL);
        }

        /// <summary>
        /// Loads the CDR to CMS ID mapping by searching based on the CDR ID.
        /// </summary>
        /// <param name="cdrid">The cdrid.</param>
        /// <returns></returns>
        public CMSIDMapping LoadCdrIDMappingByCdrid(int cdrid)
        {
            CMSIDMapping result = null;

            DataTable data = m_query.LoadCdrIDMappingByCdrid(cdrid);

            if (data != null && data.Rows.Count > 0)
            {
                result = new CMSIDMapping(Strings.ToInt(data.Rows[0]["CDRID"]),
                                            Strings.ToLong(data.Rows[0]["CMSId"]),
                                            Strings.Clean(data.Rows[0]["PrettyUrl"]));
            }

            return result;
        }

        /// <summary>
        /// Loads the CDR to CMS ID mapping by searching for it based on the PrettyURL.
        /// </summary>
        /// <param name="prettyUrl">The pretty URL.</param>
        /// <returns></returns>
        public CMSIDMapping LoadCdrIDMappingByPrettyUrl(string prettyUrl)
        {
            CMSIDMapping result = null;

            DataTable data = m_query.LoadCdrIDMappingByPrettyUrl(prettyUrl);

            if (data != null && data.Rows.Count > 0)
            {
                result = new CMSIDMapping(Strings.ToInt(data.Rows[0]["CDRID"]),
                                            Strings.ToLong(data.Rows[0]["CMSId"]),
                                            Strings.Clean(data.Rows[0]["PrettyUrl"]));
            }

            return result;
        }

        /// <summary>
        /// Updates the CDR to CMS ID mapping.
        /// </summary>
        /// <param name="cdrid">The cdrid.</param>
        /// <param name="cmsid">The cmsid.</param>
        /// <param name="prettyURL">The pretty URL.</param>
        /// <returns></returns>
        public int UpdateCdrIDMapping(int cdrid, long cmsid, string prettyURL)
        {
            return m_query.UpdateCdrIDMapping(cdrid, cmsid, prettyURL);
        }

        /// <summary>
        /// Deletes the CDR to CMS ID mapping entry.
        /// </summary>
        /// <param name="cdrid">The cdrid.</param>
        /// <returns></returns>
        public int DeleteCdrIDMapping(int cdrid)
        {
            return m_query.DeleteCdrIDMapping(cdrid);
        }
    }
}
