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

namespace ForexStrategyBuilder.Infrastructure.Interfaces
{
    public interface IDataSet
    {
        string Symbol { get; }
        DataPeriod Period { get; }
        DataId DataId { get; set; }

        int Bars { get; }
        DateTime[] Time { get; }
        double[] Open { get; }
        double[] High { get; }
        double[] Low { get; }
        double[] Close { get; }
        int[] Volume { get; }

        double Bid { get; }
        double Ask { get; }

        IInstrumentProperties Properties { get; set; }
        DataParams DataParams { get; set; }
        DateTime ServerTime { get; }
        string LoadingNote { get; set; }

        void UpdateBar(int index, Bar bar);
    }
}