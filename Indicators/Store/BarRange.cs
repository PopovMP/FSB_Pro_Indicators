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
using System.Globalization;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class BarRange : Indicator
    {
        public BarRange()
        {
            IndicatorName = "Bar Range";
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
                "Bar Range rises",
                "Bar Range falls",
                "Bar Range is higher than the Level line",
                "Bar Range is lower than the Level line"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Number of bars";
            IndParam.NumParam[0].Value = 1;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The number of bars to calculate the range.";

            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 5000;
            IndParam.NumParam[1].Point = 0;
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

            // Calculation
            int firstBar = period + previous + 2;

            var range = new double[Bars];

            for (int bar = firstBar; bar < Bars; bar++)
            {
                double maxHigh = double.MinValue;
                double minLow = double.MaxValue;
                for (int i = 0; i < period; i++)
                {
                    if (High[bar - i] > maxHigh)
                        maxHigh = High[bar - i];
                    if (Low[bar - i] < minLow)
                        minLow = Low[bar - i];
                }
                range[bar] = maxHigh - minLow;
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
            {
                CompName = "Bar Range",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Histogram,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            for (int bar = 0; bar < Bars; bar++)
            {
                Component[0].Value[bar] = Math.Round(range[bar] / Point);
            }

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
                case "Bar Range rises":
                    logicRule = IndicatorLogic.The_indicator_rises;
                    break;

                case "Bar Range falls":
                    logicRule = IndicatorLogic.The_indicator_falls;
                    break;

                case "Bar Range is higher than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] { level };
                    break;

                case "Bar Range is lower than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] { level };
                    break;
            }

            NoDirectionOscillatorLogic(firstBar, previous, range, level * Point, ref Component[1], logicRule);
            Component[2].Value = Component[1].Value;
        }

        public override void SetDescription()
        {
            var barsNumber = (int)IndParam.NumParam[0].Value;
            string levelLong = IndParam.NumParam[1].ValueToString;
            string levelShort = levelLong;

            if (barsNumber == 1)
            {
                EntryFilterLongDescription = "the range of the bar ";
                EntryFilterShortDescription = "the range of the bar ";
                ExitFilterLongDescription = "the range of the bar ";
                ExitFilterShortDescription = "the range of the bar ";
            }
            else
            {
                EntryFilterLongDescription = "the range of the last " +
                                             barsNumber.ToString(CultureInfo.InvariantCulture) +
                                             " bars ";
                EntryFilterShortDescription = "the range of the last " +
                                              barsNumber.ToString(CultureInfo.InvariantCulture) +
                                              " bars ";
                ExitFilterLongDescription = "the range of the last " + barsNumber.ToString(CultureInfo.InvariantCulture) +
                                            " bars ";
                ExitFilterShortDescription = "the range of the last " +
                                             barsNumber.ToString(CultureInfo.InvariantCulture) +
                                             " bars ";
            }
            switch (IndParam.ListParam[0].Text)
            {
                case "Bar Range rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "falls";
                    break;

                case "Bar Range falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "rises";
                    break;

                case "Bar Range is higher than the Level line":
                    EntryFilterLongDescription += "is higher than " + levelLong + " points";
                    EntryFilterShortDescription += "is higher than " + levelShort + " points";
                    ExitFilterLongDescription += "is higher than " + levelLong + " points";
                    ExitFilterShortDescription += "is higher than " + levelShort + " points";
                    break;

                case "Bar Range is lower than the Level line":
                    EntryFilterLongDescription += "is lower than " + levelLong + " points";
                    EntryFilterShortDescription += "is lower than " + levelShort + " points";
                    ExitFilterLongDescription += "is lower than " + levelLong + " points";
                    ExitFilterShortDescription += "is lower than " + levelShort + " points";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.NumParam[0].ValueToString + ", " + // Number of bars
                   IndParam.NumParam[1].ValueToString + ")"; // Level
        }
    }
}