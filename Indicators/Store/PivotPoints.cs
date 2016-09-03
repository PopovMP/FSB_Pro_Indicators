//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class PivotPoints : Indicator
    {
        public PivotPoints()
        {
            IndicatorName = "Pivot Points";
            PossibleSlots = SlotTypes.Open | SlotTypes.Close;
            SeparatedChart = false;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";

            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter long at R3 (short at S3)",
                        "Enter long at R2 (short at S2)",
                        "Enter long at R1 (short at S1)",
                        "Enter the market at the Pivot Point",
                        "Enter long at S1 (short at R1)",
                        "Enter long at S2 (short at R2)",
                        "Enter long at S3 (short at R3)"
                    };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit long at R3 (short at S3)",
                        "Exit long at R2 (short at S2)",
                        "Exit long at R1 (short at S1)",
                        "Exit the market at the Pivot Point",
                        "Exit long at S1 (short at R1)",
                        "Exit long at S2 (short at R2)",
                        "Exit long at S3 (short at R3)"
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

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] { "One day", "One bar" };
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The base price for calculation of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Vertical shift";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Max = +2000;
            IndParam.NumParam[0].Min = -2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "A vertical shift above the Resistance and below the Support levels.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            double shift = IndParam.NumParam[0].Value * Point;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = previous + 2;
            var pp = new double[Bars];
            var r1 = new double[Bars];
            var r2 = new double[Bars];
            var r3 = new double[Bars];
            var s1 = new double[Bars];
            var s2 = new double[Bars];
            var s3 = new double[Bars];

            var high = new double[Bars];
            var low = new double[Bars];
            var close = new double[Bars];

            if (IndParam.ListParam[1].Text == "One bar" ||
                Period == DataPeriod.D1 || Period == DataPeriod.W1)
            {
                high = High;
                low = Low;
                close = Close;
            }
            else
            {
                previous = 0;

                high[0] = 0;
                low[0] = 0;
                close[0] = 0;

                double top = double.MinValue;
                double bottom = double.MaxValue;

                for (int bar = 1; bar < Bars; bar++)
                {
                    if (High[bar - 1] > top)
                        top = High[bar - 1];
                    if (Low[bar - 1] < bottom)
                        bottom = Low[bar - 1];

                    if (Time[bar].Day != Time[bar - 1].Day)
                    {
                        high[bar] = top;
                        low[bar] = bottom;
                        close[bar] = Close[bar - 1];
                        top = double.MinValue;
                        bottom = double.MaxValue;
                    }
                    else
                    {
                        high[bar] = high[bar - 1];
                        low[bar] = low[bar - 1];
                        close[bar] = close[bar - 1];
                    }
                }

                // first Bar
                for (int bar = 1; bar < Bars; bar++)
                {
                    if (Time[bar].Day != Time[bar - 1].Day)
                    {
                        firstBar = bar;
                        break;
                    }
                }
            }

            for (int bar = firstBar; bar < Bars; bar++)
            {
                pp[bar] = (high[bar] + low[bar] + close[bar]) / 3;
                r1[bar] = 2 * pp[bar] - low[bar];
                s1[bar] = 2 * pp[bar] - high[bar];
                r2[bar] = pp[bar] + high[bar] - low[bar];
                s2[bar] = pp[bar] - high[bar] + low[bar];
                r3[bar] = high[bar] + 2 * (pp[bar] - low[bar]);
                s3[bar] = low[bar] - 2 * (high[bar] - pp[bar]);
            }

            Component = new IndicatorComp[9];

            for (int iComp = 0; iComp < 7; iComp++)
            {
                Component[iComp] = new IndicatorComp
                {
                    ChartType = IndChartType.Dot,
                    ChartColor = Color.Violet,
                    DataType = IndComponentType.IndicatorValue,
                    FirstBar = firstBar
                };
            }
            Component[3].ChartColor = Color.Blue;

            Component[0].Value = r3;
            Component[1].Value = r2;
            Component[2].Value = r1;
            Component[3].Value = pp;
            Component[4].Value = s1;
            Component[5].Value = s2;
            Component[6].Value = s3;

            Component[0].CompName = "Resistance 3";
            Component[1].CompName = "Resistance 2";
            Component[2].CompName = "Resistance 1";
            Component[3].CompName = "Pivot Point";
            Component[4].CompName = "Support 1";
            Component[5].CompName = "Support 2";
            Component[6].CompName = "Support 3";

            Component[7] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            Component[8] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            if (SlotType == SlotTypes.Open)
            {
                Component[7].CompName = "Long position entry price";
                Component[7].DataType = IndComponentType.OpenLongPrice;
                Component[8].CompName = "Short position entry price";
                Component[8].DataType = IndComponentType.OpenShortPrice;
            }
            else if (SlotType == SlotTypes.Close)
            {
                Component[7].CompName = "Long position closing price";
                Component[7].DataType = IndComponentType.CloseLongPrice;
                Component[8].CompName = "Short position closing price";
                Component[8].DataType = IndComponentType.CloseShortPrice;
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at R3 (short at S3)":
                case "Exit long at R3 (short at S3)":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[7].Value[bar] = r3[bar - previous] + shift;
                        Component[8].Value[bar] = s3[bar - previous] - shift;
                    }
                    break;
                case "Enter long at R2 (short at S2)":
                case "Exit long at R2 (short at S2)":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[7].Value[bar] = r2[bar - previous] + shift;
                        Component[8].Value[bar] = s2[bar - previous] - shift;
                    }
                    break;
                case "Enter long at R1 (short at S1)":
                case "Exit long at R1 (short at S1)":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[7].Value[bar] = r1[bar - previous] + shift;
                        Component[8].Value[bar] = s1[bar - previous] - shift;
                    }
                    break;
                //---------------------------------------------------------------------
                case "Enter the market at the Pivot Point":
                case "Exit the market at the Pivot Point":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[7].Value[bar] = pp[bar - previous] + shift;
                        Component[8].Value[bar] = pp[bar - previous] - shift;
                    }
                    break;
                //---------------------------------------------------------------------
                case "Enter long at S1 (short at R1)":
                case "Exit long at S1 (short at R1)":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[7].Value[bar] = s1[bar - previous] - shift;
                        Component[8].Value[bar] = r1[bar - previous] + shift;
                    }
                    break;
                case "Enter long at S2 (short at R2)":
                case "Exit long at S2 (short at R2)":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[7].Value[bar] = s2[bar - previous] - shift;
                        Component[8].Value[bar] = r2[bar - previous] + shift;
                    }
                    break;
                case "Enter long at S3 (short at R3)":
                case "Exit long at S3 (short at R3)":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[7].Value[bar] = s3[bar - previous] - shift;
                        Component[8].Value[bar] = r3[bar - previous] + shift;
                    }
                    break;
            }
        }

        public override void SetDescription()
        {
            var shift = (int)IndParam.NumParam[0].Value;

            string upperTrade;
            string lowerTrade;

            if (shift > 0)
            {
                upperTrade = shift + " points above the ";
                lowerTrade = shift + " points below the ";
            }
            else if (shift == 0)
            {
                upperTrade = "at ";
                lowerTrade = "at ";
            }
            else
            {
                upperTrade = -shift + " points below the ";
                lowerTrade = -shift + " points above the ";
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at R3 (short at S3)":
                    EntryPointLongDescription = upperTrade + "Pivot Point Resistance 3 level";
                    EntryPointShortDescription = lowerTrade + "Pivot Point Support 3 level";
                    break;
                case "Exit long at R3 (short at S3)":
                    ExitPointLongDescription = upperTrade + "Pivot Point Resistance 3 level";
                    ExitPointShortDescription = lowerTrade + "Pivot Point Support 3 level";
                    break;
                case "Enter long at R2 (short at S2)":
                    EntryPointLongDescription = upperTrade + "Pivot Point Resistance 2 level";
                    EntryPointShortDescription = lowerTrade + "Pivot Point Support 2 level";
                    break;
                case "Exit long at R2 (short at S2)":
                    ExitPointLongDescription = upperTrade + "Pivot Point Resistance 2 level";
                    ExitPointShortDescription = lowerTrade + "Pivot Point Support 2 level";
                    break;
                case "Enter long at R1 (short at S1)":
                    EntryPointLongDescription = upperTrade + "Pivot Point Resistance 1 level";
                    EntryPointShortDescription = lowerTrade + "Pivot Point Support 1 level";
                    break;
                case "Exit long at R1 (short at S1)":
                    ExitPointLongDescription = upperTrade + "Pivot Point Resistance 1 level";
                    ExitPointShortDescription = lowerTrade + "Pivot Point Support 1 level";
                    break;
                //---------------------------------------------------------------------
                case "Enter the market at the Pivot Point":
                    EntryPointLongDescription = upperTrade + "Pivot Point";
                    EntryPointShortDescription = lowerTrade + "Pivot Point";
                    break;
                case "Exit the market at the Pivot Point":
                    ExitPointLongDescription = upperTrade + "Pivot Point";
                    ExitPointShortDescription = lowerTrade + "Pivot Point";
                    break;
                //---------------------------------------------------------------------
                case "Enter long at S1 (short at R1)":
                    EntryPointLongDescription = lowerTrade + "Pivot Point Support 1 level";
                    EntryPointShortDescription = upperTrade + "Pivot Point Resistance 1 level";
                    break;
                case "Exit long at S1 (short at R1)":
                    ExitPointLongDescription = lowerTrade + "Pivot Point Support 1 level";
                    ExitPointShortDescription = upperTrade + "Pivot Point Resistance 1 level";
                    break;
                case "Enter long at S2 (short at R2)":
                    EntryPointLongDescription = lowerTrade + "Pivot Point Support 2 level";
                    EntryPointShortDescription = upperTrade + "Pivot Point Resistance 2 level";
                    break;
                case "Exit long at S2 (short at R2)":
                    ExitPointLongDescription = lowerTrade + "Pivot Point Support 2 level";
                    ExitPointShortDescription = upperTrade + "Pivot Point Resistance 2 level";
                    break;
                case "Enter long at S3 (short at R3)":
                    EntryPointLongDescription = lowerTrade + "Pivot Point Support 3 level";
                    EntryPointShortDescription = upperTrade + "Pivot Point Resistance 3 level";
                    break;
                case "Exit long at S3 (short at R3)":
                    ExitPointLongDescription = lowerTrade + "Pivot Point Support 3 level";
                    ExitPointShortDescription = upperTrade + "Pivot Point Resistance 3 level";
                    break;
            }
        }

        public override string ToString()
        {
            string text = IndicatorName +
                             (IndParam.CheckParam[0].Checked ? "*" : "");
            if (IndParam.ListParam[1].Text == "One day")
                text += "(Daily)";

            return text;
        }
    }
}