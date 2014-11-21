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
    public class DataBarsFilter : Indicator
    {
        public DataBarsFilter()
        {
            IndicatorName = "Data Bars Filter";
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

            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Do not use the newest bars",
                    "Do not use the oldest bars",
                    "Do not use the newest bars and oldest bars",
                    "Use the newest bars only",
                    "Use the oldest bars only"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Specify the entry bars.";

            // The NumericUpDown parameters.
            IndParam.NumParam[0].Caption = "Newest bars";
            IndParam.NumParam[0].Value = 1000;
            IndParam.NumParam[0].Min = 0;
            IndParam.NumParam[0].Max = 50000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The number of newest bars.";

            IndParam.NumParam[1].Caption = "Oldest bars";
            IndParam.NumParam[1].Value = 0;
            IndParam.NumParam[1].Min = 0;
            IndParam.NumParam[1].Max = 50000;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The number of oldest bars.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var newest = (int) IndParam.NumParam[0].Value;
            var oldest = (int) IndParam.NumParam[1].Value;

            // Calculation
            int firstBar = 0;
            var adBars = new double[Bars];

            // Calculation of the logic
			if (IsBacktester)
			{
				switch (IndParam.ListParam[0].Text)
				{
					case "Do not use the newest bars":
						for (int bar = firstBar; bar < Bars - newest; bar++)
							adBars[bar] = 1;
						break;
					case "Do not use the oldest bars":
						firstBar = Math.Min(oldest, Bars - 300);
						for (int bar = firstBar; bar < Bars; bar++)
							adBars[bar] = 1;
						break;
					case "Do not use the newest bars and oldest bars":
						firstBar = Math.Min(oldest, Bars - 300);
						int lastBar = Math.Max(firstBar + 300, Bars - newest);
						for (int bar = firstBar; bar < lastBar; bar++)
							adBars[bar] = 1;
						break;
					case "Use the newest bars only":
						firstBar = Math.Max(0, Bars - newest);
						firstBar = Math.Min(firstBar, Bars - 300);
						for (int bar = firstBar; bar < Bars; bar++)
							adBars[bar] = 1;
						break;
					case "Use the oldest bars only":
						oldest = Math.Max(300, oldest);
						for (int bar = firstBar; bar < oldest; bar++)
							adBars[bar] = 1;
						break;
				}
			}
			else
			{
				for (int bar = firstBar; bar < Bars; bar++)
					adBars[bar] = 1;
			}

			// Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
                {
                    CompName = "(No) Used bars",
                    DataType = IndComponentType.AllowOpenLong,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = adBars
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "(No) Used bars",
                    DataType = IndComponentType.AllowOpenShort,
                    ChartType = IndChartType.NoChart,
                    ShowInDynInfo = false,
                    FirstBar = firstBar,
                    Value = adBars
                };
        }

        public override void SetDescription()
        {
			if (!IsBacktester)
			{
				EntryFilterLongDescription  = "A back tester limitation. It hasn't effect on the trade.";
				EntryFilterShortDescription = "A back tester limitation. It hasn't effect on the trade.";
				return;
			}
		
            var newest = (int) IndParam.NumParam[0].Value;
            var oldest = (int) IndParam.NumParam[1].Value;

            EntryFilterLongDescription = "(a back tester limitation) ";
            EntryFilterShortDescription = "(a back tester limitation) ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Do not use the newest bars":
                    EntryFilterLongDescription += "Do not use the newest " + newest + " bars";
                    EntryFilterShortDescription += "Do not use the newest " + newest + " bars";
                    break;

                case "Do not use the oldest bars":
                    EntryFilterLongDescription += "Do not use the oldest " + oldest + " bars";
                    EntryFilterShortDescription += "Do not use the oldest " + oldest + " bars";
                    break;

                case "Do not use the newest bars and oldest bars":
                    EntryFilterLongDescription += "Do not use the newest " + newest + " bars and oldest " + oldest +
                                                  " bars";
                    EntryFilterShortDescription += "Do not use the newest " + newest + " bars and oldest " + oldest +
                                                   " bars";
                    break;

                case "Use the newest bars only":
                    EntryFilterLongDescription += "Use the newest " + newest + " bars only";
                    EntryFilterShortDescription += "Use the newest " + newest + " bars only";
                    break;

                case "Use the oldest bars only":
                    EntryFilterLongDescription += "Use the oldest " + newest + " bars only";
                    EntryFilterShortDescription += "Use the oldest " + newest + " bars only";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}