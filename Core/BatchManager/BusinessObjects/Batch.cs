using System;
using System.Collections.Generic;
using System.Text;

using GateKeeper.Common;

namespace GKManagers.BusinessObjects
{
    public class Batch
    {
        private int _batchID;
        private BatchStatusType _status = BatchStatusType.Invalid;
        private string _batchName;
        private string _userName;

        // Ordered list of actions to be taken for each item in the batch.
        private List<ProcessActionType> _actions = new List<ProcessActionType>();
        
        // Unordered list of RequestData IDs associated with the batch.
        private List<int> _requestDataIDs = new List<int>();

        public Batch()
        {
        }

        /// <summary>
        /// Represents a batch of request data objects to be published.    If multiple publication actions
        /// are to be performed, the first action is specified as startAction and the final action
        /// as endAction.  The intermediate actions are added automatically.  (If only one action is
        /// desired, the same value is specified for both startAction and endAction.)
        /// </summary>
        /// <param name="batchName">Descriptive name</param>
        /// <param name="userName">User ID creating the batch</param>
        /// <param name="startAction">The first action in a range of publication actions
        /// to perform.</param>
        /// <param name="endAction">The final action to perform in a range of publication actions.</param>
        public Batch(string batchName, string userName,
            ProcessActionType startAction, ProcessActionType endAction )
        {
            if (batchName == null)
                throw new ArgumentNullException("batchName");
            if (userName == null)
                throw new ArgumentNullException("userName");

            if (!Enum.IsDefined(typeof(ProcessActionType), startAction))
                throw ExceptionBuilder.InvalidValue("startAction", startAction);
            if (!Enum.IsDefined(typeof(ProcessActionType), endAction))
                throw ExceptionBuilder.InvalidValue("endAction", endAction);

            _batchName = batchName;
            _userName = userName;

            /// Don't accept start actions that come later than the end action.
            /// e.g. If the endAction is pushToPreview, don't allow pushToLive as startAction.
            if (startAction > endAction)
            {
                string format = "Initial promotion action {0} must be less than or equal to the final action {1}";
                string message = string.Format(format, startAction.ToString(), endAction.ToString());
                BatchLogBuilder.Instance.CreateError(typeof(Batch), "Batch (constructor)", message);
                throw new Exception(message);
            }

            //PromoteToLiveFast action list is a special case - build list separately
            //skip the PromoteToPreview step
            if (startAction == ProcessActionType.PromoteToStaging && endAction == ProcessActionType.PromoteToLive)
            {
                Actions.Add(ProcessActionType.PromoteToStaging);
                Actions.Add(ProcessActionType.PromoteToLiveFast);
            }
            else
            {
                for (ProcessActionType action = startAction; action <= endAction; ++action)
                {
                    Actions.Add(action);
                }
            }
        }

        #region Properties

        public int BatchID
        {
            get { return _batchID; }
            set { _batchID = value; }
        }

        public BatchStatusType Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string BatchName
        {
          get { return _batchName; }
          set { _batchName = value; }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public List<ProcessActionType> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }

        public List<int> RequestDataIDs
        {
            get { return _requestDataIDs; }
            set { _requestDataIDs = value; }
        }

        #endregion Properties
    }
}
