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
    public class WilliamsPercentRange : Indicator
    {
        public WilliamsPercentRange()
        {
            IndicatorName = "Williams' Percent Range";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = -100;
            SeparatedChartMaxValue = 0;

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
                    "WPR rises",
                    "WPR falls",
                    "WPR is higher than the Level line",
                    "WPR is lower than the Level line",
                    "WPR crosses the Level line upward",
                    "WPR crosses the Level line downward",
                    "WPR changes its direction upward",
                    "WPR changes its direction downward"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Simple;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing.";

            IndParam.NumParam[0].Caption = "Period of %R";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of calculation.";

            IndParam.NumParam[1].Caption = "Smoothing period";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of an additional smoothing.";

            IndParam.NumParam[2].Caption = "Level";
            IndParam.NumParam[2].Value = -20;
            IndParam.NumParam[2].Min = -100;
            IndParam.NumParam[2].Max = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "A critical level (for the appropriate logic).";

            // The CheckBox parameters.
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var method = (MAMethod) IndParam.ListParam[1].Index;
            var period = (int) IndParam.NumParam[0].Value;
            var smoothing = (int) IndParam.NumParam[1].Value;
            var level = (int) IndParam.NumParam[2].Value;
            int prvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period + smoothing + prvs + 2;

            var adR = new double[Bars];
            for (int bar = period; bar < Bars; bar++)
            {
                double dMin = double.MaxValue;
                double dMax = double.MinValue;
                for (int index = 0; index < period; index++)
                {
                    if (High[bar - index] > dMax) dMax = High[bar - index];
                    if (Low[bar - index] < dMin) dMin = Low[bar - index];
                }
                adR[bar] = -100*(dMax - Close[bar])/(dMax - dMin);
            }

            double[] adRSmoothed = MovingAverage(smoothing, 0, method, adR);

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "%R",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Teal,
                    FirstBar = firstBar,
                    Value = adRSmoothed
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
                case "WPR rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[] {-50};
                    break;

                case "WPR falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[] {-50};
                    break;

                case "WPR is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new double[] {level, -100 - level};
                    break;

                case "WPR is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new double[] {level, -100 - level};
                    break;

                case "WPR crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new double[] {level, -100 - level};
                    break;

                case "WPR crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new double[] {level, -100 - level};
                    break;

                case "WPR changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[] {-50};
                    break;

                case "WPR changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[] {-50};
                    break;
            }

            OscillatorLogic(firstBar, prvs, adRSmoothed, level, -100 - level, ref Component[1], ref Component[2],
                            indLogic);
        }

        public override void SetDescription()
        {
            string sLevelLong = IndParam.NumParam[2].ValueToString;
            string sLevelShort = IndParam.NumParam[2].AnotherValueToString(-100 - IndParam.NumParam[2].Value);

            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "WPR rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "WPR falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "WPR is higher than the Level line":
                    EntryFilterLongDescription += "is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is lower than the Level " + sLevelShort;
                    ExitFilterLongDescription += "is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription += "is lower than the Level " + sLevelShort;
                    break;

                case "WPR is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription += "is lower than the Level " + sLevelLong;
                    ExitFilterShortDescription += "is higher than the Level " + sLevelShort;
                    break;

                case "WPR crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + sLevelLong + " upward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription += "crosses the Level " + sLevelLong + " upward";
                    ExitFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    break;

                case "WPR crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + sLevelLong + " downward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription += "crosses the Level " + sLevelLong + " downward";
                    ExitFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    break;

                case "WPR changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "WPR changes its direction downward":
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
                   IndParam.ListParam[1].Text + ", " + // Smoothing method
                   IndParam.NumParam[0].ValueToString + ", " + // Period of %R
                   IndParam.NumParam[1].ValueToString + ")"; // Smoothing period
        }
    }
}