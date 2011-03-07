using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.GeneticsProfessional
{
    /// <summary>
    /// Class to represent family cancer syndrome.
    /// </summary>
    [Serializable]
    public class FamilyCancerSyndrome
    {
        #region Fields

        private string _syndromeName = string.Empty;
        private System.Collections.Generic.List<string> _cancerTypeSites = new List<string>();

        #endregion 

        #region Public Properties

        /// <summary>
        /// Syndrome name.
        /// </summary>
        public string SyndromeName
        {
            get { return _syndromeName; }
            internal set { _syndromeName = value; }
        }

        /// <summary>
        /// Cancertypes (family type name: cancer site)
        /// </summary>
        public System.Collections.Generic.List<string> CancerTypeSites
        {
            get { return _cancerTypeSites; }
            internal set { _cancerTypeSites = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returnes a System.String representation of the FamilyCancerSyndrome object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" SyndromeName = {0}\n", this.SyndromeName));

            sb.Append("Family Cancer Syndromes = \n");
            foreach (string cancerTypeSite in this.CancerTypeSites)
            {
                sb.Append(string.Format("CancerTypeSite = {0}\n", cancerTypeSite));
            }

            return sb.ToString();
        }

        #endregion
    }
}
