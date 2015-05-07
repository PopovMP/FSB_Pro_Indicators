//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System.Globalization;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class NBarsExit : Indicator
    {
        public NBarsExit()
        {
            IndicatorName = "N Bars Exit";
            PossibleSlots = SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.Additional;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Exit N Bars after entry"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "N Bars";
            IndParam.NumParam[0].Value = 10;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 10000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The number of bars after entry to exit the position.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            if (!IsBacktester)
            {
                // FST sends the N bars for exit to the expert. Expert watches the position and closes it.
                return;
            }

            var nExit = (int) IndParam.NumParam[0].Value;

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
                {
                    CompName = "N Bars Exit (" + nExit.ToString(CultureInfo.InvariantCulture) + ")",
                    DataType = IndComponentType.ForceClose,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = true,
                    FirstBar = 1,
                    Value = new double[Bars]
                };
        }

        public override void SetDescription()
        {
            var nExit = (int) IndParam.NumParam[0].Value;

            ExitFilterLongDescription = nExit + " bars passed after the entry";
            ExitFilterShortDescription = nExit + " bars passed after the entry";
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.NumParam[0].ValueToString + ")"; // Number of Bars
        }
    }
}