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
    public class DateFilter : Indicator
    {
        public DateFilter()
        {
            IndicatorName = "Date Filter";
            PossibleSlots = SlotTypes.OpenFilter;
            IsDeafultGroupAll = true;
            IsGeneratable = false;

            WarningMessage =
                "This indicator is designed to be used in the backtester only. It doesn't work in the trader.";

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.DateTime;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Do not open positions before",
                    "Do not open positions after"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of the date filter.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Year";
            IndParam.NumParam[0].Value = 2000;
            IndParam.NumParam[0].Min = 1900;
            IndParam.NumParam[0].Max = 2100;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The year.";

            IndParam.NumParam[1].Caption = "Month";
            IndParam.NumParam[1].Value = 1;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 12;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The month.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var year = (int) IndParam.NumParam[0].Value;
            var month = (int) IndParam.NumParam[1].Value;
            var keyDate = new DateTime(year, month, 1);

            // Calculation
            int firstBar = 0;
            var values = new double[Bars];

            // Calculation of the logic.
            if (IsBacktester)
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "Do not open positions after":
                        for (int bar = firstBar; bar < Bars; bar++)
                            if (Time[bar] < keyDate)
                                values[bar] = 1;
                        break;

                    case "Do not open positions before":
                        for (int bar = firstBar; bar < Bars; bar++)
                            if (Time[bar] >= keyDate)
                            {
                                firstBar = bar;
                                break;
                            }

                        firstBar = Math.Min(firstBar, Bars - 300);

                        for (int bar = firstBar; bar < Bars; bar++)
                            values[bar] = 1;

                        break;
                }
            }
            else
            {
                for (int bar = firstBar; bar < Bars; bar++)
                    values[bar] = 1;
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
                {
                    CompName = "Allow Open Long",
                    DataType = IndComponentType.AllowOpenLong,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = values
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Allow Open Short",
                    DataType = IndComponentType.AllowOpenShort,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = values
                };
        }

        public override void SetDescription()
        {
            if (!IsBacktester)
            {
                EntryFilterLongDescription = "A back tester limitation. It hasn't effect on the trade.";
                EntryFilterShortDescription = "A back tester limitation. It hasn't effect on the trade.";
                return;
            }

            var year = (int) IndParam.NumParam[0].Value;
            var month = (int) IndParam.NumParam[1].Value;
            var keyDate = new DateTime(year, month, 1);

            EntryFilterLongDescription = "(a back tester limitation) Do not open positions ";
            EntryFilterShortDescription = "(a back tester limitation) Do not open positions ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Do not open positions before":
                    EntryFilterLongDescription += "before " + keyDate.ToShortDateString();
                    EntryFilterShortDescription += "before " + keyDate.ToShortDateString();
                    break;

                case "Do not open positions after":
                    EntryFilterLongDescription += "after " + keyDate.ToShortDateString();
                    EntryFilterShortDescription += "after " + keyDate.ToShortDateString();
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}