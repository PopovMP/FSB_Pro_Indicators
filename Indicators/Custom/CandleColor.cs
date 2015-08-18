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

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class CandleColor : Indicator
    {
        public CandleColor()
        {
            IndicatorName = "Candle Color";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "1.1";
            IndicatorDescription = "Detects bullish and bearish candles.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                "Bullish Candle formed",
                "Bearish Candle formed"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Gives a signal when a candle is formed.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[]{"Bar range"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The indicator uses the bar range.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Min body height [pips]";
            IndParam.NumParam[0].Value = 5;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Minimum required body height in pips.";

            IndParam.NumParam[1].Caption = "Consecutive candles";
            IndParam.NumParam[1].Value = 1;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 10;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Gives a signal if there are consecutive candles.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var minHeight   = (int) IndParam.NumParam[0].Value;
            var consecutive = (int) IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            int firstBar = 2 + consecutive + previous;

            var whiteCandles = new int[Bars];
            var blackCandles = new int[Bars];
            var pipVal = dataSet.Properties.Pip*minHeight;
            for (int b = 1 + previous; b < Bars; b++)
            {
                if (Close[b - previous] - Open[b - previous] >= pipVal)
                    whiteCandles[b] = whiteCandles[b - 1] + 1;
                if (Open[b - previous] - Close[b - previous] >= pipVal)
                    blackCandles[b] = blackCandles[b - 1] + 1;
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            Component[1] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[0].DataType = IndComponentType.AllowOpenLong;
                Component[0].CompName = "Is long entry allowed";
                Component[1].DataType = IndComponentType.AllowOpenShort;
                Component[1].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[0].DataType = IndComponentType.ForceCloseLong;
                Component[0].CompName = "Close out long position";
                Component[1].DataType = IndComponentType.ForceCloseShort;
                Component[1].CompName = "Close out short position";
            }

            if(IndParam.ListParam[0].Text == "Bullish Candle formed")
                for (int b = firstBar; b < Bars; b++)
                {
                    Component[0].Value[b] = whiteCandles[b] >= consecutive ? 1 : 0;
                    Component[1].Value[b] = blackCandles[b] >= consecutive ? 1 : 0;
                }
            else if(IndParam.ListParam[0].Text == "Bearish Candle formed")
                for (int b = firstBar; b < Bars; b++)
                {
                    Component[1].Value[b] = whiteCandles[b] >= consecutive ? 1 : 0;
                    Component[0].Value[b] = blackCandles[b] >= consecutive ? 1 : 0;
                }
        }

        public override void SetDescription()
        {
            var consecutive = (int) IndParam.NumParam[1].Value;
            string white = consecutive == 1
                ? "a bullish candle is formed"
                : consecutive + " bullish candles are formed";
            string black = consecutive == 1
                ? "a bearish candle is formed"
                : consecutive + " bearish candles are formed";

            switch (IndParam.ListParam[0].Text)
            {
                case "Bullish Candle formed":
                    EntryFilterLongDescription  = white;
                    EntryFilterShortDescription = black;
                    ExitFilterLongDescription   = white;
                    ExitFilterShortDescription  = black;
                    break;

                case "Bearish Candle formed":
                    EntryFilterLongDescription  = black;
                    EntryFilterShortDescription = white;
                    ExitFilterLongDescription   = black;
                    ExitFilterShortDescription  = white;
                    break;
            }
        }
    }
}
