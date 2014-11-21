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
    public class OscillatorOfMACD : Indicator
    {
        public OscillatorOfMACD()
        {
            IndicatorName = "Oscillator of MACD";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.OscillatorOfIndicators;

            // ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Oscillator of MACD rises",
                    "Oscillator of MACD falls",
                    "Oscillator of MACD is higher than the zero line",
                    "Oscillator of MACD is lower than the zero line",
                    "Oscillator of MACD crosses the zero line upward",
                    "Oscillator of MACD crosses the zero line downward",
                    "Oscillator of MACD changes its direction upward",
                    "Oscillator of MACD changes its direction downward"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the oscillator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Exponential;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Moving Average method used for smoothing the MACD value.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[2].Index = (int) BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the indicator is based on.";

            IndParam.ListParam[3].Caption = "Signal line method";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[3].Index = (int) MAMethod.Exponential;
            IndParam.ListParam[3].Text = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled = true;
            IndParam.ListParam[3].ToolTip = "The smoothing method of the signal line.";

            IndParam.ListParam[4].Caption = "What to compare";
            IndParam.ListParam[4].ItemList = new[] {"Histograms", "Signal lines", "MACD lines"};
            IndParam.ListParam[4].Index = 0;
            IndParam.ListParam[4].Text = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[4].Enabled = true;
            IndParam.ListParam[4].ToolTip = "The smoothing method of the signal line.";

            // NumericUpDown parameters
            IndParam.NumParam[0].Caption = "MACD1 slow MA";
            IndParam.NumParam[0].Value = 26;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of first MACD slow line.";

            IndParam.NumParam[1].Caption = "MACD2 slow MA";
            IndParam.NumParam[1].Value = 32;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of second MACD slow line.";

            IndParam.NumParam[2].Caption = "MACD1 fast MA";
            IndParam.NumParam[2].Value = 12;
            IndParam.NumParam[2].Min = 1;
            IndParam.NumParam[2].Max = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The period of first MACD fast line.";

            IndParam.NumParam[3].Caption = "MACD2 fast MA";
            IndParam.NumParam[3].Value = 21;
            IndParam.NumParam[3].Min = 1;
            IndParam.NumParam[3].Max = 200;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "The period of second MACD fast line.";

            IndParam.NumParam[4].Caption = "MACD1 Signal line";
            IndParam.NumParam[4].Value = 9;
            IndParam.NumParam[4].Min = 1;
            IndParam.NumParam[4].Max = 200;
            IndParam.NumParam[4].Enabled = true;
            IndParam.NumParam[4].ToolTip = "The period of Signal line.";

            IndParam.NumParam[5].Caption = "MACD2 Signal line";
            IndParam.NumParam[5].Value = 13;
            IndParam.NumParam[5].Min = 1;
            IndParam.NumParam[5].Max = 200;
            IndParam.NumParam[5].Enabled = true;
            IndParam.NumParam[5].ToolTip = "The period of Signal line.";

            // CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            var macd1 = new MACD();
            macd1.Initialize(SlotType);
            macd1.IndParam.ListParam[1].Index = IndParam.ListParam[1].Index;
            macd1.IndParam.ListParam[2].Index = IndParam.ListParam[2].Index;
            macd1.IndParam.ListParam[3].Index = IndParam.ListParam[3].Index;
            macd1.IndParam.NumParam[0].Value = IndParam.NumParam[0].Value;
            macd1.IndParam.NumParam[1].Value = IndParam.NumParam[2].Value;
            macd1.IndParam.NumParam[2].Value = IndParam.NumParam[4].Value;
            macd1.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            macd1.Calculate(DataSet);

            var macd2 = new MACD();
            macd2.Initialize(SlotType);
            macd2.IndParam.ListParam[1].Index = IndParam.ListParam[1].Index;
            macd2.IndParam.ListParam[2].Index = IndParam.ListParam[2].Index;
            macd2.IndParam.ListParam[3].Index = IndParam.ListParam[3].Index;
            macd2.IndParam.NumParam[0].Value = IndParam.NumParam[1].Value;
            macd2.IndParam.NumParam[1].Value = IndParam.NumParam[3].Value;
            macd2.IndParam.NumParam[2].Value = IndParam.NumParam[5].Value;
            macd2.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            macd2.Calculate(DataSet);

            // Calculation
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;
            var period1 = (int) IndParam.NumParam[0].Value;
            var period2 = (int) IndParam.NumParam[1].Value;
            int firstBar = period1 + period2 + 2;
            double[] adIndicator1;
            double[] adIndicator2;

            switch (IndParam.ListParam[4].Index)
            {
                case 0:
                    adIndicator1 = macd1.Component[0].Value;
                    adIndicator2 = macd2.Component[0].Value;
                    break;
                case 1:
                    adIndicator1 = macd1.Component[1].Value;
                    adIndicator2 = macd2.Component[1].Value;
                    break;
                default:
                    adIndicator1 = macd1.Component[2].Value;
                    adIndicator2 = macd2.Component[2].Value;
                    break;
            }

            var adOscillator = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
                adOscillator[bar] = adIndicator1[bar] - adIndicator2[bar];

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "Oscillator",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Histogram,
                    FirstBar = firstBar,
                    Value = adOscillator
                };

            Component[1] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            Component[2] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            // Calculation of the logic
            var indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "Oscillator of MACD rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    break;

                case "Oscillator of MACD falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    break;

                case "Oscillator of MACD is higher than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;

                case "Oscillator of MACD is lower than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;

                case "Oscillator of MACD crosses the zero line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;

                case "Oscillator of MACD crosses the zero line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;

                case "Oscillator of MACD changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "Oscillator of MACD changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
            }

            OscillatorLogic(firstBar, previous, adOscillator, 0, 0, ref Component[1], ref Component[2], indLogic);
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Oscillator of MACD rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "Oscillator of MACD falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "Oscillator of MACD is higher than the zero line":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "Oscillator of MACD is lower than the zero line":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "Oscillator of MACD crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "Oscillator of MACD crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "Oscillator of MACD changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "Oscillator of MACD changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[4].Text + ", " + // What to compare
                   IndParam.ListParam[2].Text + ", " + // Price
                   IndParam.ListParam[1].Text + ", " + // Method
                   IndParam.ListParam[3].Text + ", " + // Signal line method
                   IndParam.NumParam[0].ValueToString + ", " + // Period
                   IndParam.NumParam[2].ValueToString + ", " + // Period
                   IndParam.NumParam[4].ValueToString + ", " + // Period
                   IndParam.NumParam[1].ValueToString + ", " + // Period
                   IndParam.NumParam[3].ValueToString + ", " + // Period
                   IndParam.NumParam[5].ValueToString + ")"; // Period
        }
    }
}