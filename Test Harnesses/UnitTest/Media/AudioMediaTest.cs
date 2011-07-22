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

namespace GateKeeper.UnitTest.Media
{
    public class AudioMediaTest
    {

        [TestCase("audio696772.xml", "PromoteToStaging")]
        [TestCase("audio696774.xml", "PromoteToStaging")]
        [TestCase("audio696806.xml", "PromoteToStaging")]
        [TestCase("audio696811.xml", "PromoteToStaging")]
        [TestCase("image415499.xml", "PromoteToStaging")]
        [TestCase("image415500.xml", "PromoteToStaging")]
        public void NewMediaTest_ToStaging(string requestDataXMLFile, string promotionAction)
        {
            promote(requestDataXMLFile, promotionAction);
        }


        [TestCase("audio696772.xml", "PromoteToPreview")]
        [TestCase("audio696774.xml", "PromoteToPreview")]
        [TestCase("audio696806.xml", "PromoteToPreview")]
        [TestCase("audio696811.xml", "PromoteToPreview")]
        [TestCase("image415499.xml", "PromoteToPreview")]
        [TestCase("image415500.xml", "PromoteToPreview")]
        public void NewMediaTest_ToPreview(string requestDataXMLFile, string promotionAction)
        {
            promote(requestDataXMLFile, promotionAction);
        }

        [TestCase("audio696772.xml", "PromoteToLive")]
        [TestCase("audio696774.xml", "PromoteToLive")]
        [TestCase("audio696806.xml", "PromoteToLive")]
        [TestCase("audio696811.xml", "PromoteToLive")]
        [TestCase("image415499.xml", "PromoteToLive")]
        [TestCase("image415500.xml", "PromoteToLive")]
        public void NewMediaTest_ToLive(string requestDataXMLFile, string promotionAction)
        {
            promote(requestDataXMLFile, promotionAction);
        }


        [TestCase("audio696772Remove.xml", "PromoteToPreview")]
        [TestCase("audio696774Remove.xml", "PromoteToPreview")]
        [TestCase("audio696806Remove.xml", "PromoteToPreview")]
        [TestCase("audio696811Remove.xml", "PromoteToPreview")]
        [TestCase("image415499Remove.xml", "PromoteToPreview")]
        [TestCase("image415500Remove.xml", "PromoteToPreview")]
        public void NewMediaTest_RemoveFromStaging(string requestDataXMLFile, string promotionAction)
        {
            promote(requestDataXMLFile, promotionAction);
        }

        [TestCase("audio696772Remove.xml", "PromoteToLive")]
        [TestCase("audio696774Remove.xml", "PromoteToLive")]
        [TestCase("audio696806Remove.xml", "PromoteToLive")]
        [TestCase("audio696811Remove.xml", "PromoteToLive")]
        [TestCase("image415499Remove.xml", "PromoteToLive")]
        [TestCase("image415500Remove.xml", "PromoteToLive")]
        public void NewMediaTest_RemoveFromLive(string requestDataXMLFile, string promotionAction)
        {
            promote(requestDataXMLFile, promotionAction);
        }

        [TestCase("audio696772Remove.xml", "PromoteToStaging")]
        [TestCase("audio696774Remove.xml", "PromoteToStaging")]
        [TestCase("audio696806Remove.xml", "PromoteToStaging")]
        [TestCase("audio696811Remove.xml", "PromoteToStaging")]
        [TestCase("image415499Remove.xml", "PromoteToStaging")]
        [TestCase("image415500Remove.xml", "PromoteToStaging")]
        public void NewMediaTest_RemoveFromPreview(string requestDataXMLFile, string promotionAction)
        {
            promote(requestDataXMLFile, promotionAction);
        }

        private void promote(string requestDataXMLFile, string promotionAction)
        {
            ProcessActionType processAction = GetPromotionAction(promotionAction);
            RequestData data = DeserializeData(@"./XMLData/" + requestDataXMLFile);
            DocumentXPathManager xPathManager = new DocumentXPathManager();

            // Instantiate a promoter and go to town.
            DocumentPromoterBase promoter =
                DocumentPromoterFactory.Create(data, 4444, processAction, "AudioMediaPromotionTester");
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
