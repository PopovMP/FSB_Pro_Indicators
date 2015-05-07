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
    public class DataStats
    {
        public string Symbol { get; set; }
        public DataPeriod Period { get; set; }
        public int Bars { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime StartTime { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public int AverageGap { get; set; }
        public int MaxGap { get; set; }
        public int AverageHighLow { get; set; }
        public int MaxHighLow { get; set; }
        public int AverageCloseOpen { get; set; }
        public int MaxCloseOpen { get; set; }
        public int MaxDaysOff { get; set; }
    }
}