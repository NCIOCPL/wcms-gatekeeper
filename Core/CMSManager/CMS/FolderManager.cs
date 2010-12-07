﻿using System;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Xml;

using NCI.WCM.CMSManager.PercussionWebSvc;

namespace NCI.WCM.CMSManager.CMS
{
    /// <summary>
    /// In Percussion, attempting to attach an item to a folder causes the folder
    /// to be created if it doesn't already exist. This mechanism is not threadsafe
    /// however and multiple same-name folders can be created.
    /// The purpose of the Foldermanager class is to provide a threadsafe mechanism
    /// for obtaining a folder's PSFolder object.
    /// </summary>
    public class FolderManager
    {
        #region Constants

        const string errorNamespace = "urn:www.percussion.com/6.0.0/faults";
        const string errorNamespacePrefix = "ns1";
        const string errorResultPath = "//ns1:PSErrorResultsFault";
        const string folderNotFoundErrorPath = "//ns1:PSError[@code='43']";

        #endregion


        #region Fields

        /// <summary>
        /// Danger! This is a deliberately static member. This dictionary
        /// is shared across *all* instances of FolderManager.  Updates are
        /// coordinated via a lock in GuaranteeFolder.
        /// </summary>
        private static Dictionary<string, PSFolder> _folderCollection = new Dictionary<string, PSFolder>();

        private contentSOAP _contentService = null;

        #endregion

        #region Constructors

        /// <summary>
        /// The presence of a static constructor forces _folderCollection to be
        /// initialized earlier immediately instead of waiting until the member
        /// is referenced.  (Fewer runtime checks this way.)
        /// </summary>
        static FolderManager()
        {
        }

        /// <summary>
        /// The public constructor.
        /// </summary>
        /// <param name="contentService">Reference to the Percussion content Service.</param>
        internal FolderManager(contentSOAP contentService)
        {
            _contentService = contentService;
        }

        #endregion

        /// <summary>
        /// Threadsafe mechanism for loading folder information and guaranteeing
        /// the folder exists.
        /// </summary>
        /// <param name="folderPath">The desired folder path.</param>
        /// <returns>A PSFolder object containing details of the folder.</returns>
        /// <remarks>The folder path specified must be fully qualified with the //Sites/ folder
        /// and the base of the specific site. e.g. //Sites/CancerGov.</remarks>
        public PSFolder GuaranteeFolder(string folderPath)
        {
            if (folderPath == null)
                throw new ArgumentNullException("folderPath");

            // Does the path already exist in the collection?
            if (!_folderCollection.ContainsKey(folderPath))
            {
                // Lock the container for possible updating.
                lock (_folderCollection)
                {
                    // Was the folder looked up while we were waiting for
                    // the lock?
                    if (!_folderCollection.ContainsKey(folderPath))
                    {
                        PSFolder folderInfo = ForceFolderToExist(folderPath);
                        _folderCollection.Add(folderPath, folderInfo);
                    }
                }
            }

            return _folderCollection[folderPath];
        }

        /// <summary>
        /// Attempts to load a folder's information. If the folder doesn't
        /// exist, create it.
        /// </summary>
        /// <param name="folderPath">Folder which is to be created.</param>
        /// <returns></returns>
        private PSFolder ForceFolderToExist(string folderPath)
        {
            PSFolder returnItem;

            returnItem = GetExistingFolder(folderPath);

            if (returnItem == null)
            {
                returnItem = CreateNewFolder(folderPath);
            }

            return returnItem;
        }

        /// <summary>
        /// Attempts to load details about an existing folder.
        /// </summary>
        /// <param name="folderPath">Path of the folder to be loaded.</param>
        /// <returns>PSFolder object.  Null if the folder doesn't exist.</returns>
        private PSFolder GetExistingFolder(string folderPath)
        {
            PSFolder returnItem;

            // Loading a non-existant folder results in a SoapException,
            // so we have to do some extra work to handle that possiblity.
            try
            {
                PSFolder[] folderCollection;
                LoadFoldersRequest req = new LoadFoldersRequest();
                req.Path = new string[] { folderPath };
                folderCollection = _contentService.LoadFolders(req);
                returnItem = folderCollection[0];
            }
            catch (SoapException ex)
            {
                if (ConfirmFolderNotFoundError(ex.Detail))
                {
                    returnItem = null;
                }
                else
                {
                    // OK, that wasn't the error we expected.
                    throw;
                }
            }

            return returnItem;
        }

        /// <summary>
        /// Parses the detail node of a SoapException to verify that the
        /// error which occured really was a Folder Not Found error.
        /// </summary>
        /// <param name="errorDetail">Detail property from a SoapException.</param>
        /// <returns>true or false depending on whether the error was
        /// the Folder Not Found.</returns>
        private bool ConfirmFolderNotFoundError(XmlNode errorDetail)
        {
            bool errorConfirmed = false;

            XmlNameTable names = new NameTable();
            XmlNamespaceManager nameMgr = new XmlNamespaceManager(names);
            nameMgr.AddNamespace(errorNamespacePrefix, errorNamespace);
            XmlNode error = errorDetail.SelectSingleNode(errorResultPath, nameMgr);
            if (error != null)
            {
                XmlNode errorCode = error.SelectSingleNode(folderNotFoundErrorPath, nameMgr);
                if (errorCode != null)
                {
                    errorConfirmed = true;
                }
            }

            return errorConfirmed;
        }

        /// <summary>
        /// Creates a new folder in Percussion and obtains the detailed
        /// PSFolder object.
        /// </summary>
        /// <param name="folderPath">Path of the folder to be created.</param>
        /// <returns>PSFolder object detailing the new folder.</returns>
        private PSFolder CreateNewFolder(string folderPath)
        {
            int splitPoint = folderPath.LastIndexOfAny(new char[] { '/', '\\' });
            string parentFolder = folderPath.Substring(0, splitPoint);
            string newFolderName = folderPath.Substring(splitPoint + 1);

            // Force the parent path to exist if it doesn't already.
            // This seems scary because GuaranteeFolder() establishes a lock which would need
            // to be released before execution could continue.  So what happens when another
            // thread has a lock inside GuaranteeFolder()? Doesn't that create a deadlock
            // since the other thread is waiting for us to finish and we're waiting for the
            // other thread?
            //
            // It turns out the lock is smart and only applies to *other* threads.
            // When the current execution encounters the lock, it's able to continue.
            // Ref: ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/dv_csref/html/656da1a4-707e-4ef6-9c6e-6d13b646af42.htm
            GuaranteeFolder(parentFolder);

            AddFolderRequest req = new AddFolderRequest();
            req.Path = parentFolder;
            req.Name = newFolderName;
            AddFolderResponse resp = _contentService.AddFolder(req);
            return resp.PSFolder;
        }
    }
}
