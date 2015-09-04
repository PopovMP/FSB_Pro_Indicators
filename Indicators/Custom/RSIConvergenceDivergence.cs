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
using ForexStrategyBuilder.Indicators.Store;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class RSIConvergenceDivergence : Indicator
    {
        public RSIConvergenceDivergence()
        {
            IndicatorName = "RSI Convergence Divergence";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            IsDeafultGroupAll = false;
            IsGeneratable = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "1.0";
            IndicatorDescription = "Measures convergence/divergence between the market and RSI." + Environment.NewLine +
                                   "The indicator compares two MAs calculated on RSI and on bar Close.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[] {"Convergence", "Divergence"};
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Determines the entry conditions";

            IndParam.ListParam[1].Caption = "RSI smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Smoothed;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing RSI value.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index = (int)BasePrice.Close;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price RSI is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "RSI Smoothing period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of smoothing of RSI value.";

            IndParam.NumParam[1].Caption = "Reference MA period";
            IndParam.NumParam[1].Value = 14;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Convergence/divergence reference MA period.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var basePrice = (BasePrice) IndParam.ListParam[2].Index;
            var referencePeriod = (int) IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation

            // ---------------------------------------------------------
            var rsi = new RSI();
            rsi.Initialize(SlotType);
            rsi.IndParam.ListParam[1].Index = IndParam.ListParam[1].Index;
            rsi.IndParam.ListParam[2].Index = IndParam.ListParam[2].Index;
            rsi.IndParam.NumParam[0].Value = IndParam.NumParam[0].Value;
            rsi.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            rsi.Calculate(DataSet);

            double[] indicatorMa = MovingAverage(referencePeriod, previous, MAMethod.Simple, rsi.Component[0].Value);
            double[] marketMa = MovingAverage(referencePeriod, previous, MAMethod.Simple, Price(basePrice));
            // ----------------------------------------------------------

            int firstBar = rsi.Component[0].FirstBar + referencePeriod + 2;
            var cd = new double[Bars];

            if (IndParam.ListParam[0].Text == "Convergence")
                for (int bar = firstBar; bar < Bars; bar++)
                    cd[bar] = IsConvergence(indicatorMa, marketMa, bar);
            else if (IndParam.ListParam[0].Text == "Divergence")
                for (int bar = firstBar; bar < Bars; bar++)
                    cd[bar] = IsDivergence(indicatorMa, marketMa, bar);

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
            {
                CompName = "RSI",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Color.Blue,
                FirstBar = firstBar,
                Value = rsi.Component[0].Value
            };

            Component[1] = new IndicatorComp
            {
                CompName = "RSI MA",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Color.Red,
                FirstBar = firstBar,
                Value = indicatorMa
            };

            Component[2] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = cd
            };

            Component[3] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = cd
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }
        }

        public override void SetDescription()
        {
            var logic = "There is a " + IndParam.ListParam[0].Text + " between the market's MA and the indicator's MA";
            EntryFilterLongDescription = logic;
            EntryFilterShortDescription = logic;
            ExitFilterLongDescription = logic;
            ExitFilterShortDescription = logic;
        }

        private double IsConvergence(double[] ma1, double[] ma2, int bar)
        {
            double sigma = Sigma();
            if (ma1[bar] > ma1[bar - 1] + sigma && ma2[bar] > ma2[bar - 1] + sigma)
                return 1;
            if (ma1[bar] < ma1[bar - 1] - sigma && ma2[bar] < ma2[bar - 1] - sigma)
                return 1;
            return 0;
        }

        private double IsDivergence(double[] ma1, double[] ma2, int bar)
        {
            double sigma = Sigma();
            if (ma1[bar] > ma1[bar - 1] + sigma && ma2[bar] < ma2[bar - 1] - sigma)
                return 1;
            if (ma1[bar] < ma1[bar - 1] - sigma && ma2[bar] > ma2[bar - 1] + sigma)
                return 1;
            return 0;
        }
    }
}