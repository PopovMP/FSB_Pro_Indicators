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

namespace ForexStrategyBuilder.Indicators
{
    public class MainChartSingleLineIndicator : Indicator
    {
        // Input parameters
        protected BasePrice IndicatorBasePrice { get; set; }
        protected MAMethod  IndicatorMaMethod  { get; set; }
        protected int       IndicatorPeriod    { get; set; }
        protected bool      UsePreviousBar     { get; set; }

        // Indicator values
        protected int       FirstBar        { get; set; }
        protected double[]  IndicatorLine   { get; set; }

        public MainChartSingleLineIndicator()
        {
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;
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
                        "Enter the market at the indicator line"
                    };
            else if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The indicator rises",
                        "The indicator falls",
                        "The indicator changes its direction upward",
                        "The indicator changes its direction downward",
                        "The bar opens above the indicator",
                        "The bar opens below the indicator",
                        "The bar opens above the indicator after opening below it",
                        "The bar opens below the indicator after opening above it",
                        "The position opens above the indicator",
                        "The position opens below the indicator"
                    };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit the market at the indicator line"
                    };
            else if (slotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The indicator rises",
                        "The indicator falls",
                        "The indicator changes its direction upward",
                        "The indicator changes its direction downward",
                        "The bar closes below the indicator",
                        "The bar closes above the indicator"
                    };
            else
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Not Defined"
                    };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The smoothing method of the indicator.";

            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Close;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the indicator is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value   = 14;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The indicator period.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        protected virtual void InitCalculation(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            IndicatorMaMethod  = (MAMethod)IndParam.ListParam[1].Index;
            IndicatorBasePrice = (BasePrice)IndParam.ListParam[2].Index;
            IndicatorPeriod    = (int)IndParam.NumParam[0].Value;
            UsePreviousBar     = IndParam.CheckParam[0].Checked;

            // TimeExecution
            if (IndicatorPeriod == 1)
            {
                if (IndicatorBasePrice == BasePrice.Open)
                    IndParam.ExecutionTime = ExecutionTime.AtBarOpening;
                else if (IndicatorBasePrice == BasePrice.Close)
                    IndParam.ExecutionTime = ExecutionTime.AtBarClosing;
            }
            else
                IndParam.ExecutionTime = ExecutionTime.DuringTheBar;

            // Initilaization
            FirstBar = IndicatorPeriod + 2;
            IndicatorLine = new double[Bars];
        }

        protected virtual void PostCalculation()
        {
            int previous = UsePreviousBar ? 1 : 0;

            // Saving the components
            if (SlotType == SlotTypes.Open || SlotType == SlotTypes.Close)
            {
                Component = new IndicatorComp[2];

                Component[1] = new IndicatorComp { Value = new double[Bars] };

                for (int bar = FirstBar; bar < Bars; bar++)
                {
                    // Covers the cases when the price can pass through the MA without a signal
                    double value = IndicatorLine[bar - previous];     // Current value
                    double value1 = IndicatorLine[bar - previous - 1]; // Previous value
                    double tempVal = value;
                    if ((value1 > High[bar - 1] && value < Open[bar]) || // The Open price jumps above the indicator
                        (value1 < Low[bar  - 1] && value > Open[bar]) || // The Open price jumps below the indicator
                        (Close[bar - 1] < value && value < Open[bar]) || // The Open price is in a positive gap
                        (Close[bar - 1] > value && value > Open[bar]))   // The Open price is in a negative gap
                        tempVal = Open[bar];
                    Component[1].Value[bar] = tempVal; // Entry or exit value
                }
            }
            else
            {
                Component = new IndicatorComp[3];
                Component[1] = new IndicatorComp { ChartType = IndChartType.NoChart, FirstBar = FirstBar, Value = new double[Bars] };
                Component[2] = new IndicatorComp { ChartType = IndChartType.NoChart, FirstBar = FirstBar, Value = new double[Bars] };
            }

            Component[0] = new IndicatorComp
            {
                CompName   = "Indicator line",
                DataType   = IndComponentType.IndicatorValue,
                ChartType  = IndChartType.Line,
                ChartColor = Color.Red,
                FirstBar   = FirstBar,
                Value      = IndicatorLine
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
                    case "The indicator rises":
                        IndicatorRisesLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;

                    case "The indicator falls":
                        IndicatorFallsLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;

                    case "The indicator changes its direction upward":
                        IndicatorChangesItsDirectionUpward(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;

                    case "The indicator changes its direction downward":
                        IndicatorChangesItsDirectionDownward(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;
                    
                    case "The bar opens above the indicator":
                        BarOpensAboveIndicatorLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;
                    
                    case "The bar opens below the indicator":
                        BarOpensBelowIndicatorLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;
                    
                    case "The bar opens above the indicator after opening below it":
                        BarOpensAboveIndicatorAfterOpeningBelowLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;
                    
                    case "The bar opens below the indicator after opening above it":
                        BarOpensBelowIndicatorAfterOpeningAboveLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;
                    
                    case "The position opens above the indicator":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyHigherSellLower;
                        Component[0].UsePreviousBar = previous;
                        Component[1].DataType = IndComponentType.Other;
                        Component[1].ShowInDynInfo = false;
                        Component[2].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        break;
                    
                    case "The position opens below the indicator":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyLowerSelHigher;
                        Component[0].UsePreviousBar = previous;
                        Component[1].DataType = IndComponentType.Other;
                        Component[1].ShowInDynInfo = false;
                        Component[2].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        break;
                    
                    case "The bar closes below the indicator":
                        BarClosesBelowIndicatorLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;
                    
                    case "The bar closes above the indicator":
                        BarClosesAboveIndicatorLogic(FirstBar, previous, IndicatorLine, ref Component[1], ref Component[2]);
                        break;
                }
            }
        }

        public override void SetDescription()
        {
            EntryPointLongDescription  = "at " + ToString();
            EntryPointShortDescription = "at " + ToString();
            ExitPointLongDescription   = "at " + ToString();
            ExitPointShortDescription  = "at " + ToString();

            switch (IndParam.ListParam[0].Text)
            {
                case "The indicator rises":
                    EntryFilterLongDescription  = ToString() + " rises";
                    EntryFilterShortDescription = ToString() + " falls";
                    ExitFilterLongDescription   = ToString() + " rises";
                    ExitFilterShortDescription  = ToString() + " falls";
                    break;

                case "The indicator falls":
                    EntryFilterLongDescription  = ToString() + " falls";
                    EntryFilterShortDescription = ToString() + " rises";
                    ExitFilterLongDescription   = ToString() + " falls";
                    ExitFilterShortDescription  = ToString() + " rises";
                    break;

                case "The indicator changes its direction upward":
                    EntryFilterLongDescription  = ToString() + " changes its direction upward";
                    EntryFilterShortDescription = ToString() + " changes its direction downward";
                    ExitFilterLongDescription   = ToString() + " changes its direction upward";
                    ExitFilterShortDescription  = ToString() + " changes its direction downward";
                    break;

                case "The indicator changes its direction downward":
                    EntryFilterLongDescription  = ToString() + " changes its direction downward";
                    EntryFilterShortDescription = ToString() + " changes its direction upward";
                    ExitFilterLongDescription   = ToString() + " changes its direction downward";
                    ExitFilterShortDescription  = ToString() + " changes its direction upward";
                    break;

                case "The bar opens above the indicator":
                    EntryFilterLongDescription  = "the bar opens above the "  + ToString();
                    EntryFilterShortDescription = "the bar opens below the " + ToString();
                    break;

                case "The bar opens below the indicator":
                    EntryFilterLongDescription  = "the bar opens below the " + ToString();
                    EntryFilterShortDescription = "the bar opens above the " + ToString();
                    break;

                case "The position opens above the indicator":
                    EntryFilterLongDescription  = "the position opening price is higher than the " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than the "  + ToString();
                    break;

                case "The position opens below the indicator":
                    EntryFilterLongDescription  = "the position opening price is lower than the "  + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than the " + ToString();
                    break;

                case "The bar opens above the indicator after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the " + ToString() + " after opening below it";
                    EntryFilterShortDescription = "the bar opens below the " + ToString() + " after opening above it";
                    break;

                case "The bar opens below the indicator after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the " + ToString() + " after opening above it";
                    EntryFilterShortDescription = "the bar opens above the " + ToString() + " after opening below it";
                    break;

                case "The bar closes above the indicator":
                    ExitFilterLongDescription  = "the bar closes above the " + ToString();
                    ExitFilterShortDescription = "the bar closes below the " + ToString();
                    break;

                case "The bar closes below the indicator":
                    ExitFilterLongDescription  = "the bar closes below the " + ToString();
                    ExitFilterShortDescription = "the bar closes above the " + ToString();
                    break;
            }
        }

        public override string ToString()
        {
            var name = IndicatorName + (IndParam.CheckParam[0].Checked ? "* " : " ");
            var parameters = "(";
            for (int i = 1; i < 5; i++)
                if (IndParam.ListParam[i].Enabled)
                    parameters += IndParam.ListParam[i].Text + ", ";
            for (int i = 0; i < 6; i++)
                if (IndParam.NumParam[i].Enabled)
                    parameters += IndParam.NumParam[i].Value + ", ";
            return name + parameters.Substring(0, parameters.Length - 2) + ")";
        }
    }
}
