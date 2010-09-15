using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using GKManagers;
using GKManagers.BusinessObjects;
using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;

namespace PromotionTester
{
    class Program
    {
        #region Fields
        readonly static private string _usageInstructions = @"
    PromotionTester <file> <promotionAction>

where:

    <file>  is a serialized RequestData object.
    <promotionAction> is one of PromoteToStaging, PromoteToPreview, PromoteToLive
";
        #endregion

        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                RequestData data = DeserializeData(args[0]);
                ProcessActionType processAction = GetPromotionAction(args[1]);

                DocumentXPathManager xPathManager = new DocumentXPathManager();

                // Instantiate a promoter and go to town.
                DocumentPromoterBase promoter =
                    DocumentPromoterFactory.Create(data, 18, processAction, "PromotionTester");
                promoter.Promote(xPathManager);

            }
            else
            {
                Console.WriteLine(_usageInstructions);
            }

        }

        /// <summary>
        /// Deserializes a RequestData object back into memory.
        /// </summary>
        /// <param name="dataFileName">File containing a serialized RequestData object in XML format.</param>
        /// <returns></returns>
        private static RequestData DeserializeData(string dataFileName)
        {
            RequestData obj;

            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(RequestData));
                TextReader reader = new StreamReader(dataFileName);

                obj = (RequestData)serial.Deserialize(reader);
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Error when attempting to Deserialize {0}.", dataFileName);
                throw;
            }

            return obj;
        }

        /// <summary>
        /// Converts an input action name to one of the valid values the ProcessActionType enum.
        /// Valid inputs are: PromoteToStaging, PromoteToPreview and PromoteToLive.
        /// </summary>
        /// <param name="actionName">String containing a promotion action name</param>
        /// <returns></returns>
        private static ProcessActionType GetPromotionAction(string actionName)
        {
            ProcessActionType action = ProcessActionType.Invalid;

            try
            {
                action = ConvertEnum<ProcessActionType>.Convert(actionName);
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Error attempting to convert {0} to ProcessActionType.", actionName);
                Console.Error.WriteLine("Valid values are: PromoteToStaging, PromoteToPreview and PromoteToLive.");
                throw;
            }


            return action;
        }
    }
}
