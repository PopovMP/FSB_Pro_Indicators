//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;

namespace ForexStrategyBuilder.Infrastructure.Interfaces
{
    public interface IIndicatorParam
    {
        string IndicatorName { get; set; }

        SlotTypes SlotType { get; set; }
        TypeOfIndicator IndicatorType { get; set; }
        ExecutionTime ExecutionTime { get; set; }
        bool IsDeafultGroupAll { get; set; }
        bool IsAllowLTF { get; set; }

        ListParam[] ListParam { get; }
        NumericParam[] NumParam { get; }
        CheckParam[] CheckParam { get; }

        IndicatorParam Clone();
        string ToString();
        string ToString(ITranslationManager tm);
    }
}