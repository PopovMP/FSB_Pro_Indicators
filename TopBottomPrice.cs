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
    public class TopBottomPrice : Indicator
    {
        public TopBottomPrice()
        {
            IndicatorName = "Top Bottom Price";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.2";
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
                    "Enter long at the top price",
                    "Enter long at the bottom price"
                };
            else if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                {
                    "The bar opens below the top price",
                    "The bar opens above the top price",
                    "The bar opens below the bottom price",
                    "The bar opens above the bottom price",
                    "The position opens below the top price",
                    "The position opens above the top price",
                    "The position opens below the bottom price",
                    "The position opens above the bottom price"
                };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Exit long at the top price",
                    "Exit long at the bottom price"
                };
            else if (SlotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                {
                    "The bar closes below the top price",
                    "The bar closes above the top price",
                    "The bar closes below the bottom price",
                    "The bar closes above the bottom price"
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
            IndParam.ListParam[1].ItemList = new[] {"High & Low"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Used price from the indicator.";

            IndParam.ListParam[2].Caption = "Base period";
            IndParam.ListParam[2].ItemList = new[] {"Previous bar", "Previous day", "Previous week", "Previous month"};
            IndParam.ListParam[2].Index = 1;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The period, the top/bottom prices are based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Vertical shift";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = -2000;
            IndParam.NumParam[0].Max = +2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "A vertical shift above the top and below the bottom price.";
        }

        private bool IsPeriodChanged(int bar)
        {
            bool isPeriodChanged = false;
            switch (IndParam.ListParam[2].Index)
            {
                case 0: // Previous bar
                    isPeriodChanged = true;
                    break;
                case 1: // Previous day
                    isPeriodChanged = Time[bar].Day != Time[bar - 1].Day;
                    break;
                case 2: // Previous week
                    isPeriodChanged = Period == DataPeriod.W1 ||
                                      Time[bar].DayOfWeek <= DayOfWeek.Wednesday &&
                                      Time[bar - 1].DayOfWeek > DayOfWeek.Wednesday;
                    break;
                case 3: // Previous month
                    isPeriodChanged = Time[bar].Month != Time[bar - 1].Month;
                    break;
            }

            return isPeriodChanged;
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            double shift = IndParam.NumParam[0].Value*Point;
            const int firstBar = 1;

            // Calculation
            var topPrice = new double[Bars];
            var bottomPrice = new double[Bars];

            topPrice[0] = 0;
            bottomPrice[0] = 0;

            double top = double.MinValue;
            double bottom = double.MaxValue;

            for (int bar = 1; bar < Bars; bar++)
            {
                if (High[bar - 1] > top)
                    top = High[bar - 1];
                if (Low[bar - 1] < bottom)
                    bottom = Low[bar - 1];

                if (IsPeriodChanged(bar))
                {
                    topPrice[bar] = top;
                    bottomPrice[bar] = bottom;
                    top = double.MinValue;
                    bottom = double.MaxValue;
                }
                else
                {
                    topPrice[bar] = topPrice[bar - 1];
                    bottomPrice[bar] = bottomPrice[bar - 1];
                }
            }

            var upperBand = new double[Bars];
            var lowerBand = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
            {
                upperBand[bar] = topPrice[bar] + shift;
                lowerBand[bar] = bottomPrice[bar] - shift;
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
            {
                CompName = "Top price",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Level,
                ChartColor = Color.DarkGreen,
                FirstBar = firstBar,
                Value = topPrice
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Bottom price",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Level,
                ChartColor = Color.DarkRed,
                FirstBar = firstBar,
                Value = bottomPrice
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

            // Sets the Component's type
            if (SlotType == SlotTypes.Open)
            {
                Component[2].CompName = "Long position entry price";
                Component[2].DataType = IndComponentType.OpenLongPrice;
                Component[3].CompName = "Short position entry price";
                Component[3].DataType = IndComponentType.OpenShortPrice;
            }
            else if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[3].CompName = "Is short entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
            }
            else if (SlotType == SlotTypes.Close)
            {
                Component[2].CompName = "Long position closing price";
                Component[2].DataType = IndComponentType.CloseLongPrice;
                Component[3].CompName = "Short position closing price";
                Component[3].DataType = IndComponentType.CloseShortPrice;
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[3].CompName = "Close out short position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the top price":
                case "Exit long at the top price":
                    Component[2].Value = upperBand;
                    Component[3].Value = lowerBand;
                    break;
                case "Enter long at the bottom price":
                case "Exit long at the bottom price":
                    Component[2].Value = lowerBand;
                    Component[3].Value = upperBand;
                    break;
                case "The bar opens below the top price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_opens_below_the_Upper_Band);
                    break;
                case "The bar opens above the top price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_opens_above_the_Upper_Band);
                    break;
                case "The bar opens below the bottom price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_opens_below_the_Lower_Band);
                    break;
                case "The bar opens above the bottom price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_opens_above_the_Lower_Band);
                    break;
                case "The bar closes below the top price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_opens_below_the_Upper_Band);
                    break;
                case "The bar closes above the top price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_closes_above_the_Upper_Band);
                    break;
                case "The bar closes below the bottom price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_closes_below_the_Lower_Band);
                    break;
                case "The bar closes above the bottom price":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3],
                        BandIndLogic.The_bar_closes_above_the_Lower_Band);
                    break;
                case "The position opens above the top price":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted top price";
                    Component[2].DataType = IndComponentType.OpenLongPrice;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[3].CompName = "Shifted bottom price";
                    Component[3].DataType = IndComponentType.OpenShortPrice;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[2].Value = upperBand;
                    Component[3].Value = lowerBand;
                    break;
                case "The position opens below the top price":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted top price";
                    Component[2].DataType = IndComponentType.OpenLongPrice;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                    Component[3].CompName = "Shifted bottom price";
                    Component[3].DataType = IndComponentType.OpenShortPrice;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                    Component[2].Value = upperBand;
                    Component[3].Value = lowerBand;
                    break;
                case "The position opens above the bottom price":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted bottom price";
                    Component[2].DataType = IndComponentType.OpenLongPrice;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[3].CompName = "Shifted top price";
                    Component[3].DataType = IndComponentType.OpenShortPrice;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[2].Value = lowerBand;
                    Component[3].Value = upperBand;
                    break;
                case "The position opens below the bottom price":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted bottom price";
                    Component[2].DataType = IndComponentType.OpenLongPrice;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                    Component[3].CompName = "Shifted top price";
                    Component[3].DataType = IndComponentType.OpenShortPrice;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                    Component[2].Value = lowerBand;
                    Component[3].Value = upperBand;
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
                upperTrade = shift + " points above the ";
                lowerTrade = shift + " points below the ";
            }
            else if (shift == 0)
            {
                if (IndParam.ListParam[0].Text == "Enter long at the top price" ||
                    IndParam.ListParam[0].Text == "Enter long at the bottom price" ||
                    IndParam.ListParam[0].Text == "Exit long at the top price" ||
                    IndParam.ListParam[0].Text == "Exit long at the bottom price")
                {
                    upperTrade = "at the ";
                    lowerTrade = "at the ";
                }
                else
                {
                    upperTrade = "the ";
                    lowerTrade = "the ";
                }
            }
            else
            {
                upperTrade = -shift + " points below the ";
                lowerTrade = -shift + " points above the ";
            }

            string period = "of the " + IndParam.ListParam[2].Text.ToLower();
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the top price":
                    EntryPointLongDescription = upperTrade + "top price " + period;
                    EntryPointShortDescription = lowerTrade + "bottom price " + period;
                    break;
                case "Enter long at the bottom price":
                    EntryPointLongDescription = lowerTrade + "bottom price " + period;
                    EntryPointShortDescription = upperTrade + "top price " + period;
                    break;
                case "Exit long at the top price":
                    ExitPointLongDescription = upperTrade + "top price " + period;
                    ExitPointShortDescription = lowerTrade + "bottom price " + period;
                    break;
                case "Exit long at the bottom price":
                    ExitPointLongDescription = lowerTrade + "bottom price " + period;
                    ExitPointShortDescription = upperTrade + "top price " + period;
                    break;

                case "The bar opens below the top price":
                    EntryFilterLongDescription = "the bar opens lower than " + upperTrade + "top price " + period;
                    EntryFilterShortDescription = "the bar opens higher than " + lowerTrade + "bottom price " + period;
                    break;
                case "The bar opens above the top price":
                    EntryFilterLongDescription = "the bar opens higher than " + upperTrade + "top price " + period;
                    EntryFilterShortDescription = "the bar opens lower than " + lowerTrade + "bottom price " + period;
                    break;
                case "The bar opens below the bottom price":
                    EntryFilterLongDescription = "the bar opens lower than " + lowerTrade + "bottom price " + period;
                    EntryFilterShortDescription = "the bar opens higher than " + upperTrade + "top price " + period;
                    break;
                case "The bar opens above the bottom price":
                    EntryFilterLongDescription = "the bar opens higher than " + lowerTrade + "bottom price " + period;
                    EntryFilterShortDescription = "the bar opens lower than " + upperTrade + "top price " + period;
                    break;

                case "The position opens below the top price":
                    EntryFilterLongDescription = "the position opens lower than " + upperTrade + "top price " + period;
                    EntryFilterShortDescription = "the position opens higher than " + lowerTrade + "bottom price " + period;
                    break;
                case "The position opens above the top price":
                    EntryFilterLongDescription = "the position opens higher than " + upperTrade + "top price " + period;
                    EntryFilterShortDescription = "the position opens lower than " + lowerTrade + "bottom price " + period;
                    break;
                case "The position opens below the bottom price":
                    EntryFilterLongDescription = "the position opens lower than " + lowerTrade + "bottom price " + period;
                    EntryFilterShortDescription = "the position opens higher than " + upperTrade + "top price " + period;
                    break;
                case "The position opens above the bottom price":
                    EntryFilterLongDescription = "the position opens higher than " + lowerTrade + "bottom price " + period;
                    EntryFilterShortDescription = "the position opens lower than " + upperTrade + "top price " + period;
                    break;

                case "The bar closes below the top price":
                    ExitFilterLongDescription = "the bar closes lower than " + upperTrade + "top price " + period;
                    ExitFilterShortDescription = "the bar closes higher than " + lowerTrade + "bottom price " + period;
                    break;
                case "The bar closes above the top price":
                    ExitFilterLongDescription = "the bar closes higher than " + upperTrade + "top price " + period;
                    ExitFilterShortDescription = "the bar closes lower than " + lowerTrade + "bottom price " + period;
                    break;
                case "The bar closes below the bottom price":
                    ExitFilterLongDescription = "the bar closes lower than " + lowerTrade + "bottom price " + period;
                    ExitFilterShortDescription = "the bar closes higher than " + upperTrade + "top price " + period;
                    break;
                case "The bar closes above the bottom price":
                    ExitFilterLongDescription = "the bar closes higher than " + lowerTrade + "bottom price " + period;
                    ExitFilterShortDescription = "the bar closes lower than " + upperTrade + "top price " + period;
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.ListParam[2].Text + ", " + // Base period
                   IndParam.NumParam[0].ValueToString + ")"; // Vertical shift
        }
    }
}