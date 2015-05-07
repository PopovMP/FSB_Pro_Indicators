//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System.Drawing;
using ForexStrategyBuilder.Infrastructure.Enums;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public class IndicatorComp
    {
        public IndicatorComp()
        {
            CompName = "Not defined";
            DataType = IndComponentType.NotDefined;
            ChartType = IndChartType.NoChart;
            ChartColor = Color.Red;
            FirstBar = 0;
            UsePreviousBar = 0;
            ShowInDynInfo = true;
            Value = new double[0];
            PosPriceDependence = PositionPriceDependence.None;
        }

        public string CompName { get; set; }
        public IndComponentType DataType { get; set; }
        public IndChartType ChartType { get; set; }
        public Color ChartColor { get; set; }
        public int FirstBar { get; set; }
        public int UsePreviousBar { get; set; }
        public bool ShowInDynInfo { get; set; }
        public PositionPriceDependence PosPriceDependence { get; set; }
        public double[] Value { get; set; }

        public IndicatorComp Clone()
        {
            var indicatorComp = new IndicatorComp
            {
                CompName = CompName,
                DataType = DataType,
                ChartType = ChartType,
                ChartColor = ChartColor,
                FirstBar = FirstBar,
                UsePreviousBar = UsePreviousBar,
                ShowInDynInfo = ShowInDynInfo,
                PosPriceDependence = PosPriceDependence
            };

            if (Value != null)
            {
                indicatorComp.Value = new double[Value.Length];
                Value.CopyTo(indicatorComp.Value, 0);
            }

            return indicatorComp;
        }
    }
}