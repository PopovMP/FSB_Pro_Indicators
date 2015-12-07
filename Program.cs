//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Text;
using ForexStrategyBuilder.Indicators.Custom;
using ForexStrategyBuilder.Indicators.Store;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;
using ForexStrategyBuilder.Services;

namespace ForexStrategyBuilder
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var doc = new DocGenerator();
            doc.GenerateDocs();

            DataLoader dataLoader = new DataLoader();
            IndicatorTester tester = new IndicatorTester();

            dataLoader.UserFilesFolder = @"C:\Program Files\Forex Strategy Builder Pro\User Files";

            // Set Data Source name, symbol, and period
            IDataSet dataSet = dataLoader.LoadDataSet("FSB Demo Data", "EURUSD", DataPeriod.M15);

            if (dataSet == null)
            {
                Console.WriteLine("Data file is not loaded!");
                Console.WriteLine("Press a key to continue!");
                Console.ReadKey();
                return;
            }

            // Create an indicator for testing
            IIndicator indicator = new CCIBuySellZones();
            indicator.Initialize(SlotTypes.Open);
            indicator.Calculate(dataSet);

            // Print first values
            PrintFirstValues(indicator, 0, 50);

            // Calculate indicator with random parameters for all available slots.
            tester.CalculateIndicatorWithRandomParameters(indicator, dataSet, 25);

            Console.WriteLine("Test completed without errors.");
            Console.WriteLine("Press a key to continue!");
            Console.ReadKey();
        }

        private static void PrintFirstValues(IIndicator indicator, int componentIndex, int countOfValuesToPrint)
        {
            int firstBar = indicator.Component[componentIndex].FirstBar;
            var sb = new StringBuilder();
            sb.AppendLine("bar        value");
            sb.AppendLine("--------------------------");
            for (int bar = firstBar; bar < firstBar + countOfValuesToPrint; bar++)
                sb.AppendLine(bar + "    : " + indicator.Component[componentIndex].Value[bar]);
            Console.WriteLine(sb.ToString());
        }
    }
}