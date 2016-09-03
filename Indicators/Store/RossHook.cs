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
using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    public class RossHook : Indicator
    {
        public RossHook()
        {
            IndicatorName = "Ross Hook";
            PossibleSlots = SlotTypes.Open | SlotTypes.Close;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.1";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (SlotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Enter long at Up Ross hook",
                    "Enter long at Down Ross hook"
                };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Exit long at Up Ross hook",
                    "Exit long at Down Ross hook"
                };
            else
                IndParam.ListParam[0].ItemList = new[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            var upHook = new double[Bars];
            var downHook = new double[Bars];

            for (int bar = 5; bar < Bars - 1; bar++)
            {
                if (High[bar] < High[bar - 1])
                {
                    if (High[bar - 3] < High[bar - 1] && High[bar - 2] < High[bar - 1])
                        upHook[bar + 1] = High[bar - 1];
                }

                if (Low[bar] > Low[bar - 1])
                {
                    if (Low[bar - 3] > Low[bar - 1] && Low[bar - 2] > Low[bar - 1])
                        downHook[bar + 1] = Low[bar - 1];
                }
            }

            // Is visible
            for (int bar = 5; bar < Bars; bar++)
            {
                if (upHook[bar - 1] > 0 && Math.Abs(upHook[bar] - 0) < Epsilon && High[bar - 1] < upHook[bar - 1])
                    upHook[bar] = upHook[bar - 1];
                if (downHook[bar - 1] > 0 && Math.Abs(downHook[bar] - 0) < Epsilon && Low[bar - 1] > downHook[bar - 1])
                    downHook[bar] = downHook[bar - 1];
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp
            {
                ChartType = IndChartType.Level,
                ChartColor = Color.SpringGreen,
                FirstBar = 5,
                Value = upHook
            };

            Component[1] = new IndicatorComp
            {
                ChartType = IndChartType.Level,
                ChartColor = Color.DarkRed,
                FirstBar = 5,
                Value = downHook
            };

            // Sets the Component's type
            if (SlotType == SlotTypes.Open)
            {
                if (IndParam.ListParam[0].Text == "Enter long at Up Ross hook")
                {
                    Component[0].DataType = IndComponentType.OpenLongPrice;
                    Component[1].DataType = IndComponentType.OpenShortPrice;
                }
                else
                {
                    Component[0].DataType = IndComponentType.OpenShortPrice;
                    Component[1].DataType = IndComponentType.OpenLongPrice;
                }
                Component[0].CompName = "Up Ross hook";
                Component[1].CompName = "Down Ross hook";
            }
            else if (SlotType == SlotTypes.Close)
            {
                if (IndParam.ListParam[0].Text == "Exit long at Up Ross hook")
                {
                    Component[0].DataType = IndComponentType.CloseLongPrice;
                    Component[1].DataType = IndComponentType.CloseShortPrice;
                }
                else
                {
                    Component[0].DataType = IndComponentType.CloseShortPrice;
                    Component[1].DataType = IndComponentType.CloseLongPrice;
                }
                Component[0].CompName = "Up Ross hook";
                Component[1].CompName = "Down Ross hook";
            }
        }

        public override void SetDescription()
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at Up Ross hook":
                    EntryPointLongDescription = "at the peak of a Up Ross hook";
                    EntryPointShortDescription = "at the bottom of a Down Ross hook";
                    break;

                case "Enter long at Down Ross hook":
                    EntryPointLongDescription = "at the bottom of a Down Ross hook";
                    EntryPointShortDescription = "at the peak of a Up Ross hook";
                    break;

                case "Exit long at Up Ross hook":
                    ExitPointLongDescription = "at the peak of a Up Ross hook";
                    ExitPointShortDescription = "at the bottom of a Down Ross hook";
                    break;

                case "Exit long at Down Ross hook":
                    ExitPointLongDescription = "at the bottom of a Down Ross hook";
                    ExitPointShortDescription = "at the peak of a Up Ross hook";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName;
        }
    }
}