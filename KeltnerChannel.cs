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
    public class KeltnerChannel : Indicator
    {
        public KeltnerChannel()
        {
            IndicatorName = "Keltner Channel";
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
            IndParam.ListParam[1].Index = (int) MAMethod.Exponential;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The method of smoothing of central Moving Average.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = new[] {"Close"};
            IndParam.ListParam[2].Index = 0;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the central Moving Average is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "MA period";
            IndParam.NumParam[0].Value = 20;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The central Moving Average period.";

            IndParam.NumParam[1].Caption = "ATR period";
            IndParam.NumParam[1].Value = 10;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 50;
            IndParam.NumParam[1].Point = 0;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Average True Range Period.";

            IndParam.NumParam[3].Caption = "ATR multiplier";
            IndParam.NumParam[3].Value = 2;
            IndParam.NumParam[3].Min = 1;
            IndParam.NumParam[3].Max = 10;
            IndParam.NumParam[3].Point = 0;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "Average True Range Multiplier.";

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
            var nMA = (int) IndParam.NumParam[0].Value;
            var atrPeriod = (int) IndParam.NumParam[1].Value;
            var atrMultiplier = (int) IndParam.NumParam[3].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            double[] adMA = MovingAverage(nMA, 0, maMethod, Price(price));
            var adAtr = new double[Bars];
            var adUpBand = new double[Bars];
            var adDnBand = new double[Bars];

            int firstBar = Math.Max(nMA, atrPeriod) + previous + 2;

            for (int iBar = 1; iBar < Bars; iBar++)
            {
                adAtr[iBar] = Math.Max(Math.Abs(High[iBar] - Close[iBar - 1]), Math.Abs(Close[iBar - 1] - Low[iBar]));
                adAtr[iBar] = Math.Max(Math.Abs(High[iBar] - Low[iBar]), adAtr[iBar]);
            }

            adAtr = MovingAverage(atrPeriod, 0, maMethod, adAtr);

            for (int iBar = nMA; iBar < Bars; iBar++)
            {
                adUpBand[iBar] = adMA[iBar] + adAtr[iBar]*atrMultiplier;
                adDnBand[iBar] = adMA[iBar] - adAtr[iBar]*atrMultiplier;
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
                    Value = adUpBand
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Moving Average",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Gold,
                    FirstBar = firstBar,
                    Value = adMA
                };

            Component[2] = new IndicatorComp
                {
                    CompName = "Lower Band",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = firstBar,
                    Value = adDnBand
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
                if (nMA > 1)
                {
                    for (int iBar = firstBar; iBar < Bars; iBar++)
                    {
                        // Covers the cases when the price can pass through the band without a signal.
                        double dOpen = Open[iBar]; // Current open price

                        // Upper band
                        double dValueUp = adUpBand[iBar - previous]; // Current value
                        double dValueUp1 = adUpBand[iBar - previous - 1]; // Previous value
                        double dTempValUp = dValueUp;

                        if ((dValueUp1 > High[iBar - 1] && dValueUp < dOpen) ||
                            // The Open price jumps above the indicator
                            (dValueUp1 < Low[iBar - 1] && dValueUp > dOpen) ||
                            // The Open price jumps below the indicator
                            (Close[iBar - 1] < dValueUp && dValueUp < dOpen) || // The Open price is in a positive gap
                            (Close[iBar - 1] > dValueUp && dValueUp > dOpen)) // The Open price is in a negative gap
                            dTempValUp = dOpen; // The entry/exit level is moved to Open price

                        // Lower band
                        double dValueDown = adDnBand[iBar - previous]; // Current value
                        double dValueDown1 = adDnBand[iBar - previous - 1]; // Previous value
                        double dTempValDown = dValueDown;

                        if ((dValueDown1 > High[iBar - 1] && dValueDown < dOpen) ||
                            // The Open price jumps above the indicator
                            (dValueDown1 < Low[iBar - 1] && dValueDown > dOpen) ||
                            // The Open price jumps below the indicator
                            (Close[iBar - 1] < dValueDown && dValueDown < dOpen) ||
                            // The Open price is in a positive gap
                            (Close[iBar - 1] > dValueDown && dValueDown > dOpen)) // The Open price is in a negative gap
                            dTempValDown = dOpen; // The entry/exit level is moved to Open price

                        if (IndParam.ListParam[0].Text == "Enter long at Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at Upper Band")
                        {
                            Component[3].Value[iBar] = dTempValUp;
                            Component[4].Value[iBar] = dTempValDown;
                        }
                        else
                        {
                            Component[3].Value[iBar] = dTempValDown;
                            Component[4].Value[iBar] = dTempValUp;
                        }
                    }
                }
                else
                {
                    for (int iBar = 2; iBar < Bars; iBar++)
                    {
                        if (IndParam.ListParam[0].Text == "Enter long at Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at Upper Band")
                        {
                            Component[3].Value[iBar] = adUpBand[iBar - previous];
                            Component[4].Value[iBar] = adDnBand[iBar - previous];
                        }
                        else
                        {
                            Component[3].Value[iBar] = adDnBand[iBar - previous];
                            Component[4].Value[iBar] = adUpBand[iBar - previous];
                        }
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below Upper Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Upper_Band);
                        break;

                    case "The bar opens above Upper Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Upper_Band);
                        break;

                    case "The bar opens below Lower Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Lower_Band);
                        break;

                    case "The bar opens above Lower Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Lower_Band);
                        break;

                    case "The bar opens below Upper Band after opening above it":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Upper Band after opening below it":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it);
                        break;

                    case "The bar opens below Lower Band after opening above it":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Lower Band after opening below it":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it);
                        break;

                    case "The position opens above Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[0].UsePreviousBar = previous;
                        Component[2].UsePreviousBar = previous;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The position opens below Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[0].UsePreviousBar = previous;
                        Component[2].UsePreviousBar = previous;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The position opens above Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[0].UsePreviousBar = previous;
                        Component[2].UsePreviousBar = previous;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The position opens below Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[0].UsePreviousBar = previous;
                        Component[2].UsePreviousBar = previous;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;

                    case "The bar closes below Upper Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_closes_below_the_Upper_Band);
                        break;

                    case "The bar closes above Upper Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_closes_above_the_Upper_Band);
                        break;

                    case "The bar closes below Lower Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
                                           BandIndLogic.The_bar_closes_below_the_Lower_Band);
                        break;

                    case "The bar closes above Lower Band":
                        BandIndicatorLogic(firstBar, previous, adUpBand, adDnBand, ref Component[3], ref Component[4],
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

                case "The position opens below Lower Band":
                    EntryFilterLongDescription = "the position opening price is lower than Lower Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than Upper Band of " +
                                                  ToString();
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
                             IndParam.ListParam[1].Text + ", " + // Method
                             IndParam.ListParam[2].Text + ", " + // Price
                             IndParam.NumParam[0].ValueToString + ", " + // MA period
                             IndParam.NumParam[1].ValueToString + ", " + // ATR Period
                             IndParam.NumParam[3].ValueToString + ")"; // ATR Multiplier
        }
    }
}