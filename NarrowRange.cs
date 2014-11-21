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
    public class NarrowRange : Indicator
    {
        public NarrowRange()
        {
            // General properties
            IndicatorName = "Narrow Range";
            PossibleSlots = SlotTypes.OpenFilter;
            SeparatedChart = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "There is a NR4 formation",
                    "There is a NR7 formation"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            int iPrvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int iStepBack = (IndParam.ListParam[0].Text == "There is a NR4 formation" ? 3 : 6);
            int iFirstBar = iStepBack + iPrvs;
            var adNr = new double[Bars];
            var adRange = new double[Bars];

            for (int iBar = 0; iBar < Bars; iBar++)
            {
                adRange[iBar] = High[iBar] - Low[iBar];
                adNr[iBar] = 0;
            }

            // Calculation of the logic
            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
                bool bNarrowRange = true;
                for (int i = 1; i <= iStepBack; i++)
                    if (adRange[iBar - i - iPrvs] <= adRange[iBar - iPrvs])
                    {
                        bNarrowRange = false;
                        break;
                    }

                if (bNarrowRange) adNr[iBar] = 1;
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "Bar Range",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Histogram,
                    FirstBar = iFirstBar,
                    Value = new double[Bars]
                };
            for (int i = 0; i < Bars; i++)
                Component[0].Value[i] = Math.Round(adRange[i]/Point);

            Component[1] = new IndicatorComp
                {
                    CompName = "Allow long entry",
                    DataType = IndComponentType.AllowOpenLong,
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = adNr
                };

            Component[2] = new IndicatorComp
                {
                    CompName = "Allow short entry",
                    DataType = IndComponentType.AllowOpenShort,
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = adNr
                };
        }

        public override void SetDescription()
        {
            string sFormation = (IndParam.ListParam[0].Text == "There is a NR4 formation" ? "NR4" : "NR7");

            EntryFilterLongDescription = "there is a " + sFormation + " formation";
            EntryFilterShortDescription = "there is a " + sFormation + " formation";
        }

        public override string ToString()
        {
            return IndicatorName + (IndParam.ListParam[0].Text == "There is a NR4 formation" ? " NR4" : " NR7");
        }
    }
}