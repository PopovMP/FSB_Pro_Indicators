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

namespace ForexStrategyBuilder.Services
{
    public class IndicatorTester
    {
        public void CalculateIndicatorWithRandomParameters(IIndicator indicator, IDataSet dataSet, int testPerSlot)
        {
            var random    = new Random(DateTime.Now.Second);
            var slotTypes = (SlotTypes[]) Enum.GetValues(typeof(SlotTypes));
            foreach (var type in slotTypes)
            {
                if (!indicator.TestPossibleSlot(type)) continue;

                for (var i = 0; i < testPerSlot; i++)
                {
                    TestIndicator(indicator, type, dataSet, random);
                    ShowResult(indicator);
                }
            }
        }

        private void TestIndicator(IIndicator indicator, SlotTypes slotType, IDataSet dataSet, Random random)
        {
            indicator.Initialize(slotType);

            // List parameters
            foreach (ListParam list in indicator.IndParam.ListParam)
            {
                if (!list.Enabled) continue;

                list.Index = random.Next(list.ItemList.Length);
                list.Text  = list.ItemList[list.Index];
            }

            // Numeric parameters
            foreach (NumericParam num in indicator.IndParam.NumParam)
            {
                if (!num.Enabled) continue;

                double step    = Math.Pow(10, -num.Point);
                double minimum = num.Min;
                double maximum = num.Max;
                double value   = minimum + step * random.Next((int)((maximum - minimum) / step));

                num.Value = Math.Round(value, num.Point);
            }

            indicator.Calculate(dataSet);
        }

        private void ShowResult(IIndicator indicator)
        {
            Console.WriteLine(indicator.ToString());
            Console.WriteLine("Slot type: {0}",  indicator.IndParam.SlotType);
            Console.WriteLine("Logic rule: {0}", indicator.IndParam.ListParam[0].Text);

            foreach (IndicatorComp component in indicator.Component)
            {
                if (component == null) continue;
                IndComponentType type = component.DataType;
                int    bars  = component.Value.Length;
                string name  = component.CompName;
                double value = component.Value[bars - 1];

                double val    = Math.Abs(value);
                string format = val < 10 ? "F5" : val < 100 ? "F4" : val < 1000 ? "F3" : val < 10000 ? "F2" : val < 100000 ? "F1" : "F0";
                if (!component.ShowInDynInfo) continue;

                if (type == IndComponentType.AllowOpenLong || type == IndComponentType.AllowOpenShort ||
                    type == IndComponentType.ForceClose    || type == IndComponentType.ForceCloseLong ||
                    type == IndComponentType.ForceCloseShort)
                    Console.WriteLine("{0}: {1}   ", name,  value < 1 ? "No" : "Yes");
                else
                    Console.WriteLine("{0}: {1}", name, value.ToString(format));
            }

            Console.WriteLine();
        }
    }
}
