//==============================================================
// Forex Strategy Builder
// Copyright © 2016 Forex Software Ltd. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class ChandeMomentumOscillator : Indicator
    {
        public ChandeMomentumOscillator()
        {
            IndicatorName  = "Chande Momentum Oscillator";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = -100;
            SeparatedChartMaxValue = 100;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "A custom indicator for FSB and FST.";
        }

        public override void Initialize(SlotTypes slotType)
        {
		    SlotType = slotType;

            // ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "CMO rises",
                "CMO falls",
                "CMO is higher than Level line",
                "CMO is lower than Level line",
                "CMO crosses Level line upward",
                "CMO crosses Level line downward",
                "CMO changes its direction upward",
                "CMO changes its direction downward"
            };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Close;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price CMO is based on.";

            // NumericUpDown parameters
            IndParam.NumParam[0].Caption   = "Smoothing period";
            IndParam.NumParam[0].Value     = 14;
            IndParam.NumParam[0].Min       = 1;
            IndParam.NumParam[0].Max       = 200;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "Period of smoothing of CMO value.";

            IndParam.NumParam[1].Caption   = "Level";
            IndParam.NumParam[1].Value     = 30;
            IndParam.NumParam[1].Min       = 0;
            IndParam.NumParam[1].Max       = 100;
            IndParam.NumParam[1].Enabled   = true;
            IndParam.NumParam[1].ToolTip   = "A critical level (for appropriate logic).";

            // CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use indicator value from previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;
			
            // Reading parameters
            BasePrice basePrice = (BasePrice)IndParam.ListParam[2].Index;
            int       period    = (int)IndParam.NumParam[0].Value;
            double    level     = IndParam.NumParam[1].Value;
            int       previous  = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int      firstBar    = period + previous + 2;
            double[] adBasePrice = Price(basePrice);
            double[] adCMO1      = new double[Bars];
            double[] adCMO2      = new double[Bars];
            double[] adCMO1Sum   = new double[Bars];
            double[] adCMO2Sum   = new double[Bars];
            double[] adCMO       = new double[Bars];

            for (int bar = 1; bar < Bars; bar++)
            {
				adCMO1[bar] = 0;
				adCMO2[bar] = 0;
                if (adBasePrice[bar] > adBasePrice[bar - 1])
					adCMO1[bar] = adBasePrice[bar] - adBasePrice[bar - 1];
                if (adBasePrice[bar] < adBasePrice[bar - 1])
					adCMO2[bar] = adBasePrice[bar - 1] - adBasePrice[bar];
            }

            for (int bar = 0; bar < period; bar++)
            {
                adCMO1Sum[period - 1] += adCMO1[bar];
                adCMO2Sum[period - 1] += adCMO2[bar];
            }

            for (int bar = period; bar < Bars; bar++)
            {
                adCMO1Sum[bar] = adCMO1Sum[bar - 1] + adCMO1[bar] - adCMO1[bar - period];
                adCMO2Sum[bar] = adCMO2Sum[bar - 1] + adCMO2[bar] - adCMO2[bar - period];

                if (adCMO1Sum[bar] + adCMO2Sum[bar] == 0)
                    adCMO[bar] = 100;
                else
                    adCMO[bar] = 100 * (adCMO1Sum[bar] - adCMO2Sum[bar]) / (adCMO1Sum[bar] + adCMO2Sum[bar]);
            }

            // Saving components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "CMO";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.RoyalBlue;
            Component[0].FirstBar   = firstBar;
            Component[0].Value      = adCMO;

            Component[1] = new IndicatorComp();
            Component[1].ChartType  = IndChartType.NoChart;
            Component[1].FirstBar   = firstBar;
            Component[1].Value      = new double[Bars];

            Component[2] = new IndicatorComp();
            Component[2].ChartType  = IndChartType.NoChart;
            Component[2].FirstBar   = firstBar;
            Component[2].Value      = new double[Bars];

            // Sets Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            // Calculation of logic
            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "CMO rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[1] { 0 };
                    break;

                case "CMO falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[1] { 0 };
                    break;

                case "CMO is higher than Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new double[2] { level, -level };
                    break;

                case "CMO is lower than Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new double[2] { level, -level };
                    break;

                case "CMO crosses Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new double[2] { level, -level };
                    break;

                case "CMO crosses Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new double[2] { level, -level };
                    break;

                case "CMO changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[1] { 0 };
                    break;

                case "CMO changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[1] { 0 };
                    break;
            }

            OscillatorLogic(firstBar, previous, adCMO, level, -level, ref Component[1], ref Component[2], indLogic);
        }

        public override void SetDescription()
        {
            string sLevelLong  = IndParam.NumParam[1].ValueToString;
            string sLevelShort = IndParam.NumParam[1].AnotherValueToString(-IndParam.NumParam[1].Value);

            EntryFilterLongDescription  = "" + ToString() + " ";
            EntryFilterShortDescription = "" + ToString() + " ";
            ExitFilterLongDescription   = "" + ToString() + " ";
            ExitFilterShortDescription  = "" + ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "CMO rises":
                    EntryFilterLongDescription  += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription   += "rises";
                    ExitFilterShortDescription  += "falls";
                    break;

                case "CMO falls":
                    EntryFilterLongDescription  += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription   += "falls";
                    ExitFilterShortDescription  += "rises";
                    break;

                case "CMO is higher than Level line":
                    EntryFilterLongDescription  += "is higher than Level " + sLevelLong;
                    EntryFilterShortDescription += "is lower than Level "  + sLevelShort;
                    ExitFilterLongDescription   += "is higher than Level " + sLevelLong;
                    ExitFilterShortDescription  += "is lower than Level "  + sLevelShort;
                    break;

                case "CMO is lower than Level line":
                    EntryFilterLongDescription  += "is lower than Level "  + sLevelLong;
                    EntryFilterShortDescription += "is higher than Level " + sLevelShort;
                    ExitFilterLongDescription   += "is lower than Level "  + sLevelLong;
                    ExitFilterShortDescription  += "is higher than Level " + sLevelShort;
                    break;

                case "CMO crosses Level line upward":
                    EntryFilterLongDescription  += "crosses Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "crosses Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "crosses Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "crosses Level " + sLevelShort + " downward";
                    break;

                case "CMO crosses Level line downward":
                    EntryFilterLongDescription  += "crosses Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "crosses Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "crosses Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "crosses Level " + sLevelShort + " upward";
                    break;

                case "CMO changes its direction upward":
                    EntryFilterLongDescription  += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription   += "changes its direction upward";
                    ExitFilterShortDescription  += "changes its direction downward";
                    break;

                case "CMO changes its direction downward":
                    EntryFilterLongDescription  += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription   += "changes its direction downward";
                    ExitFilterShortDescription  += "changes its direction upward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[2].Text         + ", " + // Base price
                IndParam.NumParam[0].ValueToString + ")";   // Smoothing period
        }
    }
}
