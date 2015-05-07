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
    ///     Show dependence from the position's opening price
    /// </summary>
    public enum PositionPriceDependence
    {
        None,
        PriceBuyHigher,
        PriceBuyLower,
        PriceSellHigher,
        PriceSellLower,
        BuyHigherSellLower,
        BuyLowerSelHigher,
        PriceBuyCrossesUpBandInwards,
        PriceBuyCrossesUpBandOutwards,
        PriceBuyCrossesDownBandInwards,
        PriceBuyCrossesDownBandOutwards,
        PriceSellCrossesUpBandInwards,
        PriceSellCrossesUpBandOutwards,
        PriceSellCrossesDownBandInwards,
        PriceSellCrossesDownBandOutwards
    }
}