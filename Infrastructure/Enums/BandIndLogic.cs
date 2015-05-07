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
    public enum BandIndLogic
    {
        The_bar_opens_below_the_Upper_Band,
        The_bar_opens_above_the_Upper_Band,
        The_bar_opens_below_the_Lower_Band,
        The_bar_opens_above_the_Lower_Band,
        The_position_opens_below_the_Upper_Band,
        The_position_opens_above_the_Upper_Band,
        The_position_opens_below_the_Lower_Band,
        The_position_opens_above_the_Lower_Band,
        The_bar_opens_below_the_Upper_Band_after_opening_above_it,
        The_bar_opens_above_the_Upper_Band_after_opening_below_it,
        The_bar_opens_below_the_Lower_Band_after_opening_above_it,
        The_bar_opens_above_the_Lower_Band_after_opening_below_it,
        The_bar_closes_below_the_Upper_Band,
        The_bar_closes_above_the_Upper_Band,
        The_bar_closes_below_the_Lower_Band,
        The_bar_closes_above_the_Lower_Band,
        It_does_not_act_as_a_filter
    }
}