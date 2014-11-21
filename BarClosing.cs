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
    public class BarClosing : Indicator
    {
        public BarClosing()
        {
            // General properties
            IndicatorName = "Bar Closing";
            PossibleSlots = SlotTypes.Open | SlotTypes.Close;
            AllowClosingFilters = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.Additional;
            IndParam.ExecutionTime = ExecutionTime.AtBarClosing;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            switch (slotType)
            {
                case SlotTypes.Open:
                    IndParam.ListParam[0].ItemList = new[] {"Enter the market at the end of the bar"};
                    IndParam.ListParam[1].ToolTip = "The execution price of all entry orders.";
                    break;
                case SlotTypes.Close:
                    IndParam.ListParam[0].ItemList = new[] {"Exit the market at the end of the bar"};
                    IndParam.ListParam[1].ToolTip = "The execution price of all exit orders.";
                    break;
            }
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Close"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Indicator opens or closes a position at Close price.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
                {
                    CompName = "Close Price",
                    DataType = (IndParam.SlotType == SlotTypes.Open) ? IndComponentType.OpenPrice : IndComponentType.ClosePrice,
                    ChartType = IndChartType.NoChart,
                    FirstBar = 2,
                    Value = Close
                };
        }

        public override void SetDescription()
        {
            EntryPointLongDescription = "at the end of the bar";
            EntryPointShortDescription = "at the end of the bar";
            ExitPointLongDescription = "at the end of the bar";
            ExitPointShortDescription = "at the end of the bar";
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}