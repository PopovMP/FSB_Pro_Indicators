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
        Dictionary<string, InstrumentProperties> InstrumentProperties { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        bool IsUseStartDate { get; set; }
        bool IsUseEndDate { get; set; }
        int MaximumBars { get; set; }
        int MinimumBars { get; set; }
        int MaxIntrabarBars { get; set; }
        bool IsCheckDataAtLoad { get; set; }
        bool IsCutOffBadData { get; set; }
        bool IsCutOffSatSunData { get; set; }
        bool IsFillInDataGaps { get; set; }
        bool IsCacheDataFiles { get; set; }
    }
}