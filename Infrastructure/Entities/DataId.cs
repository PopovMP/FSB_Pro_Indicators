//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using ForexStrategyBuilder.Infrastructure.Enums;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public struct DataId
    {
        public DataId(string source, string symbol, DataPeriod period)
            : this()
        {
            Source = source;
            Symbol = symbol;
            Period = period;
        }

        public string Source { get; }
        public string Symbol { get; }
        public DataPeriod Period { get; }

        public override string ToString()
        {
            return string.Format("{0}; {1}; {2}", Source, Symbol, Period);
        }
    }
}