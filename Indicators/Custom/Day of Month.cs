//==============================================================
// Forex Strategy Builder
// Copyright © 2016 Forex Software Ltd. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class DayOfMonth : Indicator
    {
	    public DayOfMonth()
        {
            IndicatorName   = "Day of Month";
            PossibleSlots   = SlotTypes.OpenFilter;

            IndicatorAuthor      = "Miroslav Popov";
            IndicatorVersion     = "2.0";
            IndicatorDescription = "A custom indicator for FSB and FST.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;
			
            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.DateTime;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                "Enter the market between the specified days"
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.NumParam[0].Caption  = "From (incl.)";
            IndParam.NumParam[0].Value    = 1;
            IndParam.NumParam[0].Min      = 1;
            IndParam.NumParam[0].Max      = 31;
            IndParam.NumParam[0].Point    = 0;
            IndParam.NumParam[0].Enabled  = true;
            IndParam.NumParam[0].ToolTip  = "Day of beginning for the entry period.";

            IndParam.NumParam[1].Caption  = "Until (excl.)";
            IndParam.NumParam[1].Value    = 31;
            IndParam.NumParam[1].Min      = 1;
            IndParam.NumParam[1].Max      = 31;
            IndParam.NumParam[1].Point    = 0;
            IndParam.NumParam[1].Enabled  = true;
            IndParam.NumParam[1].ToolTip  = "Day of ending for the entry period.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;
			
            // Reading the parameters
            int fromDay  = (int)IndParam.NumParam[0].Value;
            int untilDay = (int)IndParam.NumParam[1].Value;

            // Calculation
            int firstBar = 2;
            double[] signal = new double[Bars];

            // Calculation of the logic
            for (int bar = firstBar; bar < Bars; bar++)
            {
                if (fromDay < untilDay)
                    signal[bar] = Time[bar].Day >= fromDay && Time[bar].Day <  untilDay ? 1 : 0;
                else if (fromDay > untilDay)
                    signal[bar] = Time[bar].Day >= fromDay || Time[bar].Day <  untilDay ? 1 : 0;
                else
                    signal[bar] = 1;
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
            {
                CompName      = "Allow long entry",
                DataType      = IndComponentType.AllowOpenLong,
                ChartType     = IndChartType.NoChart,
                ShowInDynInfo = false,
                FirstBar      = firstBar,
                Value         = signal
            };

            Component[1] = new IndicatorComp
            {
                CompName      = "Allow short entry",
                DataType      = IndComponentType.AllowOpenShort,
                ChartType     = IndChartType.NoChart,
                ShowInDynInfo = false,
                FirstBar      = firstBar,
                Value         = signal
            };
        }

        public override void SetDescription()
        {
            int fromDay  = (int)IndParam.NumParam[0].Value;
            int untilDay = (int)IndParam.NumParam[1].Value;
            EntryFilterLongDescription  = "the day of month is from " + fromDay + " (incl.) to " + untilDay + " (excl.)";
            EntryFilterShortDescription = "the day of month is from " + fromDay + " (incl.) to " + untilDay + " (excl.)";
        }

        public override string ToString()
        {
            int fromDay  = (int)IndParam.NumParam[0].Value;
            int untilDay = (int)IndParam.NumParam[1].Value;
            return IndicatorName + " (" + fromDay  + ", " + untilDay + ")";
        }
    }
}
