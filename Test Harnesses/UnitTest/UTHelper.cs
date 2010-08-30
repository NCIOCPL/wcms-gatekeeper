using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace GateKeeper.UnitTest
{
    public static class UTHelper
    {
        public static double CalculateAverage(List<double> durationList)
        {
            double total = 0.0;
            double average = 0.0;
            foreach (double time in durationList)
            {
                total += time;
            }

            if (durationList.Count != 0)
            {
                average = total / durationList.Count;
                Console.WriteLine("Average extraction time = " + average.ToString());
            }

            return average;
        }
    }
}
