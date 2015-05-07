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
    public class AcceleratorOscillator : Indicator
    {
        public AcceleratorOscillator()
        {
            IndicatorName = "Accelerator Oscillator";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "AC rises",
                    "AC falls",
                    "AC is higher than the Level line",
                    "AC is lower than the Level line",
                    "AC crosses the Level line upward",
                    "AC crosses the Level line downward",
                    "AC changes its direction upward",
                    "AC changes its direction downward"
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
            IndParam.ListParam[1].ToolTip = "The method of Moving Average used for the calculations.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[2].Index = (int) BasePrice.Median;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the indicator is based on.";

            // NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Slow MA period";
            IndParam.NumParam[0].Value = 34;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Point = 0;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of slow Moving Average.";

            IndParam.NumParam[1].Caption = "Fast MA period";
            IndParam.NumParam[1].Value = 5;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Point = 0;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of fast Moving Average.";

            IndParam.NumParam[2].Caption = "Forming MA period";
            IndParam.NumParam[2].Value = 5;
            IndParam.NumParam[2].Min = 1;
            IndParam.NumParam[2].Max = 200;
            IndParam.NumParam[2].Point = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The period of additional Moving Average.";

            IndParam.NumParam[3].Caption = "Level";
            IndParam.NumParam[3].Value = 0;
            IndParam.NumParam[3].Min = -10;
            IndParam.NumParam[3].Max = 10;
            IndParam.NumParam[3].Point = 4;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "A critical level (for the appropriate logic).";

            // CheckBox parameters
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
            var slow = (int) IndParam.NumParam[0].Value;
            var fast = (int) IndParam.NumParam[1].Value;
            var accelerator = (int) IndParam.NumParam[2].Value;
            double level = IndParam.NumParam[3].Value;
            int prvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = slow + accelerator + 2;
            double[] maSlow = MovingAverage(slow, 0, maMethod, Price(basePrice));
            double[] maFast = MovingAverage(fast, 0, maMethod, Price(basePrice));
            var ao = new double[Bars];
            var ac = new double[Bars];

            for (int bar = slow - 1; bar < Bars; bar++)
            {
                ao[bar] = maFast[bar] - maSlow[bar];
            }

            double[] movingAverage = MovingAverage(accelerator, 0, maMethod, ao);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                ac[bar] = ao[bar] - movingAverage[bar];
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "AC",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Histogram,
                    FirstBar = firstBar,
                    Value = ac
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
            switch (SlotType)
            {
                case SlotTypes.OpenFilter:
                    Component[1].DataType = IndComponentType.AllowOpenLong;
                    Component[1].CompName = "Is long entry allowed";
                    Component[2].DataType = IndComponentType.AllowOpenShort;
                    Component[2].CompName = "Is short entry allowed";
                    break;

                case SlotTypes.CloseFilter:
                    Component[1].DataType = IndComponentType.ForceCloseLong;
                    Component[1].CompName = "Close out long position";
                    Component[2].DataType = IndComponentType.ForceCloseShort;
                    Component[2].CompName = "Close out short position";
                    break;
            }

            // Calculation of the signals
            var indicatorLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "AC rises":
                    indicatorLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[] {0};
                    break;

                case "AC falls":
                    indicatorLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[] {0};
                    break;

                case "AC is higher than the Level line":
                    indicatorLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] {level, -level};
                    break;

                case "AC is lower than the Level line":
                    indicatorLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] {level, -level};
                    break;

                case "AC crosses the Level line upward":
                    indicatorLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] {level, -level};
                    break;

                case "AC crosses the Level line downward":
                    indicatorLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] {level, -level};
                    break;

                case "AC changes its direction upward":
                    indicatorLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[] {0};
                    break;

                case "AC changes its direction downward":
                    indicatorLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[] {0};
                    break;
            }

            OscillatorLogic(firstBar, prvs, ac, level, -level, ref Component[1], ref Component[2], indicatorLogic);
        }

        public override void SetDescription()
        {
            bool isLevelZero = Math.Abs(IndParam.NumParam[3].Value - 0) < Epsilon;
            string levelLong = (isLevelZero ? "0" : IndParam.NumParam[3].ValueToString);
            string levelShort = (isLevelZero ? "0" : "-" + levelLong);

            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "AC rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "AC falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "AC is higher than the Level line":
                    EntryFilterLongDescription += "is higher than the Level " + levelLong;
                    EntryFilterShortDescription += "is lower than the Level " + levelShort;
                    ExitFilterLongDescription += "is higher than the Level " + levelLong;
                    ExitFilterShortDescription += "is lower than the Level " + levelShort;
                    break;

                case "AC is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + levelLong;
                    EntryFilterShortDescription += "is higher than the Level " + levelShort;
                    ExitFilterLongDescription += "is lower than the Level " + levelLong;
                    ExitFilterShortDescription += "is higher than the Level " + levelShort;
                    break;

                case "AC crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    break;

                case "AC crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    break;

                case "AC changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "AC changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}{1} ({2}, {3}, {4}, {5}, {6})",
                                 IndicatorName,
                                 (IndParam.CheckParam[0].Checked ? "*" : ""),
                                 IndParam.ListParam[1].Text,
                                 IndParam.ListParam[2].Text,
                                 IndParam.NumParam[0].ValueToString,
                                 IndParam.NumParam[1].ValueToString,
                                 IndParam.NumParam[2].ValueToString);
        }
    }
}