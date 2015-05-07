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
    public class EaseOfMovement : Indicator
    {
        public EaseOfMovement()
        {
            // General properties
            IndicatorName = "Ease of Movement";
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
                "Ease of Movement rises",
                "Ease of Movement falls",
                "Ease of Movement is higher than the zero line",
                "Ease of Movement is lower than the zero line",
                "Ease of Movement crosses the zero line upward",
                "Ease of Movement crosses the zero line downward",
                "Ease of Movement changes its direction upward",
                "Ease of Movement changes its direction downward"
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
            IndParam.ListParam[1].ToolTip = "The method of smoothing.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 13;
            IndParam.NumParam[0].Min = 1;
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
            var maMethod = (MAMethod) IndParam.ListParam[1].Index;
            var period = (int) IndParam.NumParam[0].Value;
            var previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            var firstBar = period + 2;

            var avEom = new double[Bars];

            double divisor = 1000000000;
            for (var bar = 1; bar < Bars; bar++)
            {
                avEom[bar] = (High[bar] - Low[bar])*
                             ((High[bar] + Low[bar])/2 - (High[bar - 1] + Low[bar - 1])/2)/
                             (Math.Max(Volume[bar], 1)/divisor);

                if (avEom[bar] > 10000)
                {
                    divisor /= 10;
                    bar = 1;
                }
            }

            avEom = MovingAverage(period, 0, maMethod, avEom);

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
            {
                CompName = "Ease of Movement",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Color.LightSeaGreen,
                FirstBar = firstBar,
                Value = avEom
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
                case "Ease of Movement rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    break;

                case "Ease of Movement falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    break;

                case "Ease of Movement is higher than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;

                case "Ease of Movement is lower than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;

                case "Ease of Movement crosses the zero line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;

                case "Ease of Movement crosses the zero line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;

                case "Ease of Movement changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "Ease of Movement changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
            }

            OscillatorLogic(firstBar, previous, avEom, 0, 0, ref Component[1], ref Component[2], indLogic);
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Ease of Movement rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "Ease of Movement falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "Ease of Movement is higher than the zero line":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "Ease of Movement is lower than the zero line":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "Ease of Movement crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "Ease of Movement crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "Ease of Movement changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "Ease of Movement changes its direction downward":
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
                   IndParam.NumParam[0].ValueToString + ")"; // Period
        }
    }
}