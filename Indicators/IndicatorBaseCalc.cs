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

namespace ForexStrategyBuilder.Indicators
{
    public partial class Indicator
    {
        /// <summary>
        ///     Converts indicator components to strategy data set time.
        /// </summary>
        public void NormalizeComponents(IDataSet strategyDataSet)
        {
            foreach (IndicatorComp indicatorComp in Component)
            {
                indicatorComp.Value = NormalizeComponentValue(indicatorComp.Value, strategyDataSet.Time);
                indicatorComp.FirstBar = NormalizeComponentFirstBar(indicatorComp.FirstBar, strategyDataSet.Time);
            }
        }

        /// <summary>
        ///     Shifts the indicators signals.
        /// </summary>
        public void ShiftSignal(int shift)
        {
            foreach (IndicatorComp indicatorComp in Component)
            {
                if (!IsSignalComponent(indicatorComp.DataType)) continue;
                var value = new double[indicatorComp.Value.Length + shift];
                indicatorComp.Value.CopyTo(value, shift);
                for (int bar = 0; bar < indicatorComp.Value.Length; bar++)
                    indicatorComp.Value[bar] = value[bar];
            }
        }

        /// <summary>
        ///     Repeats the indicators signals.
        /// </summary>
        public void RepeatSignal(int repeat)
        {
            foreach (IndicatorComp indicatorComp in Component)
            {
                if (!IsSignalComponent(indicatorComp.DataType)) continue;
                int bars = indicatorComp.Value.Length;
                for (int bar = 0; bar < bars; bar++)
                {
                    if (indicatorComp.Value[bar] < 0.5) continue;
                    for (int i = 1; i <= repeat; i++)
                        if (++bar < bars)
                            indicatorComp.Value[bar] = 1;
                }
            }
        }

        private double[] NormalizeComponentValue(double[] componentValue, DateTime[] strategyTime)
        {
            var strategyBars = strategyTime.Length;
            var value = new double[strategyBars];
            int reachedBar = 0;
            for (int ltfBar = 1; ltfBar < DataSet.Time.Length; ltfBar++)
            {
                DateTime ltfOpenTime = DataSet.Time[ltfBar];
                DateTime ltfCloseTime = ltfOpenTime.AddMinutes((int)DataSet.DataParams.Period);
                for (int bar = reachedBar; bar < strategyBars; bar++)
                {
                    reachedBar = bar;
                    DateTime time = strategyTime[bar];
                    if (time >= ltfOpenTime && time < ltfCloseTime)
                        value[bar] = componentValue[ltfBar - 1];
                    else if (time >= ltfCloseTime)
                        break;
                }
            }
            return value;
        }

        private int NormalizeComponentFirstBar(int componentFirstBar, DateTime[] strategyTime)
        {
            DateTime firstBarTime = DataSet.Time[componentFirstBar];
            for (int bar = 0; bar < strategyTime.Length; bar++)
                if (strategyTime[bar] >= firstBarTime)
                    return bar;
            return componentFirstBar;
        }

        private bool IsSignalComponent(IndComponentType componentType)
        {
            return componentType == IndComponentType.AllowOpenLong ||
            componentType == IndComponentType.AllowOpenShort ||
            componentType == IndComponentType.CloseLongPrice ||
            componentType == IndComponentType.ClosePrice ||
            componentType == IndComponentType.CloseShortPrice ||
            componentType == IndComponentType.ForceClose ||
            componentType == IndComponentType.ForceCloseLong ||
            componentType == IndComponentType.ForceCloseShort ||
            componentType == IndComponentType.OpenClosePrice ||
            componentType == IndComponentType.OpenLongPrice ||
            componentType == IndComponentType.OpenPrice ||
            componentType == IndComponentType.OpenShortPrice;
        }

        /// <summary>
        ///     Calculates the base price.
        /// </summary>
        /// <param name="priceType">The base price type.</param>
        /// <returns>Base price.</returns>
        protected double[] Price(BasePrice priceType)
        {
            var price = new double[Bars];

            switch (priceType)
            {
                case BasePrice.Open:
                    price = Open;
                    break;
                case BasePrice.High:
                    price = High;
                    break;
                case BasePrice.Low:
                    price = Low;
                    break;
                case BasePrice.Close:
                    price = Close;
                    break;
                case BasePrice.Median:
                    for (int bar = 0; bar < Bars; bar++)
                        price[bar] = (Low[bar] + High[bar])/2;
                    break;
                case BasePrice.Typical:
                    for (int bar = 0; bar < Bars; bar++)
                        price[bar] = (Low[bar] + High[bar] + Close[bar])/3;
                    break;
                case BasePrice.Weighted:
                    for (int bar = 0; bar < Bars; bar++)
                        price[bar] = (Low[bar] + High[bar] + 2*Close[bar])/4;
                    break;
            }
            return price;
        }

        /// <summary>
        ///     Calculates a Moving Average
        /// </summary>
        /// <param name="period">Period</param>
        /// <param name="shift">Shift</param>
        /// <param name="maMethod">Method of calculation</param>
        /// <param name="source">The array of source data</param>
        /// <returns>the Moving Average</returns>
        protected double[] MovingAverage(int period, int shift, MAMethod maMethod, double[] source)
        {
            var movingAverage = new double[Bars];

            if (period <= 1 && shift == 0)
                return source;

            if (period > Bars || period + shift <= 0 || period + shift > Bars)
            {
                // Wrong MovingAverage parameters
                return movingAverage;
            }

            for (int bar = 0; bar < period + shift - 1; bar++)
                movingAverage[bar] = 0;

            double sum = 0;
            for (int bar = shift; bar < period + shift; bar++)
                sum += source[bar];

            movingAverage[period + shift - 1] = sum/period;
            int lastBar = Math.Min(Bars, Bars - shift);

            switch (maMethod)
            {
                case MAMethod.Simple:
                    for (int bar = period; bar < lastBar; bar++)
                        movingAverage[bar + shift] = movingAverage[bar + shift - 1] + source[bar]/period -
                                                     source[bar - period]/period;
                    break;
                case MAMethod.Exponential:
                    {
                        double pr = 2d/(period + 1);
                        for (int bar = period; bar < lastBar; bar++)
                            movingAverage[bar + shift] = source[bar]*pr + movingAverage[bar + shift - 1]*(1 - pr);
                    }
                    break;
                case MAMethod.Weighted:
                    {
                        double weight = period*(period + 1)/2d;

                        for (int bar = period; bar < lastBar; bar++)
                        {
                            sum = 0;
                            for (int i = 0; i < period; i++)
                                sum += source[bar - i]*(period - i);
                            movingAverage[bar + shift] = sum/weight;
                        }
                    }
                    break;
                case MAMethod.Smoothed:
                    for (int bar = period; bar < lastBar; bar++)
                        movingAverage[bar + shift] = (movingAverage[bar + shift - 1]*(period - 1) + source[bar])/period;
                    break;
            }

            for (int bar = Bars + shift; bar < Bars; bar++)
                movingAverage[bar] = 0;

            return movingAverage;
        }

        /// <summary>
        ///     Maximum error for comparing indicator values
        /// </summary>
        protected double Sigma()
        {
            return SeparatedChart ? 0.000005 : Point*0.5;
        }

        protected double Epsilon { get { return 0.0000001; } }

        /// <summary>
        ///     Calculates the logic of an Oscillator.
        /// </summary>
        /// <param name="firstBar">The first bar number.</param>
        /// <param name="prvs">To use the previous bar or not.</param>
        /// <param name="adIndValue">The indicator values.</param>
        /// <param name="levelLong">The Level value for a Long position.</param>
        /// <param name="levelShort">The Level value for a Short position.</param>
        /// <param name="indCompLong">Indicator component for Long position.</param>
        /// <param name="indCompShort">Indicator component for Short position.</param>
        /// <param name="indLogic">The chosen logic.</param>
        /// <returns>True if everything is ok.</returns>
        protected void OscillatorLogic(int firstBar, int prvs, double[] adIndValue, double levelLong, double levelShort,
                                       ref IndicatorComp indCompLong, ref IndicatorComp indCompShort,
                                       IndicatorLogic indLogic)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            switch (indLogic)
            {
                case IndicatorLogic.The_indicator_rises:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - prvs;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];

                        if (!IsDiscreteValues) // Aroon oscillator uses IsDiscreteValues = true
                        {
                            bool isNoChange = true;
                            while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange &&
                                   baseBar > firstBar)
                            {
                                isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                                baseBar--;
                            }
                        }

                        indCompLong.Value[bar] = adIndValue[baseBar] < adIndValue[currentBar] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[baseBar] > adIndValue[currentBar] + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_falls:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - prvs;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];

                        if (!IsDiscreteValues) // Aroon oscillator uses IsDiscreteValues = true
                        {
                            bool isNoChange = true;
                            while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange &&
                                   baseBar > firstBar)
                            {
                                isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                                baseBar--;
                            }
                        }

                        indCompLong.Value[bar] = adIndValue[baseBar] > adIndValue[currentBar] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[baseBar] < adIndValue[currentBar] - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_higher_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = adIndValue[bar - prvs] > levelLong + sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[bar - prvs] < levelShort - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_lower_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = adIndValue[bar - prvs] < levelLong - sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[bar - prvs] > levelShort + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - prvs - 1;
                        while (Math.Abs(adIndValue[baseBar] - levelLong) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = (adIndValue[baseBar] < levelLong - sigma &&
                                                  adIndValue[bar - prvs] > levelLong + sigma)
                                                     ? 1
                                                     : 0;
                        indCompShort.Value[bar] = (adIndValue[baseBar] > levelShort + sigma &&
                                                   adIndValue[bar - prvs] < levelShort - sigma)
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - prvs - 1;
                        while (Math.Abs(adIndValue[baseBar] - levelLong) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = (adIndValue[baseBar] > levelLong + sigma &&
                                                  adIndValue[bar - prvs] < levelLong - sigma)
                                                     ? 1
                                                     : 0;
                        indCompShort.Value[bar] = (adIndValue[baseBar] < levelShort - sigma &&
                                                   adIndValue[bar - prvs] > levelShort + sigma)
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - prvs;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int iBar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[iBar2]) < sigma && iBar2 > firstBar)
                        {
                            iBar2--;
                        }

                        indCompLong.Value[bar] = (adIndValue[iBar2] > adIndValue[bar1] &&
                                                  adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1)
                                                     ? 1
                                                     : 0;
                        indCompShort.Value[bar] = (adIndValue[iBar2] < adIndValue[bar1] &&
                                                   adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1)
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - prvs;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int iBar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[iBar2]) < sigma && iBar2 > firstBar)
                        {
                            iBar2--;
                        }

                        indCompLong.Value[bar] = (adIndValue[iBar2] < adIndValue[bar1] &&
                                                  adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1)
                                                     ? 1
                                                     : 0;
                        indCompShort.Value[bar] = (adIndValue[iBar2] > adIndValue[bar1] &&
                                                   adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1)
                                                      ? 1
                                                      : 0;
                    }
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        ///     Calculates the logic of a No Direction Oscillator.
        /// </summary>
        /// <param name="firstBar">The first bar number.</param>
        /// <param name="prvs">To use the previous bar or not.</param>
        /// <param name="adIndValue">The indicator values.</param>
        /// <param name="dLevel">The Level value.</param>
        /// <param name="indComp">Indicator component where to save the results.</param>
        /// <param name="indLogic">The chosen logic.</param>
        /// <returns>True if everything is ok.</returns>
        protected void NoDirectionOscillatorLogic(int firstBar, int prvs, double[] adIndValue, double dLevel,
                                                  ref IndicatorComp indComp, IndicatorLogic indLogic)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            switch (indLogic)
            {
                case IndicatorLogic.The_indicator_rises:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - prvs;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];
                        bool isNoChange = true;

                        while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange &&
                               baseBar > firstBar)
                        {
                            isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                            baseBar--;
                        }

                        indComp.Value[bar] = adIndValue[baseBar] < adIndValue[currentBar] - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_falls:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - prvs;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];
                        bool isNoChange = true;

                        while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange &&
                               baseBar > firstBar)
                        {
                            isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                            baseBar--;
                        }

                        indComp.Value[bar] = adIndValue[baseBar] > adIndValue[currentBar] + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_higher_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indComp.Value[bar] = adIndValue[bar - prvs] > dLevel + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_lower_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indComp.Value[bar] = adIndValue[bar - prvs] < dLevel - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - prvs - 1;
                        while (Math.Abs(adIndValue[baseBar] - dLevel) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indComp.Value[bar] = (adIndValue[baseBar] < dLevel - sigma &&
                                              adIndValue[bar - prvs] > dLevel + sigma)
                                                 ? 1
                                                 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - prvs - 1;
                        while (Math.Abs(adIndValue[baseBar] - dLevel) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indComp.Value[bar] = (adIndValue[baseBar] > dLevel + sigma &&
                                              adIndValue[bar - prvs] < dLevel - sigma)
                                                 ? 1
                                                 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - prvs;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                        {
                            bar2--;
                        }

                        indComp.Value[bar] = (adIndValue[bar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] &&
                                              bar1 == bar0 - 1)
                                                 ? 1
                                                 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - prvs;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                        {
                            bar2--;
                        }

                        indComp.Value[bar] = (adIndValue[bar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] &&
                                              bar1 == bar0 - 1)
                                                 ? 1
                                                 : 0;
                    }
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        ///     Calculates the logic of a band indicator.
        /// </summary>
        /// <param name="firstBar">The first bar number.</param>
        /// <param name="prvs">To use the previous bar or not.</param>
        /// <param name="adUpperBand">The Upper band values.</param>
        /// <param name="adLowerBand">The Lower band values.</param>
        /// <param name="indCompLong">Indicator component for Long position.</param>
        /// <param name="indCompShort">Indicator component for Short position.</param>
        /// <param name="indLogic">The chosen logic.</param>
        protected void BandIndicatorLogic(int firstBar, int prvs, double[] adUpperBand, double[] adLowerBand,
                                          ref IndicatorComp indCompLong, ref IndicatorComp indCompShort,
                                          BandIndLogic indLogic)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            switch (indLogic)
            {
                case BandIndLogic.The_bar_opens_below_the_Upper_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] < adUpperBand[bar - prvs] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] > adLowerBand[bar - prvs] + sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Upper_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] > adUpperBand[bar - prvs] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] < adLowerBand[bar - prvs] - sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_below_the_Lower_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] < adLowerBand[bar - prvs] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] > adUpperBand[bar - prvs] + sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Lower_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] > adLowerBand[bar - prvs] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] < adUpperBand[bar - prvs] - sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] < adUpperBand[bar - prvs] - sigma &&
                                                 Open[baseBar] > adUpperBand[baseBar - prvs] + sigma
                                                     ? 1
                                                     : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] > adLowerBand[bar - prvs] + sigma &&
                                                  Open[baseBar] < adLowerBand[baseBar - prvs] - sigma
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] > adUpperBand[bar - prvs] + sigma &&
                                                 Open[baseBar] < adUpperBand[baseBar - prvs] - sigma
                                                     ? 1
                                                     : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] < adLowerBand[bar - prvs] - sigma &&
                                                  Open[baseBar] > adLowerBand[baseBar - prvs] + sigma
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] < adLowerBand[bar - prvs] - sigma &&
                                                 Open[baseBar] > adLowerBand[baseBar - prvs] + sigma
                                                     ? 1
                                                     : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] > adUpperBand[bar - prvs] + sigma &&
                                                  Open[baseBar] < adUpperBand[baseBar - prvs] - sigma
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] > adLowerBand[bar - prvs] + sigma &&
                                                 Open[baseBar] < adLowerBand[baseBar - prvs] - sigma
                                                     ? 1
                                                     : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - prvs]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] < adUpperBand[bar - prvs] - sigma &&
                                                  Open[baseBar] > adUpperBand[baseBar - prvs] + sigma
                                                      ? 1
                                                      : 0;
                    }
                    break;

                case BandIndLogic.The_bar_closes_below_the_Upper_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Close[bar] < adUpperBand[bar - prvs] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = Close[bar] > adLowerBand[bar - prvs] + sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_closes_above_the_Upper_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Close[bar] > adUpperBand[bar - prvs] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = Close[bar] < adLowerBand[bar - prvs] - sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_closes_below_the_Lower_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Close[bar] < adLowerBand[bar - prvs] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = Close[bar] > adUpperBand[bar - prvs] + sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_closes_above_the_Lower_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Close[bar] > adLowerBand[bar - prvs] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = Close[bar] < adUpperBand[bar - prvs] - sigma ? 1 : 0;
                    }
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "Indicator rises".
        /// </summary>
        protected void IndicatorRisesLogic(int firstBar, int prvs, double[] adIndValue, ref IndicatorComp indCompLong,
                                           ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - prvs;
                int baseBar = currentBar - 1;
                bool isNoChange = true;
                bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];

                while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange &&
                       baseBar > firstBar)
                {
                    isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] > adIndValue[baseBar] + sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] < adIndValue[baseBar] - sigma ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "Indicator falls"
        /// </summary>
        protected void IndicatorFallsLogic(int firstBar, int prvs, double[] adIndValue, ref IndicatorComp indCompLong,
                                           ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - prvs;
                int baseBar = currentBar - 1;
                bool isNoChange = true;
                bool isLower = adIndValue[currentBar] < adIndValue[baseBar];

                while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange &&
                       baseBar > firstBar)
                {
                    isNoChange = (isLower == (adIndValue[baseBar + 1] < adIndValue[baseBar]));
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] < adIndValue[baseBar] - sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] > adIndValue[baseBar] + sigma ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The Indicator is higher than the AnotherIndicator"
        /// </summary>
        protected void IndicatorIsHigherThanAnotherIndicatorLogic(int firstBar, int prvs, double[] adIndValue,
                                                                  double[] adAnotherIndValue,
                                                                  ref IndicatorComp indCompLong,
                                                                  ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - prvs;
                indCompLong.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The Indicator is lower than the AnotherIndicator"
        /// </summary>
        protected void IndicatorIsLowerThanAnotherIndicatorLogic(int firstBar, int prvs, double[] adIndValue,
                                                                 double[] adAnotherIndValue,
                                                                 ref IndicatorComp indCompLong,
                                                                 ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - prvs;
                indCompLong.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The Indicator crosses AnotherIndicator upward"
        /// </summary>
        protected void IndicatorCrossesAnotherIndicatorUpwardLogic(int firstBar, int prvs, double[] adIndValue,
                                                                   double[] adAnotherIndValue,
                                                                   ref IndicatorComp indCompLong,
                                                                   ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - prvs;
                int baseBar = currentBar - 1;
                while (Math.Abs(adIndValue[baseBar] - adAnotherIndValue[baseBar]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma &&
                                         adIndValue[baseBar] < adAnotherIndValue[baseBar] - sigma
                                             ? 1
                                             : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma &&
                                          adIndValue[baseBar] > adAnotherIndValue[baseBar] + sigma
                                              ? 1
                                              : 0;
            }
        }

        protected void IndicatorChangesItsDirectionUpward(int firstBar, int prvs, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            for (int bar = firstBar; bar < Bars; bar++)
            {
                int bar0 = bar - prvs;
                int bar1 = bar0 - 1;
                while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                    bar1--;

                int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                    bar2--;

                indCompLong.Value[bar] = (adIndValue[bar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                indCompShort.Value[bar] = (adIndValue[bar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
            }
        }

        protected void IndicatorChangesItsDirectionDownward(int firstBar, int prvs, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            for (int bar = firstBar; bar < Bars; bar++)
            {
                int bar0 = bar - prvs;
                int bar1 = bar0 - 1;
                while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                    bar1--;

                int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                    bar2--;

                indCompLong.Value[bar] = (adIndValue[bar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                indCompShort.Value[bar] = (adIndValue[bar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The Indicator crosses AnotherIndicator downward"
        /// </summary>
        protected void IndicatorCrossesAnotherIndicatorDownwardLogic(int firstBar, int prvs, double[] adIndValue,
                                                                     double[] adAnotherIndValue,
                                                                     ref IndicatorComp indCompLong,
                                                                     ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - prvs;
                int baseBar = currentBar - 1;
                while (Math.Abs(adIndValue[baseBar] - adAnotherIndValue[baseBar]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma &&
                                         adIndValue[baseBar] > adAnotherIndValue[baseBar] + sigma
                                             ? 1
                                             : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma &&
                                          adIndValue[baseBar] < adAnotherIndValue[baseBar] - sigma
                                              ? 1
                                              : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The bar opens above the Indicator"
        /// </summary>
        protected void BarOpensAboveIndicatorLogic(int firstBar, int prvs, double[] adIndValue,
                                                   ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                indCompLong.Value[bar] = Open[bar] > adIndValue[bar - prvs] + sigma ? 1 : 0;
                indCompShort.Value[bar] = Open[bar] < adIndValue[bar - prvs] - sigma ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The bar opens below the Indicator"
        /// </summary>
        protected void BarOpensBelowIndicatorLogic(int firstBar, int prvs, double[] adIndValue,
                                                   ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                indCompLong.Value[bar] = Open[bar] < adIndValue[bar - prvs] - sigma ? 1 : 0;
                indCompShort.Value[bar] = Open[bar] > adIndValue[bar - prvs] + sigma ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The bar opens above the Indicator after opening below it"
        /// </summary>
        protected void BarOpensAboveIndicatorAfterOpeningBelowLogic(int firstBar, int prvs, double[] adIndValue,
                                                                    ref IndicatorComp indCompLong,
                                                                    ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int baseBar = bar - 1;
                while (Math.Abs(Open[baseBar] - adIndValue[baseBar - prvs]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = Open[bar] > adIndValue[bar - prvs] + sigma &&
                                         Open[baseBar] < adIndValue[baseBar - prvs] - sigma
                                             ? 1
                                             : 0;
                indCompShort.Value[bar] = Open[bar] < adIndValue[bar - prvs] - sigma &&
                                          Open[baseBar] > adIndValue[baseBar - prvs] + sigma
                                              ? 1
                                              : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The bar opens below the Indicator after opening above it"
        /// </summary>
        protected void BarOpensBelowIndicatorAfterOpeningAboveLogic(int firstBar, int prvs, double[] adIndValue,
                                                                    ref IndicatorComp indCompLong,
                                                                    ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int baseBar = bar - 1;
                while (Math.Abs(Open[baseBar] - adIndValue[baseBar - prvs]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = Open[bar] < adIndValue[bar - prvs] - sigma &&
                                         Open[baseBar] > adIndValue[baseBar - prvs] + sigma
                                             ? 1
                                             : 0;
                indCompShort.Value[bar] = Open[bar] > adIndValue[bar - prvs] + sigma &&
                                          Open[baseBar] < adIndValue[baseBar - prvs] - sigma
                                              ? 1
                                              : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The bar closes above the Indicator"
        /// </summary>
        protected void BarClosesAboveIndicatorLogic(int firstBar, int prvs, double[] adIndValue,
                                                    ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                indCompLong.Value[bar] = Close[bar] > adIndValue[bar - prvs] + sigma ? 1 : 0;
                indCompShort.Value[bar] = Close[bar] < adIndValue[bar - prvs] - sigma ? 1 : 0;
            }
        }

        /// <summary>
        ///     Returns signals for the logic rule "The bar closes below the Indicator"
        /// </summary>
        protected void BarClosesBelowIndicatorLogic(int firstBar, int prvs, double[] adIndValue,
                                                    ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = Sigma();
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                indCompLong.Value[bar] = Close[bar] < adIndValue[bar - prvs] - sigma ? 1 : 0;
                indCompShort.Value[bar] = Close[bar] > adIndValue[bar - prvs] + sigma ? 1 : 0;
            }
        }

    }
}
