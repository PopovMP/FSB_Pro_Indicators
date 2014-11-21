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
    public class RateOfChange : Indicator
    {
        public RateOfChange()
        {
            IndicatorName = "Rate of Change";
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
                    "ROC rises",
                    "ROC falls",
                    "ROC is higher than the Level line",
                    "ROC is lower than the Level line",
                    "ROC crosses the Level line upward",
                    "ROC crosses the Level line downward",
                    "ROC changes its direction upward",
                    "ROC changes its direction downward"
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
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing ROC value.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[2].Index = (int) BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the indicator is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 10;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of ROC.";

            IndParam.NumParam[1].Caption = "Smoothing period";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of additional smoothing.";

            IndParam.NumParam[2].Caption = "Level";
            IndParam.NumParam[2].Value = 1;
            IndParam.NumParam[2].Min = 0;
            IndParam.NumParam[2].Max = 10;
            IndParam.NumParam[2].Point = 4;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "A critical level (for the appropriate logic).";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var method = (MAMethod) IndParam.ListParam[1].Index;
            var basePrice = (BasePrice) IndParam.ListParam[2].Index;
            var period = (int) IndParam.NumParam[0].Value;
            var smooth = (int) IndParam.NumParam[1].Value;
            double level = IndParam.NumParam[2].Value;
            int prvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            int iFirstBar = prvs + period + smooth + 2;
            var adRoc = new double[Bars];
            double[] adBasePrice = Price(basePrice);

            for (int bar = period; bar < Bars; bar++)
                adRoc[bar] = adBasePrice[bar]/adBasePrice[bar - period];

            if (smooth > 0)
            {
                adRoc = MovingAverage(smooth, 0, method, adRoc);
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                    {
                        CompName = "ROC",
                        DataType = IndComponentType.IndicatorValue,
                        ChartType = IndChartType.Line,
                        ChartColor = Color.Violet,
                        FirstBar = iFirstBar,
                        Value = adRoc
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
                case "ROC rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[] {1};
                    break;

                case "ROC falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[] {1};
                    break;

                case "ROC is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] {level, 2 - level};
                    break;

                case "ROC is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] {level, 2 - level};
                    break;

                case "ROC crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] {level, 2 - level};
                    break;

                case "ROC crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] {level, 2 - level};
                    break;

                case "ROC changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[] {1};
                    break;

                case "ROC changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[] {1};
                    break;
            }

            OscillatorLogic(iFirstBar, prvs, adRoc, level, 2 - level, ref Component[1], ref Component[2], indLogic);
        }

        public override void SetDescription()
        {
            double fLevelLong = IndParam.NumParam[2].Value;
            double fLevelShort = 2 - fLevelLong;

            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "ROC rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "ROC falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "ROC is higher than the Level line":
                    EntryFilterLongDescription += "is higher than the Level " + fLevelLong;
                    EntryFilterShortDescription += "is lower than the Level " + fLevelShort;
                    ExitFilterLongDescription += "is higher than the Level " + fLevelLong;
                    ExitFilterShortDescription += "is lower than the Level " + fLevelShort;
                    break;

                case "ROC is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + fLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + fLevelShort;
                    ExitFilterLongDescription += "is lower than the Level " + fLevelLong;
                    ExitFilterShortDescription += "is higher than the Level " + fLevelShort;
                    break;

                case "ROC crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + fLevelLong + " upward";
                    EntryFilterShortDescription += "crosses the Level " + fLevelShort + " downward";
                    ExitFilterLongDescription += "crosses the Level " + fLevelLong + " upward";
                    ExitFilterShortDescription += "crosses the Level " + fLevelShort + " downward";
                    break;

                case "ROC crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + fLevelLong + " downward";
                    EntryFilterShortDescription += "crosses the Level " + fLevelShort + " upward";
                    ExitFilterLongDescription += "crosses the Level " + fLevelLong + " downward";
                    ExitFilterShortDescription += "crosses the Level " + fLevelShort + " upward";
                    break;

                case "ROC changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "ROC changes its direction downward":
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
                   IndParam.ListParam[1].Text + ", " + // Method
                   IndParam.ListParam[2].Text + ", " + // Price
                   IndParam.NumParam[0].ValueToString + ", " + // Period
                   IndParam.NumParam[1].ValueToString + ")"; // Smoothing
        }
    }
}