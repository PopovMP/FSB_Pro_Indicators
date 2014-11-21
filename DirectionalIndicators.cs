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
    public class DirectionalIndicators : Indicator
    {
        public DirectionalIndicators()
        {
            IndicatorName = "Directional Indicators";
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

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "ADI+ rises",
                    "ADI+ falls",
                    "ADI- rises",
                    "ADI- falls",
                    "ADI+ is higher than ADI-",
                    "ADI+ is lower than ADI-",
                    "ADI+ crosses ADI- line upward",
                    "ADI+ crosses ADI- line downward",
                    "ADI+ changes its direction upward",
                    "ADI+ changes its direction downward",
                    "ADI- changes its direction upward",
                    "ADI- changes its direction downward"
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
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for ADI smoothing.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = new[] {"Bar range"};
            IndParam.ListParam[2].Index = 0;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "ADI uses the current bar range.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of ADI.";

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
            int prev = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period + 2;

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

            var adiOsc = new double[Bars];

            for (int bar = 0; bar < Bars; bar++)
                adiOsc[bar] = adiPos[bar] - adiNeg[bar];

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
                {
                    CompName = "ADI+",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Green,
                    FirstBar = firstBar,
                    Value = adiPos
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "ADI-",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Red,
                    FirstBar = firstBar,
                    Value = adiNeg
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
                case "ADI+ rises":
                    OscillatorLogic(firstBar, prev, adiPos, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_rises);
                    break;

                case "ADI+ falls":
                    OscillatorLogic(firstBar, prev, adiPos, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_falls);
                    break;

                case "ADI- rises":
                    OscillatorLogic(firstBar, prev, adiNeg, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_rises);
                    break;

                case "ADI- falls":
                    OscillatorLogic(firstBar, prev, adiNeg, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_falls);
                    break;

                case "ADI+ is higher than ADI-":
                    OscillatorLogic(firstBar, prev, adiOsc, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_is_higher_than_the_level_line);
                    break;

                case "ADI+ is lower than ADI-":
                    OscillatorLogic(firstBar, prev, adiOsc, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_is_lower_than_the_level_line);
                    break;

                case "ADI+ crosses ADI- line upward":
                    OscillatorLogic(firstBar, prev, adiOsc, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_upward);
                    break;

                case "ADI+ crosses ADI- line downward":
                    OscillatorLogic(firstBar, prev, adiOsc, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_crosses_the_level_line_downward);
                    break;

                case "ADI+ changes its direction upward":
                    OscillatorLogic(firstBar, prev, adiPos, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_changes_its_direction_upward);
                    break;

                case "ADI+ changes its direction downward":
                    OscillatorLogic(firstBar, prev, adiPos, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_changes_its_direction_downward);
                    break;

                case "ADI- changes its direction upward":
                    OscillatorLogic(firstBar, prev, adiNeg, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_changes_its_direction_upward);
                    break;

                case "ADI- changes its direction downward":
                    OscillatorLogic(firstBar, prev, adiNeg, 0, 0, ref Component[2], ref Component[3],
                                    IndicatorLogic.The_indicator_changes_its_direction_downward);
                    break;
            }
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + "; ";
            EntryFilterShortDescription = ToString() + "; ";
            ExitFilterLongDescription = ToString() + "; ";
            ExitFilterShortDescription = ToString() + "; ";

            switch (IndParam.ListParam[0].Text)
            {
                case "ADI+ rises":
                    EntryFilterLongDescription += "ADI+ rises";
                    EntryFilterShortDescription += "ADI+ falls";
                    ExitFilterLongDescription += "ADI+ rises";
                    ExitFilterShortDescription += "ADI+ falls";
                    break;

                case "ADI+ falls":
                    EntryFilterLongDescription += "ADI+ falls";
                    EntryFilterShortDescription += "ADI+ rises";
                    ExitFilterLongDescription += "ADI+ falls";
                    ExitFilterShortDescription += "ADI+ rises";
                    break;

                case "ADI- rises":
                    EntryFilterLongDescription += "ADI- rises";
                    EntryFilterShortDescription += "ADI- falls";
                    ExitFilterLongDescription += "ADI- rises";
                    ExitFilterShortDescription += "ADI- falls";
                    break;

                case "ADI- falls":
                    EntryFilterLongDescription += "ADI- falls";
                    EntryFilterShortDescription += "ADI- rises";
                    ExitFilterLongDescription += "ADI- falls";
                    ExitFilterShortDescription += "ADI- rises";
                    break;

                case "ADI+ is higher than ADI-":
                    EntryFilterLongDescription += "ADI+ is higher than ADI-";
                    EntryFilterShortDescription += "ADI+ is lower than ADI-";
                    ExitFilterLongDescription += "ADI+ is higher than ADI-";
                    ExitFilterShortDescription += "ADI+ is lower than ADI-";
                    break;

                case "ADI+ is lower than ADI-":
                    EntryFilterLongDescription += "ADI+ is lower than ADI-";
                    EntryFilterShortDescription += "ADI+ is higher than ADI-";
                    ExitFilterLongDescription += "ADI+ is lower than ADI-";
                    ExitFilterShortDescription += "ADI+ is higher than ADI-";
                    break;

                case "ADI+ crosses ADI- line upward":
                    EntryFilterLongDescription += "ADI+ crosses ADI- line upward";
                    EntryFilterShortDescription += "ADI+ crosses ADI- line downward";
                    ExitFilterLongDescription += "ADI+ crosses ADI- line upward";
                    ExitFilterShortDescription += "ADI+ crosses ADI- line downward";
                    break;

                case "ADI+ crosses ADI- line downward":
                    EntryFilterLongDescription += "ADI+ crosses ADI- line downward";
                    EntryFilterShortDescription += "ADI+ crosses ADI- line upward";
                    ExitFilterLongDescription += "ADI+ crosses ADI- line downward";
                    ExitFilterShortDescription += "ADI+ crosses ADI- line upward";
                    break;

                case "ADI+ changes its direction upward":
                    EntryFilterLongDescription += "ADI+ changes its direction upward";
                    EntryFilterShortDescription += "ADI+ changes its direction downward";
                    ExitFilterLongDescription += "ADI+ changes its direction upward";
                    ExitFilterShortDescription += "ADI+ changes its direction downward";
                    break;

                case "ADI+ changes its direction downward":
                    EntryFilterLongDescription += "ADI+ changes its direction downward";
                    EntryFilterShortDescription += "ADI+ changes its direction upward";
                    ExitFilterLongDescription += "ADI+ changes its direction downward";
                    ExitFilterShortDescription += "ADI+ changes its direction upward";
                    break;

                case "ADI- changes its direction upward":
                    EntryFilterLongDescription += "ADI- changes its direction upward";
                    EntryFilterShortDescription += "ADI- changes its direction downward";
                    ExitFilterLongDescription += "ADI- changes its direction upward";
                    ExitFilterShortDescription += "ADI- changes its direction downward";
                    break;

                case "ADI- changes its direction downward":
                    EntryFilterLongDescription += "ADI- changes its direction downward";
                    EntryFilterShortDescription += "ADI- changes its direction upward";
                    ExitFilterLongDescription += "ADI- changes its direction downward";
                    ExitFilterShortDescription += "ADI- changes its direction upward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                    (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                    IndParam.ListParam[1].Text + ", " + // Method
                    IndParam.ListParam[2].Text + ", " + // Base price
                    IndParam.NumParam[0].ValueToString + ")"; // Period
        }
    }
}