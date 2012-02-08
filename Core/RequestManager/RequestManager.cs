using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.UI.WebControls;
using System.Text;

using NCI.Messaging;

using GKManagers.BusinessObjects;
using GKManagers.DataAccess;

namespace GKManagers
{
    public static class RequestManager
    {
        /// <summary>
        /// Import Serialized XML and deserialize XML back to an Request object. No transaction required for 
        /// the request. If one requestData throws an error, abort the request.
        /// </summary>
        /// <param name="filePath">File path of an external XML file which a Request object is stored.</param>
        //public static RequestStatusType ImportRequest(HttpPostedFile filePath, string importSource, string updateUserID, ref int requestID)
        //{
        //    return ImportRequest(filePath.InputStream, importSource, updateUserID, ref requestID);
        //}

        public static RequestStatusType ImportRequest(Stream requestStream, string importSource, string updateUserID, out int requestID)
        {
            Request req = (Request)DeserializeObject(requestStream, typeof(Request));

            //Change some property of New request. 
            req.Description = "Import Request: " + req.Description;
            req.RequestPublicationType = RequestPublicationType.Import;
            req.ActualDocCount = req.RequestDatas.Length;
            req.ExpectedDocCount = req.RequestDatas.Length;
            req.PublicationTarget = RequestTargetType.GateKeeper;
            req.Status = RequestStatusType.Receiving;
            req.UpdateUserID = updateUserID;
            req.Source = importSource;

            // req.ExternalRequestID = (DateTime.UtcNow.Subtract(new DateTime(2007,1,1))).Ticks.ToString();
            req.ExternalRequestID = Math.Round((decimal)DateTime.UtcNow.Subtract(new DateTime(2007, 1, 1)).Ticks / 1000000).ToString();

            CreateNewRequest(ref req);
            requestID = req.RequestID;

            try
            {
                for (int i = 0; i < req.RequestDatas.Length; i++)
                {
                    RequestData data = (RequestData)req.RequestDatas[i];
                    //Set data property
                    data.Status = RequestDataStatusType.OK;
                    data.DependencyStatus = RequestDataDependentStatusType.OK;
                    data.Location = RequestDataLocationType.GateKeeper;

                    VerifyDocumentIntegrity(data);

                    bool result = InsertRequestData(req.ExternalRequestID, importSource, updateUserID, ref data);
                    if (!result)
                    {
                        AbortRequest(req.ExternalRequestID, updateUserID,
                            "Import aborted due to problem inserting request data.");
                        return RequestStatusType.Aborted;
                    }
                }
            }
            catch (Exception ex)
            {
                AbortRequest(req.ExternalRequestID, updateUserID,
                            "Import aborted due to problem inserting request data. Details: " + ex.Message);
                return RequestStatusType.Aborted;
            }

            CompleteRequest(req.ExternalRequestID, importSource, updateUserID, req.RequestDatas.Length);
            return RequestStatusType.DataReceived;
        }

        public static RequestStatusType CopyRequest(string updateUserID, int[] requestDataIds, out int requestID, string dtdVersion)
        {
            Request req = new Request();

            //Change some property of New request. 
            req.Description = "Reprocessing Documents";
            req.RequestPublicationType = RequestPublicationType.Reload;
            req.ActualDocCount = requestDataIds.Length;
            req.ExpectedDocCount = requestDataIds.Length;
            req.PublicationTarget = RequestTargetType.Live;
            req.Status = RequestStatusType.Receiving;
            req.UpdateUserID = updateUserID;
            req.Source = "Gatekeeper";
            req.DataType = "XML";
            req.DtdVersion = dtdVersion;
            req.ExternalRequestID = Math.Round((decimal)DateTime.UtcNow.Subtract(new DateTime(2007, 1, 1)).Ticks / 1000000).ToString();
            requestID = -1;

            string source = req.Source;


            try
            {

                CreateNewRequest(ref req);
                
                requestID = req.RequestID;

                StringBuilder sbRequestDataIds = new StringBuilder();

                foreach (int requestDataId in requestDataIds)
                {
                    if (sbRequestDataIds.Length > 0)
                        sbRequestDataIds.Append("," + requestDataId.ToString());
                    else
                        sbRequestDataIds.Append(requestDataId.ToString());
                }

                bool result = RequestQuery.CopyRequest(sbRequestDataIds.ToString(), requestID, updateUserID);

                if (!result)
                {
                    AbortRequest(req.ExternalRequestID, source, updateUserID,
                        "Copy Request aborted due to problem inserting request data.");
                    return RequestStatusType.Aborted;
                }

            }
            catch (Exception ex)
            {
                AbortRequest(req.ExternalRequestID, source, updateUserID,
                            "Copy Request aborted due to problem inserting request data. Details: " + ex.Message);
                return RequestStatusType.Aborted;
            }

            CompleteRequest(req.ExternalRequestID, req.Source, updateUserID, requestDataIds.Length);
            return RequestStatusType.DataReceived;
        }
        /// <summary>
        /// Verifies that a document's metadata matches the document's internal information.
        /// (Called from ImportRequest)
        /// </summary>
        /// <param name="docInfo">A RequestData containing a CDR document which requires validation.</param>
        private static void VerifyDocumentIntegrity(RequestData docInfo)
        {
            // Don't verify Remove requests (they contain no document).
            // Don't verify Media documents (their contents are binary data).
            if (DocumentCanBeVerified(docInfo))
            {
                XmlDocument document = docInfo.DocumentData;
                XmlAttribute idNode = document.DocumentElement.Attributes["id"];
                if (idNode == null)
                    throw new Exception("DocumentData contains no CDRID.");

                int internalCdrid = int.Parse(Regex.Replace(idNode.Value, "^CDR(0*)", "", RegexOptions.Compiled));
                int envelopeCdrid = docInfo.CdrID;
                if (internalCdrid != envelopeCdrid)
                {
                    string msg =
                        string.Format("Packet {0}: Envelope Cdrid ({1}) does not match document Cdrid ({2}).",
                            docInfo.PacketNumber, envelopeCdrid, internalCdrid);
                    throw new Exception(msg);
                }
            }
        }

        /// <summary>
        /// Reports whether a given RequestData object contains information that can be verified.
        /// e.g.  Remove Requests contain no document data and Media documents are binary data.
        /// </summary>
        /// <param name="docInfo">RequestData object to report on</param>
        /// <returns></returns>
        private static bool DocumentCanBeVerified(RequestData docInfo)
        {
            bool isVerifiable;

            if (docInfo.ActionType == RequestDataActionType.Export)
            {
                switch (docInfo.CDRDocType)
                {
                    case CDRDocumentType.Media:
                        isVerifiable = false;
                        break;
                    default:
                        isVerifiable = true;
                        break;
                }
            }
            else
            {
                // Remove documents contain no data.
                isVerifiable = false;
            }

            return isVerifiable;
        }

        /// <summary>
        /// Export a list of RequestDatas and Serialize their parent Request object into XML
        /// </summary>
        /// <param name="source">An arraylist of RequestData object to be stored in an external XML file.</param>
        public static void ExportRequest(List<int> requestDataIDs)
        {
            if (requestDataIDs == null || requestDataIDs.Count == 0)
            {
                string message = "A list of request data IDs is required.";
                RequestMgrLogBuilder.Instance.CreateError(typeof(RequestManager), "ExportRequest", message);
                throw new Exception(message);
            }

            //set target file name
            string tempDir = Path.GetTempPath();
            string tempexportFile = Path.GetTempFileName();

            if (String.IsNullOrEmpty(tempDir))
            {
                string message = "There is no temp directory in the System or a temp can't be created.";
                RequestMgrLogBuilder.Instance.CreateError(typeof(RequestManager), "ExportRequest", message);
                throw new Exception(message);
            }

            //Get Data            
            List<RequestData> requestDatas = new List<RequestData>();
            requestDatas = LoadRequestDataListByReqIDGroups(requestDataIDs);

            int parentID = 0;
            int count = 0;

            foreach (RequestData data in requestDatas)
            {
                if (count == 0)
                {
                    parentID = data.RequestID;
                }
                else
                {
                    if (data.RequestID != parentID)
                    {
                        string message = "Not all RequestDatas have the same request ID.";
                        RequestMgrLogBuilder.Instance.CreateError(typeof(RequestManager), "ExportRequest", message);
                        throw new Exception(message);
                    }
                }
                count++;
            }
            //Get request object
            Request parent = LoadRequestByID(parentID);

            parent.RequestDatas = requestDatas.ToArray();// new RequestData[requestDatas.Count];
            //string exportFile = HttpContext.Current.Server.MapPath("/") + "export.xml";

            SerializeObject(parent, tempexportFile, true);

            DownloadXML(tempexportFile);
            File.Delete(tempexportFile);

            //HttpContext.Current.Response.Clear(); 
            //HttpContext.Current.Response.ContentType = "application/octet-stream";
            //HttpContext.Current.Response.AddHeader("Content-Disposition", "attachement; filename=export.xml");
            //HttpContext.Current.Response.WriteFile(exportFile);
            //HttpContext.Current.Response.End();
        }

        private static void DownloadXML(string filepath)
        {
            System.IO.Stream iStream = null;

            byte[] buffer = new Byte[10000];// Buffer to read 10K bytes in chunk:
            int length; // Length of the file:	        
            long dataToRead;// Total bytes to read:

            // Identify the file name.
            string filename = "export.xml"; //System.IO.Path.GetFileName(filepath);

            try
            {
                // Open the file.
                iStream = new System.IO.FileStream(filepath, System.IO.FileMode.Open,
                            System.IO.FileAccess.Read, System.IO.FileShare.Read);

                // Total bytes to read:
                dataToRead = iStream.Length;

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);

                // Read the bytes.
                while (dataToRead > 0)
                {
                    // Verify that the client is connected.
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        // Read the data in buffer.
                        length = iStream.Read(buffer, 0, 10000);

                        // Write the data to the current output stream.
                        HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);

                        // Flush the data to the HTML output.
                        HttpContext.Current.Response.Flush();

                        buffer = new Byte[10000];
                        dataToRead = dataToRead - length;
                    }
                    else
                    {
                        //prevent infinite loop if user disconnects
                        dataToRead = -1;
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (iStream != null)
                {
                    //Close the file.
                    iStream.Close();
                }

                HttpContext.Current.Response.End();
            }

        }

        /// <summary>
        /// Deserialize an XML back to a object
        /// </summary>
        /// <param name="filePath">Source XML file path</param>
        /// <param name="type">Type of Deserialized object</param>
        /// <returns></returns>
        public static object DeserializeObject(Stream fileStream, Type type)
        {
            if (fileStream == null)
                throw new ArgumentNullException("fileStream");
            if (type == null)
                throw new ArgumentNullException("type");

            XmlSerializer xmlSerializer;
            //Stream fileStream = null;
            //int fileLength;

            try
            {
                xmlSerializer = new XmlSerializer(type);
                //fileStream = upload.InputStream;
                //fileLength = upload.ContentLength;
                //byte[] input = new byte[fileLength];
                //fileStream.Read(input, 0, fileLength);

                //fileStream = new FileStream(filePath, FileMode.Open,    FileAccess.Read); 
                object objectFromXml = xmlSerializer.Deserialize(fileStream);
                return objectFromXml;
            }
            catch (Exception Ex)
            {
                RequestMgrLogBuilder.Instance.CreateError(typeof(RequestManager), "DeserializeObject", Ex);
                throw Ex;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        /// <summary>
        /// Serialize a object into XML file
        /// </summary>
        /// <param name="objToXml">Source object</param>
        /// <param name="filePath">Target XML file path</param>
        /// <param name="includeNameSpace">Include namespace for XML or not</param>
        public static void SerializeObject(Object objToXml, string filePath, bool includeNameSpace)
        {
            if (objToXml == null)
                throw new ArgumentNullException("objToXml");
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentException("filepath may not be empty.");

            StreamWriter stWriter = null;
            XmlSerializer xmlSerializer;

            try
            {
                xmlSerializer = new XmlSerializer(objToXml.GetType());
                stWriter = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                if (!includeNameSpace)
                {
                    System.Xml.Serialization.XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
                    //To remove namespace and any other inline information tag
                    xs.Add("", "");
                    xmlSerializer.Serialize(stWriter, objToXml, xs);
                }
                else
                {
                    xmlSerializer.Serialize(stWriter, objToXml);
                }
            }
            catch (Exception exception)
            {
                RequestMgrLogBuilder.Instance.CreateError(typeof(RequestManager), "SerializeObject", exception);
                throw exception;
            }
            finally
            {
                if (stWriter != null)
                    stWriter.Close();
            }
        }
        /// <summary>
        /// Oversees the persistent storage of a Request object.  Upon successful completion,
        /// the object is modified to reflect its new request ID.
        /// </summary>
        /// <param name="source">A Request object to be stored.</param>
        /// <returns>True on success, False on failure.</returns>
        public static bool CreateNewRequest(ref Request source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            source.Status = RequestStatusType.Receiving;

            /// For FullLoad, change publication target to GateKeeper in order to prevent
            /// the system from automatcially scheduling a promotion.  Because the request's
            /// details are not known until all data has been received, the corresponding
            /// remove request for all documents not included in the load cannot be generated
            /// until the request is completed.
            if (source.RequestPublicationType == RequestPublicationType.FullLoad &&
                source.PublicationTarget != RequestTargetType.GateKeeper)
            {
                string fmt = "Publication target demoted from {0} to GateKeeper because of RequestPublicationType set to  FullLoad.";
                string message = string.Format(fmt, source.PublicationTarget);
                RequestMgrLogBuilder.Instance.CreateInformation(typeof(RequestManager),
                    "CreateNewRequest", message);
                source.PublicationTarget = RequestTargetType.GateKeeper;
            }

            return RequestQuery.CreateNewRequest(ref source);
        }

        /// <summary>
        /// Mark a Request as closed, all data has been received.
        /// </summary>
        /// <param name="requestID">External Identifier for the request being closed</param>
        /// <param name="updateUserID">The userID responsible for this action</param>
        /// <returns>true on sucess, false on failure</returns>
        public static bool CompleteRequest(string externalRequestID, string requestSource,
            string userID, int expectedDocCount)
        {
            return RequestQuery.CompleteRequest(externalRequestID, requestSource, userID, expectedDocCount);
        }

        /// <summary>
        /// Mark a Request as abnormally terminated.
        /// </summary>
        /// <param name="requestID">External Identifier for the request being aborted</param>
        /// <param name="updateUserID">The userID responsible for this action</param>
        /// <returns>true on sucess, false on failure</returns>
        public static bool AbortRequest(string externalRequestID, string requestSource, string userID)
        {
            bool success = RequestQuery.AbortRequest(externalRequestID, requestSource, userID);
            if (success)
                AddRequestComment(externalRequestID, requestSource, string.Format("Request aborted by {0}.", userID));
            return success;
        }

        /// <summary>
        /// Mark a Request as abnormally terminated and record a reason.
        /// </summary>
        /// <param name="requestID">External Identifier for the request being aborted</param>
        /// <param name="updateUserID">The userID responsible for this action</param>
        /// <param name="reason">A short description of why the request was aborted.</param>
        /// <returns>true on sucess, false on failure</returns>
        public static bool AbortRequest(string externalRequestID, string requestSource,
            string userID, string reason)
        {
            bool success = RequestQuery.AbortRequest(externalRequestID, requestSource, userID);
            if (success)
                AddRequestComment(externalRequestID, requestSource, reason);
            return success;
        }

        /// <summary>
        /// Stores a RequestData object and creates an association between the RequestData object
        /// and a parent Request.
        /// </summary>
        /// <param name="externalRequestID">The parent Request object's external request ID</param>
        /// <param name="child">The RequestData object to create.</param>
        /// <returns>True on success, False on failure.</returns>
        public static bool InsertRequestData(string externalRequestID, string requestSource,
            string updateUserID, ref RequestData child)
        {
            if (String.IsNullOrEmpty(externalRequestID))
                throw new ArgumentException("Argument 'externalRequestID' must not be empty.");
            if (child == null)
                throw new ArgumentNullException("child");

            string fmt;
            string msg;

            if (child.ActionType != RequestDataActionType.Remove && String.IsNullOrEmpty(child.DocumentDataString))
            {
                fmt = "Packet: {0} - Null document body on a non-remove document.";
                msg = string.Format(fmt, child.PacketNumber);
                RequestMgrLogBuilder.Instance.CreateInformation(
                    typeof(RequestManager), "InsertRequestData", msg);
                throw new Exception(msg);
            }

            // For non-remove requests, validate the XML.
            if (child.ActionType != RequestDataActionType.Remove)
            {
                string validationMessage = RequestManager.ValidateRequestData(child);
                if (validationMessage != null)
                {
                    fmt = "Failed DTD Validation: {0}\r\nRequestID: {1}\r\nPacket #: {2}\r\nCDRID: {3}";
                    msg = string.Format(fmt, validationMessage, externalRequestID,
                        child.PacketNumber, child.CdrID);
                    RequestMgrLogBuilder.Instance.CreateInformation(
                            typeof(RequestManager), "ProcessRequest", msg);
                    throw new Exception(msg);
                }
            }

            child.Status = RequestDataStatusType.OK;
            child.DependencyStatus = RequestDataDependentStatusType.OK;

            return RequestQuery.InsertRequestData(externalRequestID, requestSource, updateUserID, ref child);
        }

        /// <summary>
        /// Load a saved Request object from persistent storage based on GateKeeper's internal
        /// RequestID.
        /// </summary>
        /// <param name="requestID">GateKeeper internal request ID</param>
        /// <returns>Request object (null on error)</returns>
        public static Request LoadRequestByID(int requestID)
        {
            return RequestQuery.LoadRequestByID(requestID);
        }

        /// <summary>
        /// Load a saved Request object from persistent storage based on its external ID.
        /// </summary>
        /// <param name="systemStatus">externally generatd request ID</param>
        /// <returns>Request object (null on error)</returns>
        public static Request LoadRequestByExternalID(string externalID, string requestSource)
        {
            return RequestQuery.LoadRequestByExternalID(externalID, requestSource);
        }

        /// <summary>
        /// Load the RequestData object identified by the unique combination of
        /// parent requestID and CDRID.
        /// </summary>
        /// <param name="requestID">Gatekeeper internal identifier for the request
        /// that the RequestData object is part of.</param>
        /// <param name="cdrid">RequestData object</param>
        /// <returns></returns>
        public static RequestData LoadRequestDataByCdrid(int requestID, int cdrid)
        {
            return RequestQuery.LoadRequestDataByCdrid(requestID, cdrid);
        }

        /// <summary>
        /// Load the RequestData object identified by its unique requestID.
        /// </summary>
        /// <param name="requestDataID">Unique GateKeeper generated RequestDataID</param>
        /// <returns>RequestData object</returns>
        public static RequestData LoadRequestDataByID(int requestDataID)
        {
            return RequestQuery.LoadRequestDataByID(requestDataID);
        }

        /// <summary>
        /// Load the RequestData object identified by its unique requestID.
        /// </summary>
        /// <param name="requestDataID">Unique GateKeeper generated RequestDataID</param>
        /// <returns>RequestData object</returns>
        public static RequestDataInfo LoadRequestDataInfo(int requestDataID)
        {
            return RequestQuery.LoadRequestDataInfo(requestDataID);
        }

        /// <summary>
        /// Load the RequestData objects for up to three request data object for a given CDR ID 
        /// one for the latest request data id that's one staging, one for preview, one for live 
        /// </summary>
        /// <param name="CDRID">CDRID</param>
        /// <returns>array of RequestData objects</returns>
        public static Dictionary<RequestDataLocationType, RequestDataInfo> LoadRequestDataListForCDRLocations(int cdrID)
        {
            return RequestQuery.LoadRequestDataListForCDRLocations(cdrID);
        }

        /// <summary>
        /// Load the RequestData objects from a request with unique requestID
        /// Only a subset of documents returned filtered by the parameters in the filter object 
        /// </summary>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="filter">filter objects with search parameters</param>
        /// <param name="pageNumber">page number, 1 for first page</param>
        /// <param name="resultsPerPage">number of results per page, 0 returns all rows</param>
        /// <param name="totalRequestDataCount">out parameter for a total number documents(requestData) in a request </param>
        /// <returns>array of RequestData objects</returns>
        public static List<RequestData> LoadRequestDataListByReqID(int requestID, RequestDataFilter filter, int pageNumber, int resultsPerPage, ref int totalRequestDataCount)
        {
            return RequestQuery.LoadRequestDataListByReqIDorBatchID(requestID, 0, filter, pageNumber, resultsPerPage, ref totalRequestDataCount);
        }

        public static DataSet LoadRequests(RequestHistoryFilter filter, int pageNumber, int resultsPerPage)
        {
            return RequestQuery.LoadRequests(filter, pageNumber, resultsPerPage);
        }

        /// <summary>
        /// Load the RequestData objects from a request with unique requestID
        /// Only a subset of documents returned filtered by the parameters in the filter object 
        /// </summary>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="filter">filter objects with search parameters</param>
        /// <param name="pageNumber">page number, 1 for first page</param>
        /// <param name="resultsPerPage">number of results per page, 0 returns all rows</param>
        /// <param name="totalRequestDataCount">out parameter for a total number documents(requestData) in a request </param>
        /// <returns>array of RequestData objects</returns>
        public static List<RequestData> LoadRequestDataListByBatchID(int batchID, RequestDataFilter filter, int pageNumber, int resultsPerPage, ref int totalRequestDataCount)
        {
            return RequestQuery.LoadRequestDataListByReqIDorBatchID(0, batchID, filter, pageNumber, resultsPerPage, ref totalRequestDataCount);
        }


        /// <summary>
        /// Load the RequestData objects from a request that are in the list of groups passed in as parameter 
        /// </summary>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="groupIdList">a list of group ids</param>
        /// <returns>array of RequestData objects</returns>
        public static List<RequestData> LoadRequestDataListByReqIDGroups(int requestID, List<int> groupIdList)
        {
            return RequestQuery.LoadRequestDataListByReqIDGroups(requestID, groupIdList);
        }

        /// <summary>
        /// Load the RequestData objects from a request that are in the list of groups passed in as parameter 
        /// </summary>
        /// <param name="requestID">Internal requestID</param>
        /// <param name="groupIdList">a list of group ids</param>
        /// <returns>array of RequestData objects</returns>
        public static List<RequestData> LoadRequestDataListByReqIDGroups(List<int> groupIdList)
        {
            return RequestQuery.LoadRequestDataListByReqIDGroups(groupIdList);
        }

        /// <summary>
        /// Retrieve a list of IDs for the request data objects associated with a request.
        /// </summary>
        /// <param name="requestID">The request's Gatekeeper-internal ID</param>
        /// <returns></returns>
        public static List<int> LoadRequestDataIDList(int requestID)
        {
            return RequestQuery.LoadRequestDataIDList(requestID);
        }

        /// <summary>
        /// Adds a history entry to the RequestData table to provide a history of actions taken against
        /// a RequestData object.
        /// </summary>
        /// <param name="data">The request data object the history entry applies to</param>
        /// <param name="requestID">ID of the batch the entry originates with.</param>
        /// <param name="entryText">Human readable text summarizing the event</param>
        /// <returns>true on success, false on error.</returns>
        public static bool AddRequestDataHistoryEntry(int requestID, int requestDataID,
            int batchID, string entryText, RequestDataHistoryType entryType)
        {
            return RequestQuery.AddRequestDataHistoryEntry(requestID, requestDataID,
                batchID, entryText, entryType);
        }

        /// <summary>
        /// Loads the specified RequestData object from the database and validates against
        /// a DTD.
        /// </summary>
        /// <param name="requestDataID">ID of the request data object.</param>
        /// <returns>null on success, or a list of validation errors.</returns>
        public static string ValidateRequestData(int requestDataID)
        {
            RequestData dataObject = RequestQuery.LoadRequestDataByID(requestDataID);

            return ValidateRequestData(dataObject);
        }

        /// <summary>
        /// Validates the provided RequestData object against a DTD.
        /// </summary>
        /// <param name="dataObject">The RequestData object to validate</param>
        /// <returns>null on success, or a list of validation errors.</returns>
        public static string ValidateRequestData(RequestData dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException("dataObject");

            string dtdLocation = ConfigurationManager.AppSettings["DTDLocation"];
            string notFoundMessage = "Validation Failure: DTD file not found at {0}.";
            string resultMessage;

            if (File.Exists(dtdLocation))
            {
                resultMessage = dataObject.Validate(dtdLocation);
            }
            else
            {
                string message = string.Format(notFoundMessage, dtdLocation);
                RequestMgrLogBuilder.Instance.CreateCritical(typeof(RequestManager), "ValidateRequestData", message);
                throw new Exception(message);
            }

            return resultMessage;
        }

        /// <summary>
        /// Updates a request data object's persistent location.
        /// </summary>
        /// <param name="dataObject">the object to update</param>
        /// <returns>true on success, false on error.</returns>
        public static bool SaveDocumentLocation(RequestData dataObject)
        {
            return RequestQuery.SaveDocumentLocation(dataObject);
        }

        /// <summary>
        /// Retrieves a DocumentVersionMap containing a list of CDR Ids and document versions
        /// for the staging, preview and live databases for all documents in a given request.
        /// </summary>
        /// <param name="requestID">The requestID to look up CDR Id and version info for.</param>
        /// <returns></returns>
        public static DocumentVersionMap LoadDocumentLocationMap(int requestID)
        {
            DocumentVersionMap documentMap = null;

            Dictionary<int, DocumentVersionEntry> documentList =
                RequestQuery.LoadDocumentLocationMap(requestID);
            documentMap = new DocumentVersionMap(requestID, documentList);

            return documentMap;
        }

        /// <summary>
        /// Returns a DocumentStatusMap containing a list of requestData IDs and their
        /// current document promotion status / dependency status.
        /// </summary>
        /// <param name="requestID"></param>
        /// <returns></returns>
        public static DocumentStatusMap LoadDocumentStatusMap(int requestID)
        {
            DocumentStatusMap statusMap = null;

            Dictionary<int, DocumentStatusEntry> statusList =
                RequestQuery.LoadDocumentStatusMap(requestID);
            statusMap = new DocumentStatusMap(statusList);

            return statusMap;
        }

        /// <summary>
        /// Marks a CDR document (identified by it's RequestData ID) as having errors.
        /// The other documents in the same group are simultaneously marked as having
        /// dependency errors.
        /// </summary>
        /// <param name="requestDataID"></param>
        /// <returns></returns>
        public static bool MarkDocumentWithErrors(int requestDataID)
        {
            return RequestQuery.MarkDocumentWithErrors(requestDataID);
        }

        /// <summary>
        /// Marks a CDR document (identified by it's RequestData ID) as having warnings.
        /// The other documents in the group are unaffected.
        /// </summary>
        /// <param name="requestDataID"></param>
        /// <returns></returns>
        public static bool MarkDocumentWithWarnings(int requestDataID)
        {
            return RequestQuery.MarkDocumentWithWarnings(requestDataID);
        }

        /// <summary>
        /// Retrieves DataSet from RequestDataHistory table
        /// </summary>
        /// <param name="reqDataID"></param>
        /// <param name="EntryType"></param>
        /// <param name="DebugChecked"></param>
        /// <returns>DataSet</returns>
        public static DataSet LoadDataHistoryDS(int reqDataID, string EntryType, bool DebugChecked)
        {
            return RequestQuery.LoadDataHistoryDS(reqDataID, EntryType, DebugChecked);
        }

        /// <summary>
        /// Retrieves DataSet from BatchHistory table
        /// </summary>
        /// <param name="reqDataID"></param>
        /// <returns>DataSet</returns>
        public static DataSet LoadBatchHistoryDS(int reqBatchID)
        {
            return RequestQuery.LoadBatchHistoryDS(reqBatchID);
        }

        /// <summary>
        /// Retrieves DataSet from Batch table
        /// </summary>
        /// <param name="ReqID"></param>
        /// <returns>DataSet</returns>
        public static DataSet LoadReqBatchHistoryData(int ReqID)
        {
            return RequestQuery.LoadReqBatchHistoryData(ReqID);
        }

        /// <summary>
        /// Retrieves a Report
        /// </summary>
        /// <param name="nNumber"></param>
        /// <returns>DataSet</returns>
        public static DataSet GetReports(int nNumber)
        {
            return RequestQuery.GetReports(nNumber);
        }

        /// <summary>
        /// Retrieves List as counts for different document types
        /// </summary>
        /// <param name="ReqID"></param>
        /// <returns>List<TypeCount></returns>
        public static List<TypeCount> LoadRequestCounts(int ReqID)
        {
            List<TypeCount> arrObj = new List<TypeCount>();
            Hashtable ht = RequestQuery.LoadRequestCounts(ReqID);
            foreach (DictionaryEntry de in ht)
            {
                arrObj.Add(new TypeCount((string)de.Key, ((int)de.Value).ToString()));
            }
            arrObj.Sort();
            return arrObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CdrID"></param>
        /// <returns></returns>
        public static List<RequestLocationExternalIds> LoadSingleDocumentLocationExternalId(int CdrID)
        {
            return RequestQuery.LoadRequestLocationExternalIds(CdrID);
        }

        public static List<RequestLocationExternalIds> LoadAllDocumentLocationsExternalId()
        {
            return RequestQuery.LoadRequestLocationExternalIds(0);
        }

        /// <summary>
        /// Retrieves List 
        /// </summary>
        /// <param name="DocType"></param>
        /// <returns>List<RequestLocationInternalIds></returns>
        public static List<RequestLocationInternalIds> LoadRequestLocationInternalIds(int DocType, int CdrID)
        {
            return RequestQuery.LoadRequestLocationInternalIds(DocType, CdrID);
        }

        /// <summary>
        /// Confirms that the various GateKeeper databases are available.
        /// </summary>
        /// <param name="ready">Returns as True if the database is available, false otherwise.</param>
        /// <param name="message">Returns null under normal circumstances, a message containing
        /// the failing database name(s) otherwise.</param>
        public static void CheckDatabaseStatus(out bool ready, out string message)
        {
            RequestQuery.CheckDatabaseStatus(out ready, out message);
        }

        public static string GetMostRecentExternalID(string requestSource)
        {
            return RequestQuery.GetMostRecentExternalID(requestSource);
        }

        public static void StopGateKeeperSystem()
        {
            RequestMgrLogBuilder.Instance.CreateInformation(typeof(RequestManager),
                "StopGateKeeperSystem", "Halting the GateKeeper System.");
            RequestQuery.SetGateKeeperSystemStatus(SystemStatusType.Stopped);
        }

        public static void StartGateKeeperSystem()
        {
            RequestMgrLogBuilder.Instance.CreateInformation(typeof(RequestManager),
                "StartGateKeeperSystem", "Starting the GateKeeper System.");
            RequestQuery.SetGateKeeperSystemStatus(SystemStatusType.Normal);
            StartBatchProcessor();
        }

        public static SystemStatusType GetGateKeeperSystemStatus()
        {
            return RequestQuery.GetGateKeeperSystemStatus();
        }

        /// <summary>
        /// Signals to the Process Manager that a new batch has been added to the system.
        /// </summary>
        public static void StartBatchProcessor()
        {
            //// TODO:  Log error when queue name/server values are null
            //string queueName = ConfigurationManager.AppSettings["GateKeeperQueueName"];

            //// Don't let MSMQ failures cause errors.  The error does need to be logged though.
            //try
            //{
            //    //Connect to MSMQ Sender.
            //    MSMQSender sender = new MSMQSender(queueName);
            //    sender.AddToQueue("Start", "label");
            //}
            //catch (Exception ex)
            //{
            //    RequestMgrLogBuilder.CreateInformation(typeof(RequestManager), "StartBatchProcessor", ex);
            //}
        }

        public static RequestStatusType GetRequestStatus(int requestID)
        {
            return RequestQuery.GetRequestStatus(requestID);
        }

        public static RequestStatusType GetRequestStatusFromDocumentID(int requestDataID)
        {
            return RequestQuery.GetRequestStatusFromDocumentID(requestDataID);
        }

        public static void AddRequestComment(int requestID, string comment)
        {
            RequestQuery.AddRequestComment(requestID, comment);
        }

        public static void AddRequestComment(string externalID, string requestSource, string comment)
        {
            RequestQuery.AddRequestComment(externalID, requestSource, comment);
        }

    }
}
