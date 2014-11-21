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
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class DaysOfWeek : Indicator
    {
        public DaysOfWeek()
        {

            PossibleSlots = SlotTypes.OpenFilter;
            IndicatorName = "Day of Week";
            IsDeafultGroupAll = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.DateTime;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Enter the market between the specified days"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicators' logic.";

            IndParam.ListParam[1].Caption = "From (incl.)";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (DayOfWeek));
            IndParam.ListParam[1].Index = (int) DayOfWeek.Monday;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Day of beginning for the entry period.";

            IndParam.ListParam[2].Caption = "To (excl.)";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (DayOfWeek));
            IndParam.ListParam[2].Index = (int) DayOfWeek.Saturday;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "End day for the entry period.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var dowFromDay = (DayOfWeek) IndParam.ListParam[1].Index;
            var dowUntilDay = (DayOfWeek) IndParam.ListParam[2].Index;

            // Calculation
            const int firstBar = 1;
            var adBars = new double[Bars];

            // Calculation of the logic
            for (int iBar = firstBar; iBar < Bars; iBar++)
            {
                if (dowFromDay < dowUntilDay)
                    adBars[iBar] = Time[iBar].DayOfWeek >= dowFromDay &&
                                   Time[iBar].DayOfWeek < dowUntilDay
                                       ? 1
                                       : 0;
                else if (dowFromDay > dowUntilDay)
                    adBars[iBar] = Time[iBar].DayOfWeek >= dowFromDay ||
                                   Time[iBar].DayOfWeek < dowUntilDay
                                       ? 1
                                       : 0;
                else
                    adBars[iBar] = 1;
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
                {
                    CompName = "Allow long entry",
                    DataType = IndComponentType.AllowOpenLong,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = adBars
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Allow short entry",
                    DataType = IndComponentType.AllowOpenShort,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = adBars
                };
        }

        public override void SetDescription()
        {
            var dowFromDay = (DayOfWeek) IndParam.ListParam[1].Index;
            var dowUntilDay = (DayOfWeek) IndParam.ListParam[2].Index;

            EntryFilterLongDescription = "the day of week is from " + dowFromDay + " (incl.) to " + dowUntilDay + " (excl.)";
            EntryFilterShortDescription = "the day of week is from " + dowFromDay + " (incl.) to " + dowUntilDay + " (excl.)";
        }

        public override string ToString()
        {
            var dowFromDay = (DayOfWeek) IndParam.ListParam[1].Index;
            var dowUntilDay = (DayOfWeek) IndParam.ListParam[2].Index;

            return IndicatorName + " (" +
                    dowFromDay + ", " + // From
                    dowUntilDay + ")"; // Until
        }
    }
}