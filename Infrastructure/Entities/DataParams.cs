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
using System.Globalization;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public struct DataParams
    {
        public string DataSourceName { get; set; }
        public string Symbol { get; set; }
        public DataPeriod Period { get; set; }
        public DataId DataId { get; set; }
        public string Path { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsUseStartDate { get; set; }
        public bool IsUseEndDate { get; set; }
        public int MaximumBars { get; set; }
        public int MinimumBars { get; set; }
        public int MaxIntrabarBars { get; set; }
        public bool IsCheckDataAtLoad { get; set; }
        public bool IsCutOffBadData { get; set; }
        public bool IsCutOffSatSunData { get; set; }
        public bool IsFillInDataGaps { get; set; }
        public bool IsCacheData { get; set; }
        public bool IsLongData { get; set; }
    }
}