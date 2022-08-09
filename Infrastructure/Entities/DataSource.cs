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
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public class DataSource : IDataSource
    {
        public DataSource()
        {
            InstrumentProperties = new Dictionary<string, InstrumentProperties>(1)
            {
                {"EURUSD", new InstrumentProperties("EURUSD")}
            };
            StartDate          = new DateTime(2000,  1,  1);
            EndDate            = new DateTime(2050, 12, 31);
            IsUseStartDate     = false;
            IsUseEndDate       = false;
            MaximumBars        = 20000;
            MinimumBars        = 300;
        }

        public Dictionary<string, InstrumentProperties> InstrumentProperties { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsUseStartDate { get; set; }
        public bool IsUseEndDate { get; set; }
        public int MaximumBars { get; set; }
        public int MinimumBars { get; set; }
    }
}