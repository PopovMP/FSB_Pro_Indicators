//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class TripleMovingAverage : MainChartSingleLineIndicator
    {
        public TripleMovingAverage()
        {
            IndicatorName        = "Triple Moving Average";
            IndicatorVersion     = "1.0";
            IndicatorAuthor      = "Miroslav Popov";
            IndicatorDescription = "Triple Exponential Moving Average - TEMA";
        }

        public override void Initialize(SlotTypes slotType)
        {
            base.Initialize(slotType);

            IndParam.ListParam[1].ItemList = new string[] {"Exponential"};
            IndParam.ListParam[1].Text     = "Exponential";
            IndParam.ListParam[1].Index    = 0;
        }

        public override void Calculate(IDataSet dataSet)
        {
            InitCalculation(dataSet);

            double[] ema = MovingAverage(IndicatorPeriod, 0, IndicatorMaMethod, Price(IndicatorBasePrice));
            double[] emaOfEma = MovingAverage(IndicatorPeriod, 0, IndicatorMaMethod, ema);
            double[] emaOfEmaOfEma = MovingAverage(IndicatorPeriod, 0, IndicatorMaMethod, emaOfEma);

            for (int bar = 0; bar < Bars; bar++)
                IndicatorLine[bar] = 3*ema[bar] - 3*emaOfEma[bar] + emaOfEmaOfEma[bar];

            FirstBar = 3*IndicatorPeriod + 2;

            PostCalculation();
        }
    }
}