using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using GateKeeper.Common;
using GateKeeper.DataAccess;
using GateKeeper.DataAccess.GateKeeper;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.CancerGov;
using GateKeeper.DocumentObjects;
using GateKeeper.DocumentObjects.Media;
using GateKeeper.ContentRendering;
using GKManagers.BusinessObjects;

namespace GKManagers
{
    /// <summary>
    /// Glossary tem promoter class
    /// </summary>
    class MediaPromoter : DocumentPromoterBase
    {
        #region Public methods
        public MediaPromoter(RequestData dataBlock, int batchID,
            ProcessActionType action, string userName)
            :
            base(dataBlock, batchID, action, userName)
        {
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Method to extract/render/save media document into staging database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToStaging(DocumentXPathManager xPathManager,
                               HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote media document to the staging database.");
  
            MediaDocument media = new MediaDocument();
            media.WarningWriter = warningWriter;
            media.InformationWriter = informationWriter;
            media.VersionNumber = DataBlock.CdrVersion;
            media.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Extract media data
                media.VersionNumber = DataBlock.CdrVersion;
                // This is a special case only for media document.  The document id for media document is not in the xml file, it needs to be passed in.
                MediaExtractor.Extract(DataBlock.DocumentData, media, DataBlock.CdrID, xPathManager);
                MediaRenderer mediaRender = new MediaRenderer();
                mediaRender.Render(media);

                // Save media data into database
                using (MediaQuery orgQuery = new MediaQuery())
                {
                    orgQuery.SaveDocument(media, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove media data from database
                using (MediaQuery orgQuery = new MediaQuery())
                {
                    orgQuery.DeleteDocument(media, ContentDatabase.Staging, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid media request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            media = null;

            informationWriter("Promoting media document to the staging database succeeded.");
        }

        /// <summary>
        /// Method to call query class to push document to the preview database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToPreview(DocumentXPathManager xPathManager,
                               HistoryEntryWriter warningWriter,
                               HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote media document to the preview database.");

            MediaDocument media = new MediaDocument();
            media.WarningWriter = warningWriter;
            media.InformationWriter = informationWriter;
            media.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push media document to the preview database
                using (MediaQuery orgQuery = new MediaQuery())
                {
                    orgQuery.PushDocumentToPreview(media, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove media data from database
                using (MediaQuery orgQuery = new MediaQuery())
                {
                    orgQuery.DeleteDocument(media, ContentDatabase.Preview, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid media request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting media document to the preview database succeeded.");
        }

        /// <summary>
        /// Method to call query class to push document to the live database.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="drugInfoSummary"></param>
        protected override void PromoteToLive(DocumentXPathManager xPathManager,
                                HistoryEntryWriter warningWriter,
                                HistoryEntryWriter informationWriter)
        {
            informationWriter("Start to promote media document to the live database.");

            MediaDocument media = new MediaDocument();
            media.WarningWriter = warningWriter;
            media.InformationWriter = informationWriter;
            media.DocumentID = DataBlock.CdrID;
            if (DataBlock.ActionType == RequestDataActionType.Export)
            {
                // Push media document to the live database
                using (MediaQuery orgQuery = new MediaQuery())
                {
                    orgQuery.PushDocumentToLive(media, UserName);
                }
            }
            else if (DataBlock.ActionType == RequestDataActionType.Remove)
            {
                // Remove media data from database
                using (MediaQuery orgQuery = new MediaQuery())
                {
                    orgQuery.DeleteDocument(media, ContentDatabase.Live, UserName);
                }
            }
            else
            {
                // There should never be any invalid request.
                throw new Exception("Promoter Error: Invalid media request. RequestID = " + DataBlock.RequestDataID.ToString() + "; CDRID = " + DataBlock.CdrID.ToString());
            }

            informationWriter("Promoting media document to the live database succeeded.");
        }

        #endregion
    }
}
