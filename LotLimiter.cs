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
    public class LotLimiter : Indicator
    {
        public LotLimiter()
        {
            IndicatorName = "Lot Limiter";
            PossibleSlots = SlotTypes.OpenFilter;
            IsDeafultGroupAll = true;
            IsGeneratable = false;

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
            IndParam.ListParam[0].ItemList = new[] {"Limit the number of open lots"};
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Maximum lots";
            IndParam.NumParam[0].Value = 5;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 100;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Maximum number of open lots.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;
        }

        public override void SetDescription()
        {
            var maxLots = (int) IndParam.NumParam[0].Value;

            EntryFilterLongDescription = "the open lots cannot be more than " + maxLots +
                                         ". This rule overrides the maximum number of open lots set in the strategy properties dialog";
            EntryFilterShortDescription = "the open lots cannot be more than " + maxLots +
                                          ". This rule overrides the maximum number of open lots set in the strategy properties dialog";
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.NumParam[0].ValueToString + ")"; // Maximum lots
        }
    }
}