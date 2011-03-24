using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using GKManagers;
using NCI.Messaging;

namespace GateKeeper.ProcessManager
{
    public partial class ProcessManager : ServiceBase
    {
        private ProcessHandler _handler;

        private Timer _heartBeat;

        //private MSMQReceiver _msmqReceiver;
        //private string _queueName = ConfigurationManager.AppSettings["GateKeeperQueueName"];

        public ProcessManager()
        {
            InitializeComponent();

            CanPauseAndContinue = false;
            CanStop = true;
            CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            /// Don't start the service unless the database is avaiable.
            bool dbReady;
            string dbMessage;
            RequestManager.CheckDatabaseStatus(out dbReady, out dbMessage);
            if (!dbReady)
            {
                ProcessMgrLogBuilder.Instance.CreateCritical(this.GetType(), "OnStart", dbMessage);
                throw new Exception(dbMessage);
            }

            StartPolling();

            ProcessMgrLogBuilder.Instance.CreateInformation(this.GetType(), "OnStart",
                string.Format("The {0} Service has been started.", ServiceName));

            ////Connect to MSMQ system.
            //_msmqReceiver = new MSMQReceiver(_queueName, 10, StartProcessing);
            //_msmqReceiver.BeginReceiving();

            //MSMQSender sender = new MSMQSender(_queueName);
            //sender.AddToQueue("Start", "label");
        }

        protected override void OnStop()
        {
            if (_heartBeat != null)
            {
                _heartBeat.Dispose();
                _heartBeat = null;
            }
            _handler.HaltProcessing();
            //_msmqReceiver.Dispose();

            ProcessMgrLogBuilder.Instance.CreateInformation(this.GetType(), "OnStop",
                string.Format("The {0} Service has been stopped.", ServiceName));
        }

        private void StartProcessing(Object message)
        {
            _handler.ProcessBatches(ServiceName);
        }

        private void StartPolling()
        {
            /// <summary>
            /// Get the number of seconds to pause before polling the database for the next batch.
            /// </summary>
            int pollingFrequency;

            try
            {
                string freqInSeconds = ConfigurationManager.AppSettings["PollingFrequency"];
                if (freqInSeconds == null || freqInSeconds.Length == 0)
                    throw new Exception("PollingFrequency not set in configuration file.  Defaulting to five seconds.");

                pollingFrequency = int.Parse(freqInSeconds) * 1000;
                if (pollingFrequency < 1000)
                    throw new Exception(string.Format("Invalid PollingFrequency value ({0}).  Defaulting to five seconds.", freqInSeconds));
            }
            catch (FormatException)
            {
                ProcessMgrLogBuilder.Instance.CreateWarning(this.GetType(), "StartPolling", "Invalid PollingFrequency value.  Defaulting to five seconds.");
                pollingFrequency = 5000;
            }
            catch (Exception ex)
            {
                ProcessMgrLogBuilder.Instance.CreateWarning(this.GetType(), "StartPolling", ex.Message);
                pollingFrequency = 5000;
            }

            _handler = ProcessHandler.GetInstance();
            _heartBeat = new Timer(StartProcessing, null, pollingFrequency, pollingFrequency);
        }
    }
}
