using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NCI.WCM.CMSManager.PercussionWebSvc;

namespace NCI.WCM.CMSManager.CMS
{
    /// <summary>
    /// Utility methods for working with PSItem objects.
    /// </summary>
    public static class PSItemUtils
    {
        const ulong idMask = 0xffffffffL;

        [Obsolete("use PercussionGuid.Equals()")]
        public static bool CompareItemIds(long itemID1, long itemID2)
        {
            return ((ulong)itemID1 | idMask) == ((ulong)itemID2 | idMask);
        }

        [Obsolete("use PercussionGuid", true)]
        public static long GetID(long itemID1)
        {
            return (long)((ulong)itemID1 & idMask);
        }

        public static string GetFieldValue(PSItem item, string fieldName)
        {
            string fieldValue = null;

            // Find the named field within the PSItem's field collection.
            IEnumerable<PSField> namedField =
                item.Fields.Where(field => field.name == fieldName);
            if (namedField.Count() > 0)
            {
                PSFieldValue value = namedField.ElementAt(0).PSFieldValue[0];
                fieldValue = value.RawData;
            }
            return fieldValue;
        }
    }
}
