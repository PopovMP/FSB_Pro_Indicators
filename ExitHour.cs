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
    public class ExitHour : Indicator
    {
        public ExitHour()
        {
            IndicatorName = "Exit Hour";
            PossibleSlots = SlotTypes.Close;
            WarningMessage = "Exit Hour indicator works properly on 4H and lower time frame." + Environment.NewLine +
                             "It sends close signal when bar closes at the specified hour.";

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.1";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.ExecutionTime = ExecutionTime.AtBarClosing;
            IndParam.IndicatorType = TypeOfIndicator.DateTime;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                "Exit the market before the specified hour"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Close"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Exit price of the position.";

            // The NumericUpDown parameters.
            IndParam.NumParam[0].Caption = "Exit hour";
            IndParam.NumParam[0].Value = 0;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 24;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The position's closing hour.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var exitHour = (int) IndParam.NumParam[0].Value;
            var tsExitHour = new TimeSpan(exitHour, 0, 0);

            // Calculation
            const int firstBar = 1;
            var adBars = new double[Bars];

            // Calculation of the logic
            for (int bar = firstBar; bar < Bars; bar++)
            {
                if (Time[bar - 1].DayOfYear == Time[bar].DayOfYear &&
                    Time[bar - 1].TimeOfDay < tsExitHour &&
                    Time[bar].TimeOfDay >= tsExitHour)
                    adBars[bar - 1] = Close[bar - 1];
                else if (Time[bar - 1].DayOfYear != Time[bar].DayOfYear &&
                         Time[bar - 1].TimeOfDay < tsExitHour)
                    adBars[bar - 1] = Close[bar - 1];
                else
                    adBars[bar] = 0;
            }

            // Check the last bar
            if (Time[Bars - 1].TimeOfDay.Add(new TimeSpan(0, (int) Period, 0)) == tsExitHour)
                adBars[Bars - 1] = Close[Bars - 1];

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
            {
                CompName = "Exit hour",
                DataType = IndComponentType.ClosePrice,
                ChartType = IndChartType.NoChart,
                ShowInDynInfo = false,
                FirstBar = firstBar,
                Value = adBars
            };
        }

        public override void SetDescription()
        {
            ExitPointLongDescription = "at the end of the last bar before " + IndParam.NumParam[0].Value + " o'clock";
            ExitPointShortDescription = "at the end of the last bar before " + IndParam.NumParam[0].Value + " o'clock";
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.NumParam[0].ValueToString + ")"; // Exit Hour
        }
    }
}