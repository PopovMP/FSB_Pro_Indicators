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
    public class DayOpening : Indicator
    {
        public DayOpening()
        {

            IndicatorName = "Day Opening";
            PossibleSlots = SlotTypes.Open;
            SeparatedChart = false;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.DateTime;
            IndParam.ExecutionTime = ExecutionTime.AtBarOpening;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[] {"Enter the market at the beginning of the day"};
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Open"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The execution price of all entry orders.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Calculation
            var adOpenPrice = new double[Bars];

            for (int iBar = 1; iBar < Bars; iBar++)
                if (Time[iBar - 1].Day != Time[iBar].Day)
                    adOpenPrice[iBar] = Open[iBar];

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
                {
                    CompName = "Opening price of the day",
                    DataType = IndComponentType.OpenPrice,
                    ChartType = IndChartType.NoChart,
                    FirstBar = 2,
                    Value = adOpenPrice
                };
        }

        public override void SetDescription()
        {
            EntryPointLongDescription = "at the beginning of the day";
            EntryPointShortDescription = "at the beginning of the day";
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}