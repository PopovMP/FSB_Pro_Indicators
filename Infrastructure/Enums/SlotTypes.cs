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

namespace ForexStrategyBuilder.Infrastructure.Enums
{
    [Flags]
    public enum SlotTypes : short
    {
        NotDefined  = 0,
        Open        = 1,
        OpenFilter  = 2,
        Close       = 4,
        CloseFilter = 8
    }
}