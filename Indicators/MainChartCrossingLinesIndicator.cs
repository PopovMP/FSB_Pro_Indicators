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

namespace ForexStrategyBuilder.Indicators
{
    public class MainChartCrossingLinesIndicator : Indicator
    {
        // Input parameters
        public BasePrice IndicatorBasePrice { get; set; }
        protected MAMethod FastLineMethod { get; set; }
        protected MAMethod SlowLineMethod { get; set; }
        protected int FastLinePeriod { get; set; }
        protected int SlowLinePeriod { get; set; }
        protected int FastLineShift { get; set; }
        protected int SlowLineShift { get; set; }
        protected bool UsePreviousBar { get; set; }

        // Indicator values
        protected int FirstBar { get; set; }
        protected double[] FastLine { get; set; }
        protected double[] SlowLine { get; set; }

        public MainChartCrossingLinesIndicator()
        {
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Fast line crosses Slow line upward",
                    "Fast line crosses Slow line downlard",
                    "Fast line is higher than Slow line",
                    "Fast line is lower than Slow line"
                };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[1].Index    = (int)BasePrice.Close;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The base price for both lines.";

            IndParam.ListParam[2].Caption  = "Fast line smoothing";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[2].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[2].Text     = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "Smoothing method for Fast line.";

            IndParam.ListParam[3].Caption  = "Slow line smoothing";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[3].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[3].Text     = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[3].Enabled  = true;
            IndParam.ListParam[3].ToolTip  = "Smoothing method for Slow line.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Fast line period";
            IndParam.NumParam[0].Value   = 13;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period for calculation of Fast line.";

            IndParam.NumParam[1].Caption = "Slow line period";
            IndParam.NumParam[1].Value   = 21;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period for calculation of Slow line.";

            IndParam.NumParam[2].Caption = "Fast line shift";
            IndParam.NumParam[2].Value   = 0;
            IndParam.NumParam[2].Min     = 0;
            IndParam.NumParam[2].Max     = 100;
            IndParam.NumParam[2].Point   = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "Fast line shift in bars.";

            IndParam.NumParam[3].Caption = "Slow line shift";
            IndParam.NumParam[3].Value   = 0;
            IndParam.NumParam[3].Min     = 0;
            IndParam.NumParam[3].Max     = 100;
            IndParam.NumParam[3].Point   = 0;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "Slow line shift in bars.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        protected virtual void InitCalculation(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            IndicatorBasePrice = (BasePrice)IndParam.ListParam[1].Index;
            FastLineMethod = (MAMethod)IndParam.ListParam[2].Index;
            SlowLineMethod = (MAMethod)IndParam.ListParam[3].Index;
            FastLinePeriod = (int)IndParam.NumParam[0].Value;
            SlowLinePeriod = (int)IndParam.NumParam[1].Value;
            FastLineShift  = (int)IndParam.NumParam[2].Value;
            SlowLineShift  = (int)IndParam.NumParam[3].Value;
            UsePreviousBar = IndParam.CheckParam[0].Checked;

            // Initilaization
            FirstBar   = Math.Max(FastLinePeriod + FastLineShift, SlowLinePeriod + SlowLineShift) + 2;
            FastLine  = new double[Bars];
            SlowLine = new double[Bars];
        }

        protected virtual void PostCalculation()
        {
            int previous = UsePreviousBar ? 1 : 0;
            double[] oscillator = new double[Bars];

            for (int bar = FirstBar; bar < Bars; bar++)
                oscillator[bar] = FastLine[bar] - SlowLine[bar];

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
            {
                CompName   = "Fast line",
                ChartColor = Color.Goldenrod,
                DataType   = IndComponentType.IndicatorValue,
                ChartType  = IndChartType.Line,
                FirstBar   = FirstBar,
                Value      = FastLine
            };

            Component[1] = new IndicatorComp
            {
                CompName   = "Slow line",
                ChartColor = Color.IndianRed,
                DataType   = IndComponentType.IndicatorValue,
                ChartType  = IndChartType.Line,
                FirstBar   = FirstBar,
                Value      = SlowLine
            };

            Component[2] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = FirstBar,
                Value    = new double[Bars]
            };

            Component[3] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar  = FirstBar,
                Value     = new double[Bars]
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
                case "Fast line crosses Slow line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;
                case "Fast line crosses Slow line downlard":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;
                case "Fast line is higher than Slow line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;
                case "Fast line is lower than Slow line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;
            }

            OscillatorLogic(FirstBar, previous, oscillator, 0, 0, ref Component[2], ref Component[3], indLogic);
        }

        public override void SetDescription()
        {
            string text = ToString() + "; ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Fast line crosses Slow line upward":
                    EntryFilterLongDescription  = text + "Fast line crosses Slow line upward";
                    EntryFilterShortDescription = text + "Fast line crosses Slow line downward";
                    ExitFilterLongDescription   = text + "Fast line crosses Slow line upward";
                    ExitFilterShortDescription  = text + "Fast line crosses Slow line downward";
                    break;

                case "Fast line crosses Slow line downlard":
                    EntryFilterLongDescription  = text + "Fast line crosses Slow line downward";
                    EntryFilterShortDescription = text + "Fast line crosses Slow line upward";
                    ExitFilterLongDescription   = text + "Fast line crosses Slow line downward";
                    ExitFilterShortDescription  = text + "Fast line crosses Slow line upward";
                    break;

                case "Fast line is higher than Slow line":
                    EntryFilterLongDescription  = text + "Fast line is higher than Slow line";
                    EntryFilterShortDescription = text + "Fast line is lower than Slow line";
                    ExitFilterLongDescription   = text + "Fast line is higher than Slow line";
                    ExitFilterShortDescription  = text + "Fast line is lower than Slow line";
                    break;

                case "Fast line is lower than Slow line":
                    EntryFilterLongDescription  = text + "Fast line is lower than Slow line";
                    EntryFilterShortDescription = text + "Fast line is higher than Slow line";
                    ExitFilterLongDescription   = text + "Fast line is lower than Slow line";
                    ExitFilterShortDescription  = text + "Fast line is higher than Slow line";
                    break;
            }
        }
    }
}
