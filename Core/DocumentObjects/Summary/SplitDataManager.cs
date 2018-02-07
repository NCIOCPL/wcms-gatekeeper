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
    /// </summary>
    public class SplitDataManager
    {
        private IList<SplitData> splitConfigs = null;

        private SplitDataManager()
        {
        }

        /// <summary>
        /// Creates an instance of the SplitDataManager class.
        /// </summary>
        /// <param name="dataFile">File path of the summary split metadata file.</param>
        /// <returns>A SplitDataManager object.</returns>
        static public SplitDataManager Create(string dataFile)
        {
            SplitDataManager instance = new SplitDataManager();

            JArray arr = JArray.Parse(File.ReadAllText(dataFile));
            instance.splitConfigs = arr.ToObject<IList<SplitData>>();

            return instance;
        }
    }
}
