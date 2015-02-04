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
    public class HourlyHighLow : Indicator
    {
        public HourlyHighLow()
        {
            IndicatorName = "Hourly High Low";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.1";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (SlotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter long at the hourly high",
                        "Enter long at the hourly low"
                    };
            else if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The position opens above the hourly high",
                        "The position opens below the hourly high",
                        "The position opens above the hourly low",
                        "The position opens below the hourly low"
                    };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit long at the hourly high",
                        "Exit long at the hourly low"
                    };
            else if (SlotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The bar closes above the hourly high",
                        "The bar closes below the hourly high",
                        "The bar closes above the hourly low",
                        "The bar closes below the hourly low"
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
            IndParam.ListParam[1].ItemList = new[] {"High and Low"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Used price from the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Start hour (incl.)";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 24;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The starting hour of the period.";

            IndParam.NumParam[1].Caption = "Start minutes (incl.)";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 59;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The starting minutes of the period.";

            IndParam.NumParam[2].Caption = "End hour (excl.)";
            IndParam.NumParam[2].Value = 24;
            IndParam.NumParam[2].Min = 0;
            IndParam.NumParam[2].Max = 24;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The ending hour of the period.";

            IndParam.NumParam[3].Caption = "End minutes (excl.)";
            IndParam.NumParam[3].Value = 0;
            IndParam.NumParam[3].Min = 0;
            IndParam.NumParam[3].Max = 59;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "The ending minutes of the period.";

            IndParam.NumParam[4].Caption = "Vertical shift";
            IndParam.NumParam[4].Value = 0;
            IndParam.NumParam[4].Min = -2000;
            IndParam.NumParam[4].Max = +2000;
            IndParam.NumParam[4].Enabled = true;
            IndParam.NumParam[4].ToolTip = "A vertical shift above the high and below the low price.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            var fromHour  = (int) IndParam.NumParam[0].Value;
            var fromMin   = (int) IndParam.NumParam[1].Value;
            var untilHour = (int) IndParam.NumParam[2].Value;
            var untilMin  = (int) IndParam.NumParam[3].Value;

            var fromTime = new TimeSpan(fromHour,  fromMin,  0);
            var toTime   = new TimeSpan(untilHour, untilMin, 0);

            double shift = IndParam.NumParam[4].Value*Point;

            const int firstBar = 2;

            // Calculation
            var highPrice = new double[Bars];
            var lowPrice  = new double[Bars];

            double minPrice = Double.MaxValue;
            double maxPrice = Double.MinValue;
            highPrice[0] = 0;
            lowPrice[0]  = 0;

            bool isOnTimePrev = false;
            for (int bar = 1; bar < Bars; bar++)
            {
                bool isOnTime;

                TimeSpan barTime = Time[bar].TimeOfDay;
                if (fromTime < toTime)
                    isOnTime = barTime >= fromTime && barTime < toTime;
                else if (fromTime > toTime)
                    isOnTime = barTime >= fromTime || barTime < toTime;
                else
                    isOnTime = barTime != toTime;

                if (isOnTime)
                {
                    if (maxPrice < High[bar]) maxPrice = High[bar];
                    if (minPrice > Low[bar])  minPrice = Low[bar];
                }

                if (!isOnTime && isOnTimePrev)
                {
                    highPrice[bar] = maxPrice;
                    lowPrice[bar]  = minPrice;
                    maxPrice = Double.MinValue;
                    minPrice = Double.MaxValue;
                }
                else
                {
                    highPrice[bar] = highPrice[bar - 1];
                    lowPrice[bar]  = lowPrice[bar - 1];
                }

                isOnTimePrev = isOnTime;
            }

            // Shifting the price
            var upperBand = new double[Bars];
            var lowerBand = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
            {
                upperBand[bar] = highPrice[bar] + shift;
                lowerBand[bar] = lowPrice[bar]  - shift;
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
                {
                    CompName = "Hourly High",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Level,
                    ChartColor = Color.DarkGreen,
                    FirstBar = firstBar,
                    Value = highPrice
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Hourly Low",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Level,
                    ChartColor = Color.DarkRed,
                    FirstBar = firstBar,
                    Value = lowPrice
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
                case "Enter long at the hourly high":
                case "Exit long at the hourly high":
                    Component[2].Value = upperBand;
                    Component[3].Value = lowerBand;
                    break;
                case "Enter long at the hourly low":
                case "Exit long at the hourly low":
                    Component[2].Value = lowerBand;
                    Component[3].Value = upperBand;
                    break;
                case "The bar closes below the hourly high":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_below_the_Upper_Band);
                    break;
                case "The bar closes above the hourly high":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_above_the_Upper_Band);
                    break;
                case "The bar closes below the hourly low":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_below_the_Lower_Band);
                    break;
                case "The bar closes above the hourly low":
                    BandIndicatorLogic(firstBar, 0, upperBand, lowerBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_above_the_Lower_Band);
                    break;
                case "The position opens above the hourly high":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted hourly high";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[3].CompName = "Shifted hourly low";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[2].Value = upperBand;
                    Component[3].Value = lowerBand;
                    break;
                case "The position opens below the hourly high":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted hourly high";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                    Component[3].CompName = "Shifted hourly low";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                    Component[2].Value = upperBand;
                    Component[3].Value = lowerBand;
                    break;
                case "The position opens above the hourly low":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted hourly low";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[3].CompName = "Shifted hourly high";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[2].Value = lowerBand;
                    Component[3].Value = upperBand;
                    break;
                case "The position opens below the hourly low":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted hourly low";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                    Component[3].CompName = "Shifted hourly high";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                    Component[2].Value = lowerBand;
                    Component[3].Value = upperBand;
                    break;
            }
        }

        public override void SetDescription()
        {
            var shift = (int) IndParam.NumParam[4].Value;

            var fromHour = (int) IndParam.NumParam[0].Value;
            var fromMin  = (int) IndParam.NumParam[1].Value;
            var toHour   = (int) IndParam.NumParam[2].Value;
            var toMin    = (int) IndParam.NumParam[3].Value;

            string fromTime = fromHour.ToString("00") + ":" + fromMin.ToString("00");
            string toTimne  = toHour.ToString("00") + ":" + toMin.ToString("00");
            string interval = "(" + fromTime + " - " + toTimne + ")";

            string upperTrade;
            string lowerTrade;

            if (shift > 0)
            {
                upperTrade = shift + " points above the ";
                lowerTrade = shift + " points below the ";
            }
            else if (shift == 0)
            {
                if (IndParam.ListParam[0].Text == "Enter long at the hourly high" ||
                    IndParam.ListParam[0].Text == "Enter long at the hourly low" ||
                    IndParam.ListParam[0].Text == "Exit long at the hourly high" ||
                    IndParam.ListParam[0].Text == "Exit long at the hourly low")
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

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the hourly high":
                    EntryPointLongDescription = upperTrade + "hourly high " + interval;
                    EntryPointShortDescription = lowerTrade + "hourly low " + interval;
                    break;
                case "Enter long at the hourly low":
                    EntryPointLongDescription = lowerTrade + "hourly low " + interval;
                    EntryPointShortDescription = upperTrade + "hourly high " + interval;
                    break;
                case "Exit long at the hourly high":
                    ExitPointLongDescription = upperTrade + "hourly high " + interval;
                    ExitPointShortDescription = lowerTrade + "hourly low " + interval;
                    break;
                case "Exit long at the hourly low":
                    ExitPointLongDescription = lowerTrade + "hourly low " + interval;
                    ExitPointShortDescription = upperTrade + "hourly high " + interval;
                    break;

                case "The position opens below the hourly high":
                    EntryFilterLongDescription = "the position opens lower than " + upperTrade + "hourly high " + interval;
                    EntryFilterShortDescription = "the position opens higher than " + lowerTrade + "hourly low " + interval;
                    break;
                case "The position opens above the hourly high":
                    EntryFilterLongDescription = "the position opens higher than " + upperTrade + "hourly high " + interval;
                    EntryFilterShortDescription = "the position opens lower than " + lowerTrade + "hourly low " + interval;
                    break;
                case "The position opens below the hourly low":
                    EntryFilterLongDescription = "the position opens lower than " + lowerTrade + "hourly low " + interval;
                    EntryFilterShortDescription = "the position opens higher than " + upperTrade + "hourly high " + interval;
                    break;
                case "The position opens above the hourly low":
                    EntryFilterLongDescription = "the position opens higher than " + lowerTrade + "hourly low " + interval;
                    EntryFilterShortDescription = "the position opens lower than " + upperTrade + "hourly high " + interval;
                    break;

                case "The bar closes below the hourly high":
                    ExitFilterLongDescription = "the bar closes lower than " + upperTrade + "hourly high " + interval;
                    ExitFilterShortDescription = "the bar closes higher than " + lowerTrade + "hourly low " + interval;
                    break;
                case "The bar closes above the hourly high":
                    ExitFilterLongDescription = "the bar closes higher than " + upperTrade + "hourly high " + interval;
                    ExitFilterShortDescription = "the bar closes lower than " + lowerTrade + "hourly low " + interval;
                    break;
                case "The bar closes below the hourly low":
                    ExitFilterLongDescription = "the bar closes lower than " + lowerTrade + "hourly low " + interval;
                    ExitFilterShortDescription = "the bar closes higher than " + upperTrade + "hourly high " + interval;
                    break;
                case "The bar closes above the hourly low":
                    ExitFilterLongDescription = "the bar closes higher than " + lowerTrade + "hourly low " + interval;
                    ExitFilterShortDescription = "the bar closes lower than " + upperTrade + "hourly high " + interval;
                    break;
            }
        }

        public override string ToString()
        {
            var fromHour  = (int) IndParam.NumParam[0].Value;
            var fromMin   = (int) IndParam.NumParam[1].Value;
            var toHour = (int) IndParam.NumParam[2].Value;
            var toMin  = (int) IndParam.NumParam[3].Value;

            string fromTime  = fromHour.ToString("00")  + ":" + fromMin.ToString("00");
            string toTime = toHour.ToString("00") + ":" + toMin.ToString("00");

            return IndicatorName + " (" +
                   fromTime + " - " + // Start time
                   toTime + ", " + // End time
                   IndParam.NumParam[4].ValueToString + ")"; // Vertical shift
        }
    }
}