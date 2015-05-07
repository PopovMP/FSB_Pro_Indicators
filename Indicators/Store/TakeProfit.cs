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
    public class TakeProfit : Indicator
    {
        public TakeProfit()
        {
            IndicatorName = "Take Profit";
            PossibleSlots = SlotTypes.Close;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.1";
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
            IndParam.ListParam[0].ItemList = new[]
            {
                "Exit at Take Profit level"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Take Profit";
            IndParam.NumParam[0].Value = 200;
            IndParam.NumParam[0].Min = 5;
            IndParam.NumParam[0].Max = 5000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Take Profit value in points.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;
        }

        public override void SetDescription()
        {
            var takeProfit = (int) IndParam.NumParam[0].Value;

            ExitPointLongDescription = string.Format("when the market rises {0} points from the last entry price", takeProfit);
            ExitPointShortDescription = string.Format("when the market falls {0} points from the last entry price", takeProfit);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})",
                IndicatorName,
                IndParam.NumParam[0].ValueToString);
        }
    }
}