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
    public class HeikenAshi : Indicator
    {
        public HeikenAshi()
        {
            IndicatorName = "Heiken Ashi";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (SlotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter long at the H.A. High",
                        "Enter long at the H.A. Low"
                    };
            else if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "White H.A. bar without lower shadow",
                        "White H.A. bar",
                        "Black H.A. bar",
                        "Black H.A. bar without upper shadow"
                    };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit long at the H.A. High",
                        "Exit long at the H.A. Low"
                    };
            else if (SlotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Black H.A. bar without upper shadow",
                        "Black H.A. bar",
                        "White H.A. bar",
                        "White H.A. bar without lower shadow"
                    };
            else
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Not Defined"
                    };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of Heiken Ashi.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] { "High, Low, Open, Close" };
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The base price for calculation of the indicator.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            var haOpen = new double[Bars];
            var haHigh = new double[Bars];
            var haLow = new double[Bars];
            var haClose = new double[Bars];

            haOpen[0] = Open[0];
            haHigh[0] = High[0];
            haLow[0] = Low[0];
            haClose[0] = Close[0];

            int firstBar = 1 + previous;

            for (int bar = 1; bar < Bars; bar++)
            {
                haClose[bar] = (Open[bar] + High[bar] + Low[bar] + Close[bar]) / 4;
                haOpen[bar] = (haOpen[bar - 1] + haClose[bar - 1]) / 2;
                haHigh[bar] = High[bar] > haOpen[bar] ? High[bar] : haOpen[bar];
                haHigh[bar] = haClose[bar] > haHigh[bar] ? haClose[bar] : haHigh[bar];
                haLow[bar] = Low[bar] < haOpen[bar] ? Low[bar] : haOpen[bar];
                haLow[bar] = haClose[bar] < haLow[bar] ? haClose[bar] : haLow[bar];
            }

            // Saving the components
            Component = new IndicatorComp[6];

            Component[0] = new IndicatorComp
            {
                CompName = "H.A. Open",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Dot,
                ChartColor = Color.Green,
                FirstBar = firstBar,
                Value = haOpen
            };

            Component[1] = new IndicatorComp
            {
                CompName = "H.A. High",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Dot,
                ChartColor = Color.Blue,
                FirstBar = firstBar,
                Value = haHigh
            };

            Component[2] = new IndicatorComp
            {
                CompName = "H.A. Low",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Dot,
                ChartColor = Color.Blue,
                FirstBar = firstBar,
                Value = haLow
            };

            Component[3] = new IndicatorComp
            {
                CompName = "H.A. Close",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Dot,
                ChartColor = Color.Red,
                FirstBar = firstBar,
                Value = haClose
            };

            Component[4] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            Component[5] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.Open)
            {
                Component[4].DataType = IndComponentType.OpenLongPrice;
                Component[4].CompName = "Long position entry price";
                Component[5].DataType = IndComponentType.OpenShortPrice;
                Component[5].CompName = "Short position entry price";
            }
            else if (SlotType == SlotTypes.OpenFilter)
            {
                Component[4].DataType = IndComponentType.AllowOpenLong;
                Component[4].CompName = "Is long entry allowed";
                Component[5].DataType = IndComponentType.AllowOpenShort;
                Component[5].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.Close)
            {
                Component[4].DataType = IndComponentType.CloseLongPrice;
                Component[4].CompName = "Long position closing price";
                Component[5].DataType = IndComponentType.CloseShortPrice;
                Component[5].CompName = "Short position closing price";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[4].DataType = IndComponentType.ForceCloseLong;
                Component[4].CompName = "Close out long position";
                Component[5].DataType = IndComponentType.ForceCloseShort;
                Component[5].CompName = "Close out short position";
            }

            if (SlotType == SlotTypes.Open || SlotType == SlotTypes.Close)
            {
                for (int bar = 2; bar < Bars; bar++)
                {
                    if (IndParam.ListParam[0].Text == "Enter long at the H.A. High" ||
                        IndParam.ListParam[0].Text == "Exit long at the H.A. High")
                    {
                        Component[4].Value[bar] = haHigh[bar - previous];
                        Component[5].Value[bar] = haLow[bar - previous];
                    }
                    else
                    {
                        Component[4].Value[bar] = haLow[bar - previous];
                        Component[5].Value[bar] = haHigh[bar - previous];
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "White H.A. bar without lower shadow":
                        for (int bar = firstBar; bar < Bars; bar++)
                        {
                            Component[4].Value[bar] = haClose[bar - previous] > haOpen[bar - previous] &&
                                                       Math.Abs(haLow[bar - previous] - haOpen[bar - previous]) < Epsilon ? 1 : 0;
                            Component[5].Value[bar] = haClose[bar - previous] < haOpen[bar - previous] &&
                                                       Math.Abs(haHigh[bar - previous] - haOpen[bar - previous]) < Epsilon ? 1 : 0;
                        }
                        break;

                    case "White H.A. bar":
                        for (int bar = firstBar; bar < Bars; bar++)
                        {
                            Component[4].Value[bar] = haClose[bar - previous] > haOpen[bar - previous] ? 1 : 0;
                            Component[5].Value[bar] = haClose[bar - previous] < haOpen[bar - previous] ? 1 : 0;
                        }
                        break;

                    case "Black H.A. bar":
                        for (int bar = firstBar; bar < Bars; bar++)
                        {
                            Component[4].Value[bar] = haClose[bar - previous] < haOpen[bar - previous] ? 1 : 0;
                            Component[5].Value[bar] = haClose[bar - previous] > haOpen[bar - previous] ? 1 : 0;
                        }
                        break;

                    case "Black H.A. bar without upper shadow":
                        for (int bar = firstBar; bar < Bars; bar++)
                        {
                            Component[4].Value[bar] = haClose[bar - previous] < haOpen[bar - previous] &&
                                                       Math.Abs(haHigh[bar - previous] - haOpen[bar - previous]) < Epsilon ? 1 : 0;
                            Component[5].Value[bar] = haClose[bar - previous] > haOpen[bar - previous] &&
                                                       Math.Abs(haLow[bar - previous] - haOpen[bar - previous]) < Epsilon ? 1 : 0;
                        }
                        break;
                }
            }
        }

        public override void SetDescription()
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the H.A. High":
                    EntryPointLongDescription = "at " + ToString() + " High";
                    EntryPointShortDescription = "at " + ToString() + " Low";
                    break;

                case "Enter long at the H.A. Low":
                    EntryPointLongDescription = "at " + ToString() + " Low";
                    EntryPointShortDescription = "at " + ToString() + " High";
                    break;

                case "Exit long at the H.A. High":
                    ExitPointLongDescription = "at " + ToString() + " High";
                    ExitPointShortDescription = "at " + ToString() + " Low";
                    break;

                case "Exit long at the H.A. Low":
                    ExitPointLongDescription = "at " + ToString() + " Low";
                    ExitPointShortDescription = "at " + ToString() + " High";
                    break;

                case "White H.A. bar without lower shadow":
                    EntryFilterLongDescription = ToString() + " bar is white and without lower shadow";
                    EntryFilterShortDescription = ToString() + " bar is black and without upper shadow";
                    ExitFilterLongDescription = ToString() + " bar is white and without lower shadow";
                    ExitFilterShortDescription = ToString() + " bar is black and without upper shadow";
                    break;

                case "Black H.A. bar without upper shadow":
                    EntryFilterLongDescription = ToString() + " bar is black and without upper shadow";
                    EntryFilterShortDescription = ToString() + " bar is white and without lower shadow";
                    ExitFilterLongDescription = ToString() + " bar is black and without upper shadow";
                    ExitFilterShortDescription = ToString() + " bar is white and without lower shadow";
                    break;

                case "White H.A. bar":
                    EntryFilterLongDescription = ToString() + " bar is white";
                    EntryFilterShortDescription = ToString() + " bar is black";
                    ExitFilterLongDescription = ToString() + " bar is white";
                    ExitFilterShortDescription = ToString() + " bar is black";
                    break;

                case "Black H.A. bar":
                    EntryFilterLongDescription = ToString() + " bar is black";
                    EntryFilterShortDescription = ToString() + " bar is white";
                    ExitFilterLongDescription = ToString() + " bar is black";
                    ExitFilterShortDescription = ToString() + " bar is white";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName + (IndParam.CheckParam[0].Checked ? "*" : "");
        }
    }
}