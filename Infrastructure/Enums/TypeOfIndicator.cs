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
    ///     The type of the indicator.
    ///     It serves for arrangement of the indicators in the
    ///     indicator properties dialog window.
    ///     This doesn't affect the indicator application.
    /// </summary>
    public enum TypeOfIndicator
    {
        Indicator,
        Additional,
        OscillatorOfIndicators,
        IndicatorsMA,
        DateTime
    }
}