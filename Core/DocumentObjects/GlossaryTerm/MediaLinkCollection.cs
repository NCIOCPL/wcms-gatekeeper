using System.Collections.Generic;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;

namespace GateKeeper.DocumentObjects.GlossaryTerm
{
    /// <summary>
    /// Manages the logic for managing separate collections of MediaLink objects for multiple MediaLink audiences.
    /// </summary>
    public class MediaLinkCollection
    {
        private Dictionary<AudienceType, List<MediaLink>> LinkSet = new Dictionary<AudienceType, List<MediaLink>>();

        /// <summary>
        /// Retrieves the collection of MediaLink objects for a given audience.
        /// </summary>
        /// <param name="audience">The audience to retrieve the list for.</param>
        /// <returns>A (possibly empty) copy of the list of MediaLink objects associated with audience.</returns>
        /// <remarks>The returned List is a copy of the one stored in the MediaLinkCollection object.  Changes
        /// to the returned object will not be reflected in the MediaLinkCollection.</remarks>
        public List<MediaLink> this[AudienceType audience]
        {
            get
            {
                List<MediaLink> returnCopy;

                // If LinkSet contains a list for the audience, return a copy.
                // Otherwise, return an empty list.
                if (LinkSet.ContainsKey(audience))
                    returnCopy = new List<MediaLink>(LinkSet[audience]);
                else
                    returnCopy = new List<MediaLink>();

                return returnCopy;
            }
        }

        /// <summary>
        /// Add a MediaLink object to the collection
        /// </summary>
        /// <param name="audience">The audience the MediaLink is intended for.</param>
        /// <param name="link">The MediaLink object to be added.</param>
        public void Add(AudienceType audience, MediaLink link)
        {
            // Make sure the list we want to store in already exists.
            if (!LinkSet.ContainsKey(audience))
                LinkSet.Add(audience, new List<MediaLink>());

            LinkSet[audience].Add(link);
        }

        /// <summary>
        /// A collection of audience types stored in this link collection.
        /// </summary>
        public IEnumerable<AudienceType> Audiences
        {
            get { return LinkSet.Keys; }
        }
    }
}
