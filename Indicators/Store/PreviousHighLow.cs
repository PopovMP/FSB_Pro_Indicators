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
    public class PreviousHighLow : Indicator
    {
        public PreviousHighLow()
        {
            IndicatorName = "Previous High Low";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
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
                        "Enter long at the previous high",
                        "Enter long at the previous low"
                    };
            else if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The position opens above the previous high",
                        "The position opens below the previous high",
                        "The position opens above the previous low",
                        "The position opens below the previous low"
                    };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit long at the previous high",
                        "Exit long at the previous low"
                    };
            else if (SlotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The bar closes above the previous high",
                        "The bar closes below the previous high",
                        "The bar closes above the previous low",
                        "The bar closes below the previous low"
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
            IndParam.NumParam[0].Caption = "Vertical shift";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = -2000;
            IndParam.NumParam[0].Max = +2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "A vertical shift above the high and below the low price.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            double dShift = IndParam.NumParam[0].Value*Point;

            // Calculation
            var adHighPrice = new double[Bars];
            var adLowPrice = new double[Bars];

            const int firstBar = 2;

            for (int iBar = firstBar; iBar < Bars; iBar++)
            {
                adHighPrice[iBar] = High[iBar - 1];
                adLowPrice[iBar] = Low[iBar - 1];
            }

            var adUpperBand = new double[Bars];
            var adLowerBand = new double[Bars];
            for (int bar = firstBar; bar < Bars; bar++)
            {
                adUpperBand[bar] = adHighPrice[bar] + dShift;
                adLowerBand[bar] = adLowPrice[bar] - dShift;
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
                {
                    CompName = "Previous High",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Level,
                    ChartColor = Color.DarkGreen,
                    FirstBar = firstBar,
                    Value = adHighPrice
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Previous Low",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Level,
                    ChartColor = Color.DarkRed,
                    FirstBar = firstBar,
                    Value = adLowPrice
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
                case "Enter long at the previous high":
                case "Exit long at the previous high":
                    Component[2].Value = adUpperBand;
                    Component[3].Value = adLowerBand;
                    break;
                case "Enter long at the previous low":
                case "Exit long at the previous low":
                    Component[2].Value = adLowerBand;
                    Component[3].Value = adUpperBand;
                    break;
                case "The bar opens below the previous high":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_opens_below_the_Upper_Band);
                    break;
                case "The bar opens above the previous high":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_opens_above_the_Upper_Band);
                    break;
                case "The bar opens below the previous low":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_opens_below_the_Lower_Band);
                    break;
                case "The bar opens above the previous low":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_opens_above_the_Lower_Band);
                    break;
                case "The bar closes below the previous high":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_closes_below_the_Upper_Band);
                    break;
                case "The bar closes above the previous high":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_closes_above_the_Upper_Band);
                    break;
                case "The bar closes below the previous low":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_closes_below_the_Lower_Band);
                    break;
                case "The bar closes above the previous low":
                    BandIndicatorLogic(firstBar, 0, adUpperBand, adLowerBand, ref Component[2], ref Component[3],
                                       BandIndLogic.The_bar_closes_above_the_Lower_Band);
                    break;
                case "The position opens above the previous high":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted previous high";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[3].CompName = "Shifted previous low";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[2].Value = adUpperBand;
                    Component[3].Value = adLowerBand;
                    break;
                case "The position opens below the previous high":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted previous high";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                    Component[3].CompName = "Shifted previous low";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                    Component[2].Value = adUpperBand;
                    Component[3].Value = adLowerBand;
                    break;
                case "The position opens above the previous low":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted previous low";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[3].CompName = "Shifted previous high";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[2].Value = adLowerBand;
                    Component[3].Value = adUpperBand;
                    break;
                case "The position opens below the previous low":
                    Component[0].DataType = IndComponentType.Other;
                    Component[1].DataType = IndComponentType.Other;
                    Component[2].CompName = "Shifted previous low";
                    Component[2].DataType = IndComponentType.Other;
                    Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                    Component[3].CompName = "Shifted previous high";
                    Component[3].DataType = IndComponentType.Other;
                    Component[3].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                    Component[2].Value = adLowerBand;
                    Component[3].Value = adUpperBand;
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
                if (IndParam.ListParam[0].Text == "Enter long at the previous high" ||
                    IndParam.ListParam[0].Text == "Enter long at the previous low" ||
                    IndParam.ListParam[0].Text == "Exit long at the previous high" ||
                    IndParam.ListParam[0].Text == "Exit long at the previous low")
                {
                    sUpperTrade = "at the ";
                    sLowerTrade = "at the ";
                }
                else
                {
                    sUpperTrade = "the ";
                    sLowerTrade = "the ";
                }
            }
            else
            {
                sUpperTrade = -iShift + " points below the ";
                sLowerTrade = -iShift + " points above the ";
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the previous high":
                    EntryPointLongDescription = sUpperTrade + "previous high";
                    EntryPointShortDescription = sLowerTrade + "previous low";
                    break;
                case "Enter long at the previous low":
                    EntryPointLongDescription = sLowerTrade + "previous low";
                    EntryPointShortDescription = sUpperTrade + "previous high";
                    break;
                case "Exit long at the previous high":
                    ExitPointLongDescription = sUpperTrade + "previous high";
                    ExitPointShortDescription = sLowerTrade + "previous low ";
                    break;
                case "Exit long at the previous low":
                    ExitPointLongDescription = sLowerTrade + "previous low";
                    ExitPointShortDescription = sUpperTrade + "previous high";
                    break;

                case "The position opens below the previous high":
                    EntryFilterLongDescription = "the position opens lower than " + sUpperTrade + "previous high";
                    EntryFilterShortDescription = "the position opens higher than " + sLowerTrade + "previous low";
                    break;
                case "The position opens above the previous high":
                    EntryFilterLongDescription = "the position opens higher than " + sUpperTrade + "previous high";
                    EntryFilterShortDescription = "the position opens lower than " + sLowerTrade + "previous low";
                    break;
                case "The position opens below the previous low":
                    EntryFilterLongDescription = "the position opens lower than " + sLowerTrade + "previous low";
                    EntryFilterShortDescription = "the position opens higher than " + sUpperTrade + "previous high";
                    break;
                case "The position opens above the previous low":
                    EntryFilterLongDescription = "the position opens higher than " + sLowerTrade + "previous low";
                    EntryFilterShortDescription = "the position opens lower than " + sUpperTrade + "previous high";
                    break;

                case "The bar closes below the previous high":
                    ExitFilterLongDescription = "the bar closes lower than " + sUpperTrade + "previous high";
                    ExitFilterShortDescription = "the bar closes higher than " + sLowerTrade + "previous low ";
                    break;
                case "The bar closes above the previous high":
                    ExitFilterLongDescription = "the bar closes higher than " + sUpperTrade + "previous high";
                    ExitFilterShortDescription = "the bar closes lower than " + sLowerTrade + "previous low";
                    break;
                case "The bar closes below the previous low":
                    ExitFilterLongDescription = "the bar closes lower than " + sLowerTrade + "previous low";
                    ExitFilterShortDescription = "the bar closes higher than " + sUpperTrade + "previous high";
                    break;
                case "The bar closes above the previous low":
                    ExitFilterLongDescription = "the bar closes higher than " + sLowerTrade + "previous low ";
                    ExitFilterShortDescription = "the bar closes lower than " + sUpperTrade + "previous high";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName + "(" +
                   IndParam.NumParam[0].ValueToString + ")"; // Shift in Points
        }
    }
}