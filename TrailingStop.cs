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
    public class TrailingStop : Indicator
    {
        public TrailingStop()
        {
            IndicatorName = "Trailing Stop";
            PossibleSlots = SlotTypes.Close;

            WarningMessage = "The Trailing Stop indicator trails once per bar." +
                             Environment.NewLine +
                             "It means that the indicator doesn't move the position's SL at every new top / bottom, as in the real trade, but only when a new bar begins." +
                             Environment.NewLine +
                             "The Stop Loss remains constant during the whole bar.";

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
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Exit at the Trailing Stop level"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Trailing mode";
            IndParam.ListParam[1].ItemList = new[]
                {
                    "New bar",
                    "New tick (trader)"
                };
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Mode of operation of Trailing Stop.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Trailing Stop";
            IndParam.NumParam[0].Value = 200;
            IndParam.NumParam[0].Min = 5;
            IndParam.NumParam[0].Max = 5000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The Trailing Stop value (in points).";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
                {
                    CompName = "Trailing Stop for the transferred position",
                    DataType = IndComponentType.Other,
                    ShowInDynInfo = false,
                    FirstBar = 1,
                    Value = new double[Bars]
                };
        }

        public override void SetDescription()
        {
            ExitPointLongDescription = "at " + ToString() + " level";
            ExitPointShortDescription = "at " + ToString() + " level";
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.NumParam[0].ValueToString + ")"; // Trailing Stop
        }
    }
}