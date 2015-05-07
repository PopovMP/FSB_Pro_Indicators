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
    public interface IIndicator
    {

        string IndicatorName { get; }
        IIndicatorParam IndParam { get; set; }
        SlotTypes SlotType { get; }

        bool SeparatedChart { get; }
        IndicatorComp[] Component { get; }
        double[] SpecialValues { get; }
        double SeparatedChartMinValue { get; }
        double SeparatedChartMaxValue { get; }

        bool CustomIndicator { get; set; }
        bool OverrideMainIndicator { get; set; }
        bool LoaddedFromDll { get; set; }
        string WarningMessage { get; }
        bool AllowClosingFilters { get; }

        string EntryPointLongDescription { get; }
        string EntryPointShortDescription { get; }
        string ExitPointLongDescription { get; }
        string ExitPointShortDescription { get; }
        string EntryFilterLongDescription { get; }
        string ExitFilterLongDescription { get; }
        string EntryFilterShortDescription { get; }
        string ExitFilterShortDescription { get; }

        bool UsePreviousBarValue { get; set; }

        string IndicatorVersion { get; set; }
        string IndicatorAuthor { get; set; }
        string IndicatorDescription { get; set; }
        bool IsDeafultGroupAll { get; set; }

        bool IsGeneratable { get; set; }
        bool IsBacktester { get; set; }

        IDataSet DataSet { get; set; }

        bool TestPossibleSlot(SlotTypes slotType);

        void Initialize(SlotTypes slotType);
        void Calculate(IDataSet dataSet);
        void NormalizeComponents(IDataSet strategyDataSet);
        void ShiftSignal(int shift);
        void RepeatSignal(int repeat);

        void SetDescription();
    }
}