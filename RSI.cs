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
    public class RSI : Indicator
    {
        public RSI()
        {
            IndicatorName = "RSI";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = 0;
            SeparatedChartMaxValue = 100;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.1";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                "RSI rises",
                "RSI falls",
                "RSI is higher than the Level line",
                "RSI is lower than the Level line",
                "RSI crosses the Level line upward",
                "RSI crosses the Level line downward",
                "RSI changes its direction upward",
                "RSI changes its direction downward"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Smoothed;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing RSI value.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[2].Index = (int) BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price RSI is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Smoothing period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of smoothing of RSI value.";

            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value = 30;
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
            var maMethod = (MAMethod) IndParam.ListParam[1].Index;
            var basePrice = (BasePrice) IndParam.ListParam[2].Index;
            var period = (int) IndParam.NumParam[0].Value;
            double level = IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period + 2;
            double[] price = Price(basePrice);
            var pos = new double[Bars];
            var neg = new double[Bars];
            var rsi = new double[Bars];

            for (int bar = 1; bar < Bars; bar++)
            {
                if (price[bar] > price[bar - 1] + Epsilon)
                    pos[bar] = price[bar] - price[bar - 1];
                if (price[bar] < price[bar - 1] - Epsilon)
                    neg[bar] = price[bar - 1] - price[bar];
            }

            double[] posMa = MovingAverage(period, 0, maMethod, pos);
            double[] negMa = MovingAverage(period, 0, maMethod, neg);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                if (Math.Abs(negMa[bar]) > Epsilon)
                    rsi[bar] = 100 - (100/(1 + posMa[bar]/negMa[bar]));
                else
                {
                    if (Math.Abs(posMa[bar]) > Epsilon)
                        rsi[bar] = 100;
                    else
                        rsi[bar] = 50;
                }
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
            {
                CompName = "RSI",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Color.RoyalBlue,
                FirstBar = firstBar,
                Value = rsi
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
                case "RSI rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[] {50};
                    break;

                case "RSI falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[] {50};
                    break;

                case "RSI is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] {level, 100 - level};
                    break;

                case "RSI is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] {level, 100 - level};
                    break;

                case "RSI crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] {level, 100 - level};
                    break;

                case "RSI crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] {level, 100 - level};
                    break;

                case "RSI changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[] {50};
                    break;

                case "RSI changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[] {50};
                    break;
            }

            OscillatorLogic(firstBar, previous, rsi, level, 100 - level, ref Component[1], ref Component[2], indLogic);
        }

        public override void SetDescription()
        {
            string longLevel = IndParam.NumParam[1].ValueToString;
            string shortLevel = IndParam.NumParam[1].AnotherValueToString(100 - IndParam.NumParam[1].Value);

            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "RSI rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "RSI falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "RSI is higher than the Level line":
                    EntryFilterLongDescription += "is higher than the Level " + longLevel;
                    EntryFilterShortDescription += "is lower than the Level " + shortLevel;
                    ExitFilterLongDescription += "is higher than the Level " + longLevel;
                    ExitFilterShortDescription += "is lower than the Level " + shortLevel;
                    break;

                case "RSI is lower than the Level line":
                    EntryFilterLongDescription += "is lower than the Level " + longLevel;
                    EntryFilterShortDescription += "is higher than the Level " + shortLevel;
                    ExitFilterLongDescription += "is lower than the Level " + longLevel;
                    ExitFilterShortDescription += "is higher than the Level " + shortLevel;
                    break;

                case "RSI crosses the Level line upward":
                    EntryFilterLongDescription += "crosses the Level " + longLevel + " upward";
                    EntryFilterShortDescription += "crosses the Level " + shortLevel + " downward";
                    ExitFilterLongDescription += "crosses the Level " + longLevel + " upward";
                    ExitFilterShortDescription += "crosses the Level " + shortLevel + " downward";
                    break;

                case "RSI crosses the Level line downward":
                    EntryFilterLongDescription += "crosses the Level " + longLevel + " downward";
                    EntryFilterShortDescription += "crosses the Level " + shortLevel + " upward";
                    ExitFilterLongDescription += "crosses the Level " + longLevel + " downward";
                    ExitFilterShortDescription += "crosses the Level " + shortLevel + " upward";
                    break;

                case "RSI changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "RSI changes its direction downward":
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
                   IndParam.ListParam[2].Text + ", " + // Base price
                   IndParam.NumParam[0].ValueToString + ")"; // Smoothing period
        }
    }
}