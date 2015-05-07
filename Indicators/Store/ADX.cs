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
    public class ADX : Indicator
    {
        public ADX()
        {
            IndicatorName = "ADX";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = 0;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.Indicator;
            IndParam.ExecutionTime = ExecutionTime.DuringTheBar;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "ADX rises",
                    "ADX falls",
                    "ADX is higher than the Level line",
                    "ADX is lower than the Level line",
                    "ADX crosses the Level line upward",
                    "ADX crosses the Level line downward",
                    "ADX changes its direction upward",
                    "ADX changes its direction downward"
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
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing the ADX value.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = new[] {"Bar range"};
            IndParam.ListParam[2].Index = 0;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "ADX uses current and previous bar ranges.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of ADX.";

            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 100;
            IndParam.NumParam[1].Point = 0;
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
            var maMethod = (MAMethod) IndParam.ListParam[1].Index;
            var period = (int) IndParam.NumParam[0].Value;
            double level = IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = 2*period + 2;

            var diPos = new double[Bars];
            var diNeg = new double[Bars];

            for (int bar = 1; bar < Bars; bar++)
            {
                double trueRange = Math.Max(High[bar], Close[bar - 1]) - Math.Min(Low[bar], Close[bar - 1]);

                if (trueRange < Point)
                    trueRange = Point;

                double deltaHigh = High[bar] - High[bar - 1];
                double deltaLow = Low[bar - 1] - Low[bar];

                if (deltaHigh > 0 && deltaHigh > deltaLow)
                    diPos[bar] = 100*deltaHigh/trueRange;
                else
                    diPos[bar] = 0;

                if (deltaLow > 0 && deltaLow > deltaHigh)
                    diNeg[bar] = 100*deltaLow/trueRange;
                else
                    diNeg[bar] = 0;
            }

            double[] adiPos = MovingAverage(period, 0, maMethod, diPos);
            double[] adiNeg = MovingAverage(period, 0, maMethod, diNeg);

            var dx = new double[Bars];

            for (int bar = 0; bar < Bars; bar++)
            {
                if (Math.Abs(adiPos[bar] - adiNeg[bar]) < Epsilon)
                    dx[bar] = 0;
                else
                    dx[bar] = 100*Math.Abs((adiPos[bar] - adiNeg[bar])/(adiPos[bar] + adiNeg[bar]));
            }

            double[] adx = MovingAverage(period, 0, maMethod, dx);

            // Saving the components
            Component = new IndicatorComp[5];

            Component[0] = new IndicatorComp
                {
                    CompName = "ADX",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = firstBar,
                    Value = adx
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "ADI+",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Green,
                    FirstBar = firstBar,
                    Value = adiPos
                };

            Component[2] = new IndicatorComp
                {
                    CompName = "ADI-",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Red,
                    FirstBar = firstBar,
                    Value = adiNeg
                };

            Component[3] = new IndicatorComp
                {ChartType = IndChartType.NoChart, FirstBar = firstBar, Value = new double[Bars]};

            Component[4] = new IndicatorComp
                {ChartType = IndChartType.NoChart, FirstBar = firstBar, Value = new double[Bars]};

            // Sets the Component's type
            switch (SlotType)
            {
                case SlotTypes.OpenFilter:
                    Component[3].DataType = IndComponentType.AllowOpenLong;
                    Component[3].CompName = "Is long entry allowed";
                    Component[4].DataType = IndComponentType.AllowOpenShort;
                    Component[4].CompName = "Is short entry allowed";
                    break;
                case SlotTypes.CloseFilter:
                    Component[3].DataType = IndComponentType.ForceCloseLong;
                    Component[3].CompName = "Close out long position";
                    Component[4].DataType = IndComponentType.ForceCloseShort;
                    Component[4].CompName = "Close out short position";
                    break;
            }

            // Calculation of the logic
            IndicatorLogic logicRule;

            switch (IndParam.ListParam[0].Text)
            {
                case "ADX rises":
                    logicRule = IndicatorLogic.The_indicator_rises;
                    break;

                case "ADX falls":
                    logicRule = IndicatorLogic.The_indicator_falls;
                    break;

                case "ADX is higher than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] {level};
                    break;

                case "ADX is lower than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] {level};
                    break;

                case "ADX crosses the Level line upward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] {level};
                    break;

                case "ADX crosses the Level line downward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] {level};
                    break;

                case "ADX changes its direction upward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "ADX changes its direction downward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;

                default:
                    logicRule = IndicatorLogic.It_does_not_act_as_a_filter;
                    break;
            }

            // ADX rises equal signals in both directions!
            NoDirectionOscillatorLogic(firstBar, previous, adx, level, ref Component[3], logicRule);
            Component[4].Value = Component[3].Value;
        }

        public override void SetDescription()
        {
            string levelLong = IndParam.NumParam[1].ValueToString;
            string levelShort = levelLong;

            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "ADX rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "rises";
                    break;

                case "ADX falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "falls";
                    break;

                case "ADX is higher than the Level line":
                    EntryFilterLongDescription += "is higher than the Level " + levelLong;
                    EntryFilterShortDescription += "is higher than the Level " + levelShort;
                    ExitFilterLongDescription += "is higher than the Level " + levelLong;
                    ExitFilterShortDescription += "is higher than the Level " + levelShort;
                    break;

                case "ADX is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + levelLong;
                    EntryFilterShortDescription += "is lower than the Level " + levelShort;
                    ExitFilterLongDescription += "is lower than the Level " + levelLong;
                    ExitFilterShortDescription += "is lower than the Level " + levelShort;
                    break;

                case "ADX crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " upward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " upward";
                    break;

                case "ADX crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    EntryFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    ExitFilterLongDescription += "crosses the Level " + levelLong + " downward";
                    ExitFilterShortDescription += "crosses the Level " + levelShort + " downward";
                    break;

                case "ADX changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;

                case "ADX changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}{1} ({2}, {3}, {4})",
                                 IndicatorName,
                                 (IndParam.CheckParam[0].Checked ? "*" : ""),
                                 IndParam.ListParam[1].Text,
                                 IndParam.ListParam[2].Text,
                                 IndParam.NumParam[0].ValueToString);
        }
    }
}