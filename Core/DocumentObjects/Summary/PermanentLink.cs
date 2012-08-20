using System;
using System.Collections.Generic;
using System.Linq;

using NCI.Util;

using GateKeeper.Common;


namespace GateKeeper.DocumentObjects.Summary
{
    [Serializable]
    public class PermanentLink
    {

        /// <summary>
        /// Constructor for a Permanent Link when no Guid is available 
        /// </summary>
        /// <param name="id">Identifying number of the Permanent Link.</param>
        /// <param name="sectionID">ID of the section this link refers to.</param>
        /// <param name="sectionTitle">Title of the section its linked to. Will be reused at the title
        /// of the Permanent Link in Percussion.</param>
        /// <remarks>A section URL is something that is updated prior to creating a new link or 
        /// updating an existing one. There is no need for it in a constructor.</remarks>
        public PermanentLink(string id, string sectionID, string sectionTitle)
        {
            ID = id;
            SectionID = sectionID;
            Title = sectionTitle;
        }

        /// <summary>
        /// Constructor for a Permanent Link; Should be used when guid is already identified for the 
        /// Permanent Link, which is most likely when an existing link in Percussion is being converted 
        /// to this GateKeeper recognized type.
        /// </summary>
        /// <param name="id">Identifying number of the Permanent Link.</param>
        /// <param name="sectionID">ID of the section this link refers to.</param>
        /// <param name="sectionTitle">Title of the section its linked to. Will be reused at the title
        /// of the Permanent Link in Percussion.</param>
        /// <param name="guid">Existing Guid as identified by Percussion</param>
        /// <remarks>A section URL is something that is updated prior to creating a new link or 
        /// updating an existing one. There is no need for it in a constructor.</remarks>
        public PermanentLink(string id, string sectionID, string sectionTitle, long guid)
        {
            ID = id;
            SectionID = sectionID;
            Title = sectionTitle;
            Guid = guid;
        }


        /// <summary>
        /// The identifying number of the Permanent Link
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The title of the section that this Permanent Link is poiting to; Always set in constructor. 
        /// Will be re-used as the title of the Permanent Link in Percussion
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The ID of the section that this Permanent Link is pointing to; Always set in constructor
        /// </summary>
        public string SectionID { get; private set; }

        /// <summary>
        /// URL is determined for new & updated Permanent Links during the editing try/catch
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Guid can be determined after creation if not provided previously (by a constructor)
        /// </summary>
        public long Guid { get; set; }

        /*
        /// <summary>
        /// String instance of PermanentLink created during tested.
        /// </summary>
        /// <returns>String interpretation of the PermanentLink</returns>
        public override string ToString()
        {
            return "PermanentLink(#" + ID + ") @" + SectionID;
        }
        */ 

        /// <summary>
        /// Comparison of Permanent Links. If not comparing against another PermanentLink, automatically 
        /// false. Otherwise, a comparison based on its/their identifying numbers (IDs) is made.
        /// </summary>
        /// <param name="obj">Object being compared to. Automatically false if not a Permanent Link.</param>
        /// <returns>Boolean value determing if two Permanent Links have the same ID.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            PermanentLink other = (PermanentLink)obj;
            return ID.Equals(other.ID);
        }
    }
}
