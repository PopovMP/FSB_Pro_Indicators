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
    public class EntryTime : Indicator
    {
        public EntryTime()
        {
            IndicatorName = "Entry Time";
            PossibleSlots = SlotTypes.OpenFilter;
            IsDeafultGroupAll = true;
            IsGeneratable = false;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Enter the market between the specified hours"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "From hour (incl.)";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 23;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Beginning of the entry period.";

            IndParam.NumParam[1].Caption = "From min (incl.)";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 59;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Beginning of the entry period.";

            IndParam.NumParam[2].Caption = "Until hour (excl.)";
            IndParam.NumParam[2].Value = 24;
            IndParam.NumParam[2].Min = 0;
            IndParam.NumParam[2].Max = 24;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "End of the entry period.";

            IndParam.NumParam[3].Caption = "Until min (excl.)";
            IndParam.NumParam[3].Value = 0;
            IndParam.NumParam[3].Min = 0;
            IndParam.NumParam[3].Max = 59;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "End of the entry period.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var fromHour = (int)IndParam.NumParam[0].Value;
            var fromMin = (int)IndParam.NumParam[1].Value;
            var untilHour = (int)IndParam.NumParam[2].Value;
            var untilMin = (int)IndParam.NumParam[3].Value;
            var fromTime = new TimeSpan(fromHour, fromMin, 0);
            var untilTime = new TimeSpan(untilHour, untilMin, 0);

            // Calculation
            int firstBar = 2;
            var signal = new double[Bars];

            // Calculation of the logic
            for (int bar = firstBar; bar < Bars; bar++)
            {
                if (fromTime < untilTime)
                {
                    signal[bar] = Time[bar].TimeOfDay >= fromTime && Time[bar].TimeOfDay < untilTime ? 1 : 0;
                }
                else if (fromTime > untilTime)
                {
                    signal[bar] = Time[bar].TimeOfDay >= fromTime || Time[bar].TimeOfDay < untilTime ? 1 : 0;
                }
                else
                {
                    signal[bar] = 1;
                }
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
            {
                CompName = "Is long entry allowed",
                DataType = IndComponentType.AllowOpenLong,
                ChartType = IndChartType.NoChart,
                ShowInDynInfo = false,
                FirstBar = firstBar,
                Value = signal
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Is short entry allowed",
                DataType = IndComponentType.AllowOpenShort,
                ChartType = IndChartType.NoChart,
                ShowInDynInfo = false,
                FirstBar = firstBar,
                Value = signal
            };
        }

        public override void SetDescription()
        {
            var fromHour = (int)IndParam.NumParam[0].Value;
            var fromMin = (int)IndParam.NumParam[1].Value;
            var toHour = (int)IndParam.NumParam[2].Value;
            var toMin = (int)IndParam.NumParam[3].Value;

            string fromTime = fromHour.ToString("00") + ":" + fromMin.ToString("00");
            string toTime = toHour.ToString("00") + ":" + toMin.ToString("00");

            EntryFilterLongDescription = "the entry time is between " + fromTime + " (incl.) and " + toTime + " (excl.)";
            EntryFilterShortDescription = "the entry time is between " + fromTime + " (incl.) and " + toTime + " (excl.)";
        }

        public override string ToString()
        {
            var fromHour = (int)IndParam.NumParam[0].Value;
            var fromMin = (int)IndParam.NumParam[1].Value;
            var toHour = (int)IndParam.NumParam[2].Value;
            var toMin = (int)IndParam.NumParam[3].Value;

            string fromTime = fromHour.ToString("00") + ":" + fromMin.ToString("00");
            string untilTime = toHour.ToString("00") + ":" + toMin.ToString("00");

            return IndicatorName + " (" +
                    fromTime + " - " + // From
                    untilTime + ")"; // Until
        }
    }
}