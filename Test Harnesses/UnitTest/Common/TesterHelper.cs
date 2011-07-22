using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using GateKeeper.DataAccess;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.ContentRendering;
using GKManagers;
using GKManagers.BusinessObjects;
using NCI.WCM.CMSManager.CMS;
using GateKeeper.Common;

namespace GateKeeper.UnitTest.Common
{
    public class TesterHelper
    {
        public static void promote(string requestDataXMLFile, string promotionAction)
        {
            ProcessActionType processAction = GetPromotionAction(promotionAction);
            RequestData data = DeserializeData(@"./XMLData/" + requestDataXMLFile);
            DocumentXPathManager xPathManager = new DocumentXPathManager();

            // Instantiate a promoter and go to town.
            DocumentPromoterBase promoter =
                DocumentPromoterFactory.Create(data, 4444, processAction, "PromotionTester");
            promoter.Promote(xPathManager);
            NUnit.Framework.Assert.AreEqual(true, promoter.PromotionWasSuccessful);

        }

        /// <summary>
        /// Deserializes a RequestData object back into memory.
        /// </summary>
        /// <param name="dataFileName">File containing a serialized RequestData object in XML format.</param>
        /// <returns></returns>
        public static RequestData DeserializeData(string dataFileName)
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
        public static ProcessActionType GetPromotionAction(string actionName)
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


        private static CMSController.CMSPublishingTarget GetPublishingFlag(string flagText)
        {
            CMSController.CMSPublishingTarget flag;
            try
            {
                flag = ConvertEnum<CMSController.CMSPublishingTarget>.Convert(flagText);
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Error attempting to convert {0} to CMSController.CMSPublishingTarget.", flagText);
                throw;
            }

            return flag;
        }

    }
}
