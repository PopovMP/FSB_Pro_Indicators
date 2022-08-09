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
using System.Collections.Generic;
using ForexStrategyBuilder.Infrastructure.Entities;

namespace ForexStrategyBuilder.Infrastructure.Interfaces
{
    public interface IDataSource
    {
        Dictionary<string, InstrumentProperties> InstrumentProperties { get; }
        DateTime StartDate { get; }
        DateTime EndDate { get; }
        bool IsUseStartDate { get; }
        bool IsUseEndDate { get; }
        int MaximumBars { get; }
        int MinimumBars { get; }
    }
}