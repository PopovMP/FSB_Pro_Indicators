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
    public class RelativeVigorIndex : Indicator
    {
        public RelativeVigorIndex()
        {
            IndicatorName = "Relative Vigor Index";
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
                    "RVI line rises",
                    "RVI line falls",
                    "RVI line is higher than zero",
                    "RVI line is lower than zero",
                    "RVI line crosses the zero line upward",
                    "RVI line crosses the zero line downward",
                    "RVI line changes its direction upward",
                    "RVI line changes its direction downward",
                    "RVI line crosses the Signal line upward",
                    "RVI line crosses the Signal line downward",
                    "RVI line is higher than the Signal line",
                    "RVI line is lower than the Signal line"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "RVI period";
            IndParam.NumParam[0].Value = 10;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Slow MA.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var period = (int) IndParam.NumParam[0].Value;
            int prvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period + 4;

            var adRvi = new double[Bars];
            for (int bar = period + 3; bar < Bars; bar++)
            {
                double dNum = 0;
                double dDeNum = 0;
                for (int j = bar; j > bar - period; j--)
                {
                    double dValueUp = ((Close[j] - Open[j]) + 2*(Close[j - 1] - Open[j - 1]) +
                                       2*(Close[j - 2] - Open[j - 2]) + (Close[j - 3] - Open[j - 3]))/6;
                    double dValueDown = ((High[j] - Low[j]) + 2*(High[j - 1] - Low[j - 1]) +
                                         2*(High[j - 2] - Low[j - 2]) + (High[j - 3] - Low[j - 3]))/6;
                    dNum += dValueUp;
                    dDeNum += dValueDown;
                }
                if (Math.Abs(dDeNum - 0) > Epsilon)
                    adRvi[bar] = dNum/dDeNum;
                else
                    adRvi[bar] = dNum;
            }

            var adMASignal = new double[Bars];
            for (int iBar = 4; iBar < Bars; iBar++)
                adMASignal[iBar] = (adRvi[iBar] + 2*adRvi[iBar - 1] + 2*adRvi[iBar - 2] + adRvi[iBar - 3])/6;

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
                {
                    CompName = "RVI Line",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Green,
                    FirstBar = firstBar,
                    Value = adRvi
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Signal line",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Red,
                    FirstBar = firstBar,
                    Value = adMASignal
                };

            Component[2] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            Component[3] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "RVI line rises":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_rises);
                    break;

                case "RVI line falls":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_falls);
                    break;

                case "RVI line is higher than zero":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_is_higher_than_the_level_line);
                    break;

                case "RVI line is lower than zero":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_is_lower_than_the_level_line);
                    break;

                case "RVI line crosses the zero line upward":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_upward);
                    break;

                case "RVI line crosses the zero line downward":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_downward);
                    break;

                case "RVI line changes its direction upward":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_changes_its_direction_upward);
                    break;

                case "RVI line changes its direction downward":
                    OscillatorLogic(firstBar, prvs, adRvi, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_changes_its_direction_downward);
                    break;

                case "RVI line crosses the Signal line upward":
                    IndicatorCrossesAnotherIndicatorUpwardLogic(firstBar, prvs, adRvi, adMASignal, ref Component[2],
                                                                ref Component[3]);
                    break;

                case "RVI line crosses the Signal line downward":
                    IndicatorCrossesAnotherIndicatorDownwardLogic(firstBar, prvs, adRvi, adMASignal, ref Component[2],
                                                                  ref Component[3]);
                    break;

                case "RVI line is higher than the Signal line":
                    IndicatorIsHigherThanAnotherIndicatorLogic(firstBar, prvs, adRvi, adMASignal, ref Component[2],
                                                               ref Component[3]);
                    break;

                case "RVI line is lower than the Signal line":
                    IndicatorIsLowerThanAnotherIndicatorLogic(firstBar, prvs, adRvi, adMASignal, ref Component[2],
                                                              ref Component[3]);
                    break;
            }
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + "; RVI line ";
            EntryFilterShortDescription = ToString() + "; RVI line ";
            ExitFilterLongDescription = ToString() + "; RVI line ";
            ExitFilterShortDescription = ToString() + "; RVI line ";

            switch (IndParam.ListParam[0].Text)
            {
                case "RVI line rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "RVI line falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "RVI line is higher than zero":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "RVI line is lower than zero":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "RVI line crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "RVI line crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "RVI line changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "RVI line changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;

                case "RVI line is higher than the Signal line":
                    EntryFilterLongDescription += "is higher than the Signal line";
                    EntryFilterShortDescription += "is lower than the Signal line";
                    ExitFilterLongDescription += "is higher than the Signal line";
                    ExitFilterShortDescription += "is lower than the Signal line";
                    break;

                case "RVI line is lower than the Signal line":
                    EntryFilterLongDescription += "is lower than the Signal line";
                    EntryFilterShortDescription += "is higher than the Signal line";
                    ExitFilterLongDescription += "is lower than the Signal line";
                    ExitFilterShortDescription += "is higher than the Signal line";
                    break;

                case "RVI line crosses the Signal line upward":
                    EntryFilterLongDescription += "crosses the Signal line upward";
                    EntryFilterShortDescription += "crosses the Signal line downward";
                    ExitFilterLongDescription += "crosses the Signal line upward";
                    ExitFilterShortDescription += "crosses the Signal line downward";
                    break;

                case "RVI line crosses the Signal line downward":
                    EntryFilterLongDescription += "crosses the Signal line downward";
                    EntryFilterShortDescription += "crosses the Signal line upward";
                    ExitFilterLongDescription += "crosses the Signal line downward";
                    ExitFilterShortDescription += "crosses the Signal line upward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.NumParam[0].ValueToString + ")"; // RVI period
        }
    }
}