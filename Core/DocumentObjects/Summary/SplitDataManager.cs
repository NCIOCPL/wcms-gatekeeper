using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateKeeper.DocumentObjects.Summary
{
    /// <summary>
    /// Loads and facilitates evaluation of metadata for summaries in the summary-split SEO pilot.
    /// A single instance of this object is created prior to document processing and is then
    /// used for retrieving summary data.
    ///
    /// This class is a "disposable singleton". The intent is that there will only be one instance
    /// in existence, and accessible from scopes other than where it was instantiated. But, unlike
    /// a traditional singleton, the object is explicitly disposed once it's no longer needed.
    /// </summary>
    public class SplitDataManager : IDisposable
    {
        // Logger
        static DocumentLogBuilder Log = new DocumentLogBuilder();

        // Internal collection of split metadata.
        private IList<SplitData> splitConfigs = null;

        // The one-and-only instance of the SplitDataManager object.
        static SplitDataManager theInstance = null;

        /// <summary>
        /// Internal use only. To create a SplitDataManager object, use the Create method instead.
        /// </summary>
        private SplitDataManager()
        {
        }

        /// <summary>
        /// Disposes the SplitDataManager instance by removing the intenral reference.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the internal SplitDataManager reference when called with disposing set to true.
        /// </summary>
        /// <param name="disposing">Set to true when disposing of the object, per IDisposable.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                theInstance = null;
        }

        /// <summary>
        /// Returns an instance of SplitDataManager. The correct usage is to first create the instance via
        /// the Create method, and then use Instance to access the object from other scopes.
        /// </summary>
        public static SplitDataManager Instance
        {
            get
            {
                if (theInstance == null)
                    theInstance = new SplitDataManager();

                return theInstance;
            }
        }



        /// <summary>
        /// Creates an instance of the SplitDataManager class.
        /// </summary>
        /// <param name="dataFile">File path of the summary split metadata file.</param>
        /// <returns>A SplitDataManager object.</returns>
        static public SplitDataManager Create(string dataFile)
        {
            SplitDataManager instance = new SplitDataManager();

            try
            {
                if (!String.IsNullOrWhiteSpace(dataFile))
                {
                    JArray arr = JArray.Parse(File.ReadAllText(dataFile));
                    instance.splitConfigs = arr.ToObject<IList<SplitData>>();
                }
                else
                {
                    // Log that the configuration file is not specified.
                    Log.CreateError(typeof(SplitDataManager), "Create", "Summary split metadata datafile not specified.");
                    instance.splitConfigs = new List<SplitData>();
                }
            }
            catch (Exception)
            {
                // Log any errors, but allow execution to consider.
                Log.CreateError(typeof(SplitDataManager), "Create", "Error loading summary split metadata file.");
                instance.splitConfigs = new List<SplitData>();
            }

            return instance;
        }
    }
}
