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
    public struct InfoRecord
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public InfoRecordFlag Flag { get; set; }
        public string Unit { get; set; }
    }
}