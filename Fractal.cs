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
    public class Fractal : Indicator
    {
        public Fractal()
        {
            IndicatorName = "Fractal";
            PossibleSlots = SlotTypes.Open | SlotTypes.Close;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.1";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (SlotType == SlotTypes.Open)
            {
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Enter long at Up Fractal",
                    "Enter long at Down Fractal"
                };
            }
            else if (SlotType == SlotTypes.Close)
            {
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Exit long at Up Fractal",
                    "Exit long at Down Fractal"
                };
            }
            else
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Visibility";
            IndParam.ListParam[1].ItemList = new[]
            {
                "Visible",
                "Visible or shadowed"
            };
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Is the fractal visible from the current market point.";

            IndParam.NumParam[0].Caption = "Vertical shift";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = -2000;
            IndParam.NumParam[0].Max = +2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "A vertical shift above Up Fractal and below Down Fractal.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            bool isVisible = IndParam.ListParam[1].Text == "Visible";
            double shift = IndParam.NumParam[0].Value*Point;
            const int firstBar = 8;

            var upFractals = new double[Bars];
            var downFractals = new double[Bars];

            for (int bar = 8; bar < Bars - 1; bar++)
            {
                if (High[bar - 1] < High[bar - 2] && High[bar] < High[bar - 2])
                {
                    // Fractal type 1
                    if (High[bar - 4] < High[bar - 2] &&
                        High[bar - 3] < High[bar - 2])
                        upFractals[bar + 1] = High[bar - 2];

                    // Fractal type 2
                    if (High[bar - 5] < High[bar - 2] &&
                        High[bar - 4] < High[bar - 2] &&
                        High[bar - 3] == High[bar - 2])
                        upFractals[bar + 1] = High[bar - 2];

                    // Fractal type 3, 4
                    if (High[bar - 6] < High[bar - 2] &&
                        High[bar - 5] < High[bar - 2] &&
                        High[bar - 4] == High[bar - 2] &&
                        High[bar - 3] <= High[bar - 2])
                        upFractals[bar + 1] = High[bar - 2];

                    // Fractal type 5
                    if (High[bar - 7] < High[bar - 2] &&
                        High[bar - 6] < High[bar - 2] &&
                        High[bar - 5] == High[bar - 2] &&
                        High[bar - 4] < High[bar - 2] &&
                        High[bar - 3] == High[bar - 2])
                        upFractals[bar + 1] = High[bar - 2];

                    // Fractal type 6
                    if (High[bar - 7] < High[bar - 2] &&
                        High[bar - 6] < High[bar - 2] &&
                        High[bar - 5] == High[bar - 2] &&
                        High[bar - 4] == High[bar - 2] &&
                        High[bar - 3] < High[bar - 2])
                        upFractals[bar + 1] = High[bar - 2];

                    // Fractal type 7
                    if (High[bar - 8] < High[bar - 2] &&
                        High[bar - 7] < High[bar - 2] &&
                        High[bar - 6] == High[bar - 2] &&
                        High[bar - 5] < High[bar - 2] &&
                        High[bar - 4] == High[bar - 2] &&
                        High[bar - 3] < High[bar - 2])
                        upFractals[bar + 1] = High[bar - 2];
                }

                if (Low[bar - 1] > Low[bar - 2] && Low[bar] > Low[bar - 2])
                {
                    // Fractal type 1
                    if (Low[bar - 4] > Low[bar - 2] &&
                        Low[bar - 3] > Low[bar - 2])
                        downFractals[bar + 1] = Low[bar - 2];

                    // Fractal type 2
                    if (Low[bar - 5] > Low[bar - 2] &&
                        Low[bar - 4] > Low[bar - 2] &&
                        Low[bar - 3] == Low[bar - 2])
                        downFractals[bar + 1] = Low[bar - 2];

                    // Fractal type 3, 4
                    if (Low[bar - 6] > Low[bar - 2] &&
                        Low[bar - 5] > Low[bar - 2] &&
                        Low[bar - 4] == Low[bar - 2] &&
                        Low[bar - 3] >= Low[bar - 2])
                        downFractals[bar + 1] = Low[bar - 2];

                    // Fractal type 5
                    if (Low[bar - 7] > Low[bar - 2] &&
                        Low[bar - 6] > Low[bar - 2] &&
                        Low[bar - 5] == Low[bar - 2] &&
                        Low[bar - 4] > Low[bar - 2] &&
                        Low[bar - 3] == Low[bar - 2])
                        downFractals[bar + 1] = Low[bar - 2];

                    // Fractal type 6
                    if (Low[bar - 7] > Low[bar - 2] &&
                        Low[bar - 6] > Low[bar - 2] &&
                        Low[bar - 5] == Low[bar - 2] &&
                        Low[bar - 4] == Low[bar - 2] &&
                        Low[bar - 3] > Low[bar - 2])
                        downFractals[bar + 1] = Low[bar - 2];

                    // Fractal type 7
                    if (Low[bar - 8] > Low[bar - 2] &&
                        Low[bar - 7] > Low[bar - 2] &&
                        Low[bar - 6] == Low[bar - 2] &&
                        Low[bar - 5] > Low[bar - 2] &&
                        Low[bar - 4] == Low[bar - 2] &&
                        Low[bar - 3] > Low[bar - 2])
                        downFractals[bar + 1] = Low[bar - 2];
                }
            }

            // Is visible
            if (isVisible)
                for (int bar = firstBar; bar < Bars; bar++)
                {
                    if (upFractals[bar - 1] > 0 && Math.Abs(upFractals[bar] - 0) < Epsilon &&
                        High[bar - 1] < upFractals[bar - 1])
                        upFractals[bar] = upFractals[bar - 1];
                    if (downFractals[bar - 1] > 0 && Math.Abs(downFractals[bar] - 0) < Epsilon && Low[bar - 1] > downFractals[bar - 1])
                        downFractals[bar] = downFractals[bar - 1];
                }
            else
                for (int bar = firstBar; bar < Bars; bar++)
                {
                    if (Math.Abs(upFractals[bar] - 0) < Epsilon) upFractals[bar] = upFractals[bar - 1];
                    if (Math.Abs(downFractals[bar] - 0) < Epsilon) downFractals[bar] = downFractals[bar - 1];
                }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
            {
                CompName = "Up Fractal",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Level,
                ChartColor = Color.SpringGreen,
                FirstBar = firstBar,
                Value = upFractals
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Down Fractal",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Level,
                ChartColor = Color.DarkRed,
                FirstBar = firstBar,
                Value = downFractals
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
                case "Enter long at Up Fractal":
                case "Exit long at Up Fractal":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        if (upFractals[bar] > Point)
                            Component[2].Value[bar] = upFractals[bar] + shift;
                        if (downFractals[bar] > Point)
                            Component[3].Value[bar] = downFractals[bar] - shift;
                    }
                    break;
                case "Enter long at Down Fractal":
                case "Exit long at Down Fractal":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        if (downFractals[bar] > Point)
                            Component[2].Value[bar] = downFractals[bar] - shift;
                        if (upFractals[bar] > Point)
                            Component[3].Value[bar] = upFractals[bar] + shift;
                    }
                    break;
            }
        }

        public override void SetDescription()
        {
            var shift = (int) IndParam.NumParam[0].Value;

            string upperTrade;
            string lowerTrade;

            if (shift > 0)
            {
                upperTrade = shift + " points above ";
                lowerTrade = shift + " points below ";
            }
            else if (shift == 0)
            {
                upperTrade = "at ";
                lowerTrade = "at ";
            }
            else
            {
                upperTrade = -shift + " points below ";
                lowerTrade = -shift + " points above ";
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at Up Fractal":
                    EntryPointLongDescription = upperTrade + "Up Fractal";
                    EntryPointShortDescription = lowerTrade + "Down Fractal";
                    break;
                case "Exit long at Up Fractal":
                    ExitPointLongDescription = upperTrade + "Up Fractal";
                    ExitPointShortDescription = lowerTrade + "Down Fractal";
                    break;
                case "Enter long at Down Fractal":
                    EntryPointLongDescription = lowerTrade + "Down Fractal";
                    EntryPointShortDescription = upperTrade + "Up Fractal";
                    break;
                case "Exit long at Down Fractal":
                    ExitPointLongDescription = lowerTrade + "Down Fractal";
                    ExitPointShortDescription = upperTrade + "Up Fractal";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}