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
    public class SplitDataManager : ISplitDataManager, IDisposable
    {
        // Logger
        static DocumentLogBuilder Log = new DocumentLogBuilder();

        // Internal collection of split metadata.
        private Dictionary<int, SplitData> splitConfigs = new Dictionary<int, SplitData>();

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
            try
            {
                if (!String.IsNullOrWhiteSpace(dataFile))
                {
                    theInstance = CreateFromString(File.ReadAllText(dataFile));
                }
                else
                {
                    // Log that the configuration file is not specified.
                    Log.CreateError(typeof(SplitDataManager), "Create", "Summary split metadata datafile not specified.");
                }
            }
            catch (Exception)
            {
                // Log any errors, but allow execution to consider.
                Log.CreateError(typeof(SplitDataManager), "Create", "Error loading summary split metadata file.");
            }

            return theInstance;
        }

        /// <summary>
        /// Creates an instance of the SplitDataManager class.
        /// </summary>
        /// <param name="json">String containing the summary split metadata.</param>
        /// <returns>A SplitDataManager object.</returns>
        static public SplitDataManager CreateFromString(string json)
        {
            // Prevent old data from being used by explicitly overwriting theInstance.
            theInstance = new SplitDataManager();

            try
            {
                if (!String.IsNullOrWhiteSpace(json))
                {
                    JArray arr = JArray.Parse(json);
                    IList <SplitData> configList = arr.ToObject<IList<SplitData>>();
                    foreach(SplitData item in configList)
                    {
                        if (!Instance.splitConfigs.ContainsKey(item.CdrId))
                            Instance.splitConfigs.Add(item.CdrId, item);
                        else
                            Log.CreateError(typeof(SplitDataManager), "CreateFromString", String.Format("Duplicate document ID found '{0}'.", item.CdrId));
                    }
                }
                else
                {
                    // Log that the configuration data is missing.
                    Log.CreateError(typeof(SplitDataManager), "CreateFromString", "Summary split metadata string is empty.");
                }
            }
            catch (Exception)
            {
                // Log any errors, but allow execution to consider.
                Log.CreateError(typeof(SplitDataManager), "CreateFromString", "Error parsing summary split metadata text.");
            }

            return theInstance;
        }

        /// <summary>
        /// Checks whether summaryID exists in the collection of summary data.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <returns>True if the summary is supposed to be split, false otherwise.</returns>
        public bool SummaryIsSplit(int summaryID)
        {
            return splitConfigs.ContainsKey(summaryID);
        }

        /// <summary>
        /// Retrieves a SplitData object containing information about how to split a given summary.
        /// </summary>
        /// <param name="summaryID"></param>
        /// <returns>SplitData object. NULL if summaryID is not found.</returns>
        public SplitData GetSplitData(int summaryID)
        {
            if (splitConfigs.ContainsKey(summaryID))
                return splitConfigs[summaryID];
            else
                return null;
        }

    }
}
