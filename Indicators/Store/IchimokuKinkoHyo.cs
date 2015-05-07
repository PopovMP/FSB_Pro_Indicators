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
    public class IchimokuKinkoHyo : Indicator
    {
        public IchimokuKinkoHyo()
        {
            IndicatorName = "Ichimoku Kinko Hyo";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close;
            SeparatedChart = false;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (SlotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter the market at Tenkan Sen",
                        "Enter the market at Kijun Sen"
                    };
            else if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Tenkan Sen rises",
                        "Kijun Sen rises",
                        "Tenkan Sen is higher than Kijun Sen",
                        "Tenkan Sen crosses Kijun Sen upward",
                        "The bar opens above Tenkan Sen",
                        "The bar opens above Kijun Sen",
                        "Chikou Span is above closing price",
                        "The position opens above Kumo",
                        "The position opens inside or above Kumo",
                        "Tenkan Sen is above Kumo",
                        "Tenkan Sen is inside or above Kumo",
                        "Kijun Sen is above Kumo",
                        "Kijun Sen is inside or above Kumo",
                        "Senkou Span A is higher than Senkou Span B",
                        "Senkou Span A crosses Senkou Span B upward"
                    };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit the market at Tenkan Sen",
                        "Exit the market at Kijun Sen"
                    };
            else
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Not Defined"
                    };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Application of Ichimoku Kinko Hyo.";

            // NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Tenkan";
            IndParam.NumParam[0].Value = 9;
            IndParam.NumParam[0].Min = 6;
            IndParam.NumParam[0].Max = 12;
            IndParam.NumParam[0].Point = 0;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Tenkan Sen period.";

            IndParam.NumParam[2].Caption = "Kijun";
            IndParam.NumParam[2].Value = 26;
            IndParam.NumParam[2].Min = 18;
            IndParam.NumParam[2].Max = 34;
            IndParam.NumParam[2].Point = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "Kijun Sen period, Chikou Span period and Senkou Span shift.";

            IndParam.NumParam[4].Caption = "Senkou Span B";
            IndParam.NumParam[4].Value = 52;
            IndParam.NumParam[4].Min = 36;
            IndParam.NumParam[4].Max = 84;
            IndParam.NumParam[4].Point = 0;
            IndParam.NumParam[4].Enabled = true;
            IndParam.NumParam[4].ToolTip = "Senkou Span B period.";

            // CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use indicator value from previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var tenkan = (int) IndParam.NumParam[0].Value;
            var kijun = (int) IndParam.NumParam[2].Value;
            var senkou = (int) IndParam.NumParam[4].Value;
            int previousBar = IndParam.CheckParam[0].Checked ? 1 : 0;

            int firstBar = 1 + kijun + senkou;

            var adTenkanSen = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
            {
                double highestHigh = double.MinValue;
                double lowestLow = double.MaxValue;
                for (int i = 0; i < tenkan; i++)
                {
                    if (High[bar - i] > highestHigh)
                        highestHigh = High[bar - i];
                    if (Low[bar - i] < lowestLow)
                        lowestLow = Low[bar - i];
                }
                adTenkanSen[bar] = (highestHigh + lowestLow)/2;
            }

            var adKijunSen = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow = double.MaxValue;
                for (int i = 0; i < kijun; i++)
                {
                    if (High[bar - i] > dHighestHigh)
                        dHighestHigh = High[bar - i];
                    if (Low[bar - i] < dLowestLow)
                        dLowestLow = Low[bar - i];
                }
                adKijunSen[bar] = (dHighestHigh + dLowestLow)/2;
            }

            var adChikouSpan = new double[Bars];
            for (int bar = 0; bar < Bars - kijun; bar++)
            {
                adChikouSpan[bar] = Close[bar + kijun];
            }

            var adSenkouSpanA = new double[Bars];
            for (int bar = firstBar; bar < Bars - kijun; bar++)
            {
                adSenkouSpanA[bar + kijun] = (adTenkanSen[bar] + adKijunSen[bar])/2;
            }

            var adSenkouSpanB = new double[Bars];
            for (int bar = firstBar; bar < Bars - kijun; bar++)
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow = double.MaxValue;
                for (int i = 0; i < senkou; i++)
                {
                    if (High[bar - i] > dHighestHigh)
                        dHighestHigh = High[bar - i];
                    if (Low[bar - i] < dLowestLow)
                        dLowestLow = Low[bar - i];
                }
                adSenkouSpanB[bar + kijun] = (dHighestHigh + dLowestLow)/2;
            }

            // Saving components
            Component = SlotType == SlotTypes.OpenFilter ? new IndicatorComp[7] : new IndicatorComp[6];

            Component[0] = new IndicatorComp
                {
                    CompName = "Tenkan Sen",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Red,
                    FirstBar = firstBar,
                    Value = adTenkanSen
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Kijun Sen",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = firstBar,
                    Value = adKijunSen
                };

            Component[2] = new IndicatorComp
                {
                    CompName = "Chikou Span",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Green,
                    FirstBar = firstBar,
                    Value = adChikouSpan
                };

            Component[3] = new IndicatorComp
                {
                    CompName = "Senkou Span A",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.CloudUp,
                    ChartColor = Color.SandyBrown,
                    FirstBar = firstBar,
                    Value = adSenkouSpanA
                };

            Component[4] = new IndicatorComp
                {
                    CompName = "Senkou Span B",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.CloudDown,
                    ChartColor = Color.Thistle,
                    FirstBar = firstBar,
                    Value = adSenkouSpanB
                };

            Component[5] = new IndicatorComp
                {
                    FirstBar = firstBar,
                    Value = new double[Bars],
                    DataType = IndComponentType.Other
                };

            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[5].CompName = "Is long entry allowed";
                Component[5].DataType = IndComponentType.AllowOpenLong;

                Component[6] = new IndicatorComp
                    {
                        FirstBar = firstBar,
                        Value = new double[Bars],
                        CompName = "Is short entry allowed",
                        DataType = IndComponentType.AllowOpenShort
                    };
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter the market at Tenkan Sen":
                    Component[5].CompName = "Tenkan Sen entry price";
                    Component[5].DataType = IndComponentType.OpenPrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adTenkanSen[bar - previousBar];
                    }
                    break;
                case "Enter the market at Kijun Sen":
                    Component[5].CompName = "Kijun Sen entry price";
                    Component[5].DataType = IndComponentType.OpenPrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adKijunSen[bar - previousBar];
                    }
                    break;
                case "Exit the market at Tenkan Sen":
                    Component[5].CompName = "Tenkan Sen exit price";
                    Component[5].DataType = IndComponentType.ClosePrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adTenkanSen[bar - previousBar];
                    }
                    break;
                case "Exit the market at Kijun Sen":
                    Component[5].CompName = "Kijun Sen exit price";
                    Component[5].DataType = IndComponentType.ClosePrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adKijunSen[bar - previousBar];
                    }
                    break;
                case "Tenkan Sen rises":
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adTenkanSen[bar - previousBar] >
                                                  adTenkanSen[bar - previousBar - 1] + Sigma()
                                                      ? 1
                                                      : 0;
                        Component[6].Value[bar] = adTenkanSen[bar - previousBar] <
                                                  adTenkanSen[bar - previousBar - 1] - Sigma()
                                                      ? 1
                                                      : 0;
                    }
                    break;
                case "Kijun Sen rises":
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adKijunSen[bar - previousBar] >
                                                  adKijunSen[bar - previousBar - 1] + Sigma()
                                                      ? 1
                                                      : 0;
                        Component[6].Value[bar] = adKijunSen[bar - previousBar] <
                                                  adKijunSen[bar - previousBar - 1] - Sigma()
                                                      ? 1
                                                      : 0;
                    }
                    break;
                case "Tenkan Sen is higher than Kijun Sen":
                    IndicatorIsHigherThanAnotherIndicatorLogic(firstBar, previousBar, adTenkanSen, adKijunSen,
                                                               ref Component[5], ref Component[6]);
                    break;
                case "Tenkan Sen crosses Kijun Sen upward":
                    IndicatorCrossesAnotherIndicatorUpwardLogic(firstBar, previousBar, adTenkanSen, adKijunSen,
                                                                ref Component[5], ref Component[6]);
                    break;
                case "The bar opens above Tenkan Sen":
                    BarOpensAboveIndicatorLogic(firstBar, previousBar, adTenkanSen, ref Component[5], ref Component[6]);
                    break;
                case "The bar opens above Kijun Sen":
                    BarOpensAboveIndicatorLogic(firstBar, previousBar, adKijunSen, ref Component[5], ref Component[6]);
                    break;
                case "Chikou Span is above closing price":
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adChikouSpan[bar - kijun - previousBar] >
                                                  Close[bar - kijun - previousBar] + Sigma()
                                                      ? 1
                                                      : 0;
                        Component[6].Value[bar] = adChikouSpan[bar - kijun - previousBar] <
                                                  Close[bar - kijun - previousBar] - Sigma()
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case "The position opens above Kumo":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = Math.Max(adSenkouSpanA[bar], adSenkouSpanB[bar]);
                        Component[6].Value[bar] = Math.Min(adSenkouSpanA[bar], adSenkouSpanB[bar]);
                    }
                    Component[5].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[5].DataType = IndComponentType.Other;
                    Component[5].UsePreviousBar = previousBar;
                    Component[5].ShowInDynInfo = false;

                    Component[6].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[6].DataType = IndComponentType.Other;
                    Component[6].UsePreviousBar = previousBar;
                    Component[6].ShowInDynInfo = false;
                    break;

                case "The position opens inside or above Kumo":
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = Math.Min(adSenkouSpanA[bar], adSenkouSpanB[bar]);
                        Component[6].Value[bar] = Math.Max(adSenkouSpanA[bar], adSenkouSpanB[bar]);
                    }
                    Component[5].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[5].DataType = IndComponentType.Other;
                    Component[5].UsePreviousBar = previousBar;
                    Component[5].ShowInDynInfo = false;

                    Component[6].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[6].DataType = IndComponentType.Other;
                    Component[6].UsePreviousBar = previousBar;
                    Component[6].ShowInDynInfo = false;
                    break;

                case "Tenkan Sen is above Kumo":
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adTenkanSen[bar - previousBar] >
                                                  Math.Max(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) + Sigma()
                                                      ? 1
                                                      : 0;
                        Component[6].Value[bar] = adTenkanSen[bar - previousBar] <
                                                  Math.Min(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) - Sigma()
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case "Tenkan Sen is inside or above Kumo":
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adTenkanSen[bar - previousBar] >
                                                  Math.Min(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) + Sigma()
                                                      ? 1
                                                      : 0;
                        Component[6].Value[bar] = adTenkanSen[bar - previousBar] <
                                                  Math.Max(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) - Sigma()
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case "Kijun Sen is above Kumo":
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adKijunSen[bar - previousBar] >
                                                  Math.Max(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) + Sigma()
                                                      ? 1
                                                      : 0;
                        Component[6].Value[bar] = adKijunSen[bar - previousBar] <
                                                  Math.Min(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) - Sigma()
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case "Kijun Sen is inside or above Kumo":
                    for (int bar = firstBar + previousBar; bar < Bars; bar++)
                    {
                        Component[5].Value[bar] = adKijunSen[bar - previousBar] >
                                                  Math.Min(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) + Sigma()
                                                      ? 1
                                                      : 0;
                        Component[6].Value[bar] = adKijunSen[bar - previousBar] <
                                                  Math.Max(adSenkouSpanA[bar - previousBar],
                                                           adSenkouSpanB[bar - previousBar]) - Sigma()
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case "Senkou Span A is higher than Senkou Span B":
                    IndicatorIsHigherThanAnotherIndicatorLogic(firstBar, previousBar, adSenkouSpanA, adSenkouSpanB,
                                                               ref Component[5], ref Component[6]);
                    break;

                case "Senkou Span A crosses Senkou Span B upward":
                    IndicatorCrossesAnotherIndicatorUpwardLogic(firstBar, previousBar, adSenkouSpanA, adSenkouSpanB,
                                                                ref Component[5], ref Component[6]);
                    break;
            }
        }

        public override void SetDescription()
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter the market at Tenkan Sen":
                    EntryPointLongDescription = "at Tenkan Sen of " + ToString();
                    EntryPointShortDescription = "at Tenkan Sen of " + ToString();
                    break;
                case "Enter the market at Kijun Sen":
                    EntryPointLongDescription = "at Kijun Sen of " + ToString();
                    EntryPointShortDescription = "at Kijun Sen of " + ToString();
                    break;
                case "Exit the market at Tenkan Sen":
                    ExitPointLongDescription = "at Tenkan Sen of " + ToString();
                    ExitPointShortDescription = "at Tenkan Sen of " + ToString();
                    break;
                case "Exit the market at Kijun Sen":
                    ExitPointLongDescription = "at Kijun Sen of " + ToString();
                    ExitPointShortDescription = "at Kijun Sen of " + ToString();
                    break;
                case "Tenkan Sen rises":
                    EntryFilterLongDescription = "Tenkan Sen of " + ToString() + " rises";
                    EntryFilterShortDescription = "Tenkan Sen of " + ToString() + " fals";
                    break;
                case "Kijun Sen rises":
                    EntryFilterLongDescription = "Kijun Sen of " + ToString() + " rises";
                    EntryFilterShortDescription = "Kijun Sen of " + ToString() + " fals";
                    break;
                case "Tenkan Sen is higher than Kijun Sen":
                    EntryFilterLongDescription = ToString() + " - Tenkan Sen is higher than Kijun Sen";
                    EntryFilterShortDescription = ToString() + " - Tenkan Sen is lower than Kijun Sen";
                    break;
                case "Tenkan Sen crosses Kijun Sen upward":
                    EntryFilterLongDescription = ToString() + " - Tenkan Sen crosses Kijun Sen upward";
                    EntryFilterShortDescription = ToString() + " - Tenkan Sen crosses Kijun Sen downward";
                    break;
                case "The bar opens above Tenkan Sen":
                    EntryFilterLongDescription = "bar opens above Tenkan Sen of " + ToString();
                    EntryFilterShortDescription = "bar opens below Tenkan Sen of " + ToString();
                    break;
                case "The bar opens above Kijun Sen":
                    EntryFilterLongDescription = "bar opens above Kijun Sen of " + ToString();
                    EntryFilterShortDescription = "bar opens below Kijun Sen of " + ToString();
                    break;
                case "Chikou Span is above closing price":
                    EntryFilterLongDescription = "Chikou Span of " + ToString() +
                                                 " is above closing price of corresponding bar";
                    EntryFilterShortDescription = "Chikou Span of " + ToString() +
                                                  " is below closing price of corresponding bar";
                    break;
                case "The position opens above Kumo":
                    EntryFilterLongDescription = "position opens above Kumo of " + ToString();
                    EntryFilterShortDescription = "position opens below Kumo of " + ToString();
                    break;
                case "The position opens inside or above Kumo":
                    EntryFilterLongDescription = "position opens inside or above Kumo of " + ToString();
                    EntryFilterShortDescription = "position opens inside or below Kumo of " + ToString();
                    break;
                case "Tenkan Sen is above Kumo":
                    EntryFilterLongDescription = ToString() + " - Tenkan Sen is above Kumo";
                    EntryFilterShortDescription = ToString() + " - Tenkan Sen is below Kumo";
                    break;
                case "Tenkan Sen is inside or above Kumo":
                    EntryFilterLongDescription = ToString() + " - Tenkan Sen is inside or above Kumo";
                    EntryFilterShortDescription = ToString() + " - Tenkan Sen is inside or below Kumo";
                    break;
                case "Kijun Sen is above Kumo":
                    EntryFilterLongDescription = ToString() + " - Kijun Sen is above Kumo";
                    EntryFilterShortDescription = ToString() + " - Kijun Sen is below Kumo";
                    break;
                case "Kijun Sen is inside or above Kumo":
                    EntryFilterLongDescription = ToString() + " - Kijun Sen is inside or above Kumo";
                    EntryFilterShortDescription = ToString() + " - Kijun Sen is inside or below Kumo";
                    break;
                case "Senkou Span A is higher than Senkou Span B":
                    EntryFilterLongDescription = ToString() + " - Senkou Span A is higher than Senkou Span B";
                    EntryFilterShortDescription = ToString() + " - Senkou Span A is lower than Senkou Span B";
                    break;
                case "Senkou Span A crosses Senkou Span B upward":
                    EntryFilterLongDescription = ToString() + " - Senkou Span A crosses Senkou Span B upward";
                    EntryFilterShortDescription = ToString() + " - Senkou Span A crosses Senkou Span B downward";
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}{1} ({2}, {3}, {4})",
                                 IndicatorName,
                                 (IndParam.CheckParam[0].Checked ? "*" : ""),
                                 IndParam.NumParam[0].ValueToString,
                                 IndParam.NumParam[2].ValueToString,
                                 IndParam.NumParam[4].ValueToString);
        }
    }
}