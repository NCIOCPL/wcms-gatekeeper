using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;

using NCI.Messaging;

//using GateKeeper.ProcessManager;

namespace ProcessTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //// TODO:  Log error when queue name/server values are null
            //string queueName = ConfigurationManager.AppSettings["GateKeeperQueueName"];

            //MSMQSender sender = new MSMQSender(queueName);

            //ProcessHandler handler = new ProcessHandler("TEST");
            //Thread runner = null;

            //ConsoleKey key;

            //menu();
            //while ((key = Console.ReadKey(true).Key) != ConsoleKey.Q)
            //{
            //    switch (key)
            //    {
            //        case ConsoleKey.R:

            //            if (runner == null || !runner.IsAlive)
            //            {
            //                runner = new Thread(new ThreadStart(handler.ProcessBatches));
            //                runner.IsBackground = false;
            //                runner.Start();
            //            }
            //            break;

            //        case ConsoleKey.S:
            //            sender.AddToQueue("Start", "label");
            //            break;

            //        case ConsoleKey.D1:
            //        case ConsoleKey.NumPad1:
            //            for (int i = 0; i < 100; ++i)
            //            {
            //                sender.AddToQueue("Start", "label");
            //            }
            //            break;
            //    }

            //    menu();
            //}

            //Console.WriteLine("Halting - 1");
            //if( runner != null && runner.IsAlive)
            //    handler.HaltProcessing();
            //Console.WriteLine("Halting - 2");
        }

        static void menu()
        {
            Console.WriteLine("Q - Quit");
            Console.WriteLine("R - Run Batches");
            Console.WriteLine("S - Send \"New Batch\" message.");
            Console.WriteLine("1 - Send 100 \"New Batch\" messages.");
            Console.WriteLine("---------------");
        }
    }
}
