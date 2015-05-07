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
    public class DonchianChannel : Indicator
    {
        public DonchianChannel()
        {
            IndicatorName = "Donchian Channel";
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
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter long at Upper Band",
                        "Enter long at Lower Band"
                    };
            else if (slotType == SlotTypes.OpenFilter)
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
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit long at Upper Band",
                        "Exit long at Lower Band"
                    };
            else if (slotType == SlotTypes.CloseFilter)
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
            IndParam.ListParam[0].ToolTip = "Logic of application of the Donchian Channel.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"High & Low"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Indicator uses High and Low prices.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 10;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The width of the range we are looking for an extreme in.";

            IndParam.NumParam[1].Caption = "Shift";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The number of bars to shift with.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var iPeriod = (int) IndParam.NumParam[0].Value;
            var iShift = (int) IndParam.NumParam[1].Value;
            int iPrvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            var adUpBand = new double[Bars];
            var adDnBand = new double[Bars];

            int iFirstBar = iPeriod + iShift + iPrvs + 2;

            for (int iBar = iFirstBar; iBar < Bars - iShift; iBar++)
            {
                double dMax = double.MinValue;
                double dMin = double.MaxValue;
                for (int i = 0; i < iPeriod; i++)
                {
                    if (High[iBar - i] > dMax) dMax = High[iBar - i];
                    if (Low[iBar - i] < dMin) dMin = Low[iBar - i];
                }
                adUpBand[iBar + iShift] = dMax;
                adDnBand[iBar + iShift] = dMin;
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
                {
                    CompName = "Upper Band",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = iFirstBar,
                    Value = adUpBand
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Lower Band",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Blue,
                    FirstBar = iFirstBar,
                    Value = adDnBand
                };

            Component[2] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = new double[Bars]
                };

            Component[3] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = new double[Bars]
                };

            // Sets the Component's type.
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
                if (iPeriod > 1)
                {
                    for (int iBar = iFirstBar; iBar < Bars; iBar++)
                    {
                        // Covers the cases when the price can pass through the band without a signal.
                        double dOpen = Open[iBar]; // Current open price

                        // Upper band
                        double dValueUp = adUpBand[iBar - iPrvs]; // Current value
                        double dValueUp1 = adUpBand[iBar - iPrvs - 1]; // Previous value
                        double dTempValUp = dValueUp;

                        if ((dValueUp1 > High[iBar - 1] && dValueUp < dOpen) ||
                            // The Open price jumps above the indicator
                            (dValueUp1 < Low[iBar - 1] && dValueUp > dOpen) ||
                            // The Open price jumps below the indicator
                            (Close[iBar - 1] < dValueUp && dValueUp < dOpen) || // The Open price is in a positive gap
                            (Close[iBar - 1] > dValueUp && dValueUp > dOpen)) // The Open price is in a negative gap
                            dTempValUp = dOpen; // The entry/exit level is moved to Open price

                        // Lower band
                        double dValueDown = adDnBand[iBar - iPrvs]; // Current value
                        double dValueDown1 = adDnBand[iBar - iPrvs - 1]; // Previous value
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
                            Component[2].Value[iBar] = dTempValUp;
                            Component[3].Value[iBar] = dTempValDown;
                        }
                        else
                        {
                            Component[2].Value[iBar] = dTempValDown;
                            Component[3].Value[iBar] = dTempValUp;
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
                            Component[2].Value[iBar] = adUpBand[iBar - iPrvs];
                            Component[3].Value[iBar] = adDnBand[iBar - iPrvs];
                        }
                        else
                        {
                            Component[2].Value[iBar] = adDnBand[iBar - iPrvs];
                            Component[3].Value[iBar] = adUpBand[iBar - iPrvs];
                        }
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_below_the_Upper_Band);
                        break;

                    case "The bar opens above Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_above_the_Upper_Band);
                        break;

                    case "The bar opens below Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_below_the_Lower_Band);
                        break;

                    case "The bar opens above Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_above_the_Lower_Band);
                        break;

                    case "The bar opens below Upper Band after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Upper Band after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it);
                        break;

                    case "The bar opens below Lower Band after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it);
                        break;

                    case "The bar opens above Lower Band after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it);
                        break;

                    case "The position opens above Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens above Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The bar closes below Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_closes_below_the_Upper_Band);
                        break;

                    case "The bar closes above Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_closes_above_the_Upper_Band);
                        break;

                    case "The bar closes below Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
                                           BandIndLogic.The_bar_closes_below_the_Lower_Band);
                        break;

                    case "The bar closes above Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3],
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
                    EntryFilterLongDescription = "the position opening price is higher than Upper Band of " +
                                                 ToString();
                    EntryFilterShortDescription = "the position opening price is lower than Lower Band of " +
                                                  ToString();
                    break;

                case "The position opens below Upper Band":
                    EntryFilterLongDescription = "the position opening price is lower than Upper Band of " +
                                                 ToString();
                    EntryFilterShortDescription = "the position opening price is higher than Lower Band of " +
                                                  ToString();
                    break;

                case "The position opens above Lower Band":
                    EntryFilterLongDescription = "the position opening price is higher than Lower Band of " +
                                                 ToString();
                    EntryFilterShortDescription = "the position opening price is lower than Upper Band of " +
                                                  ToString();
                    break;

                case "The position opens below Lower Band":
                    EntryFilterLongDescription = "the position opening price is lower than Lower Band of " +
                                                 ToString();
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
                IndParam.NumParam[0].ValueToString + ", " + // Period
                IndParam.NumParam[1].ValueToString + ")"; // Shift
        }
    }
}