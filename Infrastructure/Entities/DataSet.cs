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
using System.Linq;
using System.Text;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    /// <summary>
    ///     The DataSet holds history data together with
    ///     their statistics and instrument properties.
    /// </summary>
    public class DataSet : IDataSet
    {
        public DataSet(string symbol, DataPeriod period, int bars)
        {
            InitializeDataSet(symbol, period, bars);
        }

        public DataSet()
        {
        }

        public string Symbol { get; private set; }
        public DataPeriod Period { get; private set; }
        public DataId DataId { get; set; }

        public int Bars { get; private set; }
        public DateTime[] Time { get; private set; }
        public double[] Open { get; private set; }
        public double[] High { get; private set; }
        public double[] Low { get; private set; }
        public double[] Close { get; private set; }
        public int[] Volume { get; private set; }

        public double Bid { get; private set; }
        public double Ask { get; private set; }

        public IInstrumentProperties Properties { get; set; }
        public DataParams DataParams { get; set; }

        public DateTime ServerTime { get; private set; }

        public void UpdateBar(int index, Bar bar)
        {
            if (index >= Bars) throw new IndexOutOfRangeException("index");

            Time[index] = bar.Time;
            Open[index] = bar.Open;
            High[index] = bar.High;
            Low[index] = bar.Low;
            Close[index] = bar.Close;
            Volume[index] = bar.Volume;
        }

        public void SetBidAsk(double bid, double ask)
        {
            Bid = bid;
            Ask = ask;
        }

        public void SetServerTime(DateTime time)
        {
            ServerTime = time;
        }

        public void ResetData(int bars)
        {
            Bars = bars;
            Time = new DateTime[bars];
            Open = new double[bars];
            High = new double[bars];
            Low = new double[bars];
            Close = new double[bars];
            Volume = new int[bars];
        }

        public void AddOrUpdate(Bar bar, int length)
        {
            for (int b = Bars - 1; b >= 0; b--)
            {
                if (bar.Time > Time[Bars - 1])
                {
                    if (Time.Length < length)
                        ExpandData();
                    else
                        ShiftData();
                    UpdateBar(Bars - 1, bar);
                    break;
                }
                if (bar.Time == Time[b])
                {
                    UpdateBar(b, bar);
                    break;
                }
            }
        }

        public IDataSet GetClone()
        {
            IDataSet clone = new DataSet(Symbol, Period, Bars);
            Time.CopyTo(clone.Time, 0);
            Open.CopyTo(clone.Open, 0);
            High.CopyTo(clone.High, 0);
            Low.CopyTo(clone.Low, 0);
            Close.CopyTo(clone.Close, 0);
            Volume.CopyTo(clone.Volume, 0);
            clone.SetBidAsk(Bid, Ask);
            clone.Properties = Properties.GetClone();
            clone.DataParams = DataParams;
            clone.SetServerTime(ServerTime);
            clone.LoadingNote = LoadingNote;
            clone.IsLoadingErrors = IsLoadingErrors;
            return clone;
        }

        public DataStats CalculateStats()
        {
            var stats = new DataStats
            {
                Symbol = Symbol,
                Period = Period,
                Bars = Bars,
                StartTime = Time[0],
                UpdatedOn = Updated,
                MinPrice = double.MaxValue,
                MaxPrice = double.MinValue,
                MaxDaysOff = 0
            };

            double maxHighLowPrice = double.MinValue;
            double maxCloseOpenPrice = double.MinValue;
            double sumHighLow = 0;
            double sumCloseOpen = 0;
            double sumGap = 0;
            double instrMaxGap = double.MinValue;

            for (int bar = 1; bar < Bars; bar++)
            {
                if (High[bar] > stats.MaxPrice)
                    stats.MaxPrice = High[bar];

                if (Low[bar] < stats.MinPrice)
                    stats.MinPrice = Low[bar];

                if (Math.Abs(High[bar] - Low[bar]) > maxHighLowPrice)
                    maxHighLowPrice = Math.Abs(High[bar] - Low[bar]);
                sumHighLow += Math.Abs(High[bar] - Low[bar]);

                if (Math.Abs(Close[bar] - Open[bar]) > maxCloseOpenPrice)
                    maxCloseOpenPrice = Math.Abs(Close[bar] - Open[bar]);
                sumCloseOpen += Math.Abs(Close[bar] - Open[bar]);

                int dayDiff = (Time[bar] - Time[bar - 1]).Days;
                if (stats.MaxDaysOff < dayDiff)
                    stats.MaxDaysOff = dayDiff;

                double gap = Math.Abs(Open[bar] - Close[bar - 1]);
                sumGap += gap;
                if (instrMaxGap < gap)
                    instrMaxGap = gap;
            }

            double point = Properties.Point;

            stats.AverageGap = (int) (sumGap/((Bars - 1)*point));
            stats.MaxGap = (int) (instrMaxGap/Properties.Point);
            stats.AverageHighLow = (int) (sumHighLow/(Bars*point));
            stats.MaxHighLow = (int) (maxHighLowPrice/point);
            stats.AverageCloseOpen = (int) (sumCloseOpen/(Bars*point));
            stats.MaxCloseOpen = (int) (maxCloseOpenPrice/point);

            return stats;
        }

        public string ToCsvString()
        {
            string ff = string.Format("F{0}", Properties.Digits);
            var sb = new StringBuilder();
            for (int bar = 0; bar < Bars; bar++)
            {
                sb.Append(Time[bar].ToString("yyyy-MM-dd") + "\t");
                sb.Append(Time[bar].ToString("HH:mm") + "\t");
                sb.Append(Open[bar].ToString(ff) + "\t");
                sb.Append(High[bar].ToString(ff) + "\t");
                sb.Append(Low[bar].ToString(ff) + "\t");
                sb.Append(Close[bar].ToString(ff) + "\t");
                sb.Append(Volume[bar] + Environment.NewLine);
            }
            return sb.ToString();
        }

        public string LoadingNote { get; set; }
        public bool IsLoadingErrors { get; set; }

        public DateTime Updated
        {
            get { return Time.Last().AddMinutes((int) Period); }
        }

        public Bar Bar(int bar)
        {
            return new Bar
            {
                Time = Time[bar],
                Open = Open[bar],
                High = High[bar],
                Low = Low[bar],
                Close = Close[bar],
                Volume = Volume[bar]
            };
        }

        private void ExpandData()
        {
            Bars++;
            Time = ExpandArray(Time);
            Open = ExpandArray(Open);
            High = ExpandArray(High);
            Low = ExpandArray(Low);
            Close = ExpandArray(Close);
            Volume = ExpandArray(Volume);
        }

        private void ShiftData()
        {
            Time = ShiftArray(Time);
            Open = ShiftArray(Open);
            High = ShiftArray(High);
            Low = ShiftArray(Low);
            Close = ShiftArray(Close);
            Volume = ShiftArray(Volume);
        }

        private T[] ExpandArray<T>(T[] input) where T : new()
        {
            var output = new T[input.Length + 1];
            Array.Copy(input, 0, output, 0, input.Length);
            return output;
        }

        private T[] ShiftArray<T>(T[] input) where T : new()
        {
            var output = new T[input.Length];
            Array.Copy(input, 1, output, 0, input.Length - 1);
            return output;
        }

        private void InitializeDataSet(string symbol, DataPeriod period, int bars)
        {
            ResetData(bars);

            Symbol = symbol;
            Period = period;
        }
    }
}