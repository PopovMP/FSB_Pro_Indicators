//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;

namespace ForexStrategyBuilder.Infrastructure.Interfaces
{
    public interface IInstrumentProperties
    {

        int Digits { get; set; }
        double Point { get; set; }
        double Pip { get; set; }
        bool IsFiveDigits { get; set; }

        InstrumentProperties GetClone();
    }
}