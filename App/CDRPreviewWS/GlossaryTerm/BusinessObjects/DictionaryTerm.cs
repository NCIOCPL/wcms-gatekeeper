using System;
using System.Runtime.Serialization;

namespace NCI.Web.Dictionary.BusinessObjects
{
    [DataContract()]
    public class DictionaryTerm
    {
        public DictionaryTerm()
        {
            // Guarantee that the arrays are never null.
            Aliases = new Alias[] { };
            Images = new ImageReference[] { };
        }

        /// <summary>
        ///  The Term's ID
        /// </summary>
        [DataMember(Name = "id" )]
        public int ID { get; set; }

        /// <summary>
        /// The Term's name
        /// </summary>
        [DataMember(Name = "term")]
        public String Term { get; set; }

        /// <summary>
        /// Data structure describing how a dictionary Term is pronounced.
        /// </summary>
        [DataMember(Name = "pronunciation")]
        public Pronunciation Pronunciation { get; set; }

        /// <summary>
        /// Does the term have a pronuncation element?
        /// </summary>
        public Boolean HasPronunciation
        {
            get
            {
                return (Pronunciation != null)
                  && Pronunciation.HasPronunciation;
            }
        }

        /// <summary>
        /// The date the Term was first published
        /// </summary>
        [DataMember(Name = "date_first_published")]
        public String DateFirstPublished { get; set; }

        /// <summary>
        /// The date the Term was last modified.
        /// </summary>
        [DataMember(Name = "date_last_modified")]
        public String DateLastModified { get; set; }

        /// <summary>
        /// Possibly empty array of strings containing the URLs of images associated with this definition.
        /// </summary>
        [DataMember(Name = "images")]
        public ImageReference[] Images { get; set; }

        /// <summary>
        /// Data structure containing the Term's definition.
        /// </summary>
        [DataMember(Name = "definition")]
        public Definition Definition { get; set; }

        /// <summary>
        /// Possibly empty array of other names for the Term.  (Only populated for drug dictionary)
        /// </summary>
        [DataMember(Name = "alias")]
        public Alias[] Aliases { get; set; }

        /// <summary>
        /// Does the term actually have any aliases?
        /// </summary>
        public bool HasAliases
        {
            get { return Aliases != null && Aliases.Length > 0; }
        }

        /// <summary>
        /// related links. (Cancer Term and Genetics only)
        /// </summary>
        [DataMember(Name = "related")]
        public RelatedItems Related { get; set; }

        /// <summary>
        /// Does the term include a Related Items structure?
        /// </summary>
        public bool HasRelatedItems
        {
            get { return this.Related != null; }
        }
    }
}
