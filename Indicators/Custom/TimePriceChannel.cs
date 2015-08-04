//==============================================================
// Forex Strategy Builder
// Copyright © Forex Software Ltd. All rights reserved.
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

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class TimePriceChannel : Indicator
    {
        public TimePriceChannel()
        {
            IndicatorName = "Time Price Channel";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "1.0";
            IndicatorDescription = "Sets a channel based on the price at a specified time.";
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

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Open"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = "Open";
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The reference price is bar Open";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Hour";
            IndParam.NumParam[0].Value = 8;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 23;
            IndParam.NumParam[0].Point = 0;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The hour of the reference time";

            IndParam.NumParam[1].Caption = "Minutes";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 59;
            IndParam.NumParam[1].Point = 0;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The minute of the reference time";

            IndParam.NumParam[2].Caption = "Price deviation [point]";
            IndParam.NumParam[2].Value = 200;
            IndParam.NumParam[2].Min = 5;
            IndParam.NumParam[2].Max = 2000;
            IndParam.NumParam[2].Point = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "Determines the channel width.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            int    hour     = (int)IndParam.NumParam[0].Value;
            int    minute   = (int)IndParam.NumParam[1].Value;
            double margin   = IndParam.NumParam[2].Value * Point;
            int    previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            double[] upperBand = new double[Bars];
            double[] lowerBand = new double[Bars];

            int firstBar = 1;
            for (int bar = 1; bar < Bars; bar++)
            {
                if (Time[bar].Hour == hour && Time[bar].Minute == minute)
                {
                    if (firstBar == 0)
                        firstBar = bar;
                    upperBand[bar] = Open[bar] + margin;
                    lowerBand[bar] = Open[bar] - margin;
                }
                else
                {
                    upperBand[bar] = upperBand[bar-1];
                    lowerBand[bar] = lowerBand[bar-1];
                }
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
            {
                CompName = "Upper Band",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Level,
                ChartColor = Color.Chocolate,
                FirstBar = firstBar,
                Value = upperBand
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Lower Band",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Level,
                ChartColor = Color.Chocolate,
                FirstBar = firstBar,
                Value = lowerBand
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
                Component[2].DataType = IndComponentType.OpenLongPrice;
                Component[2].CompName = "Long position entry price";
                Component[3].DataType = IndComponentType.OpenShortPrice;
                Component[3].CompName = "Short position entry price";
            }
            else if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.Close)
            {
                Component[2].DataType = IndComponentType.CloseLongPrice;
                Component[2].CompName = "Long position closing price";
                Component[3].DataType = IndComponentType.CloseShortPrice;
                Component[3].CompName = "Short position closing price";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }

            if (SlotType == SlotTypes.Open || SlotType == SlotTypes.Close)
            {
                if (IndParam.ListParam[0].Text == "Enter long at Upper Band" ||
                    IndParam.ListParam[0].Text == "Exit long at Upper Band")
                {
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[2].Value[bar] = upperBand[bar - previous];
                        Component[3].Value[bar] = lowerBand[bar - previous];
                    }
                }
                else
                {
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        Component[2].Value[bar] = lowerBand[bar - previous];
                        Component[3].Value[bar] = upperBand[bar - previous];
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_below_the_Upper_Band);
                        break;

                    case "The bar opens above Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_above_the_Upper_Band);
                        break;

                    case "The bar opens below Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_below_the_Lower_Band);
                        break;

                    case "The bar opens above Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_above_the_Lower_Band);
                        break;

                    case "The bar opens below Upper Band after opening above it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Upper Band after opening below it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it);
                        break;

                    case "The bar opens below Lower Band after opening above it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Lower Band after opening below it":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it);
                        break;

                    case "The position opens above Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[0].UsePreviousBar = previous;
                        Component[1].UsePreviousBar = previous;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[0].UsePreviousBar = previous;
                        Component[1].UsePreviousBar = previous;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens above Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[0].UsePreviousBar = previous;
                        Component[1].UsePreviousBar = previous;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[0].UsePreviousBar = previous;
                        Component[1].UsePreviousBar = previous;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The bar closes below Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_closes_below_the_Upper_Band);
                        break;

                    case "The bar closes above Upper Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_closes_above_the_Upper_Band);
                        break;

                    case "The bar closes below Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
                            BandIndLogic.The_bar_closes_below_the_Lower_Band);
                        break;

                    case "The bar closes above Lower Band":
                        BandIndicatorLogic(firstBar, previous, upperBand, lowerBand, ref Component[2], ref Component[3],
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
                   IndParam.NumParam[0].ValueToString + ", " +
                   IndParam.NumParam[1].ValueToString + ", " +
                   IndParam.NumParam[2].ValueToString + ")";
        }
    }
}