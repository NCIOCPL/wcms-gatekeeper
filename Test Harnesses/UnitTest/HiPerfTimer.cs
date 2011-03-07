using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace Utilities.Timers
{
    /// <summary>
    /// Hi performance timer from codrproject.com.
    /// </summary>
    /// <see cref="http://www.codeproject.com/csharp/highperformancetimercshar.asp?df=100&forumid=4400&exp=0&select=1151395"/>
    public class HiPerfTimer
    {
        #region Fields

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public HiPerfTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception("High-performance counter not supported!");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the timer
        /// </summary>
        public void Start()
        {
            // lets the waiting threads do their work
            Thread.Sleep(0);

            QueryPerformanceCounter(out startTime);
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the duration of the timer (in seconds).
        /// </summary>
        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }

        #endregion
    }
}