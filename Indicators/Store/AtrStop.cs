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
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class AtrStop : Indicator
    {
        public AtrStop()
        {
            IndicatorName = "ATR Stop";
            PossibleSlots = SlotTypes.Close;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.Additional;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Exit at ATR Stop level"
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
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing the ATR.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = new[] {"Bar range"};
            IndParam.ListParam[2].Index = 0;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "ATR uses the range of the current bar";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Smoothing period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of ATR smoothing.";

            IndParam.NumParam[1].Caption = "Multiplier";
            IndParam.NumParam[1].Value = 2;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 10;
            IndParam.NumParam[1].Point = 2;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Determines the stop level.";

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
            var period = (int) IndParam.NumParam[0].Value;
            var multipl = (int) IndParam.NumParam[1].Value;
            int prev = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period + 2;

            var atr = new double[Bars];

            for (int bar = 1; bar < Bars; bar++)
                atr[bar] = Math.Max(High[bar], Close[bar - 1]) - Math.Min(Low[bar], Close[bar - 1]);

            atr = MovingAverage(period, 0, maMethod, atr);

            var atrStop = new double[Bars];
            double pip = (Digits == 5 || Digits == 3) ? 10*Point : Point;
            double minStop = 5*pip;

            for (int bar = firstBar; bar < Bars - prev; bar++)
                atrStop[bar + prev] = Math.Max(atr[bar]*multipl, minStop);

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
                {
                    CompName = "ATR Stop margin",
                    DataType = IndComponentType.IndicatorValue,
                    FirstBar = firstBar,
                    ShowInDynInfo = false,
                    Value = atrStop
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "ATR Stop for the transferred position",
                    DataType = IndComponentType.Other,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };
        }

        public override void SetDescription()
        {
            ExitPointLongDescription = "when the market falls to the " + ToString() + " level";
            ExitPointShortDescription = "when the market rises to the " + ToString() + " level";
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Smoothing method
                   IndParam.NumParam[0].ValueToString + ", " + // Smoothing period
                   IndParam.NumParam[1].ValueToString + ")"; // Multiplier
        }
    }
}