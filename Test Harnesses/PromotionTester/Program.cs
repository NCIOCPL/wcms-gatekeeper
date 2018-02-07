using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GKManagers;
using GKManagers.BusinessObjects;
using NCI.WCM.CMSManager.CMS;
using GateKeeper.Common;
using GateKeeper.DataAccess.GateKeeper;
using System.Threading;
using System.Configuration;
using Newtonsoft.Json.Linq;
using GateKeeper.DocumentObjects.Summary;

namespace PromotionTester
{
    public class Program
    {
        #region Fields
        readonly static private string _usageInstructions = @"
    PromotionTester <file> <promotionAction>

where:

    <file>  is a serialized RequestData object.
    <promotionAction> is one of PromoteToStaging, PromoteToPreview, PromoteToLive, PromoteToLiveFast
";
        #endregion

        static void Main(string[] args)
        {
            try
            {
                if (ConfigurationSettings.AppSettings["MultiThread"] == "True")
                {
                    //Process documents using multi thread.
                    RunUsingThreads(args);
                }
                else if (ConfigurationSettings.AppSettings["SingleThread"] == "True")
                {

                    //Document processing as a single thread
                    RunAsSingleThread(args);
                }

                else
                {
                    RunAsStandard(args);

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("I fall down and go boom.");
                Console.ReadKey();
            }

        }

        private static void RunAsStandard(string[] args)
        {
            System.Collections.ArrayList promotions = new System.Collections.ArrayList();
            if (args.Length >= 2 && args.Length <= 3)
            {
                if (args[1].ToLower() == "all")
                {
                    promotions.Add(ProcessActionType.PromoteToStaging); 
                    promotions.Add(ProcessActionType.PromoteToPreview);
                    promotions.Add(ProcessActionType.PromoteToLive);
                }
                else if (args[1].ToLower() == "promotetolivefast")
                {
                    //PromoteToLiveFast action list is a special case - build list separately
                    //skip the PromoteToPreview step
                    promotions.Add(ProcessActionType.PromoteToStaging);
                    promotions.Add(ProcessActionType.PromoteToLiveFast);
                }
                else
                {
                    promotions.Add(GetPromotionAction(args[1]));
                }

                RequestData data = DeserializeData(args[0]);


                string splitDataFile = ConfigurationManager.AppSettings["summary-split-file-location"];
                if (String.IsNullOrWhiteSpace(splitDataFile))
                    throw new ConfigurationErrorsException("Required setting summary-split-file-location not set.");
                SplitDataManager splitData = SplitDataManager.Create(splitDataFile);


                foreach (ProcessActionType processActionType in promotions)
                {
                    ProcessActionType processAction = processActionType;

                    DocumentXPathManager xPathManager = new DocumentXPathManager();

                    // Instantiate a promoter and go to town.
                    DocumentPromoterBase promoter =
                        DocumentPromoterFactory.Create(data, 18, processAction, "PromotionTester");
                    promoter.Promote(xPathManager);
                }

                CMSController.CMSPublishingTarget? publishingFlag = null;
                if (args.Length == 3)
                    publishingFlag = GetPublishingFlag(args[2]);

                if (publishingFlag.HasValue)
                {
                    CMSController controller = new CMSController();
                    controller.StartPublishing(publishingFlag.Value);
                }

            }
            else
            {
                Console.WriteLine(_usageInstructions);
            }
        }

        private static void RunAsSingleThread(string[] args)
        {
            int i = 4;
            if (args.Length > 2)
            {
                //RequestData data = DeserializeData(args[0]);
                //ProcessActionType processAction = GetPromotionAction(args[1]);


                for (i = 0; i <= 3; i++)
                {
                    RequestData data = DeserializeData(args[i]);
                    ProcessActionType processAction = GetPromotionAction(args[4]);

                    DocumentXPathManager xPathManager = new DocumentXPathManager();

                    // Instantiate a promoter and go to town.
                    DocumentPromoterBase promoter =
                        DocumentPromoterFactory.Create(data, 18, processAction, "PromotionTester");
                    promoter.Promote(xPathManager);
                }

            }
            else
            {
                Console.WriteLine(_usageInstructions);
            }
        }

        private static void RunUsingThreads(string[] args)
        {
            ProcessDocumentThread processDocumentThread1 = new ProcessDocumentThread(args, 0);
            Thread firstThread = new Thread(new ThreadStart(processDocumentThread1.DocumentThread));

            ProcessDocumentThread processDocumentThread2 = new ProcessDocumentThread(args, 1);
            Thread SecondThread = new Thread(new ThreadStart(processDocumentThread2.DocumentThread));


            ProcessDocumentThread processDocumentThread3 = new ProcessDocumentThread(args, 2);
            Thread ThirdThread = new Thread(new ThreadStart(processDocumentThread3.DocumentThread));


            ProcessDocumentThread processDocumentThread4 = new ProcessDocumentThread(args, 3);
            Thread FourthThread = new Thread(new ThreadStart(processDocumentThread4.DocumentThread));


            firstThread.Start();
            SecondThread.Start();
            ThirdThread.Start();
            FourthThread.Start();
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

    public class ProcessDocumentThread : Program
    {
        private string docPath;
        public ProcessDocumentThread(string[] args, int i)
        {
            docPath = args[i];
        }

        public void DocumentThread()
        {

            RequestData data = DeserializeData(docPath);
            ProcessActionType processAction = GetPromotionAction("PromoteToStaging");

            DocumentXPathManager xPathManager = new DocumentXPathManager();

            // Instantiate a promoter and go to town.
            DocumentPromoterBase promoter =
                DocumentPromoterFactory.Create(data, 18, processAction, "PromotionTester");
            promoter.Promote(xPathManager);
        }
    }
}
