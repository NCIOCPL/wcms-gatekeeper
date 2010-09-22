using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using NCI.Data;

namespace GKManagers.CMSManager
{
    /// <summary>
    /// Data access layer for manipulating the collection of CDR to CMS ID mappings.
    /// </summary>
    internal class IDMapQuery
    {
        #region Fields

        private const string InsertCMSMapping = "usp_InsertCMSIDMap";
        private const string GetCMSMappingByCdird = "usp_GetCMSIDByCDRID";
        private const string GetCMSMappingByPrettyUrl = "usp_GetCMSIDByCDRID";
        private const string UpdateCMSMapping = "usp_UpdateCMSIDMap";
        private const string DeleteCMSMapping = "usp_DeleteCMSID";

        private string GateKeeperConnectString;

        #endregion

        public IDMapQuery()
        {
            GateKeeperConnectString = ConfigurationManager.ConnectionStrings["GateKeeper"].ConnectionString;
        }


        /// <summary>
        /// Creates a new CDR ID to CMS ID mapping entry.
        /// </summary>
        /// <param name="cdrid">The cdrid.</param>
        /// <param name="cmsid">The cmsid.</param>
        /// <param name="prettyURL">The pretty URL.</param>
        /// <returns></returns>
        public int InsertCdrIDMapping(int cdrid, long cmsid, string prettyURL)
        {
            SqlParameter[] parms ={
                            new SqlParameter("@CDRID", SqlDbType.Int){Value=cdrid},
                            new SqlParameter("@CMSID", SqlDbType.BigInt){Value=cmsid},
                            new SqlParameter("@PrettyURL", SqlDbType.VarChar){Value=prettyURL},
                            new SqlParameter("@Status_Text", SqlDbType.VarChar){Direction = ParameterDirection.Output, Size = 255}
                                  };

            return SqlHelper.ExecuteNonQuery(GateKeeperConnectString,
                    CommandType.StoredProcedure,
                    InsertCMSMapping, parms);
        }

        /// <summary>
        /// Loads the CDR ID Mapping by searching for the CDR ID.
        /// </summary>
        /// <param name="cdrid">The cdrid.</param>
        /// <returns></returns>
        public DataTable LoadCdrIDMappingByCdrid(int cdrid)
        {
            DataTable results;

            SqlParameter[] parms ={
                              new SqlParameter("@CDRID", SqlDbType.Int){Value = cdrid},
                              new SqlParameter("@Status_Text", SqlDbType.VarChar){Direction = ParameterDirection.Output, Size = 255}
                                  };

            results = SqlHelper.ExecuteDatatable(GateKeeperConnectString,
                    CommandType.StoredProcedure,
                    GetCMSMappingByCdird, parms);

            return results;
        }

        /// <summary>
        /// Loads the CDR ID Mapping by searching for an exact match on the pretty URL.
        /// </summary>
        /// <param name="prettyUrl">The pretty URL.</param>
        /// <returns></returns>
        public DataTable LoadCdrIDMappingByPrettyUrl(string prettyUrl)
        {
            DataTable results;

            SqlParameter[] parms ={
                              new SqlParameter("@PrettyURL", SqlDbType.Int){Value = prettyUrl},
                              new SqlParameter("@Status_Text", SqlDbType.VarChar){Direction = ParameterDirection.Output, Size = 255}
                                  };

            results = SqlHelper.ExecuteDatatable(GateKeeperConnectString,
                    CommandType.StoredProcedure,
                    GetCMSMappingByPrettyUrl, parms);

            return results;
        }

        /// <summary>
        /// Updates the CDR ID mapping.
        /// </summary>
        /// <param name="cdrid">The cdrid.</param>
        /// <param name="cmsid">The cmsid.</param>
        /// <param name="prettyURL">The pretty URL.</param>
        /// <returns></returns>
        public int UpdateCdrIDMapping(int cdrid, long cmsid, string prettyURL)
        {
            SqlParameter[] parms ={
                            new SqlParameter("@CDRID", SqlDbType.Int){Value=cdrid},
                            new SqlParameter("@CMSID", SqlDbType.BigInt){Value=cmsid},
                            new SqlParameter("@PrettyURL", SqlDbType.VarChar){Value=prettyURL},
                            new SqlParameter("@Status_Text", SqlDbType.VarChar){Direction = ParameterDirection.Output, Size = 255}
                                  };

            return SqlHelper.ExecuteNonQuery(GateKeeperConnectString,
                    CommandType.StoredProcedure,
                    UpdateCMSMapping, parms);
        }

        /// <summary>
        /// Deletes the CDR ID mapping.
        /// </summary>
        /// <param name="cdrid">The cdrid.</param>
        /// <returns></returns>
        public int DeleteCdrIDMapping(int cdrid)
        {
            SqlParameter[] parms ={
                            new SqlParameter("@CDRID", SqlDbType.Int){Value=cdrid},
                            new SqlParameter("@Status_Text", SqlDbType.VarChar){Direction = ParameterDirection.Output, Size = 255}
                                  };

            return SqlHelper.ExecuteNonQuery(GateKeeperConnectString,
                    CommandType.StoredProcedure,
                    DeleteCMSMapping, parms);
        }
    }
}