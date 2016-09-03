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
    public class MoneyFlowIndex : Indicator
    {
        public MoneyFlowIndex()
        {
            // General properties
            IndicatorName = "Money Flow Index";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = 0;
            SeparatedChartMaxValue = 100;

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
                    "MFI rises",
                    "MFI falls",
                    "MFI is higher than the Level line",
                    "MFI is lower than the Level line",
                    "MFI crosses the Level line upward",
                    "MFI crosses the Level line downward",
                    "MFI changes its direction upward",
                    "MFI changes its direction downward"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of MFI.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Money Flow Index.";

            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value = 50;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 100;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "A signal level.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var period = (int)IndParam.NumParam[0].Value;
            double level = IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            int firstBar = period + previous + 2;

            // Calculating Money Flow
            var moneyFlow = new double[Bars];
            for (int bar = 1; bar < Bars; bar++)
            {
                double average = (High[bar] + Low[bar] + Close[bar]) / 3;
                double average1 = (High[bar - 1] + Low[bar - 1] + Close[bar - 1]) / 3;
                if (average > average1)
                    moneyFlow[bar] = moneyFlow[bar - 1] + average * Volume[bar];
                else if (average < average1)
                    moneyFlow[bar] = moneyFlow[bar - 1] - average * Volume[bar];
                else
                    moneyFlow[bar] = moneyFlow[bar - 1];
            }

            // Calculating Money Flow Index
            var moneyFlowIndex = new double[Bars];
            for (int bar = period + 1; bar < Bars; bar++)
            {
                double pos = 0;
                double neg = 0;
                for (int index = 0; index < period; index++)
                {
                    if (moneyFlow[bar - index] > moneyFlow[bar - index - 1])
                        pos += moneyFlow[bar - index] - moneyFlow[bar - index - 1];
                    if (moneyFlow[bar - index] < moneyFlow[bar - index - 1])
                        neg += moneyFlow[bar - index - 1] - moneyFlow[bar - index];
                }

                if (Math.Abs(neg - 0) < Epsilon)
                    moneyFlowIndex[bar] = 100.0;
                else
                    moneyFlowIndex[bar] = 100.0 - (100.0 / (1.0 + (pos / neg)));
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
            {
                CompName = "Money Flow Index",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Color.Blue,
                FirstBar = firstBar,
                Value = moneyFlowIndex
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
            var logicRule = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "MFI rises":
                    logicRule = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[] { 50 };
                    break;

                case "MFI falls":
                    logicRule = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[] { 50 };
                    break;

                case "MFI is higher than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] { level, 100 - level };
                    break;

                case "MFI is lower than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] { level, 100 - level };
                    break;

                case "MFI crosses the Level line upward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] { level, 100 - level };
                    break;

                case "MFI crosses the Level line downward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] { level, 100 - level };
                    break;

                case "MFI changes its direction upward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[] { 50 };
                    break;

                case "MFI changes its direction downward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[] { 50 };
                    break;
            }

            OscillatorLogic(firstBar, previous, moneyFlowIndex, level, 100 - level, ref Component[1], ref Component[2], logicRule);
        }

        public override void SetDescription()
        {
            string levelLong = IndParam.NumParam[1].ValueToString;
            string levelShort = IndParam.NumParam[1].AnotherValueToString(100 - IndParam.NumParam[1].Value);

            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "MFI rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "MFI falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "MFI is higher than the Level line":
                    EntryFilterLongDescription += "is higher than the Level " + levelLong;
                    EntryFilterShortDescription += "is lower than the Level " + levelShort;
                    ExitFilterLongDescription += "is higher than the Level " + levelLong;
                    ExitFilterShortDescription += "is lower than the Level " + levelShort;
                    break;

                case "MFI is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + levelLong;
                    EntryFilterShortDescription += "is higher than the Level " + levelShort;
                    ExitFilterLongDescription += "is lower than the Level " + levelLong;
                    ExitFilterShortDescription += "is higher than the Level " + levelShort;
                    break;

                case "MFI crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    break;

                case "MFI crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    break;

                case "MFI changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "MFI changes its direction downward":
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
                   IndParam.NumParam[0].ValueToString + ")"; // MFI period
        }
    }
}