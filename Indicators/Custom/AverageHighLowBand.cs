//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class AverageHighLowBand : MainChartBandsIndicator
    {
        public AverageHighLowBand()
        {
            IndicatorName        = "Average High Low Band";
            IndicatorVersion     = "1.0";
            IndicatorAuthor      = "Miroslav Popov";
            IndicatorDescription = "Plots two MA lines calculated on High and Low";
        }

        public override void Initialize(SlotTypes slotType)
        {
            base.Initialize(slotType);

            IndParam.ListParam[2].Enabled = false; // Hides the default base price
        }

        public override void Calculate(IDataSet dataSet)
        {
            InitCalculation(dataSet);

            UpperBand = MovingAverage(IndicatorPeriod, 0, IndicatorMaMethod, High);
            LowerBand = MovingAverage(IndicatorPeriod, 0, IndicatorMaMethod, Low);
            
            PostCalculation();

            // Ovverides the deafult colors
            Component[0].ChartColor = Color.ForestGreen;
            Component[1].ChartColor = Color.Tomato;
        }
    }
}
