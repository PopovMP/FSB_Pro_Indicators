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
    public class ParabolicSar : Indicator
    {
        public ParabolicSar()
        {
            IndicatorName = "Parabolic SAR";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.Close;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.IsAllowLTF = false;

            if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                {
                    "The price is higher than the PSAR value"
                };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Exit the market at PSAR"
                };
            else
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Starting AF";
            IndParam.NumParam[0].Value = 0.02;
            IndParam.NumParam[0].Min = 0.00;
            IndParam.NumParam[0].Max = 5.00;
            IndParam.NumParam[0].Point = 2;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The starting value of Acceleration Factor.";

            IndParam.NumParam[1].Caption = "Increment";
            IndParam.NumParam[1].Value = 0.02;
            IndParam.NumParam[1].Min = 0.01;
            IndParam.NumParam[1].Max = 5.00;
            IndParam.NumParam[1].Point = 2;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Increment value.";

            IndParam.NumParam[2].Caption = "Maximum AF";
            IndParam.NumParam[2].Value = 2.00;
            IndParam.NumParam[2].Min = 0.01;
            IndParam.NumParam[2].Max = 9.00;
            IndParam.NumParam[2].Point = 2;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The maximum value of the Acceleration Factor.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            var afMin = IndParam.NumParam[0].Value;
            var afInc = IndParam.NumParam[1].Value;
            var afMax = IndParam.NumParam[2].Value;

            // Reading the parameters
            double extremum;
            double pSarNew = 0;
            var direction = new int[Bars];
            var pSar = new double[Bars];

            //----	Calculating the initial values
            pSar[0] = 0;
            var af = afMin;
            var newDir = 0;
            if (Close[1] > Open[0])
            {
                direction[0] = 1;
                direction[1] = 1;
                extremum = Math.Max(High[0], High[1]);
                pSar[1] = Math.Min(Low[0], Low[1]);
            }
            else
            {
                direction[0] = -1;
                direction[1] = -1;
                extremum = Math.Min(Low[0], Low[1]);
                pSar[1] = Math.Max(High[0], High[1]);
            }

            for (var bar = 2; bar < Bars; bar++)
            {
                //----	PSAR for the current period
                if (newDir != 0)
                {
                    // The direction was changed during the last period
                    direction[bar] = newDir;
                    newDir = 0;
                    pSar[bar] = pSarNew + af * (extremum - pSarNew);
                }
                else
                {
                    direction[bar] = direction[bar - 1];
                    pSar[bar] = pSar[bar - 1] + af * (extremum - pSar[bar - 1]);
                }

                // PSAR has to be out of the previous two bars limits
                if (direction[bar] > 0 && pSar[bar] > Math.Min(Low[bar - 1], Low[bar - 2]))
                    pSar[bar] = Math.Min(Low[bar - 1], Low[bar - 2]);
                else if (direction[bar] < 0 && pSar[bar] < Math.Max(High[bar - 1], High[bar - 2]))
                    pSar[bar] = Math.Max(High[bar - 1], High[bar - 2]);

                //----	PSAR for the next period

                // Calculation of the new values of flPExtr and flAF
                // if there is a new extreme price in the PSAR direction
                if (direction[bar] > 0 && High[bar] > extremum)
                {
                    extremum = High[bar];
                    af = Math.Min(af + afInc, afMax);
                }

                if (direction[bar] < 0 && Low[bar] < extremum)
                {
                    extremum = Low[bar];
                    af = Math.Min(af + afInc, afMax);
                }

                // Whether the price reaches PSAR
                if (Low[bar] <= pSar[bar] && pSar[bar] <= High[bar])
                {
                    newDir = -direction[bar];
                    pSarNew = extremum;
                    af = afMin;
                    extremum = newDir > 0 ? High[bar] : Low[bar];
                }
            }
            const int firstBar = 8;

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp
            {
                CompName = "PSAR value",
                DataType = SlotType == SlotTypes.Close ? IndComponentType.ClosePrice : IndComponentType.IndicatorValue,
                ChartType = IndChartType.Dot,
                ChartColor = Color.Violet,
                FirstBar = firstBar,
                PosPriceDependence = PositionPriceDependence.BuyHigherSellLower,
                Value = pSar
            };
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = "the price is higher than the " + ToString();
            EntryFilterShortDescription = "the price is lower than the " + ToString();
            ExitPointLongDescription = "at " + ToString() + ". It determines the position direction also";
            ExitPointShortDescription = "at " + ToString() + ". It determines the position direction also";
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.NumParam[0].ValueToString + ", " + // Starting AF
                   IndParam.NumParam[1].ValueToString + ", " + // Increment
                   IndParam.NumParam[2].ValueToString + ")"; // Max AF
        }
    }
}