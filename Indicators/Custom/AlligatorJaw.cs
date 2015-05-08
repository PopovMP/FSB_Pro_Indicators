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
using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class AlligatorJaw : MainChartSingleLineIndicator
    {
        public AlligatorJaw()
        {
            IndicatorName        = "Alligator Jaw";
            IndicatorVersion     = "1.0";
            IndicatorAuthor      = "Miroslav Popov";
            IndicatorDescription = "Bill Williams Alligator Jaw";
        }

        public override void Initialize(SlotTypes slotType)
        {
            base.Initialize(slotType);

            // List parameters
            IndParam.ListParam[1].Index = (int) MAMethod.Smoothed;
            IndParam.ListParam[1].Text  = MAMethod.Smoothed.ToString();

            IndParam.ListParam[2].Index = (int) BasePrice.Median;
            IndParam.ListParam[2].Text  = BasePrice.Median.ToString();

            // Adds two numeric parameters more
            IndParam.NumParam[0].Value = 13; // Default period
            IndParam.NumParam[1].Value = 8;  // Default shift
        }

        public override void Calculate(IDataSet dataSet)
        {
            InitCalculation(dataSet);

            IndicatorLine = MovingAverage(IndicatorPeriod, IndicatorShift, IndicatorMaMethod, Price(IndicatorBasePrice));

            PostCalculation();

            Component[0].ChartColor = Color.Blue; // Alligator Jaw is Blue
        }
    }
}
