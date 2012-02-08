using System;

using GateKeeper.Common;
using GateKeeper.DocumentObjects;

namespace GKManagers.Processors
{
    public class ProcessorPool
    {
        public Processor[] Processors { get; set; }

        /// <summary>
        /// Gets the collection of processing targets.
        /// </summary>
        /// <param name="docType">Type document to retrieve targets for.</param>
        /// <returns></returns>
        public ProcessingTarget[] GetProcessingTargets(DocumentType docType)
        {
            ProcessingTarget[] retval;

            Processor[] filteredProcessors = Array.FindAll(Processors, processor => processor.Type == docType);

            if (filteredProcessors.Length == 0)
            {
                string message = "No processing targets found for {0} document type.";
                throw new ProcessingConfigurationException(string.Format(message, docType));
            }

            if (filteredProcessors.Length == 1)
                retval = filteredProcessors[0].ProcessingTargets;
            else
            {
                string message = "Multiple processing target groups found for {0} document type.";
                throw new ProcessingConfigurationException(string.Format(message, docType));
            }

            // Enforce targets being in the order specified in the order specified in the
            // TargetedDevice enum.  It is assumed that the screen device is always first.
            Array.Sort(retval);

            return retval;
        }
    }
}
