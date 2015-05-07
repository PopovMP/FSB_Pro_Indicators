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

namespace ForexStrategyBuilder.Indicators.Store
{
    public class RandomFilter : Indicator
    {
        public RandomFilter()
        {
            IndicatorName = "Random Filter";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            IsDeafultGroupAll = true;
            IsGeneratable = false;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Gives a random entry signal"
                    };
            else if (SlotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Gives a random exit signal"
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

            // The NumericUpDown parameters
            if (SlotType == SlotTypes.OpenFilter)
            {
                IndParam.NumParam[0].Caption = "Probability";
                IndParam.NumParam[0].Value = 80;
                IndParam.NumParam[0].Min = 0;
                IndParam.NumParam[0].Max = 100;
                IndParam.NumParam[0].Enabled = true;
                IndParam.NumParam[0].ToolTip = "The probability to allow a new position opening in %.";

                IndParam.NumParam[1].Caption = "Long vs short";
                IndParam.NumParam[1].Value = 50;
                IndParam.NumParam[1].Min = 0;
                IndParam.NumParam[1].Max = 100;
                IndParam.NumParam[1].Enabled = true;
                IndParam.NumParam[1].ToolTip = "The probability to open Long vs. short in %.";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                IndParam.NumParam[0].Caption = "Probability";
                IndParam.NumParam[0].Value = 20;
                IndParam.NumParam[0].Min = 0;
                IndParam.NumParam[0].Max = 100;
                IndParam.NumParam[0].Enabled = true;
                IndParam.NumParam[0].ToolTip = "The probability to close the position in %.";
            }
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var iProbability = (int) IndParam.NumParam[0].Value;
            var iLongShort = (int) IndParam.NumParam[1].Value;

            var random = new Random();

            // Saving the components
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component = new IndicatorComp[2];

                Component[0] = new IndicatorComp
                    {
                        ChartType = IndChartType.NoChart,
                        FirstBar = 0,
                        Value = new double[Bars],
                        DataType = IndComponentType.AllowOpenLong,
                        CompName = "Is long entry allowed"
                    };

                Component[1] = new IndicatorComp
                    {
                        ChartType = IndChartType.NoChart,
                        FirstBar = 0,
                        Value = new double[Bars],
                        DataType = IndComponentType.AllowOpenShort,
                        CompName = "Is short entry allowed"
                    };

                // Calculation of the logic
                for (int i = 0; i < Bars; i++)
                {
                    if (random.Next(100) < iProbability)
                    {
                        int iRandNumb = random.Next(100);
                        Component[0].Value[i] = (iRandNumb <= iLongShort) ? 1 : 0;
                        Component[1].Value[i] = (iRandNumb > iLongShort) ? 1 : 0;
                    }
                    else
                    {
                        Component[0].Value[i] = 0;
                        Component[1].Value[i] = 0;
                    }
                }
            }
            else
            {
                Component = new IndicatorComp[1];

                Component[0] = new IndicatorComp
                    {
                        ChartType = IndChartType.NoChart,
                        FirstBar = 0,
                        Value = new double[Bars],
                        DataType = IndComponentType.ForceClose,
                        CompName = "Force Close"
                    };

                for (int i = 0; i < Bars; i++)
                {
                    Component[0].Value[i] = (random.Next(100) < iProbability) ? 1 : 0;
                }
            }
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + " allows a long position";
            EntryFilterShortDescription = ToString() + " allows a short position";
            ExitFilterLongDescription = ToString() + " allows closing";
            ExitFilterShortDescription = ToString() + " allows closing";
        }

        public override string ToString()
        {
            return IndicatorName + " (" +
                   IndParam.NumParam[0].ValueToString + ", " + // Probability
                   IndParam.NumParam[1].ValueToString + ")"; // Long vs Short
        }
    }
}