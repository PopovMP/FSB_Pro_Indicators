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
    public class StarcBands : Indicator
    {
        public StarcBands()
        {
            // General properties
            IndicatorName = "Starc Bands";
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
                        "Enter long at Upper Band",
                        "Enter long at Lower Band"
                    };
            else if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The bar opens below Upper Band",
                        "The bar opens above Upper Band",
                        "The bar opens below Lower Band",
                        "The bar opens above Lower Band",
                        "The position opens below Upper Band",
                        "The position opens above Upper Band",
                        "The position opens below Lower Band",
                        "The position opens above Lower Band",
                        "The bar opens below Upper Band after opening above it",
                        "The bar opens above Upper Band after opening below it",
                        "The bar opens below Lower Band after opening above it",
                        "The bar opens above Lower Band after opening below it"
                    };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit long at Upper Band",
                        "Exit long at Lower Band"
                    };
            else if (SlotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The bar closes below Upper Band",
                        "The bar closes above Upper Band",
                        "The bar closes below Lower Band",
                        "The bar closes above Lower Band"
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

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Simple;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The method of smoothing of central Moving Average.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = new[] {"Close"};
            IndParam.ListParam[2].Index = 0;
            IndParam.ListParam[2].Text = "Close";
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the central Moving Average is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "MA period";
            IndParam.NumParam[0].Value = 20;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The central Moving Average period.";

            IndParam.NumParam[1].Caption = "Multiplier";
            IndParam.NumParam[1].Value = 2;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 5;
            IndParam.NumParam[1].Point = 2;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Determines the width of the Starc Bands.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var maMethod = (MAMethod) IndParam.ListParam[1].Index;
            const BasePrice price = BasePrice.Close;
            var maPeriod = (int) IndParam.NumParam[0].Value;
            double multiplier = IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            double[] maPrice = Price(price);
            double[] movingAverage = MovingAverage(maPeriod, 0, maMethod, maPrice);
            var upperBand = new double[Bars];
            var lowerBand = new double[Bars];

            int firstBar = maPeriod + previous + 2;

            var averageTrueRange = new double[Bars];

            for (int bar = 1; bar < Bars; bar++)
            {
                averageTrueRange[bar] = Math.Max(Math.Abs(High[bar] - Close[bar - 1]), Math.Abs(Close[bar - 1] - Low[bar]));
                averageTrueRange[bar] = Math.Max(Math.Abs(High[bar] - Low[bar]), averageTrueRange[bar]);
            }

            averageTrueRange = MovingAverage(maPeriod, 0, maMethod, averageTrueRange);

            for (int bar = maPeriod; bar < Bars; bar++)
            {
                upperBand[bar] = movingAverage[bar] + multiplier*averageTrueRange[bar];
                lowerBand[bar] = movingAverage[bar] - multiplier*averageTrueRange[bar];
            }

            // Saving the components
            Component = new IndicatorComp[5];

            Component[0] = new IndicatorComp
                {
                    CompName = "Upper Band",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = firstBar,
                    Value = upperBand
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Moving Average",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Gold,
                    FirstBar = firstBar,
                    Value = movingAverage
                };

            Component[2] = new IndicatorComp
                {
                    CompName = "Lower Band",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = firstBar,
                    Value = lowerBand
                };

            Component[3] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            Component[4] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            // Sets the Component's type
            if (SlotType == SlotTypes.Open)
            {
                Component[3].DataType = IndComponentType.OpenLongPrice;
                Component[3].CompName = "Long position entry price";
                Component[4].DataType = IndComponentType.OpenShortPrice;
                Component[4].CompName = "Short position entry price";
            }
            else if (SlotType == SlotTypes.OpenFilter)
            {
                Component[3].DataType = IndComponentType.AllowOpenLong;
                Component[3].CompName = "Is long entry allowed";
                Component[4].DataType = IndComponentType.AllowOpenShort;
                Component[4].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.Close)
            {
                Component[3].DataType = IndComponentType.CloseLongPrice;
                Component[3].CompName = "Long position closing price";
                Component[4].DataType = IndComponentType.CloseShortPrice;
                Component[4].CompName = "Short position closing price";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[3].DataType = IndComponentType.ForceCloseLong;
                Component[3].CompName = "Close out long position";
                Component[4].DataType = IndComponentType.ForceCloseShort;
                Component[4].CompName = "Close out short position";
            }

            if (SlotType == SlotTypes.Open || SlotType == SlotTypes.Close)
            {
                if (maPeriod > 1)
                {
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        // Covers the cases when the price can pass through the band without a signal.
                        double open = Open[bar]; // Current open price

                        // Upper band
                        double valueUp = upperBand[bar - previous]; // Current value
                        double valueUp1 = upperBand[bar - previous - 1]; // Previous value
                        double tempValUp = valueUp;

                        if ((valueUp1 > High[bar - 1] && valueUp < open) ||
                            // The Open price jumps above the indicator
                            (valueUp1 < Low[bar - 1] && valueUp > open) ||
                            // The Open price jumps below the indicator
                            (Close[bar - 1] < valueUp && valueUp < open) || // The Open price is in a positive gap
                            (Close[bar - 1] > valueUp && valueUp > open)) // The Open price is in a negative gap
                            tempValUp = open; // The entry/exit level is moved to Open price

                        // Lower band
                        double valueDown = lowerBand[bar - previous]; // Current value
                        double valueDown1 = lowerBand[bar - previous - 1]; // Previous value
                        double tempValDown = valueDown;

                        if ((valueDown1 > High[bar - 1] && valueDown < open) ||
                            // The Open price jumps above the indicator
                            (valueDown1 < Low[bar - 1] && valueDown > open) ||
                            // The Open price jumps below the indicator
                            (Close[bar - 1] < valueDown && valueDown < open) ||
                            // The Open price is in a positive gap
                            (Close[bar - 1] > valueDown && valueDown > open)) // The Open price is in a negative gap
                            tempValDown = open; // The entry/exit level is moved to Open price

                        if (IndParam.ListParam[0].Text == "Enter long at Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at Upper Band")
                        {
                            Component[3].Value[bar] = tempValUp;
                            Component[4].Value[bar] = tempValDown;
                        }
                        else
                        {
                            Component[3].Value[bar] = tempValDown;
                            Component[4].Value[bar] = tempValUp;
                        }
                    }
                }
                else
                {
                    for (int bar = 2; bar < Bars; bar++)
                    {
                        if (IndParam.ListParam[0].Text == "Enter long at Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at Upper Band")
                        {
                            Component[3].Value[bar] = upperBand[bar - previous];
                            Component[4].Value[bar] = lowerBand[bar - previous];
                        }
                        else
                        {
                            Component[3].Value[bar] = lowerBand[bar - previous];
                            Component[4].Value[bar] = upperBand[bar - previous];
                        }
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Upper_Band);
                        break;

                    case "The bar opens above Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Upper_Band);
                        break;

                    case "The bar opens below Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Lower_Band);
                        break;

                    case "The bar opens above Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Lower_Band);
                        break;

                    case "The bar opens below Upper Band after opening above it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Upper Band after opening below it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it);
                        break;

                    case "The bar opens below Lower Band after opening above it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Lower Band after opening below it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it);
                        break;

                    case "The position opens above Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The position opens below Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The position opens above Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The position opens below Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The bar closes below Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_closes_below_the_Upper_Band);
                        break;

                    case "The bar closes above Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_closes_above_the_Upper_Band);
                        break;

                    case "The bar closes below Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_closes_below_the_Lower_Band);
                        break;

                    case "The bar closes above Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_closes_above_the_Lower_Band);
                        break;
                }
            }
        }

        public override void SetDescription()
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at Upper Band":
                    EntryPointLongDescription = "at Upper Band of " + ToString();
                    EntryPointShortDescription = "at Lower Band of " + ToString();
                    break;

                case "Enter long at Lower Band":
                    EntryPointLongDescription = "at Lower Band of " + ToString();
                    EntryPointShortDescription = "at Upper Band of " + ToString();
                    break;

                case "Exit long at Upper Band":
                    ExitPointLongDescription = "at Upper Band of " + ToString();
                    ExitPointShortDescription = "at Lower Band of " + ToString();
                    break;

                case "Exit long at Lower Band":
                    ExitPointLongDescription = "at Lower Band of " + ToString();
                    ExitPointShortDescription = "at Upper Band of " + ToString();
                    break;

                case "The bar opens below Upper Band":
                    EntryFilterLongDescription = "the bar opens below Upper Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens above Lower Band of " + ToString();
                    break;

                case "The bar opens above Upper Band":
                    EntryFilterLongDescription = "the bar opens above Upper Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens below Lower Band of " + ToString();
                    break;

                case "The bar opens below Lower Band":
                    EntryFilterLongDescription = "the bar opens below Lower Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens above Upper Band of " + ToString();
                    break;

                case "The bar opens above Lower Band":
                    EntryFilterLongDescription = "the bar opens above Lower Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens below Upper Band of " + ToString();
                    break;

                case "The position opens above Upper Band":
                    EntryFilterLongDescription = "the position opening price is higher than Upper Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than Lower Band of " + ToString();
                    break;

                case "The position opens below Upper Band":
                    EntryFilterLongDescription = "the position opening price is lower than Upper Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than Lower Band of " +
                                                  ToString();
                    break;

                case "The position opens above Lower Band":
                    EntryFilterLongDescription = "the position opening price is higher than Lower Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than Upper Band of " + ToString();
                    break;

                case "The bar opens below Upper Band after opening above it":
                    EntryFilterLongDescription = "the bar opens below Upper Band of " + ToString() +
                                                 " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above Lower Band of " + ToString() +
                                                  " after the previous bar has opened below it";
                    break;

                case "The bar opens above Upper Band after opening below it":
                    EntryFilterLongDescription = "the bar opens above Upper Band of " + ToString() +
                                                 " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below Lower Band of " + ToString() +
                                                  " after the previous bar has opened above it";
                    break;

                case "The bar opens below Lower Band after opening above it":
                    EntryFilterLongDescription = "the bar opens below Lower Band of " + ToString() +
                                                 " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above Upper Band of " + ToString() +
                                                  " after the previous bar has opened below it";
                    break;

                case "The bar opens above Lower Band after opening below it":
                    EntryFilterLongDescription = "the bar opens above Lower Band of " + ToString() +
                                                 " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below Upper Band of " + ToString() +
                                                  " after the previous bar has opened above it";
                    break;

                case "The position opens below Lower Band":
                    EntryFilterLongDescription = "the position opening price is lower than Lower Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than Upper Band of " +
                                                  ToString();
                    break;

                case "The bar closes below Upper Band":
                    ExitFilterLongDescription = "the bar closes below Upper Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes above Lower Band of " + ToString();
                    break;

                case "The bar closes above Upper Band":
                    ExitFilterLongDescription = "the bar closes above Upper Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes below Lower Band of " + ToString();
                    break;

                case "The bar closes below Lower Band":
                    ExitFilterLongDescription = "the bar closes below Lower Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes above Upper Band of " + ToString();
                    break;

                case "The bar closes above Lower Band":
                    ExitFilterLongDescription = "the bar closes above Lower Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes below Upper Band of " + ToString();
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Smoothing method
                   IndParam.ListParam[2].Text + ", " + // Base price
                   IndParam.NumParam[0].ValueToString + ", " + // MA period
                   IndParam.NumParam[1].ValueToString + ")"; // Multiplier
        }
    }
}