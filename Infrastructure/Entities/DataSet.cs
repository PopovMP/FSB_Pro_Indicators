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

        public double Bid { get; set; }
        public double Ask { get; set; }

        public IInstrumentProperties Properties { get; set; }
        public DataParams DataParams { get; set; }

        public DateTime ServerTime { get; set; }

        public void UpdateBar(int index, Bar bar)
        {
            if (index >= Bars) throw new IndexOutOfRangeException("index");

            Time  [index] = bar.Time;
            Open  [index] = bar.Open;
            High  [index] = bar.High;
            Low   [index] = bar.Low;
            Close [index] = bar.Close;
            Volume[index] = bar.Volume;
        }

        private void ResetData(int bars)
        {
            Bars   = bars;
            Time   = new DateTime[bars];
            Open   = new double[bars];
            High   = new double[bars];
            Low    = new double[bars];
            Close  = new double[bars];
            Volume = new int   [bars];
        }

        public string LoadingNote { get; set; }

        private void InitializeDataSet(string symbol, DataPeriod period, int bars)
        {
            ResetData(bars);

            Symbol = symbol;
            Period = period;
        }
    }
}