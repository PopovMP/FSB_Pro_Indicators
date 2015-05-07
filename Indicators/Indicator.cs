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
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators
{
    public partial class Indicator : IIndicator
    {
        private string indicatorName;
        private SlotTypes slotType;

        public Indicator()
        {
            IndParam = new IndicatorParam();

            IndicatorName = string.Empty;
            PossibleSlots = SlotTypes.NotDefined;
            SlotType = SlotTypes.NotDefined;

            SeparatedChart = false;
            SeparatedChartMinValue = double.MaxValue;
            SeparatedChartMaxValue = double.MinValue;
            SpecialValues = new double[0];

            IsDiscreteValues = false;
            CustomIndicator = false;
            IsBacktester = true;
            IsGeneratable = true;
            WarningMessage = string.Empty;
            AllowClosingFilters = false;

            IndicatorAuthor = "Forex Software Ltd";
            IndicatorVersion = "1.0";
            IndicatorDescription = "Bundled in FSB distribution.";

            ExitFilterShortDescription = "Not defined";
            EntryFilterShortDescription = "Not defined";
            ExitFilterLongDescription = "Not defined";
            EntryFilterLongDescription = "Not defined";
            ExitPointShortDescription = "Not defined";
            ExitPointLongDescription = "Not defined";
            EntryPointShortDescription = "Not defined";
            EntryPointLongDescription = "Not defined";

            Component = new IndicatorComp[] {};
        }

        /// <summary>
        ///     Gets or sets the possible slots
        /// </summary>
        protected SlotTypes PossibleSlots { private get; set; }

        /// <summary>
        ///     Shows if the indicator has discrete values.
        /// </summary>
        protected bool IsDiscreteValues { private get; set; }

        /// <summary>
        ///     Time frame of the loaded history data
        /// </summary>
        protected DataPeriod Period
        {
            get { return DataSet.Period; }
        }

        /// <summary>
        ///     The minimal price change.
        /// </summary>
        protected double Point
        {
            get { return DataSet.Properties.Point; }
        }

        /// <summary>
        ///     Number of digits after the decimal point of the history data.
        /// </summary>
        protected int Digits
        {
            get { return DataSet.Properties.Digits; }
        }

        /// <summary>
        ///     Number of loaded bars
        /// </summary>
        protected int Bars
        {
            get { return DataSet.Bars; }
        }

        /// <summary>
        ///     Bar opening date and time
        /// </summary>
        protected DateTime[] Time
        {
            get { return DataSet.Time; }
        }

        /// <summary>
        ///     Bar opening price
        /// </summary>
        protected double[] Open
        {
            get { return DataSet.Open; }
        }

        /// <summary>
        ///     Bar highest price
        /// </summary>
        protected double[] High
        {
            get { return DataSet.High; }
        }

        /// <summary>
        ///     Bar lowest price
        /// </summary>
        protected double[] Low
        {
            get { return DataSet.Low; }
        }

        /// <summary>
        ///     Bar closing price
        /// </summary>
        protected double[] Close
        {
            get { return DataSet.Close; }
        }

        /// <summary>
        ///     Bar volume
        /// </summary>
        protected int[] Volume
        {
            get { return DataSet.Volume; }
        }

        /// <summary>
        ///     Current server time.
        /// </summary>
        protected DateTime ServerTime { get { return DataSet.ServerTime; } }

        /// <summary>
        ///     Current data set;
        /// </summary>
        public IDataSet DataSet { get; set; }

        /// <summary>
        ///     Gets or sets the indicator name.
        /// </summary>
        public string IndicatorName
        {
            get { return indicatorName; }
            protected set
            {
                indicatorName = value;
                IndParam.IndicatorName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the indicator current parameters.
        /// </summary>
        public IIndicatorParam IndParam { get; set; }

        /// <summary>
        ///     Type of the slot for the current instance.
        /// </summary>
        public SlotTypes SlotType
        {
            get { return slotType; }
            protected set
            {
                slotType = value;
                IndParam.SlotType = value;
            }
        }

        /// <summary>
        ///     Gets if the default group is "All"
        /// </summary>
        public bool IsDeafultGroupAll
        {
            get { return IndParam.IsDeafultGroupAll; }
            set { IndParam.IsDeafultGroupAll = value; }
        }

        /// <summary>
        ///     If the chart is drown in separated panel.
        /// </summary>
        public bool SeparatedChart { get; protected set; }

        /// <summary>
        ///     Gets the indicator components.
        /// </summary>
        public IndicatorComp[] Component { get; protected set; }

        /// <summary>
        ///     Gets the indicator's special values.
        /// </summary>
        public double[] SpecialValues { get; protected set; }

        /// <summary>
        ///     Gets the indicator's min value.
        /// </summary>
        public double SeparatedChartMinValue { get; protected set; }

        /// <summary>
        ///     Gets the indicator's max value.
        /// </summary>
        public double SeparatedChartMaxValue { get; protected set; }

        /// <summary>
        ///     Shows if the indicator is custom.
        /// </summary>
        public bool CustomIndicator { get; set; }

        /// <summary>
        ///     Gets or sets a warning message about the indicator
        /// </summary>
        public string WarningMessage { get; protected set; }

        /// <summary>
        ///     Shows if a closing point indicator can be used with closing logic conditions.
        /// </summary>
        public bool AllowClosingFilters { get; protected set; }

        /// <summary>
        ///     Gets the indicator Entry Point Long Description
        /// </summary>
        public string EntryPointLongDescription { get; protected set; }

        /// <summary>
        ///     Gets the indicator Entry Point Short Description
        /// </summary>
        public string EntryPointShortDescription { get; protected set; }

        /// <summary>
        ///     Gets the indicator Exit Point Long Description
        /// </summary>
        public string ExitPointLongDescription { get; protected set; }

        /// <summary>
        ///     Gets the indicator Exit Point Short Description
        /// </summary>
        public string ExitPointShortDescription { get; protected set; }

        /// <summary>
        ///     Gets the indicator Entry Filter Description
        /// </summary>
        public string EntryFilterLongDescription { get; protected set; }

        /// <summary>
        ///     Gets the indicator Exit Filter Description
        /// </summary>
        public string ExitFilterLongDescription { get; protected set; }

        /// <summary>
        ///     Gets the indicator Entry Filter Description
        /// </summary>
        public string EntryFilterShortDescription { get; protected set; }

        /// <summary>
        ///     Gets the indicator Exit Filter Description
        /// </summary>
        public string ExitFilterShortDescription { get; protected set; }

        /// <summary>
        ///     Replaces main indicator with the same name.
        /// </summary>
        public bool OverrideMainIndicator { get; set; }

        /// <summary>
        ///     Shows if the indicator is loaded from a dll.
        /// </summary>
        public bool LoaddedFromDll { get; set; }

        /// <summary>
        ///     Gets the version text of the indicator
        /// </summary>
        public string IndicatorVersion { get; set; }

        /// <summary>
        ///     Gets the author's name of the indicator
        /// </summary>
        public string IndicatorAuthor { get; set; }

        /// <summary>
        ///     Gets the description text of the indicator
        /// </summary>
        public string IndicatorDescription { get; set; }

        public bool IsGeneratable { get; set; }
        public bool IsBacktester { get; set; }

        /// <summary>
        ///     Tests if this is one of the possible slots.
        /// </summary>
        /// <param name="slot">The slot we test.</param>
        /// <returns>True if the slot is possible.</returns>
        public bool TestPossibleSlot(SlotTypes slot)
        {
            if ((slot & PossibleSlots) == SlotTypes.Open)
                return true;

            if ((slot & PossibleSlots) == SlotTypes.OpenFilter)
                return true;

            if ((slot & PossibleSlots) == SlotTypes.Close)
                return true;

            if ((slot & PossibleSlots) == SlotTypes.CloseFilter)
                return true;

            return false;
        }

        /// <summary>
        ///     Gets or sets UsePreviousBarValue parameter.
        /// </summary>
        public bool UsePreviousBarValue
        {
            get
            {
                foreach (CheckParam checkParam in IndParam.CheckParam)
                    if (checkParam.Caption == "Use previous bar value")
                        return checkParam.Checked;
                return false;
            }
            set
            {
                foreach (CheckParam checkParam in IndParam.CheckParam)
                    if (checkParam.Caption == "Use previous bar value")
                        checkParam.Checked = value;
            }
        }

        /// <summary>
        ///     Initializes parameters.
        /// </summary>
        public virtual void Initialize(SlotTypes slot)
        {
        }

        /// <summary>
        ///     Calculates the components.
        /// </summary>
        public virtual void Calculate(IDataSet dataSet)
        {
        }

        /// <summary>
        ///     Sets the indicator logic description.
        /// </summary>
        public virtual void SetDescription()
        {
        }

        public override string ToString()
        {
            var name = IndicatorName + (IndParam.CheckParam[0].Checked ? "* " : " ");
            var parameters = "(";
            for (int i = 1; i < 5; i++)
                if (IndParam.ListParam[i].Enabled)
                    parameters += IndParam.ListParam[i].Text + ", ";
            for (int i = 0; i < 6; i++)
                if (IndParam.NumParam[i].Enabled)
                    parameters += IndParam.NumParam[i].Value + ", ";
            return name + parameters.Substring(0, parameters.Length - 2) + ")";
        }
    }
}