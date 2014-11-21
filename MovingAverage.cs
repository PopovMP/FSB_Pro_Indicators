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
    public class MovingAverage : Indicator
    {
        public MovingAverage()
        {
            IndicatorName = "Moving Average";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.1";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.Indicator;
            IndParam.ExecutionTime = ExecutionTime.DuringTheBar;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter the market at Moving Average"
                    };
            else if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Moving Average rises",
                        "Moving Average falls",
                        "The bar opens above Moving Average",
                        "The bar opens below Moving Average",
                        "The bar opens above Moving Average after opening below it",
                        "The bar opens below Moving Average after opening above it",
                        "The position opens above Moving Average",
                        "The position opens below Moving Average"
                    };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit the market at Moving Average"
                    };
            else if (slotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Moving Average rises",
                        "Moving Average falls",
                        "The bar closes below Moving Average",
                        "The bar closes above Moving Average"
                    };
            else
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Not Defined"
                    };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of Moving Average.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Simple;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The smoothing method of Moving Average.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[2].Index = (int) BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price Moving Average is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Moving Average period.";

            IndParam.NumParam[1].Caption = "Shift";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "How many bars to shift with.";

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
            var price = (BasePrice) IndParam.ListParam[2].Index;
            var period = (int) IndParam.NumParam[0].Value;
            var shift = (int) IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // TimeExecution
            if (period == 1 && shift == 0)
            {
                if (price == BasePrice.Open)
                    IndParam.ExecutionTime = ExecutionTime.AtBarOpening;
                else if (price == BasePrice.Close)
                    IndParam.ExecutionTime = ExecutionTime.AtBarClosing;
            }

            // Calculation
            double[] movingAverage = MovingAverage(period, shift, maMethod, Price(price));
            int firstBar = period + shift + 1 + previous;

            // Saving the components
            if (SlotType == SlotTypes.Open || SlotType == SlotTypes.Close)
            {
                Component = new IndicatorComp[2];

                Component[1] = new IndicatorComp {Value = new double[Bars]};

                for (int bar = firstBar; bar < Bars; bar++)
                {
                    // Covers the cases when the price can pass through the MA without a signal
                    double value = movingAverage[bar - previous]; // Current value
                    double value1 = movingAverage[bar - previous - 1]; // Previous value
                    double tempVal = value;
                    if ((value1 > High[bar - 1] && value < Open[bar]) || // The Open price jumps above the indicator
                        (value1 < Low[bar - 1] && value > Open[bar]) || // The Open price jumps below the indicator
                        (Close[bar - 1] < value && value < Open[bar]) || // The Open price is in a positive gap
                        (Close[bar - 1] > value && value > Open[bar])) // The Open price is in a negative gap
                        tempVal = Open[bar];
                    Component[1].Value[bar] = tempVal; // Entry or exit value
                }
            }
            else
            {
                Component = new IndicatorComp[3];

                Component[1] = new IndicatorComp
                    {ChartType = IndChartType.NoChart, FirstBar = firstBar, Value = new double[Bars]};

                Component[2] = new IndicatorComp
                    {ChartType = IndChartType.NoChart, FirstBar = firstBar, Value = new double[Bars]};
            }

            Component[0] = new IndicatorComp
                {
                    CompName = "MA Value",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    ChartColor = Color.Red,
                    FirstBar = firstBar,
                    Value = movingAverage
                };

            switch (SlotType)
            {
                case SlotTypes.Open:
                    Component[1].CompName = "Position opening price";
                    Component[1].DataType = IndComponentType.OpenPrice;
                    break;
                case SlotTypes.OpenFilter:
                    Component[1].DataType = IndComponentType.AllowOpenLong;
                    Component[1].CompName = "Is long entry allowed";
                    Component[2].DataType = IndComponentType.AllowOpenShort;
                    Component[2].CompName = "Is short entry allowed";
                    break;
                case SlotTypes.Close:
                    Component[1].CompName = "Position closing price";
                    Component[1].DataType = IndComponentType.ClosePrice;
                    break;
                case SlotTypes.CloseFilter:
                    Component[1].DataType = IndComponentType.ForceCloseLong;
                    Component[1].CompName = "Close out long position";
                    Component[2].DataType = IndComponentType.ForceCloseShort;
                    Component[2].CompName = "Close out short position";
                    break;
            }

            if (SlotType == SlotTypes.OpenFilter || SlotType == SlotTypes.CloseFilter)
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "Moving Average rises":
                        IndicatorRisesLogic(firstBar, previous, movingAverage, ref Component[1], ref Component[2]);
                        break;

                    case "Moving Average falls":
                        IndicatorFallsLogic(firstBar, previous, movingAverage, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens above Moving Average":
                        BarOpensAboveIndicatorLogic(firstBar, previous, movingAverage, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens below Moving Average":
                        BarOpensBelowIndicatorLogic(firstBar, previous, movingAverage, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens above Moving Average after opening below it":
                        BarOpensAboveIndicatorAfterOpeningBelowLogic(firstBar, previous, movingAverage, ref Component[1],
                                                                     ref Component[2]);
                        break;

                    case "The bar opens below Moving Average after opening above it":
                        BarOpensBelowIndicatorAfterOpeningAboveLogic(firstBar, previous, movingAverage, ref Component[1],
                                                                     ref Component[2]);
                        break;

                    case "The position opens above Moving Average":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyHigherSellLower;
                        Component[0].UsePreviousBar = previous;
                        Component[1].DataType = IndComponentType.Other;
                        Component[1].ShowInDynInfo = false;
                        Component[2].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        break;

                    case "The position opens below Moving Average":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyLowerSelHigher;
                        Component[0].UsePreviousBar = previous;
                        Component[1].DataType = IndComponentType.Other;
                        Component[1].ShowInDynInfo = false;
                        Component[2].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        break;

                    case "The bar closes below Moving Average":
                        BarClosesBelowIndicatorLogic(firstBar, previous, movingAverage, ref Component[1], ref Component[2]);
                        break;

                    case "The bar closes above Moving Average":
                        BarClosesAboveIndicatorLogic(firstBar, previous, movingAverage, ref Component[1], ref Component[2]);
                        break;
                }
            }
        }

        public override void SetDescription()
        {
            EntryPointLongDescription = "at " + ToString();
            EntryPointShortDescription = "at " + ToString();
            ExitPointLongDescription = "at " + ToString();
            ExitPointShortDescription = "at " + ToString();

            switch (IndParam.ListParam[0].Text)
            {
                case "Moving Average rises":
                    EntryFilterLongDescription = ToString() + " rises";
                    EntryFilterShortDescription = ToString() + " falls";
                    ExitFilterLongDescription = ToString() + " rises";
                    ExitFilterShortDescription = ToString() + " falls";
                    break;

                case "Moving Average falls":
                    EntryFilterLongDescription = ToString() + " falls";
                    EntryFilterShortDescription = ToString() + " rises";
                    ExitFilterLongDescription = ToString() + " falls";
                    ExitFilterShortDescription = ToString() + " rises";
                    break;

                case "The bar opens above Moving Average":
                    EntryFilterLongDescription = "the bar opens above the " + ToString();
                    EntryFilterShortDescription = "the bar opens below the " + ToString();
                    break;

                case "The bar opens below Moving Average":
                    EntryFilterLongDescription = "the bar opens below the " + ToString();
                    EntryFilterShortDescription = "the bar opens above the " + ToString();
                    break;

                case "The position opens above Moving Average":
                    EntryFilterLongDescription = "the position opening price is higher than the " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than the " + ToString();
                    break;

                case "The position opens below Moving Average":
                    EntryFilterLongDescription = "the position opening price is lower than the " + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than the " + ToString();
                    break;

                case "The bar opens above Moving Average after opening below it":
                    EntryFilterLongDescription = "the bar opens above the " + ToString() + " after opening below it";
                    EntryFilterShortDescription = "the bar opens below the " + ToString() + " after opening above it";
                    break;

                case "The bar opens below Moving Average after opening above it":
                    EntryFilterLongDescription = "the bar opens below the " + ToString() + " after opening above it";
                    EntryFilterShortDescription = "the bar opens above the " + ToString() + " after opening below it";
                    break;

                case "The bar closes above Moving Average":
                    ExitFilterLongDescription = "the bar closes above the " + ToString();
                    ExitFilterShortDescription = "the bar closes below the " + ToString();
                    break;

                case "The bar closes below Moving Average":
                    ExitFilterLongDescription = "the bar closes below the " + ToString();
                    ExitFilterShortDescription = "the bar closes above the " + ToString();
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}{1} ({2}, {3}, {4}, {5})",
                                 IndicatorName,
                                 (IndParam.CheckParam[0].Checked ? "*" : ""),
                                 IndParam.ListParam[1].Text,
                                 IndParam.ListParam[2].Text,
                                 IndParam.NumParam[0].ValueToString,
                                 IndParam.NumParam[1].ValueToString);
        }
    }
}