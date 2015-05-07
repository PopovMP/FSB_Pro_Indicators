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
    public class AdaptiveMovingAverage : MainChartSingleLineIndicator
    {
        public AdaptiveMovingAverage()
        {
            IndicatorName        = "Adaptive Moving Average";
            IndicatorVersion     = "1.0";
            IndicatorAuthor      = "Miroslav Popov";
            IndicatorDescription = "Adaptive Moving Average - AMA";
        }

        public override void Initialize(SlotTypes slotType)
        {
            base.Initialize(slotType);

            // List parameters
            IndParam.ListParam[1].Enabled = false; // Hides the MA method beacuse is not used

            // Adds two numeric parameters more
            IndParam.NumParam[1].Caption = "Fast period";
            IndParam.NumParam[1].Value   = 2;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Period of fast EMA.";

            IndParam.NumParam[2].Caption = "Slow period";
            IndParam.NumParam[2].Value   = 30;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "Period of slow EMA.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            InitCalculation(dataSet);

            int fastPeriod = (int)IndParam.NumParam[1].Value;
            int slowPeriod = (int)IndParam.NumParam[2].Value;

            FirstBar = IndicatorPeriod + fastPeriod + slowPeriod;

            double fastConstant = 2.0/(fastPeriod + 1);
            double slowConstant = 2.0/(slowPeriod + 1);
            double[] basePrice = Price(IndicatorBasePrice);

            for (int bar = FirstBar; bar < Bars; bar++)
            {
                double ef = CalculateEfficiencyRatio(basePrice, IndicatorPeriod, bar);
                double sc = ef*(fastConstant - slowConstant) + slowConstant;
                IndicatorLine[bar] = IndicatorLine[bar - 1] + sc*sc*(basePrice[bar] - IndicatorLine[bar - 1]);
            }

            PostCalculation();

            Component[0].ChartColor = Color.DarkOrchid; // Override default color
        }

        private double CalculateEfficiencyRatio(double[] basePrice, int period, int bar)
        {
            double direction = basePrice[bar] - basePrice[bar - period];
            double volatility = 0;
            for (int i = 0; i < period; i++)
                volatility += Math.Abs(basePrice[bar - i] - basePrice[bar - i - 1]);
            return volatility < Point ? 0 : direction/volatility;
        }
    }
}
