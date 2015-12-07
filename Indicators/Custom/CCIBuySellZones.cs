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
using ForexStrategyBuilder.Indicators.Store;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Custom
{
    public class CCIBuySellZones : Indicator
    {
        public CCIBuySellZones()
        {
            IndicatorName  = "CCI Buy Sell Zones";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;

            IndicatorAuthor      = "Miroslav Popov";
            IndicatorVersion     = "1.0";
            IndicatorDescription = "Determines buy and sell zones.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
            {
                SlotType == SlotTypes.OpenFilter
                    ? "Buy zone is formed"
                    : "Sell zone is formed"
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the oscillator.";

            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index    = (int) MAMethod.Simple;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The Moving Average method used for smoothing the CCI value.";

            IndParam.ListParam[2].Caption  = "MA method";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[2].Index    = (int) MAMethod.Exponential;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The Moving Average method used for smoothing the signal line.";

            IndParam.ListParam[3].Caption  = "Base price";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[3].Index    = (int) BasePrice.Typical;
            IndParam.ListParam[3].Text     = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled  = true;
            IndParam.ListParam[3].ToolTip  = "The base price of Commodity Channel Index.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "CCI period";
            IndParam.NumParam[0].Value   = 14;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Commodity Channel Index.";

            IndParam.NumParam[1].Caption = "MA period";
            IndParam.NumParam[1].Value   = 9;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of signal line.";

            IndParam.NumParam[2].Caption = "Level";
            IndParam.NumParam[2].Value   = 100;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 1000;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "MA signal level";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var maMethod  = (MAMethod) IndParam.ListParam[2].Index;
            var periodCci = (int) IndParam.NumParam[0].Value;
            var periodMa  = (int) IndParam.NumParam[1].Value;
            var level     = (int) IndParam.NumParam[2].Value;
            int previous  = IndParam.CheckParam[0].Checked ? 1 : 0;

            SpecialValues = new double[] {-level, level};

            // Calculation
            int firstBar = periodCci + periodMa + 2;

            var cciIndicator = new CommodityChannelIndex();
            cciIndicator.Initialize(SlotType);
            cciIndicator.IndParam.ListParam[1].Index    = IndParam.ListParam[1].Index;
            cciIndicator.IndParam.ListParam[2].Index    = IndParam.ListParam[3].Index;
            cciIndicator.IndParam.NumParam[0].Value     = IndParam.NumParam[0].Value;
            cciIndicator.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            cciIndicator.Calculate(DataSet);

            double[] cci      = cciIndicator.Component[0].Value;
            double[] maCci    = MovingAverage(periodMa, 0, maMethod, cci);
            double[] buyZone  = new double[Bars];
            double[] sellZone = new double[Bars];

            for (int bar = firstBar; bar < Bars; bar++)
            {
                if (buyZone[bar - 1] > Epsilon && maCci[bar] < -level)
                {
                    // Continue Buy zone
                    buyZone[bar] = 1;
                    continue;
                }

                if (sellZone[bar - 1] > Epsilon && maCci[bar] > level)
                {
                    // Continue Sell zone
                    sellZone[bar] = 1;
                    continue;
                }

                if (cci[bar] > maCci[bar] + Epsilon && cci[bar - 1] <= maCci[bar - 1] &&
                    maCci[bar] < -level)
                {
                    // Start Buy zone
                    buyZone[bar] = 1;
                    continue;
                }

                if (cci[bar] < maCci[bar] - Epsilon && cci[bar - 1] >= maCci[bar - 1] &&
                    maCci[bar] > level)
                {
                    // Start Sell zone
                    sellZone[bar] = 1;
                }
            }

            // Shift signal if it is necessary
            if (previous > 0)
            {
                for (int bar = Bars - 1; bar >= firstBar; bar--)
                {
                    buyZone[bar]  = buyZone[bar - 1];
                    sellZone[bar] = sellZone[bar - 1];
                }
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp
            {
                CompName   = "CCI",
                DataType   = IndComponentType.IndicatorValue,
                ChartType  = IndChartType.Line,
                ChartColor = Color.RoyalBlue,
                FirstBar   = firstBar,
                Value      = cci
            };

            Component[1] = new IndicatorComp
            {
                CompName   = "MA CCI",
                DataType   = IndComponentType.IndicatorValue,
                ChartType  = IndChartType.Line,
                ChartColor = Color.Red,
                FirstBar   = firstBar,
                Value      = maCci
            };

            Component[2] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar
            };

            Component[3] = new IndicatorComp
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[2].Value    = buyZone;
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
                Component[3].Value    = sellZone;
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[2].Value    = sellZone;
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
                Component[3].Value    = buyZone;
            }
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription  = ToString() + " forms a buy zone";
            EntryFilterShortDescription = ToString() + " forms a sell zone";
            ExitFilterLongDescription   = ToString() + " forms a sell zone";
            ExitFilterShortDescription  = ToString() + " forms a buy zone";
        }
    }
}
