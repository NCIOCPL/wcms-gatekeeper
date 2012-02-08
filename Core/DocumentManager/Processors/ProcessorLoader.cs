using System;
using System.IO;
using System.Xml.Serialization;

using GateKeeper.Common;
using GKManagers.Configuration;

namespace GKManagers.Processors
{
    static class ProcessorLoader
    {
        /// <summary>
        /// Retrieves the list of processing directives.
        /// 
        /// It would be better to not deserialize the configuration file repeatedly,
        /// but the Extractors, Renderers and everything they instantiate would be shared
        /// across multiple threads.  Ideally, the only state data in those objects would
        /// from the configuration, in which case they'd be thread-safe.
        /// 
        /// Unfortunately, there's a strong tendency to create class members containing data
        /// about the items being processed, rather than passing such data between methods.
        /// That's a surefire recipe for race conditions.
        /// 
        /// A better design then might be to deserialize (just once) a list of Factories with all
        /// the settings they need for each device.
        /// </summary>
        /// <returns></returns>
        static public ProcessorPool Load()
        {
            ProcessorPool pool;

            DocumentProcessingSection config = DocumentProcessingSection.Instance;

            string filePath = config.ProcessingConfigurationFile.Value;
            XmlSerializer serializer = new XmlSerializer(typeof(ProcessorPool));

            try
            {
                using (TextReader reader = new StreamReader(filePath))
                {
                    pool = (ProcessorPool)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                string fmt = "Error deserializing configuration file {0}.";
                throw new ProcessingConfigurationException(string.Format(fmt, filePath), ex);
            }


            return pool;
        }

    }
}
