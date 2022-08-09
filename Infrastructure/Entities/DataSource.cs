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
            var instrumentProperties = new InstrumentProperties("EURUSD", InstrumentType.Forex);
            instrumentProperties.SetPrecision();
            InstrumentProperties = new Dictionary<string, InstrumentProperties>(1)
            {
                {"EURUSD", instrumentProperties}
            };
            StartDate          = new DateTime(2000,  1,  1);
            EndDate            = new DateTime(2050, 12, 31);
            IsUseStartDate     = false;
            IsUseEndDate       = false;
            MaximumBars        = 20000;
            MinimumBars        = 300;
            MaxIntrabarBars    = 50000;
            IsCheckDataAtLoad  = true;
            IsCutOffBadData    = false;
            IsCutOffSatSunData = false;
            IsFillInDataGaps   = false;
            IsCacheDataFiles   = true;
        }

        public Dictionary<string, InstrumentProperties> InstrumentProperties { get; set; }
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
        public bool IsCacheDataFiles { get; set; }
    }
}