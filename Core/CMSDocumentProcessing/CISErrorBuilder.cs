using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSDocumentProcessing
{
    static class CISErrorBuilder
    {
        /// <summary>
        /// Helper method to create an error message for summary pages referenced from non-Summary content items.
        /// </summary>
        /// <param name="targetPageID">PercussionGuid of the summary page being referenced.</param>
        /// <param name="targetCdrid">CDR ID containing the page being referenced.</param>
        /// <param name="parentItem">The offending content item containing the reference.</param>
        /// <returns>A nicely formatted error message.</returns>
        static public string BuildNonPDQReferenceMessage(PercussionGuid targetPageID, int targetCdrid, PSItem parentItem)
        {
            string message;
            string fmt = "A page in CDRID {0} is referenced from a non-PDQ content item. Referenced page id: {1}, referenced from content item {2} with path '{3}' and named '{4}'.";

            string path = string.Empty;
            if (parentItem.Folders != null && parentItem.Folders.Length > 0)
            {
                path = parentItem.Folders[0].path;
            }

            string itemName = PSItemUtils.GetFieldValue(parentItem, "sys_title");

            PercussionGuid parentID = new PercussionGuid(parentItem.id);

            message = string.Format(fmt, targetCdrid, targetPageID, parentID, path, itemName);

            return message;
        }

        static public string BuildMissingReferenceMessage(int targetCdrid, string targetSectionID, PSItem parentItem)
        {
            string message;
            string fmt = "Section ID {0} in CDRID {1} is being referenced by the content item with the path {2} ('{3}'), but this section does not exist. The referring content item is {4}.";

            string path = string.Empty;
            if (parentItem.Folders != null && parentItem.Folders.Length > 0)
            {
                path = parentItem.Folders[0].path;
            }

            string itemName = PSItemUtils.GetFieldValue(parentItem, "sys_title");

            PercussionGuid parentID = new PercussionGuid(parentItem.id);

            message = string.Format(fmt, targetSectionID, targetCdrid, path, itemName, parentID);

            return message;
        }

        /// <summary>
        /// Helper method to create an error message for Permanent Links that have an external 
        /// relationship that is not permitted.
        /// </summary>
        /// <param name="targetPageID">PercussionGuid of the PermanentLink being referenced.</param>
        /// <param name="targetCdrid">CDR ID containing the page being referenced.</param>
        /// <param name="parentItem">The offending content item containing the reference.</param>
        /// <returns>A nicely formatted error message.</returns>
        /// <remarks>Originally to prevent a PermanentLink from being deleted if there is a 
        /// CustomLink pointing to it.</remarks>
        static public string BuildPermanentLinkError(PercussionGuid permanentLink, int targetCdrid, PSItem parentItem)
        {
            string message;
            string fmt = "A Permanent Link in CDRD {0} is referenced by a non-PDQ content item that is not permitted. Referenced Permanent Link Percussion ID is '{1}', referred by the ID '{2}', found in path '{3}'.";

            string path = string.Empty;
            if (parentItem.Folders != null && parentItem.Folders.Length > 0)
            {
                path = parentItem.Folders[0].path;
            }

            string itemName = PSItemUtils.GetFieldValue(parentItem, "sys_title");

            PercussionGuid parentID = new PercussionGuid(parentItem.id);

            message = string.Format(fmt, targetCdrid, permanentLink, parentID, path);

            return message;
        }
    }
}
