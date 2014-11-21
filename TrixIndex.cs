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
    public class TrixIndex : Indicator
    {
        public TrixIndex()
        {
            IndicatorName = "Trix Index";
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
                    "Trix Index line rises",
                    "Trix Index line falls",
                    "Trix Index line is higher than zero",
                    "Trix Index line is lower than zero",
                    "Trix Index line crosses the zero line upward",
                    "Trix Index line crosses the zero line downward",
                    "Trix Index line changes its direction upward",
                    "Trix Index line changes its direction downward",
                    "Trix Index line crosses the Signal line upward",
                    "Trix Index line crosses the Signal line downward",
                    "Trix Index line is higher than the Signal line",
                    "Trix Index line is lower than the Signal line"
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
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing Trix Index value.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[2].Index = (int) BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the indicator is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period of Trix";
            IndParam.NumParam[0].Value = 9;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Trix Moving Averages.";

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
            var basePrice = (BasePrice) IndParam.ListParam[2].Index;
            var nPeriod = (int) IndParam.NumParam[0].Value;
            int iPrvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = 2*nPeriod + 2;

            double[] ma1 = MovingAverage(nPeriod, 0, maMethod, Price(basePrice));
            double[] ma2 = MovingAverage(nPeriod, 0, maMethod, ma1);
            double[] ma3 = MovingAverage(nPeriod, 0, maMethod, ma2);

            var adTrix = new double[Bars];

            for (int bar = firstBar; bar < Bars; bar++)
                adTrix[bar] = 100*(ma3[bar] - ma3[bar - 1])/ma3[bar - 1];

            double[] adSignal = MovingAverage(nPeriod, 0, maMethod, adTrix);

            // adHistogram represents Trix Index oscillator
            var adHistogram = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
                adHistogram[bar] = adTrix[bar] - adSignal[bar];

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
                    CompName = "Signal",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Gold,
                    FirstBar = firstBar,
                    Value = adSignal
                };

            Component[2] = new IndicatorComp
                {
                    CompName = "Trix Line",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = firstBar,
                    Value = adTrix
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

            switch (IndParam.ListParam[0].Text)
            {
                case "Trix Index line rises":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_rises);
                    break;

                case "Trix Index line falls":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_falls);
                    break;

                case "Trix Index line is higher than zero":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_higher_than_the_level_line);
                    break;

                case "Trix Index line is lower than zero":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_lower_than_the_level_line);
                    break;

                case "Trix Index line crosses the zero line upward":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_upward);
                    break;

                case "Trix Index line crosses the zero line downward":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_downward);
                    break;

                case "Trix Index line changes its direction upward":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_changes_its_direction_upward);
                    break;

                case "Trix Index line changes its direction downward":
                    OscillatorLogic(firstBar, iPrvs, adTrix, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_changes_its_direction_downward);
                    break;

                case "Trix Index line crosses the Signal line upward":
                    OscillatorLogic(firstBar, iPrvs, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_upward);
                    break;

                case "Trix Index line crosses the Signal line downward":
                    OscillatorLogic(firstBar, iPrvs, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_downward);
                    break;

                case "Trix Index line is higher than the Signal line":
                    OscillatorLogic(firstBar, iPrvs, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_higher_than_the_level_line);
                    break;

                case "Trix Index line is lower than the Signal line":
                    OscillatorLogic(firstBar, iPrvs, adHistogram, 0, 0, ref Component[3], ref Component[4],
                                    IndicatorLogic.The_indicator_is_lower_than_the_level_line);
                    break;
            }
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Trix Index line rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "Trix Index line falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "Trix Index line is higher than zero":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "Trix Index line is lower than zero":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "Trix Index line crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "Trix Index line crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "Trix Index line changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "Trix Index line changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;

                case "Trix Index line crosses the Signal line upward":
                    EntryFilterLongDescription += "crosses the Signal line upward";
                    EntryFilterShortDescription += "crosses the Signal line downward";
                    ExitFilterLongDescription += "crosses the Signal line upward";
                    ExitFilterShortDescription += "crosses the Signal line downward";
                    break;

                case "Trix Index line crosses the Signal line downward":
                    EntryFilterLongDescription += "crosses the Signal line downward";
                    EntryFilterShortDescription += "crosses the Signal line upward";
                    ExitFilterLongDescription += "crosses the Signal line downward";
                    ExitFilterShortDescription += "crosses the Signal line upward";
                    break;

                case "Trix Index line is higher than the Signal line":
                    EntryFilterLongDescription += "is higher than the Signal line";
                    EntryFilterShortDescription += "is lower than the Signal line";
                    ExitFilterLongDescription += "is higher than the Signal line";
                    ExitFilterShortDescription += "is lower than the Signal line";
                    break;

                case "Trix Index line is lower than the Signal line":
                    EntryFilterLongDescription += "is lower than the Signal line";
                    EntryFilterShortDescription += "is higher than the Signal line";
                    ExitFilterLongDescription += "is lower than the Signal line";
                    ExitFilterShortDescription += "is higher than the Signal line";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Smoothing method
                   IndParam.ListParam[2].Text + ", " + // Base price
                   IndParam.NumParam[0].ValueToString + ")"; // Period of Trix
        }
    }
}