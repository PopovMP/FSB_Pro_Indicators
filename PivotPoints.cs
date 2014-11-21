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
            IndParam.ListParam[1].ItemList = new[] {"One day", "One bar"};
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
            double dShift = IndParam.NumParam[0].Value*Point;
            int prvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = 1;
            var adPp = new double[Bars];
            var adR1 = new double[Bars];
            var adR2 = new double[Bars];
            var adR3 = new double[Bars];
            var adS1 = new double[Bars];
            var adS2 = new double[Bars];
            var adS3 = new double[Bars];

            var adH = new double[Bars];
            var adL = new double[Bars];
            var adC = new double[Bars];

            if (IndParam.ListParam[1].Text == "One bar" ||
                Period == DataPeriod.D1 || Period == DataPeriod.W1)
            {
                adH = High;
                adL = Low;
                adC = Close;
            }
            else
            {
                prvs = 0;

                adH[0] = 0;
                adL[0] = 0;
                adC[0] = 0;

                double dTop = double.MinValue;
                double dBottom = double.MaxValue;

                for (int iBar = 1; iBar < Bars; iBar++)
                {
                    if (High[iBar - 1] > dTop)
                        dTop = High[iBar - 1];
                    if (Low[iBar - 1] < dBottom)
                        dBottom = Low[iBar - 1];

                    if (Time[iBar].Day != Time[iBar - 1].Day)
                    {
                        adH[iBar] = dTop;
                        adL[iBar] = dBottom;
                        adC[iBar] = Close[iBar - 1];
                        dTop = double.MinValue;
                        dBottom = double.MaxValue;
                    }
                    else
                    {
                        adH[iBar] = adH[iBar - 1];
                        adL[iBar] = adL[iBar - 1];
                        adC[iBar] = adC[iBar - 1];
                    }
                }

                // first Bar
                for (int iBar = 1; iBar < Bars; iBar++)
                {
                    if (Time[iBar].Day != Time[iBar - 1].Day)
                    {
                        firstBar = iBar;
                        break;
                    }
                }
            }

            for (int iBar = firstBar; iBar < Bars; iBar++)
            {
                adPp[iBar] = (adH[iBar] + adL[iBar] + adC[iBar])/3;
                adR1[iBar] = 2*adPp[iBar] - adL[iBar];
                adS1[iBar] = 2*adPp[iBar] - adH[iBar];
                adR2[iBar] = adPp[iBar] + adH[iBar] - adL[iBar];
                adS2[iBar] = adPp[iBar] - adH[iBar] + adL[iBar];
                adR3[iBar] = adH[iBar] + 2*(adPp[iBar] - adL[iBar]);
                adS3[iBar] = adL[iBar] - 2*(adH[iBar] - adPp[iBar]);
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

            Component[0].Value = adR3;
            Component[1].Value = adR2;
            Component[2].Value = adR1;
            Component[3].Value = adPp;
            Component[4].Value = adS1;
            Component[5].Value = adS2;
            Component[6].Value = adS3;

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
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[7].Value[iBar] = adR3[iBar - prvs] + dShift;
                        Component[8].Value[iBar] = adS3[iBar - prvs] - dShift;
                    }
                    break;
                case "Enter long at R2 (short at S2)":
                case "Exit long at R2 (short at S2)":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[7].Value[iBar] = adR2[iBar - prvs] + dShift;
                        Component[8].Value[iBar] = adS2[iBar - prvs] - dShift;
                    }
                    break;
                case "Enter long at R1 (short at S1)":
                case "Exit long at R1 (short at S1)":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[7].Value[iBar] = adR1[iBar - prvs] + dShift;
                        Component[8].Value[iBar] = adS1[iBar - prvs] - dShift;
                    }
                    break;
                    //---------------------------------------------------------------------
                case "Enter the market at the Pivot Point":
                case "Exit the market at the Pivot Point":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[7].Value[iBar] = adPp[iBar - prvs] + dShift;
                        Component[8].Value[iBar] = adPp[iBar - prvs] - dShift;
                    }
                    break;
                    //---------------------------------------------------------------------
                case "Enter long at S1 (short at R1)":
                case "Exit long at S1 (short at R1)":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[7].Value[iBar] = adS1[iBar - prvs] - dShift;
                        Component[8].Value[iBar] = adR1[iBar - prvs] + dShift;
                    }
                    break;
                case "Enter long at S2 (short at R2)":
                case "Exit long at S2 (short at R2)":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[7].Value[iBar] = adS2[iBar - prvs] - dShift;
                        Component[8].Value[iBar] = adR2[iBar - prvs] + dShift;
                    }
                    break;
                case "Enter long at S3 (short at R3)":
                case "Exit long at S3 (short at R3)":
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        Component[7].Value[iBar] = adS3[iBar - prvs] - dShift;
                        Component[8].Value[iBar] = adR3[iBar - prvs] + dShift;
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
                sUpperTrade = "at ";
                sLowerTrade = "at ";
            }
            else
            {
                sUpperTrade = -iShift + " points below the ";
                sLowerTrade = -iShift + " points above the ";
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at R3 (short at S3)":
                    EntryPointLongDescription = sUpperTrade + "Pivot Point Resistance 3 level";
                    EntryPointShortDescription = sLowerTrade + "Pivot Point Support 3 level";
                    break;
                case "Exit long at R3 (short at S3)":
                    ExitPointLongDescription = sUpperTrade + "Pivot Point Resistance 3 level";
                    ExitPointShortDescription = sLowerTrade + "Pivot Point Support 3 level";
                    break;
                case "Enter long at R2 (short at S2)":
                    EntryPointLongDescription = sUpperTrade + "Pivot Point Resistance 2 level";
                    EntryPointShortDescription = sLowerTrade + "Pivot Point Support 2 level";
                    break;
                case "Exit long at R2 (short at S2)":
                    ExitPointLongDescription = sUpperTrade + "Pivot Point Resistance 2 level";
                    ExitPointShortDescription = sLowerTrade + "Pivot Point Support 2 level";
                    break;
                case "Enter long at R1 (short at S1)":
                    EntryPointLongDescription = sUpperTrade + "Pivot Point Resistance 1 level";
                    EntryPointShortDescription = sLowerTrade + "Pivot Point Support 1 level";
                    break;
                case "Exit long at R1 (short at S1)":
                    ExitPointLongDescription = sUpperTrade + "Pivot Point Resistance 1 level";
                    ExitPointShortDescription = sLowerTrade + "Pivot Point Support 1 level";
                    break;
                    //---------------------------------------------------------------------
                case "Enter the market at the Pivot Point":
                    EntryPointLongDescription = sUpperTrade + "Pivot Point";
                    EntryPointShortDescription = sLowerTrade + "Pivot Point";
                    break;
                case "Exit the market at the Pivot Point":
                    ExitPointLongDescription = sUpperTrade + "Pivot Point";
                    ExitPointShortDescription = sLowerTrade + "Pivot Point";
                    break;
                    //---------------------------------------------------------------------
                case "Enter long at S1 (short at R1)":
                    EntryPointLongDescription = sLowerTrade + "Pivot Point Support 1 level";
                    EntryPointShortDescription = sUpperTrade + "Pivot Point Resistance 1 level";
                    break;
                case "Exit long at S1 (short at R1)":
                    ExitPointLongDescription = sLowerTrade + "Pivot Point Support 1 level";
                    ExitPointShortDescription = sUpperTrade + "Pivot Point Resistance 1 level";
                    break;
                case "Enter long at S2 (short at R2)":
                    EntryPointLongDescription = sLowerTrade + "Pivot Point Support 2 level";
                    EntryPointShortDescription = sUpperTrade + "Pivot Point Resistance 2 level";
                    break;
                case "Exit long at S2 (short at R2)":
                    ExitPointLongDescription = sLowerTrade + "Pivot Point Support 2 level";
                    ExitPointShortDescription = sUpperTrade + "Pivot Point Resistance 2 level";
                    break;
                case "Enter long at S3 (short at R3)":
                    EntryPointLongDescription = sLowerTrade + "Pivot Point Support 3 level";
                    EntryPointShortDescription = sUpperTrade + "Pivot Point Resistance 3 level";
                    break;
                case "Exit long at S3 (short at R3)":
                    ExitPointLongDescription = sLowerTrade + "Pivot Point Support 3 level";
                    ExitPointShortDescription = sUpperTrade + "Pivot Point Resistance 3 level";
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