
using GateKeeper.DocumentObjects.Summary;
using System;
using System.Xml;

namespace GKManagers.Preprocessors
{
    /// <summary>
    /// Preprocessor for Summary documents
    /// </summary>
    class SummaryPreprocessor
    {

        const string CDRID_ATTRIBUTE = "id";
        public void Preprocess(XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            // Get Split data. (Data is loaded prior to document processing.)
            SplitDataManager splitData = SplitDataManager.Instance;

        }
    }
}
