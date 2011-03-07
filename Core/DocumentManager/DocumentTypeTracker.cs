using System;
using System.Collections.Generic;
using System.Text;

using GKManagers.BusinessObjects;

namespace GKManagers
{
    class DocumentTypeTracker
    {
        private DocumentTypeFlag _documentTypes = DocumentTypeFlag.Empty;

        public DocumentTypeTracker()
        {
        }

        public void AddDocumentType(CDRDocumentType docType)
        {
            _documentTypes |= DocumentTypeFlagUtil.MapDocumentType(docType);
        }
    
        public bool Contains(DocumentTypeFlag types)
        {
            bool result = false;

            if ((_documentTypes & types) != DocumentTypeFlag.Empty)
                result = true;

            return result;
        }
    }
}
