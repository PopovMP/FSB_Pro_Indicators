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
            IndicatorName  = "Stochastics";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
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
            IndParam.ListParam[0].Caption  = "Logic";
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
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of indicator.";

            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The MA method used for smoothing.";

            IndParam.NumParam[0].Caption = "%K period";
            IndParam.NumParam[0].Value   = 5;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "the smoothing period of %K.";

            IndParam.NumParam[1].Caption = "Fast %D period";
            IndParam.NumParam[1].Value   = 3;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The smoothing period of Fast %D.";

            IndParam.NumParam[2].Caption = "Slow %D period";
            IndParam.NumParam[2].Value   = 3;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The smoothing period of Slow %D.";

            IndParam.NumParam[3].Caption = "Level";
            IndParam.NumParam[3].Value   = 20;
            IndParam.NumParam[3].Min     = 0;
            IndParam.NumParam[3].Max     = 100;
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
            int iK     = (int)IndParam.NumParam[0].Value;
            int iDFast = (int)IndParam.NumParam[1].Value;
            int iDSlow = (int)IndParam.NumParam[2].Value;
            int iLevel = (int)IndParam.NumParam[3].Value;
            int iPrvs  = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int iFirstBar = iK + iDFast + iDSlow + 3;

            double[] adHighs = new double[Bars];
            double[] adLows  = new double[Bars];
            for (int iBar = 0; iBar < iK; iBar++)
            {
                double dMin = double.MaxValue;
                double dMax = double.MinValue;
                for (int i = 0; i < iBar; i++)
                {
                    if (High[iBar - i] > dMax) dMax = High[iBar - i];
                    if (Low[iBar  - i] < dMin) dMin = Low[iBar  - i];
                }
                adHighs[iBar] = dMax;
                adLows[iBar]  = dMin;
            }
            adHighs[0] = High[0];
            adLows[0]  = Low[0];

            for (int iBar = iK; iBar < Bars; iBar++)
            {
                double dMin = double.MaxValue;
                double dMax = double.MinValue;
                for (int i = 0; i < iK; i++)
                {
                    if (High[iBar - i] > dMax) dMax = High[iBar - i];
                    if (Low[iBar  - i] < dMin) dMin = Low[iBar  - i];
                }
                adHighs[iBar] = dMax;
                adLows[iBar]  = dMin;
            }

            double[] adK = new double[Bars];
            for (int iBar = iK; iBar < Bars; iBar++)
            {
                if (adHighs[iBar] == adLows[iBar])
                    adK[iBar] = 50;
                else
                    adK[iBar] = 100 * (Close[iBar] - adLows[iBar]) / (adHighs[iBar] - adLows[iBar]);
            }

            double[] adDFast = new double[Bars];
            for (int iBar = iDFast; iBar < Bars; iBar++)
            {
                double dSumHigh = 0;
                double dSumLow  = 0;
                for (int i = 0; i < iDFast; i++)
                {
                    dSumLow  += Close[iBar - i]   - adLows[iBar - i];
                    dSumHigh += adHighs[iBar - i] - adLows[iBar - i];
                }
                if (dSumHigh == 0)
                    adDFast[iBar] = 100;
                else
                    adDFast[iBar] = 100 * dSumLow / dSumHigh;
            }

            double[] adDSlow = MovingAverage(iDSlow, 0, maMethod, adDFast);

            // Saving components
            Component = new IndicatorComp[5];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "%K";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Brown;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adK;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Fast %D";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Gold;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adDFast;

            Component[2] = new IndicatorComp();
            Component[2].CompName   = "Slow %D";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Blue;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = adDSlow;

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = iFirstBar;
            Component[3].Value     = new double[Bars];

            Component[4] = new IndicatorComp();
            Component[4].ChartType = IndChartType.NoChart;
            Component[4].FirstBar  = iFirstBar;
            Component[4].Value     = new double[Bars];

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
            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            if (IndParam.ListParam[0].Text == "%K crosses Slow %D upward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorUpwardLogic(iFirstBar, iPrvs,adK, adDSlow, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "%K crosses Slow %D downward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorDownwardLogic(iFirstBar, iPrvs, adK, adDSlow, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "%K is higher than Slow %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsHigherThanAnotherIndicatorLogic(iFirstBar, iPrvs, adK, adDSlow, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "%K is lower than Slow %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsLowerThanAnotherIndicatorLogic(iFirstBar, iPrvs, adK, adDSlow, ref Component[3], ref Component[4]);
                return;
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "Slow %D rises":
                        indLogic = IndicatorLogic.The_indicator_rises;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "Slow %D falls":
                        indLogic = IndicatorLogic.The_indicator_falls;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "Slow %D is higher than Level line":
                        indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "Slow %D is lower than Level line":
                        indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "Slow %D crosses Level line upward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "Slow %D crosses Level line downward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "Slow %D changes its direction upward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "Slow %D changes its direction downward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                        SpecialValues = new double[1] { 50 };
                        break;
                }

                OscillatorLogic(iFirstBar, iPrvs, adDSlow, iLevel, 100 - iLevel, ref Component[3], ref Component[4], indLogic);
            }
        }

        public override void SetDescription()
        {
            string sLevelLong  = IndParam.NumParam[3].ValueToString;
            string sLevelShort = IndParam.NumParam[3].AnotherValueToString(100 - IndParam.NumParam[3].Value);

            EntryFilterLongDescription  = ToString() + " - ";
            EntryFilterShortDescription = ToString() + " - ";
            ExitFilterLongDescription   = ToString() + " - ";
            ExitFilterShortDescription  = ToString() + " - ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Slow %D rises":
                    EntryFilterLongDescription  += "Slow %D rises";
                    EntryFilterShortDescription += "Slow %D falls";
                    ExitFilterLongDescription   += "Slow %D rises";
                    ExitFilterShortDescription  += "Slow %D falls";
                    break;

                case "Slow %D falls":
                    EntryFilterLongDescription  += "Slow %D falls";
                    EntryFilterShortDescription += "Slow %D rises";
                    ExitFilterLongDescription   += "Slow %D falls";
                    ExitFilterShortDescription  += "Slow %D rises";
                    break;

                case "Slow %D is higher than Level line":
                    EntryFilterLongDescription  += "Slow %D is higher than Level " + sLevelLong;
                    EntryFilterShortDescription += "Slow %D is lower than Level "  + sLevelShort;
                    ExitFilterLongDescription   += "Slow %D is higher than Level " + sLevelLong;
                    ExitFilterShortDescription  += "Slow %D is lower than Level "  + sLevelShort;
                    break;

                case "Slow %D is lower than Level line":
                    EntryFilterLongDescription  += "Slow %D is lower than Level "  + sLevelLong;
                    EntryFilterShortDescription += "Slow %D is higher than Level " + sLevelShort;
                    ExitFilterLongDescription   += "Slow %D is lower than Level "  + sLevelLong;
                    ExitFilterShortDescription  += "Slow %D is higher than Level " + sLevelShort;
                    break;

                case "Slow %D crosses Level line upward":
                    EntryFilterLongDescription  += "Slow %D crosses Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "Slow %D crosses Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "Slow %D crosses Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "Slow %D crosses Level " + sLevelShort + " downward";
                    break;

                case "Slow %D crosses Level line downward":
                    EntryFilterLongDescription  += "Slow %D crosses Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "Slow %D crosses Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "Slow %D crosses Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "Slow %D crosses Level " + sLevelShort + " upward";
                    break;

                case "%K crosses Slow %D upward":
                    EntryFilterLongDescription  += "%K crosses Slow %D upward";
                    EntryFilterShortDescription += "%K crosses Slow %D downward";
                    ExitFilterLongDescription   += "%K crosses Slow %D upward";
                    ExitFilterShortDescription  += "%K crosses Slow %D downward";
                    break;

                case "%K crosses Slow %D downward":
                    EntryFilterLongDescription  += "%K crosses Slow %D downward";
                    EntryFilterShortDescription += "%K crosses Slow %D upward";
                    ExitFilterLongDescription   += "%K crosses Slow %D downward";
                    ExitFilterShortDescription  += "%K crosses Slow %D upward";
                    break;

                case "%K is higher than Slow %D":
                    EntryFilterLongDescription  += "%K is higher than Slow %D";
                    EntryFilterShortDescription += "%K is lower than Slow %D";
                    ExitFilterLongDescription   += "%K is higher than Slow %D";
                    ExitFilterShortDescription  += "%K is lower than Slow %D";
                    break;

                case "%K is lower than  Slow %D":
                    EntryFilterLongDescription  += "%K is lower than Slow %D";
                    EntryFilterShortDescription += "%K is higher than than Slow %D";
                    ExitFilterLongDescription   += "%K is lower than Slow %D";
                    ExitFilterShortDescription  += "%K is higher than than Slow %D";
                    break;

                case "Slow %D changes its direction upward":
                    EntryFilterLongDescription  += "Slow %D changes its direction upward";
                    EntryFilterShortDescription += "Slow %D changes its direction downward";
                    ExitFilterLongDescription   += "Slow %D changes its direction upward";
                    ExitFilterShortDescription  += "Slow %D changes its direction downward";
                    break;

                case "Slow %D changes its direction downward":
                    EntryFilterLongDescription  += "Slow %D changes its direction downward";
                    EntryFilterShortDescription += "Slow %D changes its direction upward";
                    ExitFilterLongDescription   += "Slow %D changes its direction downward";
                    ExitFilterShortDescription  += "Slow %D changes its direction upward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[1].Text         + ", " + // Smoothing method
                IndParam.NumParam[0].ValueToString + ", " + // %K period
                IndParam.NumParam[1].ValueToString + ", " + // Fast %D period
                IndParam.NumParam[2].ValueToString + ")";   // Slow %D period
        }
    }
}
