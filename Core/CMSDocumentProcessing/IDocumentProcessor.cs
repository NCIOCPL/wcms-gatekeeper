using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GateKeeper.DocumentObjects;

namespace GKManagers.CMSDocumentProcessing
{
    /// <summary>
    /// Defines the generic interface for all Document Processors. The variety of data structures
    /// involved in storing and promoting a given type of document requires different concrete
    /// classes.
    /// </summary>
    public interface IDocumentProcessor
    {
        /// <summary>
        /// Entry point for processing a PDQ Document object and manipulating it
        /// through the CMS.
        /// </summary>
        /// <param name="documentObject">Object representing a specific PDQ document type</param>
        /// <remarks>All concrete PDQ Document classes derive from Document, but each has specific
        /// properties. Using the base Document class allows us to use a single interface for all
        /// document processors. The tradeoff for this flexibility is that every class
        /// implementing IDocumentProcessor is required to verify that the document it receives
        /// is of the specific type it uses.</remarks>
        void ProcessDocument(Document documentObject);

        /// <summary>
        /// Deletes the content item.
        /// </summary>
        /// <param name="documentCdrID">The document ID.</param>
        void DeleteContentItem(int documentCdrID);
    }
}
