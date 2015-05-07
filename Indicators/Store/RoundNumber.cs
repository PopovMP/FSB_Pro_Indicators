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
    public class RoundNumber : Indicator
    {
        public RoundNumber()
        {
            IndicatorName = "Round Number";
            PossibleSlots = SlotTypes.Open | SlotTypes.Close;

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
            if (SlotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter long at the higher round number",
                        "Enter long at the lower round number"
                    };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit long at the higher round number",
                        "Exit long at the lower round number"
                    };
            else
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Not Defined"
                    };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Vertical shift";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = -2000;
            IndParam.NumParam[0].Max = +2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "A vertical shift above the higher and below the lower round number.";

            // The NumericUpDown parameters
            IndParam.NumParam[1].Caption = "Digits";
            IndParam.NumParam[1].Value = 2;
            IndParam.NumParam[1].Min = 2;
            IndParam.NumParam[1].Max = 4;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Number of digits to be rounded.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            double shift = IndParam.NumParam[0].Value*Point;
            var digids = (int) IndParam.NumParam[1].Value;

            // Calculation
            var upperRn = new double[Bars];
            var lowerRn = new double[Bars];

            const int firstBar = 1;

            for (int iBar = 1; iBar < Bars; iBar++)
            {
                double dNearestRound;

                int iCutDigids = Digits - digids;
                if (iCutDigids >= 0)
                    dNearestRound = Math.Round(Open[iBar], iCutDigids);
                else
                    dNearestRound = Math.Round(Open[iBar]*Math.Pow(10, iCutDigids))/Math.Pow(10, iCutDigids);


                if (dNearestRound < Open[iBar])
                {
                    upperRn[iBar] = dNearestRound + (Point*Math.Pow(10, digids));
                    lowerRn[iBar] = dNearestRound;
                }
                else
                {
                    upperRn[iBar] = dNearestRound;
                    lowerRn[iBar] = dNearestRound - (Point*Math.Pow(10, digids));
                }
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
                {
                    CompName = "Higher round number",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Level,
                    ChartColor = Color.SpringGreen,
                    FirstBar = firstBar,
                    Value = upperRn
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Lower round number",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Level,
                    ChartColor = Color.DarkRed,
                    FirstBar = firstBar,
                    Value = lowerRn
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

            if (SlotType == SlotTypes.Open)
            {
                Component[2].CompName = "Long position entry price";
                Component[2].DataType = IndComponentType.OpenLongPrice;
                Component[3].CompName = "Short position entry price";
                Component[3].DataType = IndComponentType.OpenShortPrice;
            }
            else if (SlotType == SlotTypes.Close)
            {
                Component[2].CompName = "Long position closing price";
                Component[2].DataType = IndComponentType.CloseLongPrice;
                Component[3].CompName = "Short position closing price";
                Component[3].DataType = IndComponentType.CloseShortPrice;
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the higher round number":
                case "Exit long at the higher round number":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[2].Value[iBar] = upperRn[iBar] + shift;
                        Component[3].Value[iBar] = lowerRn[iBar] - shift;
                    }
                    break;
                case "Enter long at the lower round number":
                case "Exit long at the lower round number":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[2].Value[iBar] = lowerRn[iBar] - shift;
                        Component[3].Value[iBar] = upperRn[iBar] + shift;
                    }
                    break;
            }
        }

        public override void SetDescription()
        {
            var iShift = (int) IndParam.NumParam[0].Value;

            string sUpperTrade;
            string sLowerTrade;

            if (iShift > 0)
            {
                sUpperTrade = iShift + " points above the ";
                sLowerTrade = iShift + " points below the ";
            }
            else if (iShift == 0)
            {
                sUpperTrade = "at the ";
                sLowerTrade = "at the ";
            }
            else
            {
                sUpperTrade = -iShift + " points below the ";
                sLowerTrade = -iShift + " points above the ";
            }
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the higher round number":
                    EntryPointLongDescription = sUpperTrade + "higher round number";
                    EntryPointShortDescription = sLowerTrade + "lower round number";
                    break;
                case "Exit long at the higher round number":
                    ExitPointLongDescription = sUpperTrade + "higher round number";
                    ExitPointShortDescription = sLowerTrade + "lower round number";
                    break;
                case "Enter long at the lower round number":
                    EntryPointLongDescription = sLowerTrade + "lower round number";
                    EntryPointShortDescription = sUpperTrade + "higher round number";
                    break;
                case "Exit long at the lower round number":
                    ExitPointLongDescription = sLowerTrade + "lower round number";
                    ExitPointShortDescription = sUpperTrade + "higher round number";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.NumParam[0].ValueToString + ")"; // Vertical shift
        }
    }
}