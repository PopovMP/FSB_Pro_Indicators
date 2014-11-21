//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class AccountPercentStop : Indicator
    {
        public AccountPercentStop()
        {
            IndicatorName = "Account Percent Stop";
            PossibleSlots = SlotTypes.Close;
            SeparatedChart = false;

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
                    "Limit the risk to percent of the account"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Account percent";
            IndParam.NumParam[0].Value = 2;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 20;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Maximum account to risk.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Calculation
            const int firstBar = 1;

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
                {
                    CompName = "Stop to a transferred position",
                    DataType = IndComponentType.Other,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };
        }

        public override void SetDescription()
        {
            var percent = (int) IndParam.NumParam[0].Value;

            ExitPointLongDescription = "at a loss of " + percent + "% of the account";
            ExitPointShortDescription = "at a loss of " + percent + "% of the account";
        }

        public override string ToString()
        {
            return IndicatorName + " (" + IndParam.NumParam[0].ValueToString + ")";
        }
    }
}