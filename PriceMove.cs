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
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[1].Index = (int) BasePrice.Open;
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
            var price = (BasePrice) IndParam.ListParam[1].Index;
            double margin = IndParam.NumParam[0].Value*Point;
            int prvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // TimeExecution
            if (price == BasePrice.Open && Math.Abs(margin - 0) < Epsilon)
                IndParam.ExecutionTime = ExecutionTime.AtBarOpening;
            else if (price == BasePrice.Close && Math.Abs(margin - 0) < Epsilon)
                IndParam.ExecutionTime = ExecutionTime.AtBarClosing;

            // Calculation
            double[] adBasePr = Price(price);
            var adUpBand = new double[Bars];
            var adDnBand = new double[Bars];

            int firstBar = 1 + prvs;

            for (int iBar = firstBar; iBar < Bars; iBar++)
            {
                adUpBand[iBar] = adBasePr[iBar - prvs] + margin;
                adDnBand[iBar] = adBasePr[iBar - prvs] - margin;
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
                {
                    CompName = "Up Price",
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = adUpBand
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Down Price",
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = adDnBand
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
            var iMargin = (int) IndParam.NumParam[0].Value;
            string sBasePrice = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index].ToLower();
            string sPrevious = (IndParam.CheckParam[0].Checked ? " previous" : "");

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long after an upward move":
                    EntryPointLongDescription = iMargin + " points above the" + sPrevious + " bar " + sBasePrice +
                                                " price";
                    EntryPointShortDescription = iMargin + " points below the" + sPrevious + " bar " + sBasePrice +
                                                 " price";
                    break;

                case "Enter long after a downward move":
                    EntryPointLongDescription = iMargin + " points below the" + sPrevious + " bar " + sBasePrice +
                                                " price";
                    EntryPointShortDescription = iMargin + " points above the" + sPrevious + " bar " + sBasePrice +
                                                 " price";
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