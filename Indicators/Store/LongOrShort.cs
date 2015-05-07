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
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class LongOrShort : Indicator
    {
        public LongOrShort()
        {
            IndicatorName = "Long or Short";
            PossibleSlots = SlotTypes.OpenFilter;
            IsDeafultGroupAll = true;
            IsGeneratable = false;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.Additional;
            IndParam.IsAllowLTF = false;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Open long positions only",
                    "Open short positions only"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
                {
                    CompName = "Is long entry allowed",
                    DataType = IndComponentType.AllowOpenLong,
                    ChartType = IndChartType.NoChart,
                    FirstBar = 0,
                    Value = new double[Bars]
                };

            Component[1] = new IndicatorComp
                {
                    CompName = "Is short entry allowed",
                    DataType = IndComponentType.AllowOpenShort,
                    ChartType = IndChartType.NoChart,
                    FirstBar = 0,
                    Value = new double[Bars]
                };

            // Calculation of the logic
            switch (IndParam.ListParam[0].Text)
            {
                case "Open long positions only":
                    for (int i = 0; i < Bars; i++)
                    {
                        Component[0].Value[i] = 1;
                        Component[1].Value[i] = 0;
                    }
                    break;

                case "Open short positions only":
                    for (int i = 0; i < Bars; i++)
                    {
                        Component[0].Value[i] = 0;
                        Component[1].Value[i] = 1;
                    }
                    break;
            }
        }

        public override void SetDescription()
        {
            // Calculation of the logic
            switch (IndParam.ListParam[0].Text)
            {
                case "Open long positions only":
                    EntryFilterLongDescription = "Long or Short filter permits long opening";
                    EntryFilterShortDescription = "Long or Short filter does not permit short opening";
                    break;

                case "Open short positions only":
                    EntryFilterLongDescription = "Long or Short filter does not permit long opening";
                    EntryFilterShortDescription = "Long or Short filter permits short opening";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}