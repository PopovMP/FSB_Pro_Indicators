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
    public class MACD : Indicator
    {
        public MACD()
        {
            IndicatorName = "MACD";
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
                    "MACD line rises",
                    "MACD line falls",
                    "MACD line is higher than zero",
                    "MACD line is lower than zero",
                    "MACD line crosses the zero line upward",
                    "MACD line crosses the zero line downward",
                    "MACD line changes its direction upward",
                    "MACD line changes its direction downward",
                    "MACD line crosses the Signal line upward",
                    "MACD line crosses the Signal line downward",
                    "MACD line is higher than the Signal line",
                    "MACD line is lower than the Signal line"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Exponential;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The smoothing method of Moving Averages.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[2].Index = (int) BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the Moving Averages are based on.";

            IndParam.ListParam[3].Caption = "Signal line method";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[3].Index = (int) MAMethod.Simple;
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

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var maMethod = (MAMethod) IndParam.ListParam[1].Index;
            var slMethod = (MAMethod) IndParam.ListParam[3].Index;
            var basePrice = (BasePrice) IndParam.ListParam[2].Index;
            var slowPeriod = (int) IndParam.NumParam[0].Value;
            var fastPeriod = (int) IndParam.NumParam[1].Value;
            var signalLinePeriod = (int) IndParam.NumParam[2].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = slowPeriod + fastPeriod + 3;

            double[] adMASlow = MovingAverage(slowPeriod, 0, maMethod, Price(basePrice));
            double[] adMAFast = MovingAverage(fastPeriod, 0, maMethod, Price(basePrice));

            var adMACD = new double[Bars];

            for (int bar = slowPeriod - 1; bar < Bars; bar++)
                adMACD[bar] = adMAFast[bar] - adMASlow[bar];

            double[] maSignalLine = MovingAverage(signalLinePeriod, 0, slMethod, adMACD);

            // adHistogram represents MACD oscillator
            var adHistogram = new double[Bars];
            for (int bar = slowPeriod + signalLinePeriod - 1; bar < Bars; bar++)
                adHistogram[bar] = adMACD[bar] - maSignalLine[bar];

            // Saving the components
            Component = new IndicatorComp[5];

            Component[0] = new IndicatorComp
                {
                    CompName = "Histogram",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Histogram,
                    FirstBar = firstBar,
                    Value = adHistogram
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
                    Value = adMACD
                };

            Component[3] = new IndicatorComp
                {ChartType = IndChartType.NoChart, FirstBar = firstBar, Value = new double[Bars]};

            Component[4] = new IndicatorComp
                {ChartType = IndChartType.NoChart, FirstBar = firstBar, Value = new double[Bars]};

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

            switch (IndParam.ListParam[0].Text)
            {
                case "MACD line rises":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_rises);
                    break;

                case "MACD line falls":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_falls);
                    break;

                case "MACD line is higher than zero":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_higher_than_the_level_line);
                    break;

                case "MACD line is lower than zero":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_lower_than_the_level_line);
                    break;

                case "MACD line crosses the zero line upward":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_upward);
                    break;

                case "MACD line crosses the zero line downward":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_downward);
                    break;

                case "MACD line changes its direction upward":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_changes_its_direction_upward);
                    break;

                case "MACD line changes its direction downward":
                    OscillatorLogic(firstBar, previous, adMACD, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_changes_its_direction_downward);
                    break;

                case "MACD line crosses the Signal line upward":
                    OscillatorLogic(firstBar, previous, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_upward);
                    break;

                case "MACD line crosses the Signal line downward":
                    OscillatorLogic(firstBar, previous, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_downward);
                    break;

                case "MACD line is higher than the Signal line":
                    OscillatorLogic(firstBar, previous, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_higher_than_the_level_line);
                    break;

                case "MACD line is lower than the Signal line":
                    OscillatorLogic(firstBar, previous, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_lower_than_the_level_line);
                    break;
            }
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + "; MACD line ";
            EntryFilterShortDescription = ToString() + "; MACD line ";
            ExitFilterLongDescription = ToString() + "; MACD line ";
            ExitFilterShortDescription = ToString() + "; MACD line ";

            switch (IndParam.ListParam[0].Text)
            {
                case "MACD line rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "MACD line falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "MACD line is higher than zero":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "MACD line is lower than zero":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "MACD line crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "MACD line crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "MACD line changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "MACD line changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;

                case "MACD line is higher than the Signal line":
                    EntryFilterLongDescription += "is higher than the Signal line";
                    EntryFilterShortDescription += "is lower than the Signal line";
                    ExitFilterLongDescription += "is higher than the Signal line";
                    ExitFilterShortDescription += "is lower than the Signal line";
                    break;

                case "MACD line is lower than the Signal line":
                    EntryFilterLongDescription += "is lower than the Signal line";
                    EntryFilterShortDescription += "is higher than the Signal line";
                    ExitFilterLongDescription += "is lower than the Signal line";
                    ExitFilterShortDescription += "is higher than the Signal line";
                    break;

                case "MACD line crosses the Signal line upward":
                    EntryFilterLongDescription += "crosses the Signal line upward";
                    EntryFilterShortDescription += "crosses the Signal line downward";
                    ExitFilterLongDescription += "crosses the Signal line upward";
                    ExitFilterShortDescription += "crosses the Signal line downward";
                    break;

                case "MACD line crosses the Signal line downward":
                    EntryFilterLongDescription += "crosses the Signal line downward";
                    EntryFilterShortDescription += "crosses the Signal line upward";
                    ExitFilterLongDescription += "crosses the Signal line downward";
                    ExitFilterShortDescription += "crosses the Signal line upward";
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}{1} ({2}, {3}, {4}, {5}, {6}, {7})",
                                 IndicatorName,
                                 (IndParam.CheckParam[0].Checked ? "*" : ""),
                                 IndParam.ListParam[1].Text,
                                 IndParam.ListParam[2].Text,
                                 IndParam.ListParam[3].Text,
                                 IndParam.NumParam[0].ValueToString,
                                 IndParam.NumParam[1].ValueToString,
                                 IndParam.NumParam[2].ValueToString);
        }
    }
}