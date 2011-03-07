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

        public static string GetFieldValue(PSItem item, string fieldName)
        {
            return GetFieldValue(item.Fields, fieldName);
        }

        public static string GetFieldValue(PSField[] fieldCollection, string fieldName)
        {
            string fieldValue = string.Empty;

            IEnumerable<PSField> namedField =
                fieldCollection.Where(field => field.name == fieldName);
            if (namedField != null)
            {
                if (namedField.Count() > 0)
                {
                    PSFieldValue value = namedField.ElementAt(0).PSFieldValue[0];
                    fieldValue = value.RawData;
                }
            }

            return fieldValue;
        }

        public static string[] GetChildFieldValues(PSItem item, string collectionName, string subfieldName)
        {
            string[] foundValues = new string[] { };

            if (item.Children != null && item.Children.Length > 0)
            {
                PSItemChildren targetedField = Array.Find(item.Children, childField => childField.Name == collectionName);
                if (targetedField != null && targetedField.PSChildEntry!= null)
                {
                    List<string> resultBuilder = new List<string>();
                    Array.ForEach(targetedField.PSChildEntry, childFieldset =>
                    {
                        resultBuilder.Add(GetFieldValue(childFieldset.PSField, subfieldName));
                    });
                    foundValues = resultBuilder.ToArray();
                }
            }

            return foundValues;
        }
    }
}