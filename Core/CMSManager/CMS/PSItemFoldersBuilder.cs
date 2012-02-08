using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.PercussionWebSvc;

namespace NCI.WCM.CMSManager.CMS
{
    static class PSItemFoldersBuilder
    {
        /// <summary>
        /// Builds an array of PSItemFolders objects.
        /// </summary>
        /// <param name="basePath">The server base path.</param>
        /// <param name="potentialPaths">A collection of strings containing relative paths for the collection,
        /// existing PSItemFolders objects to include in the collection, and IEnumerable collections of
        /// strings or PSItemFolders objects.</param>
        /// <returns>An array combining all of the paths transformed into absolute paths and embedded
        /// in PSItemFolders objects.</returns>
        static public PSItemFolders[] Build(string basePath, params Object[] potentialPaths)
        {
            List<string> newPaths = new List<string>();
            List<PSItemFolders> existingPaths = new List<PSItemFolders>();
            PSItemFolders[] collection;

            // Put all paths in a single collection.
            foreach (object item in potentialPaths)
            {
                // skip the empties.
                if (item == null)
                    continue;

                if (item is string)
                    newPaths.Add(item as string);
                else if (item is IEnumerable<string>)
                    newPaths.AddRange(item as IEnumerable<string>);
                else if (item is PSItemFolders)
                    existingPaths.Add(item as PSItemFolders);
                else if (item is IEnumerable<PSItemFolders>)
                    existingPaths.AddRange(item as IEnumerable<PSItemFolders>);
                else
                    throw new ArgumentException("Arguments must be of type string or an IEnumerable<> collection of string.");
            }

            // Convert collection of new paths into a collection of PSItemFolders objects,
            // Add to the list of existing paths,
            // Convert the aggregated list to an array.
            existingPaths.AddRange(newPaths.Select(path =>
            {
                PSItemFolders folder = new PSItemFolders();
                folder.path = basePath + path;
                return folder;
            }));
            collection = existingPaths.ToArray();

            return collection;
        }
    }
}
