using System;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    [DataContract()]
    public class DictionarySuggestion
    {
        /// <summary>
        ///  The Term's ID
        /// </summary>
        [DataMember(Name = "id")]
        public int ID { get; set; }

        /// <summary>
        /// The Term's name
        /// </summary>
        [DataMember(Name = "term")]
        public String Term { get; set; }
    }
}
