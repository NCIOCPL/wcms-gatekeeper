using System;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    /// <summary>
    /// Describes how a dictionary Term is pronounced.
    /// </summary>
    [DataContract()]
    public class Pronunciation
    {
        public Pronunciation()
        {
            // Guarantee non-null values.
            Key = string.Empty;
            Audio = string.Empty;
        }

        /// <summary>
        /// Does the term have a pronunciation?
        /// </summary>
        public Boolean HasPronunciation
        {
            get { return HasKey || HasAudio; }
        }

        /// <summary>
        /// Pronunciation key.  Possibly empty.
        /// </summary>
        [DataMember( Name = "key" )]
        public String Key { get; set; }

        /// <summary>
        /// Does the pronunciation include a key?  (Most likely not for Spanish.)
        /// </summary>
        public Boolean HasKey { get { return !String.IsNullOrEmpty(Key); } }

        /// <summary>
        /// Possibly empty URL of an audio file containing the pronunciation. (Cancer Term and Genetics only)
        /// </summary>
        [DataMember(Name = "audio")]
        public String Audio { get; set; }

        /// <summary>
        /// Does the pronunciation include an audio example?
        /// </summary>
        public Boolean HasAudio { get { return !String.IsNullOrEmpty(Audio); } }
    }
}
