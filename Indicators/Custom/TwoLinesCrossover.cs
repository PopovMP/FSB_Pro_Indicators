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
    public class TwoLinesCrossover : MainChartCrossingLinesIndicator
    {
        public TwoLinesCrossover()
        {
            IndicatorName        = "Two Lines Crossover";
            IndicatorVersion     = "1.0";
            IndicatorAuthor      = "Miroslav Popov";
            IndicatorDescription = "Fast and SLow MA crossover.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            base.Initialize(slotType);

        }

        public override void Calculate(IDataSet dataSet)
        {
            InitCalculation(dataSet);

            double[] basePrice = Price(IndicatorBasePrice);
            FastLine = MovingAverage(FastLinePeriod, FastLineShift, FastLineMethod, basePrice);
            SlowLine = MovingAverage(SlowLinePeriod, SlowLineShift, SlowLineMethod, basePrice);

            PostCalculation();
        }
    }
}
