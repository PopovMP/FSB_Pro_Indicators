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
    public enum IndComponentType
    {
        NotDefined,
        OpenLongPrice,   // Contains the long positions opening price.
        OpenShortPrice,  // Contains the short positions opening price.
        OpenPrice,       // Contains the long or short positions opening price.
        CloseLongPrice,  // Contains the long positions closing price.
        CloseShortPrice, // Contains the short positions closing price.
        ClosePrice,      // Contains the long or short positions closing price.
        OpenClosePrice,  // SlotTypes.Close or SlotTypes.Close
        IndicatorValue,  // It's not a signal or opening / closing price
        AllowOpenLong,   // Long positions opening signal
        AllowOpenShort,  // Short positions opening signal
        ForceCloseLong,  // Long positions closing signal
        ForceCloseShort, // Short positions closing signal
        ForceClose,      // Long or Short positions closing signal
        Other
    }
}