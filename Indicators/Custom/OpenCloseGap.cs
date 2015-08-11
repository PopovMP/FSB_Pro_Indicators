//==============================================================
// Forex Strategy Builder
// Copyright © Forex Software Ltd. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Globalization;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class OpenCloseGap : Indicator
    {
        public OpenCloseGap()
        {
            IndicatorName = "Open Close Gap";
            PossibleSlots = SlotTypes.OpenFilter;
            SeparatedChart = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "1.0";
            IndicatorDescription = "The indicator measures the bar Close-Open gap.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                "Do not trade on gap",
                "Trade on gap",
                "Trade long on positive gap",
                "Trade long on negative gap"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Determines the entry conditions";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Open"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = "Open";
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Current bar Open minus previous bar Close.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Gap height [points]";
            IndParam.NumParam[0].Value = 100;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Gap height in points";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            double gapLimit = IndParam.NumParam[0].Value*Point;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            double[] histogram = new double[Bars];
            double[] longSignal = new double[Bars];
            double[] shortSignal = new double[Bars];

            int firstBar = 2;
            for (int bar = 1; bar < Bars; bar++)
                histogram[bar] = Open[bar] - Close[bar - 1];

            double sigma = Sigma();
            if (IndParam.ListParam[0].Text == "Do not trade on gap")
            {
                for (int bar = 1 + previous; bar < Bars; bar++)
                {
                    double trade = Math.Abs(histogram[bar - previous]) < gapLimit - sigma ? 1 : 0;
                    longSignal[bar] = trade;
                    shortSignal[bar] = trade;
                }
            }
            else if (IndParam.ListParam[0].Text == "Trade on gap")
            {
                for (int bar = 1 + previous; bar < Bars; bar++)
                {
                    double trade = Math.Abs(histogram[bar - previous]) >= gapLimit ? 1 : 0;
                    longSignal[bar] = trade;
                    shortSignal[bar] = trade;
                }
            }
            else if (IndParam.ListParam[0].Text == "Trade long on positive gap")
            {
                for (int bar = 1 + previous; bar < Bars; bar++)
                {
                    longSignal[bar] = histogram[bar - previous] >= gapLimit ? 1 : 0;
                    shortSignal[bar] = histogram[bar - previous] <= -gapLimit ? 1 : 0;
                }
            }
            else if (IndParam.ListParam[0].Text == "Trade long on negative gap")
            {
                for (int bar = 1 + previous; bar < Bars; bar++)
                {
                    longSignal[bar] = histogram[bar - previous] <= -gapLimit ? 1 : 0;
                    shortSignal[bar] = histogram[bar - previous] >= gapLimit ? 1 : 0;
                }
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
            {
                CompName = "Gap Histogram",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Histogram,
                FirstBar = firstBar,
                Value = histogram
            };

            Component[1] = new IndicatorComp
            {
                CompName = "Is long entry allowed",
                DataType = IndComponentType.AllowOpenLong,
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = longSignal
            };

            Component[2] = new IndicatorComp
            {
                CompName = "Is short entry allowed",
                DataType = IndComponentType.AllowOpenShort,
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = shortSignal
            };
        }

        public override void SetDescription()
        {
            double gapLimit = IndParam.NumParam[0].Value;
            string posGapText = gapLimit.ToString(CultureInfo.InvariantCulture);
            string negGapText = (-gapLimit).ToString(CultureInfo.InvariantCulture);

            EntryFilterLongDescription = "Open Close gap ";
            EntryFilterShortDescription = "Open Close gap ";

            if (IndParam.ListParam[0].Text == "Do not trade on gap")
            {
                EntryFilterLongDescription += "is less than " + posGapText;
                EntryFilterShortDescription += "is less than " + posGapText;
            }
            else if (IndParam.ListParam[0].Text == "Trade on gap")
            {
                EntryFilterLongDescription += "is more than " + posGapText;
                EntryFilterShortDescription += "is more than " + posGapText;
            }
            else if (IndParam.ListParam[0].Text == "Trade long on positive gap")
            {
                EntryFilterLongDescription += "is more than " + posGapText;
                EntryFilterShortDescription += "is less than " + negGapText;
            }
            else if (IndParam.ListParam[0].Text == "Trade long on negative gap")
            {
                EntryFilterLongDescription += "is less than " + negGapText;
                EntryFilterShortDescription += "is more than " + posGapText;
            }
        }
    }
}