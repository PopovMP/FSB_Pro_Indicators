//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class RisingVolume : Indicator
    {
        public RisingVolume()
        {
            IndicatorName = "Rising Volume";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = false;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "1.0";
            IndicatorDescription = "Determines rising volume bars.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                "Higher average volume"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logical rule for the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Volume period";
            IndParam.NumParam[0].Value = 10;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period for calculating the average volume.";

            IndParam.NumParam[1].Caption = "Volume threshold";
            IndParam.NumParam[1].Value = 1.5;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 5;
            IndParam.NumParam[1].Point = 1;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Average volume threshold.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            int period = (int) IndParam.NumParam[0].Value;
            double threshold = IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period + previous + 2;

            // Average volume
            double[] average = new double[Bars];
            double sum = 0;
            for (int bar = 0; bar < period; bar++)
                sum += Volume[bar];
            average[period - 1] = sum/period;
            for (int bar = period; bar < Bars; bar++)
                average[bar] = average[bar - 1] + (Volume[bar] - Volume[bar - period])/(float) period;

            // Signal
            double[] signal = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
                signal[bar] = Volume[bar - previous] >= threshold*average[bar - previous] ? 1 : 0;

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                Value = signal,
                FirstBar = firstBar
            };

            Component[1] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                Value = signal,
                FirstBar = firstBar
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[0].DataType = IndComponentType.AllowOpenLong;
                Component[0].CompName = "Is long entry allowed";
                Component[1].DataType = IndComponentType.AllowOpenShort;
                Component[1].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[0].DataType = IndComponentType.ForceCloseLong;
                Component[0].CompName = "Close out long position";
                Component[1].DataType = IndComponentType.ForceCloseShort;
                Component[1].CompName = "Close out short position";
            }
        }

        public override void SetDescription()
        {
            string text = "there is a volume higher than the average";
            EntryFilterLongDescription = text;
            EntryFilterShortDescription = text;
            ExitFilterLongDescription = text;
            ExitFilterShortDescription = text;
        }
    }
}