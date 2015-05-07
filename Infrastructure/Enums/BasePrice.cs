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
    /// The type of base price
    /// </summary>
    public enum BasePrice
    {
        Open,
        High,
        Low,
        Close,
        Median,  // Price[bar] = (Low[bar] + High[bar]) / 2;
        Typical, // Price[bar] = (Low[bar] + High[bar] + Close[bar]) / 3;
        Weighted // Price[bar] = (Low[bar] + High[bar] + 2 * Close[bar]) / 4;
    }
}