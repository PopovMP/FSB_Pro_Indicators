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
            IIndicator indicator = new PriceMARelation();

            tester.CalculateIndicatorWithRandomParameters(indicator, dataSet, 25);

            Console.WriteLine("Test completed without errors.");
            Console.WriteLine("Press a key to continue!");
            Console.ReadKey();
        }
    }
}