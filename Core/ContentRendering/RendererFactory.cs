using System;
using System.Collections.Generic;
using System.Text;
using GateKeeper.DocumentObjects;

namespace GateKeeper.ContentRendering
{
    public class RendererFactory
    {
        /// <summary>
        /// Creates a specific document type content rendering class.
        /// NOTE: Might not need this class (probably going to be done in the manager classes).
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        public static DocumentRenderer Create(DocumentType documentType)
        {
            DocumentRenderer renderer = null;

            switch (documentType)
            {
                case DocumentType.Summary:
                    renderer = new SummaryRenderer();
                    break;
                case DocumentType.Protocol:
                    renderer = new ProtocolRenderer();
                    break;
                case DocumentType.Media:
                    renderer = new MediaRenderer();
                    break;
                case DocumentType.GlossaryTerm:
                    renderer = new GlossaryTermRenderer();
                    break;
                case DocumentType.DrugInfoSummary:
                    renderer = new DrugInfoSummaryRenderer();
                    break;
                case DocumentType.GENETICSPROFESSIONAL:
                    renderer = new GeneticsProfessionalRenderer();
                    break;
                    // TODO: default: ?
            }

            return renderer;
        }
    }
}
