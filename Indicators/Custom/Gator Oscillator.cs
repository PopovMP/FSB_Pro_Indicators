//==============================================================
// Forex Strategy Builder
// Copyright © 2016 Forex Software Ltd. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class Gator_Oscillator : Indicator
    {
        public Gator_Oscillator()
        {
            IndicatorName   = "Gator Oscillator";
            PossibleSlots   = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart  = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "A custom indicator for FSB and FST.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "The Gator Oscillator expands",
                "The Gator Oscillator contracts"
            };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Smoothed;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The method of Moving Average used for the calculations.";

            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Median;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the indicator is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Jaws period";
            IndParam.NumParam[0].Value   = 13;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The Moving Average period.";

            IndParam.NumParam[1].Caption = "Jaws shift";
            IndParam.NumParam[1].Value   = 8;
            IndParam.NumParam[1].Min     = 0;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "How many bars to shift with.";

            IndParam.NumParam[2].Caption = "Teeth period";
            IndParam.NumParam[2].Value   = 8;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The Moving Average period.";

            IndParam.NumParam[3].Caption = "Teeth shift";
            IndParam.NumParam[3].Value   = 5;
            IndParam.NumParam[3].Min     = 0;
            IndParam.NumParam[3].Max     = 200;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "How many bars to shift with.";

            IndParam.NumParam[4].Caption = "Lips period";
            IndParam.NumParam[4].Value   = 5;
            IndParam.NumParam[4].Min     = 1;
            IndParam.NumParam[4].Max     = 200;
            IndParam.NumParam[4].Enabled = true;
            IndParam.NumParam[4].ToolTip = "The Moving Average period.";

            IndParam.NumParam[5].Caption = "Lips shift";
            IndParam.NumParam[5].Value   = 3;
            IndParam.NumParam[5].Min     = 0;
            IndParam.NumParam[5].Max     = 200;
            IndParam.NumParam[5].Enabled = true;
            IndParam.NumParam[5].ToolTip = "How many bars to shift with.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            MAMethod  maMethod  = (MAMethod )IndParam.ListParam[1].Index;
            BasePrice basePrice = (BasePrice)IndParam.ListParam[2].Index;
            int iNJaws  = (int)IndParam.NumParam[0].Value;
            int iSJaws  = (int)IndParam.NumParam[1].Value;
            int iNTeeth = (int)IndParam.NumParam[2].Value;
            int iSTeeth = (int)IndParam.NumParam[3].Value;
            int iNLips  = (int)IndParam.NumParam[4].Value;
            int iSLips  = (int)IndParam.NumParam[5].Value;
            int previous   = IndParam.CheckParam[0].Checked ? 1 : 0;

            int firstBar = Math.Max(iNJaws + iSJaws, iNTeeth + iSTeeth);
            firstBar = Math.Max(firstBar, iNLips + iSLips);
            firstBar += previous + 2;

            // Calculation
            double[] adJaws  = MovingAverage(iNJaws , iSJaws , maMethod, Price(basePrice));
            double[] adTeeth = MovingAverage(iNTeeth, iSTeeth, maMethod, Price(basePrice));
            double[] adLips  = MovingAverage(iNLips , iSLips , maMethod, Price(basePrice));

            double[] adUpperGator = new double[Bars];
            double[] adLowerGator = new double[Bars];

            for (int bar = 0; bar < Bars; bar++)
            {
                adUpperGator[bar] =  Math.Abs(adJaws[bar]  - adTeeth[bar]);
                adLowerGator[bar] = -Math.Abs(adTeeth[bar] - adLips[bar]);
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Upper Gator";
            Component[0].DataType  = IndComponentType.IndicatorValue;
            Component[0].ChartType = IndChartType.Histogram;
            Component[0].FirstBar  = firstBar;
            Component[0].Value     = adUpperGator;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Lower Gator";
            Component[1].DataType  = IndComponentType.IndicatorValue;
            Component[1].ChartType = IndChartType.Histogram;
            Component[1].FirstBar  = firstBar;
            Component[1].Value     = adLowerGator;

            Component[2] = new IndicatorComp();
            Component[2].ChartType = IndChartType.NoChart;
            Component[2].FirstBar  = firstBar;
            Component[2].Value     = new double[Bars];

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = firstBar;
            Component[3].Value     = new double[Bars];

            // Sets the Component's type.
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "The Gator Oscillator expands":
					for (int bar = firstBar; bar < Bars; bar++)
					{
						Component[2].Value[bar] = (adUpperGator[bar - previous] - adLowerGator[bar - previous]) > (adUpperGator[bar - previous - 1] - adLowerGator[bar - previous - 1]) + Sigma() ? 1 : 0;
						Component[3].Value[bar] = (adUpperGator[bar - previous] - adLowerGator[bar - previous]) > (adUpperGator[bar - previous - 1] - adLowerGator[bar - previous - 1]) + Sigma() ? 1 : 0;
					}
                    break;

                case "The Gator Oscillator contracts":
					for (int bar = firstBar; bar < Bars; bar++)
					{
						Component[2].Value[bar] = (adUpperGator[bar - previous] - adLowerGator[bar - previous]) < (adUpperGator[bar - previous - 1] - adLowerGator[bar - previous - 1]) - Sigma() ? 1 : 0;
						Component[3].Value[bar] = (adUpperGator[bar - previous] - adLowerGator[bar - previous]) < (adUpperGator[bar - previous - 1] - adLowerGator[bar - previous - 1]) - Sigma() ? 1 : 0;
					}
                    break;
            }
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription  = "the " + ToString() + " ";
            EntryFilterShortDescription = "the " + ToString() + " ";
            ExitFilterLongDescription   = "the " + ToString() + " ";
            ExitFilterShortDescription  = "the " + ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "The Gator Oscillator expands":
                    EntryFilterLongDescription  += "expands";
                    EntryFilterShortDescription += "expands";
                    ExitFilterLongDescription   += "expands";
                    ExitFilterShortDescription  += "expands";
                    break;

                case "The Gator Oscillator contracts":
                    EntryFilterLongDescription  += "contracts";
                    EntryFilterShortDescription += "contracts";
                    ExitFilterLongDescription   += "contracts";
                    ExitFilterShortDescription  += "contracts";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[1].Text         + ", "+ // Method
                IndParam.ListParam[2].Text         + ", "+ // Price
                IndParam.NumParam[0].ValueToString + ", "+ // Jaws period
                IndParam.NumParam[1].ValueToString + ", "+ // Jaws shift
                IndParam.NumParam[2].ValueToString + ", "+ // Teeth period
                IndParam.NumParam[3].ValueToString + ", "+ // Teeth shift
                IndParam.NumParam[4].ValueToString + ", "+ // Lips period
                IndParam.NumParam[5].ValueToString + ")";  // Lips shift
        }
    }
}
