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
    public class PriceMove : Indicator
    {
        public PriceMove()
        {
            IndicatorName = "Price Move";
            PossibleSlots = SlotTypes.Open;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.Additional;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Enter long after an upward move",
                    "Enter long after a downward move"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[1].Index = (int)BasePrice.Open;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The price where the move starts from.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Price move";
            IndParam.NumParam[0].Value = 20;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The price move in points.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = false;
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var basePrice = (BasePrice)IndParam.ListParam[1].Index;
            double margin = IndParam.NumParam[0].Value * Point;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // TimeExecution
            if (basePrice == BasePrice.Open && Math.Abs(margin - 0) < Epsilon)
                IndParam.ExecutionTime = ExecutionTime.AtBarOpening;
            else if (basePrice == BasePrice.Close && Math.Abs(margin - 0) < Epsilon)
                IndParam.ExecutionTime = ExecutionTime.AtBarClosing;
            else
                IndParam.ExecutionTime = ExecutionTime.DuringTheBar;

            // Calculation
            double[] price = Price(basePrice);
            var upperBand = new double[Bars];
            var lowerBand = new double[Bars];

            int firstBar = previous + 2;

            for (int bar = firstBar; bar < Bars; bar++)
            {
                upperBand[bar] = price[bar - previous] + margin;
                lowerBand[bar] = price[bar - previous] - margin;
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
            {
                CompName = "Up Price",
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = upperBand
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Down Price",
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = lowerBand
            };

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long after an upward move":
                    Component[0].DataType = IndComponentType.OpenLongPrice;
                    Component[1].DataType = IndComponentType.OpenShortPrice;
                    break;

                case "Enter long after a downward move":
                    Component[0].DataType = IndComponentType.OpenShortPrice;
                    Component[1].DataType = IndComponentType.OpenLongPrice;
                    break;
            }
        }

        public override void SetDescription()
        {
            var margin = (int)IndParam.NumParam[0].Value;
            string basePrice = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index].ToLower();
            string previous = (IndParam.CheckParam[0].Checked ? " previous" : "");

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long after an upward move":
                    EntryPointLongDescription = string.Format("{0} points above the{1} bar {2} price", margin, previous, basePrice);
                    EntryPointShortDescription = string.Format("{0} points below the{1} bar {2} price", margin, previous, basePrice);
                    break;

                case "Enter long after a downward move":
                    EntryPointLongDescription = string.Format("{0} points below the{1} bar {2} price", margin, previous, basePrice);
                    EntryPointShortDescription = string.Format("{0} points above the{1} bar {2} price", margin, previous, basePrice);
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Base Price
                   IndParam.NumParam[0].ValueToString + ")"; // Margin in points
        }
    }
}