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
    public class EntryHour : Indicator
    {
        public EntryHour()
        {
            // General properties
            IndicatorName = "Entry Hour";
            PossibleSlots = SlotTypes.Open;
            IsDeafultGroupAll = true;

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
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Enter the market at the specified hour"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Open"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The execution price of all entry orders.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Entry hour";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 23;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The position's opening hour.";

            // The NumericUpDown parameters
            IndParam.NumParam[1].Caption = "Entry minutes";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 59;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The position's opening minute.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var entryHour = (int) IndParam.NumParam[0].Value;
            var entryMinute = (int) IndParam.NumParam[1].Value;
            var entryTime = new TimeSpan(entryHour, entryMinute, 0);

            // Calculation
            const int firstBar = 1;
            var adBars = new double[Bars];

            // Calculation of the logic
            for (int bar = firstBar; bar < Bars; bar++)
            {
                adBars[bar] = Time[bar].TimeOfDay == entryTime ? Open[bar] : 0;
            }

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
                {
                    CompName = "Entry hour",
                    DataType = IndComponentType.OpenPrice,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = adBars
                };
        }

        public override void SetDescription()
        {
            var entryHour = (int) IndParam.NumParam[0].Value;
            var entryMinute = (int) IndParam.NumParam[1].Value;
            string entryTime = entryHour.ToString("00") + ":" + entryMinute.ToString("00");

            EntryPointLongDescription = "at the beginning of the first bar after " + entryTime + " hours";
            EntryPointShortDescription = "at the beginning of the first bar after " + entryTime + " hours";
        }

        public override string ToString()
        {
            var entryHour = (int) IndParam.NumParam[0].Value;
            var entryMinute = (int) IndParam.NumParam[1].Value;

            string entryTime = entryHour.ToString("00") + ":" + entryMinute.ToString("00");

            return IndicatorName + " (" +
                             entryTime + ")"; // Entry Hour
        }
    }
}