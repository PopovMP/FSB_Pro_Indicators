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
using System.Text;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public class IndicatorParam : IIndicatorParam
    {
        public IndicatorParam()
        {
            SlotNumber        = 0;
            IsDefined         = false;
            SlotType          = SlotTypes.NotDefined;
            IndicatorName     = String.Empty;
            IndicatorType     = TypeOfIndicator.Indicator;
            ExecutionTime     = ExecutionTime.DuringTheBar;
            IsDeafultGroupAll = false;
            IsAllowLTF        = true;
            ListParam         = new ListParam[5];
            NumParam          = new NumericParam[6];
            CheckParam        = new CheckParam[2];

            for (int i = 0; i < 5; i++)
                ListParam[i] = new ListParam();

            for (int i = 0; i < 6; i++)
                NumParam[i] = new NumericParam();

            for (int i = 0; i < 2; i++)
                CheckParam[i] = new CheckParam();
        }

        /// <summary>
        ///     Gets or sets the number of current slot.
        /// </summary>
        private int SlotNumber { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the indicator is defined.
        /// </summary>
        private bool IsDefined { get; set; }

        /// <summary>
        ///     Gets or sets the type of the slot.
        /// </summary>
        public SlotTypes SlotType { get; set; }

        /// <summary>
        ///     Gets or sets the indicator name.
        /// </summary>
        public string IndicatorName { get; set; }

        /// <summary>
        ///     Gets or sets the type of the indicator
        /// </summary>
        public TypeOfIndicator IndicatorType { get; set; }

        /// <summary>
        ///     Gets if the default group is "All"
        /// </summary>
        public bool IsDeafultGroupAll { get; set; }

        /// <summary>
        ///     Shows if the indicator allows using Longer Time Frame, Signal Shift and Signal Repeat
        /// </summary>
        public bool IsAllowLTF { get; set; }

        /// <summary>
        ///     Gets or sets the type of the time execution of the indicator
        /// </summary>
        public ExecutionTime ExecutionTime { get; set; }

        /// <summary>
        ///     Gets or sets a parameter represented by a ComboBox.
        /// </summary>
        public ListParam[] ListParam { get; private set; }

        /// <summary>
        ///     Gets or sets a parameter represented by a NumericUpDown.
        /// </summary>
        public NumericParam[] NumParam { get; private set; }

        /// <summary>
        ///     Gets or sets a parameter represented by a CheckBox.
        /// </summary>
        public CheckParam[] CheckParam { get; private set; }

        /// <summary>
        ///     Returns a copy
        /// </summary>
        public IndicatorParam Clone()
        {
            var indicatorParam = new IndicatorParam
                {
                    SlotNumber        = SlotNumber,
                    IsDefined         = IsDefined,
                    SlotType          = SlotType,
                    IndicatorName     = IndicatorName,
                    IndicatorType     = IndicatorType,
                    ExecutionTime     = ExecutionTime,
                    IsDeafultGroupAll = IsDeafultGroupAll,
                    IsAllowLTF        = IsAllowLTF,
                    ListParam         = new ListParam[5],
                    NumParam          = new NumericParam[6],
                    CheckParam        = new CheckParam[2]
                };

            for (int i = 0; i < 5; i++)
                indicatorParam.ListParam[i] = ListParam[i].Clone();

            for (int i = 0; i < 6; i++)
                indicatorParam.NumParam[i] = NumParam[i].Clone();

            for (int i = 0; i < 2; i++)
                indicatorParam.CheckParam[i] = CheckParam[i].Clone();

            return indicatorParam;
        }

        /// <summary>
        ///     Represents the indicator parameters in a readable form.
        /// </summary>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            foreach (ListParam listParam in ListParam)
                if (listParam.Enabled)
                    stringBuilder.AppendLine($"{listParam.Caption}: {listParam.Text}");

            foreach (NumericParam numParam in NumParam)
                if (numParam.Enabled)
                    stringBuilder.AppendLine($"{numParam.Caption}: {numParam.ValueToString}");

            foreach (CheckParam checkParam in CheckParam)
                if (checkParam.Enabled)
                    stringBuilder.AppendLine($"{checkParam.Caption}: {(checkParam.Checked ? "Yes" : "No")}");

            return stringBuilder.ToString();
        }

        public string ToString(ITranslationManager tm)
        {
            var stringBuilder = new StringBuilder();

            foreach (ListParam listParam in ListParam)
                if (listParam.Enabled)
                    stringBuilder.AppendLine(
                        $"{tm.TranslateIndicatorParam(listParam.Caption, IndicatorName)}: {tm.TranslateIndicatorParam(listParam.Text, IndicatorName)}");

            foreach (NumericParam numParam in NumParam)
                if (numParam.Enabled)
                    stringBuilder.AppendLine(
                        $"{tm.TranslateIndicatorParam(numParam.Caption, IndicatorName)}: {numParam.ValueToString}");

            foreach (CheckParam checkParam in CheckParam)
                if (checkParam.Enabled)
                    stringBuilder.AppendLine(
                        $"{tm.TranslateIndicatorParam(checkParam.Caption, IndicatorName)}: {(checkParam.Checked ? tm.T("Statistics", "Yes") : tm.T("Statistics", "No"))}");

            return stringBuilder.ToString();
        }
    }
}