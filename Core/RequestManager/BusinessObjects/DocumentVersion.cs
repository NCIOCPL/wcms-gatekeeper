using System;
using System.Collections.Generic;
using System.Text;

namespace GKManagers.BusinessObjects
{
    public class DocumentVersionEntry
    {
        /// <summary>
        /// Locations with null values (i.e. No version of the document has ever been promoted that far
        /// or the document has been deleted) are represented by -1.  Any version of the document is
        /// newer than no version.
        /// </summary>
        private int _stagingVersion;
        private int _previewVersion;
        private int _liveVersion;

        private int _groupID;


        public DocumentVersionEntry(int groupID, int stagingVersion, int previewVersion, int liveVersion)
        {
            _groupID = groupID;
            _stagingVersion = stagingVersion;
            _previewVersion = previewVersion;
            _liveVersion = liveVersion;
        }

        #region Properties

        public int GroupID
        {
            get { return _groupID; }
            set { _groupID = value; }
        }

        public int StagingVersion
        {
            get { return _stagingVersion; }
            set { _stagingVersion = value; }
        }

        public int PreviewVersion
        {
            get { return _previewVersion; }
            set { _previewVersion = value; }
        }
        
        public int LiveVersion
        {
            get { return _liveVersion; }
            set { _liveVersion = value; }
        }

        #endregion
    }

    public class GroupPresentEntry
    {
        private bool _inStaging;
        private bool _inPreview;
        private bool _inLive;

        public GroupPresentEntry(bool inStaging, bool inPreview, bool inLive)
        {
            _inStaging = inStaging;
            _inPreview = inPreview;
            _inLive = inLive;
        }

        public bool InStaging
        {
            get { return _inStaging; }
            set { _inStaging = value; }
        }

        public bool InPreview
        {
            get { return _inPreview; }
            set { _inPreview = value; }
        }

        public bool InLive
        {
            get { return _inLive; }
            set { _inLive = value; }
        }
    }

    public class DocumentVersionMap
    {
        public const int DOCUMENT_NOT_PRESENT = -1;

        private Dictionary<int, DocumentVersionEntry> _documentMap;
        private Dictionary<int, GroupPresentEntry> _groupPresenceMap;
        private int _requestID;

        public DocumentVersionMap(int requestID, Dictionary<int, DocumentVersionEntry> map)
        {
            _requestID = requestID;
            _documentMap = map;

            // RefreshGroupPresence references _documentMap, so it has to
            // appear later in the sequence.
            _groupPresenceMap = new Dictionary<int, GroupPresentEntry>();
            RefreshGroupPresence();
        }

        #region Manage Document Versions

        /// <summary>
        /// Compares the version of the CDR document in staging against the specified
        /// requestID.
        /// </summary>
        /// <param name="cdrid">CDR document ID</param>
        /// <param name="requestID">RequestID to compare against Staging</param>
        /// <returns>
        /// If the input request ID is older, the return is negative.
        /// If the input request ID is newer, the return is positive.
        /// If they’re the same revision, the return is zero.
        /// </returns>
        public int MatchStagingVersion(int cdrid, int requestID)
        {
            lock (_documentMap[cdrid])
            {
                return requestID - _documentMap[cdrid].StagingVersion;
            }
        }

        public void UpdateStagingVersion(int cdrid, int requestID)
        {
            lock (_documentMap[cdrid])
            {
                _documentMap[cdrid].StagingVersion = requestID;
            }
        }

        public void DeleteStagingVersion(int cdrid, int requestID)
        {
            UpdateStagingVersion(cdrid, DOCUMENT_NOT_PRESENT);
        }

        public bool StagingVersionExists(int cdrid)
        {
            bool exists;

            lock (_documentMap[cdrid])
            {
                exists = _documentMap[cdrid].StagingVersion > 0;
            }
            return exists;
        }

        /// <summary>
        /// Compares the version of the CDR document on the preview site against the specified
        /// requestID.
        /// </summary>
        /// <param name="cdrid">CDR document ID</param>
        /// <param name="requestID">RequestID to compare against Preview</param>
        /// <returns>
        /// If the input request ID is older, the return is negative.
        /// If the input request ID is newer, the return is positive.
        /// If they’re the same revision, the return is zero.
        /// </returns>
        public int MatchPreviewVersion(int cdrid, int requestID)
        {
            lock (_documentMap[cdrid])
            {
                return requestID - _documentMap[cdrid].PreviewVersion;
            }
        }

        public void UpdatePreviewVersion(int cdrid, int requestID)
        {
            lock (_documentMap[cdrid])
            {
                _documentMap[cdrid].PreviewVersion = requestID;
            }
        }

        public void DeletePreviewVersion(int cdrid, int requestID)
        {
            UpdatePreviewVersion(cdrid, DOCUMENT_NOT_PRESENT);
        }

        public bool PreviewVersionExists(int cdrid)
        {
            bool exists;

            lock (_documentMap[cdrid])
            {
                exists = _documentMap[cdrid].PreviewVersion > 0;
            }
            return exists;
        }

        /// <summary>
        /// Compares the version of the CDR document on the live site against the specified
        /// requestID.
        /// </summary>
        /// <param name="cdrid">CDR document ID</param>
        /// <param name="requestID">RequestID to compare against Live</param>
        /// <returns>
        /// If the input request ID is older, the return is negative.
        /// If the input request ID is newer, the return is positive.
        /// If they’re the same revision, the return is zero.
        /// </returns>
        public int MatchLiveVersion(int cdrid, int requestID)
        {
            lock (_documentMap[cdrid])
            {
                return requestID - _documentMap[cdrid].LiveVersion;
            }
        }

        public void UpdateLiveVersion(int cdrid, int requestID)
        {
            lock (_documentMap[cdrid])
            {
                _documentMap[cdrid].LiveVersion = requestID;
            }
        }

        public void DeleteLiveVersion(int cdrid, int requestID)
        {
            UpdateLiveVersion(cdrid, DOCUMENT_NOT_PRESENT);
        }

        public bool LiveVersionExists(int cdrid)
        {
            bool exists;

            lock (_documentMap[cdrid])
            {
                exists = _documentMap[cdrid].LiveVersion > 0;
            }
            return exists;
        }

        #endregion


        /// <summary>
        /// Verifies that a given CDR document ID is present in the map.
        /// </summary>
        /// <param name="cdrid">CDR document ID to verify.</param>
        /// <returns>true if the document is present, false otherwise.</returns>
        public bool Contains(int cdrid)
        {
            /// This method only verifies that a record exists and makes no attempt
            /// to evaluate it.  Since the class offers no ability to add/remove records,
            /// there is no need to lock the document map.
            return _documentMap.ContainsKey(cdrid);
        }

        /// <summary>
        /// Refreshes the list of which groups are present at each promotion level.
        /// This method should be called after performing a group of promotions
        /// at a given level.  It is not necessary to reload the location map as long as
        /// the appropriate Update... and Delete... version methods have been called
        /// after each promotion/deletion.
        /// </summary>
        public void RefreshGroupPresence()
        {
            int groupID;

            lock (_groupPresenceMap)
            {
                _groupPresenceMap.Clear();

                lock (_documentMap)
                {
                    // Walk through the list of pairs of CDR and Request IDs. 
                    foreach (KeyValuePair<int, DocumentVersionEntry> entry in _documentMap)
                    {
                        // Get the current document's group ID.
                        groupID = entry.Value.GroupID;

                        /// If the group entry isn't already present, add it.
                        if (!_groupPresenceMap.ContainsKey(groupID))
                            _groupPresenceMap.Add(groupID, new GroupPresentEntry(true, true, true));

                        /// Check whether the current request ID matches the ID for each
                        /// promotion level.  If any single document is not present in a given
                        /// level, then the entire group is considered "not present" at that level.
                        /// If no version of the document is present (DOCUMENT_NOT_PRESENT), then
                        /// the document is considered to have been removed and does not affect
                        /// whether the group is present at that level.
                        if (entry.Value.StagingVersion == _requestID ||
                            entry.Value.StagingVersion == DOCUMENT_NOT_PRESENT)
                            _groupPresenceMap[groupID].InStaging &= true;
                        else
                            _groupPresenceMap[groupID].InStaging &= false;

                        if (entry.Value.PreviewVersion == _requestID ||
                            entry.Value.PreviewVersion == DOCUMENT_NOT_PRESENT)
                            _groupPresenceMap[groupID].InPreview &= true;
                        else
                            _groupPresenceMap[groupID].InPreview &= false;

                        if (entry.Value.LiveVersion == _requestID ||
                            entry.Value.LiveVersion == DOCUMENT_NOT_PRESENT)
                            _groupPresenceMap[groupID].InLive &= true;
                        else
                            _groupPresenceMap[groupID].InLive &= false;

                    }
                }
            }
        }

        public bool GroupIsPresentInGateKeeper(int groupID)
        {
            // Groups are always present in GateKeeper.
            return true;
        }

        public bool GroupIsPresentInStaging(int groupID)
        {
            bool isPresent = false;

            lock (_groupPresenceMap)
            {
                if (_groupPresenceMap.ContainsKey(groupID) &&
                    _groupPresenceMap[groupID].InStaging)
                    isPresent = true;
            }

            return isPresent;
        }

        public bool GroupIsPresentInPreview(int groupID)
        {
            bool isPresent = false;

            lock (_groupPresenceMap)
            {
                if (_groupPresenceMap.ContainsKey(groupID) &&
                    _groupPresenceMap[groupID].InPreview)
                    isPresent = true;
            }

            return isPresent;
        }

        public bool GroupIsPresentInLive(int groupID)
        {
            bool isPresent = false;

            lock (_groupPresenceMap)
            {
                if (_groupPresenceMap.ContainsKey(groupID) &&
                    _groupPresenceMap[groupID].InLive)
                    isPresent = true;
            }

            return isPresent;
        }
    }
}
