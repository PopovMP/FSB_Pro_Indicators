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

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class ExitTime : Indicator
    {
        public ExitTime()
        {
            IndicatorName = "Exit Time";
            PossibleSlots = SlotTypes.CloseFilter;
            IsDeafultGroupAll = true;
            IsGeneratable = false;
            WarningMessage = "The indicator rises a signal when the bar Close time is equal to the specified time." + Environment.NewLine +
                             "The strategy time frame has to allow closing at the specified time.";

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "1.0";
            IndicatorDescription = "Forces an exit at a specified time";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[] {"Exit the market at a specified time"};
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Exit hour";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 23;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The hour component of the exit time.";

            IndParam.NumParam[1].Caption = "Exit minute";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 59;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The minutes component of the exit time.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            int hour = (int) IndParam.NumParam[0].Value;
            int minute = (int) IndParam.NumParam[1].Value;

            // Calculation
            const int firstBar = 2;
            double[] signal = new double[Bars];

            // Calculation of the logic
            for (int bar = firstBar; bar < Bars; bar++)
            {
                DateTime closeTime = Time[bar].AddMinutes((int) Period);
                if (closeTime.Hour == hour && closeTime.Minute == minute)
                    signal[bar] = 1;
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
            {
                CompName = "Close out long position",
                DataType = IndComponentType.ForceCloseLong,
                ChartType = IndChartType.NoChart,
                ShowInDynInfo = true,
                FirstBar = firstBar,
                Value = signal
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Close out short position",
                DataType = IndComponentType.ForceCloseShort,
                ChartType = IndChartType.NoChart,
                ShowInDynInfo = true,
                FirstBar = firstBar,
                Value = signal
            };
        }

        public override void SetDescription()
        {
            int hour = (int) IndParam.NumParam[0].Value;
            int minute = (int) IndParam.NumParam[1].Value;
            string exitTimeText = hour.ToString("D2") + ":" + minute.ToString("D2");
            ExitFilterLongDescription = "Exit the market at " + exitTimeText;
            ExitFilterShortDescription = "Exit the market at " + exitTimeText;
        }
    }
}