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
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public class InstrumentProperties : IInstrumentProperties
    {
        public InstrumentProperties(string symbol)
        {
            Symbol       = symbol;
            Digits       = symbol.Contains("JPY") ? 3 : 5;
            BaseFileName = symbol;
            IsFiveDigits = Digits == 3 || Digits == 5;
            Point        = 1/Math.Pow(10, Digits);
            Pip          = IsFiveDigits ? 10*Point : Point;
        }

        public string Symbol { get; set; }
        public string BaseFileName { get; set; }
        public int Digits { get; set; }
        public double Point { get; set; }
        public double Pip { get; set; }
        public bool IsFiveDigits { get; set; }

        /// <summary>
        ///     Returns a clone of the class.
        /// </summary>
        public InstrumentProperties GetClone()
        {
            return new InstrumentProperties(Symbol)
            {
                Symbol       = Symbol,
                Digits       = Digits,
                Point        = Point,
                Pip          = Pip,
                IsFiveDigits = IsFiveDigits,
                BaseFileName = BaseFileName,
            };
        }
    }
}