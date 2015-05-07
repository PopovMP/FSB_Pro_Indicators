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
            IndParam.NumParam[1].ToolTip = "A critical level (for the appropriate logic).";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var iPeriod = (int) IndParam.NumParam[0].Value;
            double dLevel = IndParam.NumParam[1].Value;
            int iPrvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            int iFirstBar = iPeriod + iPrvs;

            // Calculating Money Flow
            var adMf = new double[Bars];
            for (int iBar = 1; iBar < Bars; iBar++)
            {
                double dAvg = (High[iBar] + Low[iBar] + Close[iBar])/3;
                double dAvg1 = (High[iBar - 1] + Low[iBar - 1] + Close[iBar - 1])/3;
                if (dAvg > dAvg1)
                    adMf[iBar] = adMf[iBar - 1] + dAvg*Volume[iBar];
                else if (dAvg < dAvg1)
                    adMf[iBar] = adMf[iBar - 1] - dAvg*Volume[iBar];
                else
                    adMf[iBar] = adMf[iBar - 1];
            }

            // Calculating Money Flow Index
            var adMfi = new double[Bars];
            for (int iBar = iPeriod + 1; iBar < Bars; iBar++)
            {
                double dPmf = 0;
                double dNmf = 0;
                for (int index = 0; index < iPeriod; index++)
                {
                    if (adMf[iBar - index] > adMf[iBar - index - 1])
                        dPmf += adMf[iBar - index] - adMf[iBar - index - 1];
                    if (adMf[iBar - index] < adMf[iBar - index - 1])
                        dNmf += adMf[iBar - index - 1] - adMf[iBar - index];
                }
                if (Math.Abs(dNmf - 0) < Epsilon)
                    adMfi[iBar] = 100.0;
                else
                    adMfi[iBar] = 100.0 - (100.0/(1.0 + (dPmf/dNmf)));
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "Money Flow Index",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = iFirstBar,
                    Value = adMfi
                };

            Component[1] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = new double[Bars]
                };

            Component[2] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
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
                case "MFI rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[] {50};
                    break;

                case "MFI falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[] {50};
                    break;

                case "MFI is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] {dLevel, 100 - dLevel};
                    break;

                case "MFI is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] {dLevel, 100 - dLevel};
                    break;

                case "MFI crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] {dLevel, 100 - dLevel};
                    break;

                case "MFI crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] {dLevel, 100 - dLevel};
                    break;

                case "MFI changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[] {50};
                    break;

                case "MFI changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[] {50};
                    break;
            }

            OscillatorLogic(iFirstBar, iPrvs, adMfi, dLevel, 100 - dLevel, ref Component[1], ref Component[2], indLogic);
        }

        public override void SetDescription()
        {
            string sLevelLong = IndParam.NumParam[1].ValueToString;
            string sLevelShort = IndParam.NumParam[1].AnotherValueToString(100 - IndParam.NumParam[1].Value);

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
                    EntryFilterLongDescription += "is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is lower than the Level " + sLevelShort;
                    ExitFilterLongDescription += "is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription += "is lower than the Level " + sLevelShort;
                    break;

                case "MFI is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription += "is lower than the Level " + sLevelLong;
                    ExitFilterShortDescription += "is higher than the Level " + sLevelShort;
                    break;

                case "MFI crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + sLevelLong + " upward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription += "crosses the Level " + sLevelLong + " upward";
                    ExitFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    break;

                case "MFI crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + sLevelLong + " downward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription += "crosses the Level " + sLevelLong + " downward";
                    ExitFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
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