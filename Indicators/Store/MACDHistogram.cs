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
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class MACDHistogram : Indicator
    {
        public MACDHistogram()
        {
            IndicatorName = "MACD Histogram";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "MACD histogram rises",
                    "MACD histogram falls",
                    "MACD histogram is higher than the Level line",
                    "MACD histogram is lower than the Level line",
                    "MACD histogram crosses the Level line upward",
                    "MACD histogram crosses the Level line downward",
                    "MACD histogram changes its direction upward",
                    "MACD histogram changes its direction downward"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index = (int)MAMethod.Exponential;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The smoothing method of Moving Averages.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index = (int)BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the Moving Averages are based on.";

            IndParam.ListParam[3].Caption = "Signal line method";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[3].Index = (int)MAMethod.Simple;
            IndParam.ListParam[3].Text = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled = true;
            IndParam.ListParam[3].ToolTip = "The smoothing method of the signal line.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Slow MA period";
            IndParam.NumParam[0].Value = 26;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Slow MA.";

            IndParam.NumParam[1].Caption = "Fast MA period";
            IndParam.NumParam[1].Value = 12;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of Fast MA.";

            IndParam.NumParam[2].Caption = "Signal line period";
            IndParam.NumParam[2].Value = 9;
            IndParam.NumParam[2].Min = 1;
            IndParam.NumParam[2].Max = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The period of Signal line.";

            IndParam.NumParam[3].Caption = "Level";
            IndParam.NumParam[3].Value = 0;
            IndParam.NumParam[3].Min = 0;
            IndParam.NumParam[3].Max = 5;
            IndParam.NumParam[3].Point = 4;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "A signal level.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var maMethod = (MAMethod)IndParam.ListParam[1].Index;
            var slMethod = (MAMethod)IndParam.ListParam[3].Index;
            var basePrice = (BasePrice)IndParam.ListParam[2].Index;
            var slowPeriod = (int)IndParam.NumParam[0].Value;
            var fastPeriod = (int)IndParam.NumParam[1].Value;
            var signalPeriod = (int)IndParam.NumParam[2].Value;
            double level = IndParam.NumParam[3].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = Math.Max(Math.Max(slowPeriod, fastPeriod), signalPeriod) + previous + 2;

            double[] maSlow = MovingAverage(slowPeriod, 0, maMethod, Price(basePrice));
            double[] maFast = MovingAverage(fastPeriod, 0, maMethod, Price(basePrice));

            var macd = new double[Bars];

            for (int bar = slowPeriod - 1; bar < Bars; bar++)
            {
                macd[bar] = maFast[bar] - maSlow[bar];
            }

            double[] maSignalLine = MovingAverage(signalPeriod, 0, slMethod, macd);

            // The histogram represents MACD oscillator
            var histogram = new double[Bars];
            for (int bar = slowPeriod + signalPeriod - 1; bar < Bars; bar++)
            {
                histogram[bar] = macd[bar] - maSignalLine[bar];
            }

            // Saving the components
            Component = new IndicatorComp[5];

            Component[0] = new IndicatorComp
            {
                CompName = "Histogram",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Histogram,
                FirstBar = firstBar,
                Value = histogram
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Signal line",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Color.Gold,
                FirstBar = firstBar,
                Value = maSignalLine
            };

            Component[2] = new IndicatorComp
            {
                CompName = "MACD line",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Color.Blue,
                FirstBar = firstBar,
                Value = macd
            };

            Component[3] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            Component[4] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[3].DataType = IndComponentType.AllowOpenLong;
                Component[3].CompName = "Is long entry allowed";
                Component[4].DataType = IndComponentType.AllowOpenShort;
                Component[4].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[3].DataType = IndComponentType.ForceCloseLong;
                Component[3].CompName = "Close out long position";
                Component[4].DataType = IndComponentType.ForceCloseShort;
                Component[4].CompName = "Close out short position";
            }

            // Calculation of the logic
            var logicRule = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "MACD histogram rises":
                    logicRule = IndicatorLogic.The_indicator_rises;
                    break;

                case "MACD histogram falls":
                    logicRule = IndicatorLogic.The_indicator_falls;
                    break;

                case "MACD histogram is higher than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] { level, -level };
                    break;

                case "MACD histogram is lower than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] { level, -level };
                    break;

                case "MACD histogram crosses the Level line upward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] { level, -level };
                    break;

                case "MACD histogram crosses the Level line downward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] { level, -level };
                    break;

                case "MACD histogram changes its direction upward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "MACD histogram changes its direction downward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
            }

            OscillatorLogic(firstBar, previous, histogram, level, -level, ref Component[3], ref Component[4], logicRule);
        }

        public override void SetDescription()
        {
            string levelLong = (Math.Abs(IndParam.NumParam[3].Value - 0) < Epsilon ? "0" : IndParam.NumParam[3].ValueToString);
            string levelShort = (Math.Abs(IndParam.NumParam[3].Value - 0) < Epsilon ? "0" : "-" + IndParam.NumParam[3].ValueToString);

            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "MACD histogram rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "MACD histogram falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "MACD histogram is higher than the Level line":
                    EntryFilterLongDescription += "is higher than the Level " + levelLong;
                    EntryFilterShortDescription += "is lower than the Level " + levelShort;
                    ExitFilterLongDescription += "is higher than the Level " + levelLong;
                    ExitFilterShortDescription += "is lower than the Level " + levelShort;
                    break;

                case "MACD histogram is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + levelLong;
                    EntryFilterShortDescription += "is higher than the Level " + levelShort;
                    ExitFilterLongDescription += "is lower than the Level " + levelLong;
                    ExitFilterShortDescription += "is higher than the Level " + levelShort;
                    break;

                case "MACD histogram crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    break;

                case "MACD histogram crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    break;

                case "MACD histogram changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "MACD histogram changes its direction downward":
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
                   IndParam.ListParam[1].Text + ", " + // Method
                   IndParam.ListParam[2].Text + ", " + // Price
                   IndParam.ListParam[3].Text + ", " + // Signal MA Method
                   IndParam.NumParam[0].ValueToString + ", " + // Slow MA period
                   IndParam.NumParam[1].ValueToString + ", " + // Fast MA period
                   IndParam.NumParam[2].ValueToString + ")"; // Signal MA period
        }
    }
}