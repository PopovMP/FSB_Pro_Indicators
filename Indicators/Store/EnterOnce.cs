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
    public class EnterOnce : Indicator
    {
        public EnterOnce()
        {
            IndicatorName = "Enter Once";
            PossibleSlots = SlotTypes.OpenFilter;
            IsDeafultGroupAll = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.DateTime;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Enter no more than once a bar",
                    "Enter no more than once a day",
                    "Enter no more than once a week",
                    "Enter no more than once a month"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;
        }

        public override void SetDescription()
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter no more than once a bar":
                    EntryFilterLongDescription = "this is the first entry during the bar";
                    EntryFilterShortDescription = "this is the first entry during the bar";
                    break;
                case "Enter no more than once a day":
                    EntryFilterLongDescription = "this is the first entry during the day";
                    EntryFilterShortDescription = "this is the first entry during the day";
                    break;
                case "Enter no more than once a week":
                    EntryFilterLongDescription = "this is the first entry during the week";
                    EntryFilterShortDescription = "this is the first entry during the week";
                    break;
                case "Enter no more than once a month":
                    EntryFilterLongDescription = "this is the first entry during the month";
                    EntryFilterShortDescription = "this is the first entry during the month";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}