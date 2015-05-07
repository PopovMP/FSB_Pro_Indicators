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
using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class MovingAveragesCrossover : Indicator
    {
        public MovingAveragesCrossover()
        {
            IndicatorName = "Moving Averages Crossover";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Fast MA crosses Slow MA upward",
                    "Fast MA crosses Slow MA downward",
                    "Fast MA is higher than Slow MA",
                    "Fast MA is lower than Slow MA"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[1].Index = (int) BasePrice.Close;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The price both Moving Averages are based on.";

            IndParam.ListParam[3].Caption = "Fast MA method";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[3].Index = (int) MAMethod.Simple;
            IndParam.ListParam[3].Text = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled = true;
            IndParam.ListParam[3].ToolTip = "The method used for smoothing Fast Moving Averages.";

            IndParam.ListParam[4].Caption = "Slow MA method";
            IndParam.ListParam[4].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[4].Index = (int) MAMethod.Simple;
            IndParam.ListParam[4].Text = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[4].Enabled = true;
            IndParam.ListParam[4].ToolTip = "The method used for smoothing Slow Moving Averages.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Fast MA period";
            IndParam.NumParam[0].Value = 13;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Fast MA.";

            IndParam.NumParam[1].Caption = "Slow MA period";
            IndParam.NumParam[1].Value = 21;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of Slow MA.";

            IndParam.NumParam[2].Caption = "Fast MA shift";
            IndParam.NumParam[2].Value = 0;
            IndParam.NumParam[2].Min = 0;
            IndParam.NumParam[2].Max = 100;
            IndParam.NumParam[2].Point = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The shifting value of Fast MA.";

            IndParam.NumParam[3].Caption = "Slow MA shift";
            IndParam.NumParam[3].Value = 0;
            IndParam.NumParam[3].Min = 0;
            IndParam.NumParam[3].Max = 100;
            IndParam.NumParam[3].Point = 0;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "The shifting value of Slow MA.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var basePrice = (BasePrice) IndParam.ListParam[1].Index;
            var fastMAMethod = (MAMethod) IndParam.ListParam[3].Index;
            var slowMAMethod = (MAMethod) IndParam.ListParam[4].Index;
            var iNFastMA = (int) IndParam.NumParam[0].Value;
            var iNSlowMA = (int) IndParam.NumParam[1].Value;
            var iSFastMA = (int) IndParam.NumParam[2].Value;
            var iSSlowMA = (int) IndParam.NumParam[3].Value;
            int iPrvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            int iFirstBar = Math.Max(iNFastMA + iSFastMA, iNSlowMA + iSSlowMA) + 2;
            double[] adMAFast = MovingAverage(iNFastMA, iSFastMA, fastMAMethod, Price(basePrice));
            double[] adMASlow = MovingAverage(iNSlowMA, iSSlowMA, slowMAMethod, Price(basePrice));
            var adMAOscillator = new double[Bars];

            for (int iBar = iFirstBar; iBar < Bars; iBar++)
                adMAOscillator[iBar] = adMAFast[iBar] - adMASlow[iBar];

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
                {
                    CompName = "Fast Moving Average",
                    ChartColor = Color.Goldenrod,
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    FirstBar = iFirstBar,
                    Value = adMAFast
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Slow Moving Average",
                    ChartColor = Color.IndianRed,
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Line,
                    FirstBar = iFirstBar,
                    Value = adMASlow
                };

            Component[2] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = new double[Bars]
                };

            Component[3] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = new double[Bars]
                };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }

            // Calculation of the logic
            var indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "Fast MA crosses Slow MA upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;
                case "Fast MA crosses Slow MA downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;
                case "Fast MA is higher than Slow MA":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;
                case "Fast MA is lower than Slow MA":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;
            }

            OscillatorLogic(iFirstBar, iPrvs, adMAOscillator, 0, 0, ref Component[2], ref Component[3], indLogic);
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + "; Fast MA ";
            EntryFilterShortDescription = ToString() + "; Fast MA ";
            ExitFilterLongDescription = ToString() + "; Fast MA ";
            ExitFilterShortDescription = ToString() + "; Fast MA ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Fast MA crosses Slow MA upward":
                    EntryFilterLongDescription += "crosses Slow MA upward";
                    EntryFilterShortDescription += "crosses Slow MA downward";
                    ExitFilterLongDescription += "crosses Slow MA upward";
                    ExitFilterShortDescription += "crosses Slow MA downward";
                    break;

                case "Fast MA crosses Slow MA downward":
                    EntryFilterLongDescription += "crosses Slow MA downward";
                    EntryFilterShortDescription += "crosses Slow MA upward";
                    ExitFilterLongDescription += "crosses Slow MA downward";
                    ExitFilterShortDescription += "crosses Slow MA upward";
                    break;

                case "Fast MA is higher than Slow MA":
                    EntryFilterLongDescription += "is higher than Slow MA";
                    EntryFilterShortDescription += "is lower than Slow MA";
                    ExitFilterLongDescription += "is higher than Slow MA";
                    ExitFilterShortDescription += "is lower than Slow MA";
                    break;

                case "Fast MA is lower than Slow MA":
                    EntryFilterLongDescription += "is lower than Slow MA";
                    EntryFilterShortDescription += "is higher than Slow MA";
                    ExitFilterLongDescription += "is lower than Slow MA";
                    ExitFilterShortDescription += "is higher than Slow MA";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Price
                   IndParam.ListParam[3].Text + ", " + // Fast MA Method
                   IndParam.ListParam[4].Text + ", " + // Slow MA Method
                   IndParam.NumParam[0].ValueToString + ", " + // Fast MA period
                   IndParam.NumParam[1].ValueToString + ", " + // Slow MA period
                   IndParam.NumParam[2].ValueToString + ", " + // Fast MA shift
                   IndParam.NumParam[3].ValueToString + ")"; // Slow MA shift
        }
    }
}