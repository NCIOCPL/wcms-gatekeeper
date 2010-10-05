using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GKManagers.CMSManager.PercussionWebSvc;

namespace GKManagers.CMSManager.CMS
{
    /// <summary>
    /// Utility methods for working with PSItem objects.
    /// </summary>
    static class PSItemUtils
    {
        public static bool CompareItemIds(long itemID1, long itemID2)
        {
            return ((ulong)itemID1 | 0xffffffff) == ((ulong)itemID2 | 0xffffffff);
        }

        public static string GetFieldValue(PSItem item, string fieldName)
        {
            string fieldValue = null;

            // Find the field with the pretty url name.
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
