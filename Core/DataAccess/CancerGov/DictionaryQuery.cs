using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Dictionary;
using GateKeeper.DataAccess.StoreProcedures;

namespace GateKeeper.DataAccess.CancerGov
{
    /// <summary>
    /// This is a helper class, used by the GlossaryTermQuery and TerminologyQuery
    /// classes to implement the shared functionality of working with the Dictionary tables.
    /// Note that this class is intended to be used as a helper object and is not meant
    /// to be subclassed.
    /// </summary>
    class DictionaryQuery
    {
        public bool SaveDocument(int termId, List<GeneralDictionaryEntry> entries, string userID)
        {
            DataTable dictionary = new DataTable("dictionary");
            dictionary.Columns.Add("TermID", typeof(int));
            dictionary.Columns.Add("TermName", typeof(String));
            dictionary.Columns.Add("Dictionary", typeof(String));
            dictionary.Columns.Add("Language", typeof(String));
            dictionary.Columns.Add("Audience", typeof(String));
            dictionary.Columns.Add("ApiVers", typeof(String));
            dictionary.Columns.Add("Object", typeof(String));

            foreach (GeneralDictionaryEntry entry in entries)
            {
                dictionary.Rows.Add(entry.TermID, entry.TermName, entry.Dictionary, entry.Language, entry.Audience, entry.ApiVersion, entry.Object);
            }
            
            throw new NotImplementedException();
        }

        public void DeleteDocument(Document document, ContentDatabase dbName, string userID)
        {
            throw new NotImplementedException();
        }

        public void PushDocumentToPreview(Document document, string userID)
        {
            throw new NotImplementedException();
        }

        public void PushDocumentToLive(Document document, string userID)
        {
            throw new NotImplementedException();
        }


        ///// <summary>
        ///// Not implemented.  It is an error, by design, to call SaveDBDocument on
        ///// objects of type DictionaryQuery.  Do not attempt to use the one in
        ///// the base class.
        ///// 
        ///// Dictionary objects do not appear in the Document table.
        ///// The GlossaryTermQuery and TerminologyQuery classes are responsible for
        ///// making their own calls.
        ///// </summary>
        ///// <param name="doc"></param>
        ///// <param name="db"></param>
        ///// <param name="transaction"></param>
        //new protected void SaveDBDocument(Document doc, Database db, DbTransaction transaction)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
