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
    public class DayClosing : Indicator
    {
        public DayClosing()
        {
            IndicatorName = "Day Closing";
            PossibleSlots = SlotTypes.Close;
            AllowClosingFilters = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.DateTime;
            IndParam.ExecutionTime = ExecutionTime.AtBarClosing;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[] {"Exit the market at the end of the day"};
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "The execution price of all exit orders.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Close"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Indicator closes a position at Close price.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Calculation
            var adClosePrice = new double[Bars];

            for (int bar = 1; bar < Bars; bar++)
                if (Time[bar - 1].Day != Time[bar].Day)
                    adClosePrice[bar - 1] = Close[bar - 1];

            // Check the last bar
            TimeSpan tsBarClosing = Time[Bars - 1].TimeOfDay.Add(new TimeSpan(0, (int) Period, 0));
            var tsDayClosing = new TimeSpan(24, 0, 0);
            if (tsBarClosing == tsDayClosing)
                adClosePrice[Bars - 1] = Close[Bars - 1];

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
                {
                    CompName = "Closing price of the day",
                    DataType = IndComponentType.ClosePrice,
                    ChartType = IndChartType.NoChart,
                    FirstBar = 2,
                    Value = adClosePrice
                };
        }

        public override void SetDescription()
        {
            ExitPointLongDescription = "at the end of the day";
            ExitPointShortDescription = "at the end of the day";
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}