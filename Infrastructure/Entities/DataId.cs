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

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public struct DataId
    {

        public DataId(string dataId)
            : this()
        {
            string[] partList = dataId.Split(new[] { ';' });
            Symbol = partList[partList.Length - 2].TrimStart(new[] { ' ' });
            Period = (DataPeriod)Enum.Parse(typeof(DataPeriod), partList[partList.Length - 1].TrimStart(new[] { ' ' }));
            if (partList.Length == 3)
                Source = partList[0];
        }

        public DataId(string source, string symbol, DataPeriod period)
            : this()
        {
            Source = source;
            Symbol = symbol;
            Period = period;
        }

        public DataId(string symbol, DataPeriod period)
            : this()
        {
            Symbol = symbol;
            Period = period;
        }

        public string Source { get; private set; }
        public string Symbol { get; private set; }
        public DataPeriod Period { get; private set; }

        public bool Equal(DataId dataId)
        {
            return dataId.Source == Source && dataId.Symbol == Symbol && dataId.Period == Period;
        }

        public override string ToString()
        {
            return string.Format("{0}; {1}; {2}", Source, Symbol, Period);
        }

        public string ToNormalizedString()
        {
            return string.Format("{0} {1} {2}", Source, Symbol, Period);
        }

        public string ToShortString()
        {
            return string.Format("{0}; {1}", Symbol, Period);
        }
    }
}