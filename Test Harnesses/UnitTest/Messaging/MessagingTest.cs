using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using NCI.Messaging;
using NUnit.Framework;

namespace GateKeeper.UnitTest.Messaging
{
    [TestFixture]
    public class MessagingTest
    {
        /// <summary>
        /// Send and receive a simple message via MSMQ.
        /// </summary>
        [Test]
        public void SendReceiveMessageTest()
        {
            string queueName = ConfigurationManager.AppSettings["GateKeeperQueueName"];

            // Send a message
            MSMQSender sender = new MSMQSender(queueName);
            sender.AddToQueue("Start", "label");

            // Receive a message.
            MSMQReceiver receiver = new MSMQReceiver(queueName, 10, Processor);
            receiver.BeginReceiving();
        }

        private void Processor(Object m)
        {

        }


    }
}
