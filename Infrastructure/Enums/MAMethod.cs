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
    ///     The method of Moving Average used for the calculations
    /// </summary>
    public enum MAMethod
    {
        Simple,
        Weighted,
        Exponential,
        Smoothed
    }
}