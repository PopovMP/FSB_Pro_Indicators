//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class Stochastics : Indicator
    {
        public Stochastics()
        {
            IndicatorName = "Stochastics";
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

            // ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "Slow %D rises",
                "Slow %D falls",
                "Slow %D is higher than Level line",
                "Slow %D is lower than Level line",
                "Slow %D crosses Level line upward",
                "Slow %D crosses Level line downward",
                "Slow %D changes its direction upward",
                "Slow %D changes its direction downward",
                "%K is higher than Slow %D",
                "%K is lower than Slow %D",
                "%K crosses Slow %D upward",
                "%K crosses Slow %D downward",
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of indicator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index = (int)MAMethod.Simple;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The MA method used for smoothing.";

            IndParam.NumParam[0].Caption = "%K period";
            IndParam.NumParam[0].Value = 5;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "the smoothing period of %K.";

            IndParam.NumParam[1].Caption = "Fast %D period";
            IndParam.NumParam[1].Value = 3;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The smoothing period of Fast %D.";

            IndParam.NumParam[2].Caption = "Slow %D period";
            IndParam.NumParam[2].Value = 3;
            IndParam.NumParam[2].Min = 1;
            IndParam.NumParam[2].Max = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The smoothing period of Slow %D.";

            IndParam.NumParam[3].Caption = "Level";
            IndParam.NumParam[3].Value = 20;
            IndParam.NumParam[3].Min = 0;
            IndParam.NumParam[3].Max = 100;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "A critical level (for appropriate logic).";

            // CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use indicator value from previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading parameters
            var maMethod = (MAMethod)IndParam.ListParam[1].Index;
            int periodK = (int)IndParam.NumParam[0].Value;
            int periodDFast = (int)IndParam.NumParam[1].Value;
            int periodDSlow = (int)IndParam.NumParam[2].Value;
            int level = (int)IndParam.NumParam[3].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = periodK + periodDFast + periodDSlow + 3;

            double[] highs = new double[Bars];
            double[] lows = new double[Bars];
            for (int bar = 0; bar < periodK; bar++)
            {
                double min = double.MaxValue;
                double max = double.MinValue;
                for (int i = 0; i < bar; i++)
                {
                    if (High[bar - i] > max) max = High[bar - i];
                    if (Low[bar - i] < min) min = Low[bar - i];
                }
                highs[bar] = max;
                lows[bar] = min;
            }
            highs[0] = High[0];
            lows[0] = Low[0];

            for (int bar = periodK; bar < Bars; bar++)
            {
                double min = double.MaxValue;
                double max = double.MinValue;
                for (int i = 0; i < periodK; i++)
                {
                    if (High[bar - i] > max) max = High[bar - i];
                    if (Low[bar - i] < min) min = Low[bar - i];
                }
                highs[bar] = max;
                lows[bar] = min;
            }

            double[] adK = new double[Bars];
            for (int bar = periodK; bar < Bars; bar++)
            {
                if (highs[bar] == lows[bar])
                    adK[bar] = 50;
                else
                    adK[bar] = 100 * (Close[bar] - lows[bar]) / (highs[bar] - lows[bar]);
            }

            double[] fastD = new double[Bars];
            for (int bar = periodDFast; bar < Bars; bar++)
            {
                double sumHigh = 0;
                double sumLow = 0;
                for (int i = 0; i < periodDFast; i++)
                {
                    sumLow += Close[bar - i] - lows[bar - i];
                    sumHigh += highs[bar - i] - lows[bar - i];
                }
                if (sumHigh == 0)
                    fastD[bar] = 100;
                else
                    fastD[bar] = 100 * sumLow / sumHigh;
            }

            double[] slowD = MovingAverage(periodDSlow, 0, maMethod, fastD);

            // Saving components
            Component = new IndicatorComp[5];

            Component[0] = new IndicatorComp();
            Component[0].CompName = "%K";
            Component[0].DataType = IndComponentType.IndicatorValue;
            Component[0].ChartType = IndChartType.Line;
            Component[0].ChartColor = Color.Brown;
            Component[0].FirstBar = firstBar;
            Component[0].Value = adK;

            Component[1] = new IndicatorComp();
            Component[1].CompName = "Fast %D";
            Component[1].DataType = IndComponentType.IndicatorValue;
            Component[1].ChartType = IndChartType.Line;
            Component[1].ChartColor = Color.Gold;
            Component[1].FirstBar = firstBar;
            Component[1].Value = fastD;

            Component[2] = new IndicatorComp();
            Component[2].CompName = "Slow %D";
            Component[2].DataType = IndComponentType.IndicatorValue;
            Component[2].ChartType = IndChartType.Line;
            Component[2].ChartColor = Color.Blue;
            Component[2].FirstBar = firstBar;
            Component[2].Value = slowD;

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar = firstBar;
            Component[3].Value = new double[Bars];

            Component[4] = new IndicatorComp();
            Component[4].ChartType = IndChartType.NoChart;
            Component[4].FirstBar = firstBar;
            Component[4].Value = new double[Bars];

            // Sets Component's type
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

            // Calculation of logic
            IndicatorLogic logicRule = IndicatorLogic.It_does_not_act_as_a_filter;

            if (IndParam.ListParam[0].Text == "%K crosses Slow %D upward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorUpwardLogic(firstBar, previous, adK, slowD, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "%K crosses Slow %D downward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorDownwardLogic(firstBar, previous, adK, slowD, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "%K is higher than Slow %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsHigherThanAnotherIndicatorLogic(firstBar, previous, adK, slowD, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "%K is lower than Slow %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsLowerThanAnotherIndicatorLogic(firstBar, previous, adK, slowD, ref Component[3], ref Component[4]);
                return;
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "Slow %D rises":
                        logicRule = IndicatorLogic.The_indicator_rises;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "Slow %D falls":
                        logicRule = IndicatorLogic.The_indicator_falls;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "Slow %D is higher than Level line":
                        logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                        SpecialValues = new double[2] { level, 100 - level };
                        break;

                    case "Slow %D is lower than Level line":
                        logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                        SpecialValues = new double[2] { level, 100 - level };
                        break;

                    case "Slow %D crosses Level line upward":
                        logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                        SpecialValues = new double[2] { level, 100 - level };
                        break;

                    case "Slow %D crosses Level line downward":
                        logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                        SpecialValues = new double[2] { level, 100 - level };
                        break;

                    case "Slow %D changes its direction upward":
                        logicRule = IndicatorLogic.The_indicator_changes_its_direction_upward;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "Slow %D changes its direction downward":
                        logicRule = IndicatorLogic.The_indicator_changes_its_direction_downward;
                        SpecialValues = new double[1] { 50 };
                        break;
                }

                OscillatorLogic(firstBar, previous, slowD, level, 100 - level, ref Component[3], ref Component[4], logicRule);
            }
        }

        public override void SetDescription()
        {
            string levelLong = IndParam.NumParam[3].ValueToString;
            string levelShort = IndParam.NumParam[3].AnotherValueToString(100 - IndParam.NumParam[3].Value);

            EntryFilterLongDescription = ToString() + " - ";
            EntryFilterShortDescription = ToString() + " - ";
            ExitFilterLongDescription = ToString() + " - ";
            ExitFilterShortDescription = ToString() + " - ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Slow %D rises":
                    EntryFilterLongDescription += "Slow %D rises";
                    EntryFilterShortDescription += "Slow %D falls";
                    ExitFilterLongDescription += "Slow %D rises";
                    ExitFilterShortDescription += "Slow %D falls";
                    break;

                case "Slow %D falls":
                    EntryFilterLongDescription += "Slow %D falls";
                    EntryFilterShortDescription += "Slow %D rises";
                    ExitFilterLongDescription += "Slow %D falls";
                    ExitFilterShortDescription += "Slow %D rises";
                    break;

                case "Slow %D is higher than Level line":
                    EntryFilterLongDescription += "Slow %D is higher than Level " + levelLong;
                    EntryFilterShortDescription += "Slow %D is lower than Level " + levelShort;
                    ExitFilterLongDescription += "Slow %D is higher than Level " + levelLong;
                    ExitFilterShortDescription += "Slow %D is lower than Level " + levelShort;
                    break;

                case "Slow %D is lower than Level line":
                    EntryFilterLongDescription += "Slow %D is lower than Level " + levelLong;
                    EntryFilterShortDescription += "Slow %D is higher than Level " + levelShort;
                    ExitFilterLongDescription += "Slow %D is lower than Level " + levelLong;
                    ExitFilterShortDescription += "Slow %D is higher than Level " + levelShort;
                    break;

                case "Slow %D crosses Level line upward":
                    EntryFilterLongDescription += "Slow %D crosses Level " + levelLong + " upward";
                    EntryFilterShortDescription += "Slow %D crosses Level " + levelShort + " downward";
                    ExitFilterLongDescription += "Slow %D crosses Level " + levelLong + " upward";
                    ExitFilterShortDescription += "Slow %D crosses Level " + levelShort + " downward";
                    break;

                case "Slow %D crosses Level line downward":
                    EntryFilterLongDescription += "Slow %D crosses Level " + levelLong + " downward";
                    EntryFilterShortDescription += "Slow %D crosses Level " + levelShort + " upward";
                    ExitFilterLongDescription += "Slow %D crosses Level " + levelLong + " downward";
                    ExitFilterShortDescription += "Slow %D crosses Level " + levelShort + " upward";
                    break;

                case "%K crosses Slow %D upward":
                    EntryFilterLongDescription += "%K crosses Slow %D upward";
                    EntryFilterShortDescription += "%K crosses Slow %D downward";
                    ExitFilterLongDescription += "%K crosses Slow %D upward";
                    ExitFilterShortDescription += "%K crosses Slow %D downward";
                    break;

                case "%K crosses Slow %D downward":
                    EntryFilterLongDescription += "%K crosses Slow %D downward";
                    EntryFilterShortDescription += "%K crosses Slow %D upward";
                    ExitFilterLongDescription += "%K crosses Slow %D downward";
                    ExitFilterShortDescription += "%K crosses Slow %D upward";
                    break;

                case "%K is higher than Slow %D":
                    EntryFilterLongDescription += "%K is higher than Slow %D";
                    EntryFilterShortDescription += "%K is lower than Slow %D";
                    ExitFilterLongDescription += "%K is higher than Slow %D";
                    ExitFilterShortDescription += "%K is lower than Slow %D";
                    break;

                case "%K is lower than  Slow %D":
                    EntryFilterLongDescription += "%K is lower than Slow %D";
                    EntryFilterShortDescription += "%K is higher than than Slow %D";
                    ExitFilterLongDescription += "%K is lower than Slow %D";
                    ExitFilterShortDescription += "%K is higher than than Slow %D";
                    break;

                case "Slow %D changes its direction upward":
                    EntryFilterLongDescription += "Slow %D changes its direction upward";
                    EntryFilterShortDescription += "Slow %D changes its direction downward";
                    ExitFilterLongDescription += "Slow %D changes its direction upward";
                    ExitFilterShortDescription += "Slow %D changes its direction downward";
                    break;

                case "Slow %D changes its direction downward":
                    EntryFilterLongDescription += "Slow %D changes its direction downward";
                    EntryFilterShortDescription += "Slow %D changes its direction upward";
                    ExitFilterLongDescription += "Slow %D changes its direction downward";
                    ExitFilterShortDescription += "Slow %D changes its direction upward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[1].Text + ", " + // Smoothing method
                IndParam.NumParam[0].ValueToString + ", " + // %K period
                IndParam.NumParam[1].ValueToString + ", " + // Fast %D period
                IndParam.NumParam[2].ValueToString + ")";   // Slow %D period
        }
    }
}
