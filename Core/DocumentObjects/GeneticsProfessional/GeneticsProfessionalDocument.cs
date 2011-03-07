using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.GeneticsProfessional
{
    /// <summary>
    /// Genetics professional document object.
    /// </summary>
    [Serializable]
    public class GeneticsProfessionalDocument : Document
    {
        #region Fields

        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _shortName = string.Empty;
        private string _suffix = string.Empty;
        private List<string> _degrees = new List<string>();
        private List<string> _specialties = new List<string>();
        private List<PracticeLocation> _practiceLocations = new List<PracticeLocation>();
        private List<FamilyCancerSyndrome> _familyCancerSyndromes = new List<FamilyCancerSyndrome>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Genetics professional first name.
        /// </summary>
        public string FirstName
        {
            get { return _firstName; }
            internal set { _firstName = value; }
        }

        /// <summary>
        /// Genetics professional last name.
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            internal set { _lastName = value; }
        }

        /// <summary>
        /// Genetics professional short name.
        /// </summary>
        public string ShortName
        {
            get { return _shortName; }
            internal set { _shortName = value; }
        }

        /// <summary>
        /// Genetics professional suffix (MD, PHD...etc).
        /// </summary>
        public string Suffix
        {
            get { return _suffix; }
            internal set { _suffix = value; }
        }

        /// <summary>
        /// Degrees held by the genetics professional.
        /// </summary>
        public List<string> Degrees
        {
            get { return _degrees; }
            internal set { _degrees = value; }
        }

        /// <summary>
        /// Genetics professional specialties.
        /// </summary>
        public List<string> Specialties
        {
            get { return _specialties; }
            internal set { _specialties = value; }
        }

        /// <summary>
        /// Collection of practice locations.
        /// </summary>
        public List<PracticeLocation> PracticeLocations
        {
            get { return _practiceLocations; }
            internal set { _practiceLocations = value; }
        }

        /// <summary>
        /// Collection of cancer syndromes and related cancer types.
        /// </summary>
        public List<FamilyCancerSyndrome> FamilyCancerSyndromes
        {
            get { return _familyCancerSyndromes; }
            internal set { _familyCancerSyndromes = value; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returnes a System.String representation of the GeneticsProfessionalDocument object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(string.Format(" FirstName = {0} LastName = {1} ShortName = {2} Suffix = {3}\n",
                this.FirstName, this.LastName, this.ShortName, this.Suffix));

            sb.Append("Specialties = \n");
            foreach (string specialty in this.Specialties)
            {
                sb.Append(string.Format("Specialty = {0}\n", specialty));
            }

            sb.Append("Degrees = \n");
            foreach (string degree in this.Degrees)
            {
                sb.Append(string.Format("Degree = {0}\n", degree));
            }

            sb.Append("Practice Locations = \n");
            foreach (PracticeLocation practiceLocation in this.PracticeLocations)
            {
                sb.Append(string.Format("Practice Location = {0}\n", practiceLocation.ToString()));
            }

            sb.Append("Family Cancer Syndromes = \n");
            foreach (FamilyCancerSyndrome familyCancerSyndrome in this.FamilyCancerSyndromes)
            {
                sb.Append(string.Format("Family Cancer Syndrome = {0}\n", familyCancerSyndrome.ToString()));
            }

            sb.Append(string.Format("Html = {0}",this.Html));

            return sb.ToString();
        }

        #endregion
    }
}
