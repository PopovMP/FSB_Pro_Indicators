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

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class PriceMARelation : Indicator
    {
        public PriceMARelation()
        {
            IndicatorName = "Price MA Relation";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "1.0";
            IndicatorDescription = "Compares a bar price with a Moving Average";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                "Bar price is higher than MA",
                "Bar price is lower than MA"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Releation between a bar price and a MA";

            IndParam.ListParam[1].Caption = "MA base price";
            IndParam.ListParam[1].ItemList = new[] {"Close"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = "Close";
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Base price of the MA";

            IndParam.ListParam[2].Caption = "MA method";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[2].Index = (int) MAMethod.Simple;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "Method for calculating the MA";

            IndParam.ListParam[3].Caption = "Bar reference price";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[3].Index = (int) BasePrice.Close;
            IndParam.ListParam[3].Text = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled = true;
            IndParam.ListParam[3].ToolTip = "Bar reference price.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "MA period";
            IndParam.NumParam[0].Value = 13;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Bars used for calculating the MA";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var maBasePrice = (BasePrice) IndParam.ListParam[1].Index;
            var maMethod = (MAMethod) IndParam.ListParam[2].Index;
            var maPeriod = (int) IndParam.NumParam[0].Value;
            var previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            var barBasePriceLong = (BasePrice) IndParam.ListParam[3].Index;
            var barBasePriceShort = barBasePriceLong;
            if (barBasePriceLong == BasePrice.High)
                barBasePriceShort = BasePrice.Low;
            if (barBasePriceLong == BasePrice.Low)
                barBasePriceShort = BasePrice.High;

            var ma = MovingAverage(maPeriod, 0, maMethod, Price(maBasePrice));
            var barLong = Price(barBasePriceLong);
            var barShort = Price(barBasePriceShort);
            var firstBar = maPeriod + previous + 1;

            // Initializing components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
            {
                CompName = "Moving Average",
                ChartColor = Color.DarkViolet,
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                FirstBar = firstBar,
                Value = ma
            };

            Component[1] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            Component[2] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            // Sets the components signals
            if (IndParam.ListParam[0].Text == "Bar price is higher than MA")
            {
                var sigma = Sigma();
                for (int bar = firstBar; bar < Bars; bar++)
                {
                    if (barLong[bar - previous] > ma[bar - previous] + sigma)
                        Component[1].Value[bar] = 1;
                    if (barShort[bar - previous] < ma[bar - previous] - sigma)
                        Component[2].Value[bar] = 1;
                }
            }
            else if (IndParam.ListParam[0].Text == "Bar price is lower than MA")
            {
                var sigma = Sigma();
                for (int bar = firstBar + previous; bar < Bars; bar++)
                {
                    if (barLong[bar - previous] < ma[bar - previous] - sigma)
                        Component[1].Value[bar] = 1;
                    if (barShort[bar - previous] > ma[bar - previous] + sigma)
                        Component[2].Value[bar] = 1;
                }
            }
        }

        public override void SetDescription()
        {
            var maPeriod = (int) IndParam.NumParam[0].Value;
            var maMethod = (MAMethod)IndParam.ListParam[2].Index;
            var previous = IndParam.CheckParam[0].Checked ? "*" : String.Empty;
            var maText = String.Format("MA{0}({1}, {2})", previous, maMethod, maPeriod);

            var barBasePriceLong = (BasePrice) IndParam.ListParam[3].Index;
            var barBasePriceShort = barBasePriceLong;
            if (barBasePriceLong == BasePrice.High)
                barBasePriceShort = BasePrice.Low;
            if (barBasePriceLong == BasePrice.Low)
                barBasePriceShort = BasePrice.High;

            var barTextLong = String.Format("Bar {0}", barBasePriceLong);
            var barTextShort = String.Format("Bar {0}", barBasePriceShort);

            switch (IndParam.ListParam[0].Text)
            {
                case "Bar price is higher than MA":
                    EntryFilterLongDescription = barTextLong + " is higher than " + maText;
                    EntryFilterShortDescription = barTextShort + " is lower than " + maText;
                    ExitFilterLongDescription = barTextLong + " is higher than " + maText;
                    ExitFilterShortDescription = barTextShort + " is lower than " + maText;
                    break;

                case "Bar price is lower than MA":
                    EntryFilterLongDescription = barTextLong + " is lower than " + maText;
                    EntryFilterShortDescription = barTextShort + " is higher than " + maText;
                    ExitFilterLongDescription = barTextLong + " is lower than " + maText;
                    ExitFilterShortDescription = barTextShort + " is higher than " + maText;
                    break;
            }
        }
    }
}