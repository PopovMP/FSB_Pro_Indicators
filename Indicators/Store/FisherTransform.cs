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
    public class FisherTransform : Indicator
    {
        public FisherTransform()
        {
            IndicatorName = "Fisher Transform";
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
                    "Fisher Transform rises",
                    "Fisher Transform falls",
                    "Fisher Transform is higher than the zero line",
                    "Fisher Transform is lower than the zero line",
                    "Fisher Transform crosses the zero line upward",
                    "Fisher Transform crosses the zero line downward",
                    "Fisher Transform changes its direction upward",
                    "Fisher Transform changes its direction downward"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[1].Index = (int)BasePrice.Median;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The price Fisher Transform is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 10;
            IndParam.NumParam[0].Min = 3;
            IndParam.NumParam[0].Max = 100;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The smoothing period.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var basePrice = (BasePrice)IndParam.ListParam[1].Index;
            var period = (int)IndParam.NumParam[0].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period + previous + 2;

            double[] price = Price(basePrice);
            var signal = new double[Bars];

            for (int bar = 0; bar < period; bar++)
                signal[bar] = 0;

            for (int bar = period; bar < Bars; bar++)
            {
                double highestHigh = double.MinValue;
                double lowestLow = double.MaxValue;
                for (int i = 0; i < period; i++)
                {
                    if (price[bar - i] > highestHigh)
                        highestHigh = price[bar - i];
                    if (price[bar - i] < lowestLow)
                        lowestLow = price[bar - i];
                }

                if (Math.Abs(highestHigh - lowestLow) < Epsilon)
                    highestHigh = lowestLow + Point;
                if (Math.Abs(highestHigh - lowestLow - 0.5) < Epsilon)
                    highestHigh += Point;

                signal[bar] = 0.33 * 2 * ((price[bar] - lowestLow) / (highestHigh - lowestLow) - 0.5) +
                                0.67 * signal[bar - 1];
            }

            var fisher = new double[Bars];
            fisher[0] = 0;
            for (int bar = 1; bar < Bars; bar++)
            {
                fisher[bar] = 0.5 * Math.Log10((1 + signal[bar]) / (1 - signal[bar])) + 0.5 * fisher[bar - 1];
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
            {
                CompName = "Fisher Transform",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Histogram,
                FirstBar = firstBar,
                Value = fisher
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
                case "Fisher Transform rises":
                    logicRule = IndicatorLogic.The_indicator_rises;
                    break;

                case "Fisher Transform falls":
                    logicRule = IndicatorLogic.The_indicator_falls;
                    break;

                case "Fisher Transform is higher than the zero line":
                    logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;

                case "Fisher Transform is lower than the zero line":
                    logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;

                case "Fisher Transform crosses the zero line upward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;

                case "Fisher Transform crosses the zero line downward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;

                case "Fisher Transform changes its direction upward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "Fisher Transform changes its direction downward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
            }

            OscillatorLogic(firstBar, previous, fisher, 0, 0, ref Component[1], ref Component[2], logicRule);
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Fisher Transform rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "Fisher Transform falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "Fisher Transform is higher than the zero line":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "Fisher Transform is lower than the zero line":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "Fisher Transform crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "Fisher Transform crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "Fisher Transform changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "Fisher Transform changes its direction downward":
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
                   IndParam.ListParam[1].Text + ", " + // Price
                   IndParam.NumParam[0].ValueToString + ")"; // Period
        }
    }
}