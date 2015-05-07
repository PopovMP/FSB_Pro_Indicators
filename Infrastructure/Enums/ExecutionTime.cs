//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

namespace ForexStrategyBuilder.Infrastructure.Enums
{
    /// <summary>
    /// The type of the time execution indicator.
    /// It is used with the indicators, which set opening / closing position price.
    /// </summary>
    public enum ExecutionTime
    {
        DuringTheBar,   // The opening / closing price can be everywhere in the bar.
        AtBarOpening,   // The opening / closing price is at the beginning of the bar.
        AtBarClosing,   // The opening / closing price is at the end of the bar.
        CloseAndReverse // For the close and reverse logic.
    }
}