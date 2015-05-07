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
using System.Collections.Generic;
using System.Globalization;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public class InstrumentProperties : IInstrumentProperties
    {
        public InstrumentProperties()
        {
        }

        public InstrumentProperties(string symbol, InstrumentType instrType)
        {
            if (instrType == InstrumentType.Forex)
                SetDefaultForexParams(symbol);
            else
                SetDefaultIndexParams(symbol, instrType);
            SetPrecision();
        }

        public string Symbol { get; set; }
        public InstrumentType InstrType { get; set; }
        public string Comment { get; set; }
        public string PriceIn { get; set; }
        public string BaseFileName { get; set; }
        public int LotSize { get; set; }
        public int Slippage { get; set; }
        public double Spread { get; set; }
        public double SwapLong { get; set; }
        public double SwapShort { get; set; }
        public double Commission { get; set; }
        public double RateToUSD { get; set; }
        public double RateToEUR { get; set; }
        public double RateToGBP { get; set; }
        public double RateToJPY { get; set; }
        public CommissionType SwapType { get; set; }
        public CommissionType CommissionType { get; set; }
        public CommissionScope CommissionScope { get; set; }
        public CommissionTime CommissionTime { get; set; }
        public int Digits { get; set; }
        public double Point { get; set; }
        public double Pip { get; set; }
        public bool IsFiveDigits { get; set; }
        public double StopLevel { get; set; }
        public double TickValue { get; set; }
        public double MinLot { get; set; }
        public double MaxLot { get; set; }
        public double LotStep { get; set; }
        public double MarginRequired { get; set; }

        /// <summary>
        ///     Sets IsFiveDigits, Points and Pip values.
        ///     Must be call after Digits are set.
        /// </summary>
        public void SetPrecision()
        {
            IsFiveDigits = (Digits == 3 || Digits == 5);
            Point = 1/Math.Pow(10, Digits);
            Pip = IsFiveDigits ? 10*Point : Point;
        }

        /// <summary>
        ///     Returns a clone of the class.
        /// </summary>
        public InstrumentProperties GetClone()
        {
            return new InstrumentProperties(Symbol, InstrType)
            {
                Symbol = Symbol,
                InstrType = InstrType,
                Comment = Comment,
                Digits = Digits,
                Point = Point,
                Pip = Pip,
                IsFiveDigits = IsFiveDigits,
                LotSize = LotSize,
                Spread = Spread,
                SwapType = SwapType,
                SwapLong = SwapLong,
                SwapShort = SwapShort,
                CommissionType = CommissionType,
                CommissionScope = CommissionScope,
                CommissionTime = CommissionTime,
                Commission = Commission,
                PriceIn = PriceIn,
                Slippage = Slippage,
                RateToEUR = RateToEUR,
                RateToUSD = RateToUSD,
                RateToGBP = RateToGBP,
                RateToJPY = RateToJPY,
                BaseFileName = BaseFileName,
                StopLevel = StopLevel,
                TickValue = TickValue,
                MinLot = MinLot,
                MaxLot = MaxLot,
                LotStep = LotStep,
                MarginRequired = MarginRequired
            };
        }

        public InfoRecord[] GetInfoRecords(ITranslationManager tm)
        {
            const string tg = "Statistics";
            var records = new List<InfoRecord>
            {
                new InfoRecord
                {
                    Name = tm.T(tg, "Type"),
                    Value = InstrType.ToString()
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Comment"),
                    Value = Comment
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Digits"),
                    Value = Digits.ToString(CultureInfo.InvariantCulture)
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Point value"),
                    Value = Point.ToString("0.#####")
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Lot size"),
                    Value = LotSize.ToString(CultureInfo.InvariantCulture)
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Spread"),
                    Value = Spread.ToString("F2") + " " + tm.T(tg, "points")
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Swap long"),
                    Value = SwapLong.ToString("F2") + " " + tm.T(tg, SwapType.ToString().ToLower())
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Swap short"),
                    Value = SwapShort.ToString("F2") + " " + tm.T(tg, SwapType.ToString().ToLower())
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Commission"),
                    Value = Commission.ToString("F2") + " " + tm.T(tg, CommissionType.ToString().ToLower())
                },
                new InfoRecord
                {
                    Name = tm.T(tg, "Slippage"),
                    Value = Slippage.ToString("F2") + " " + tm.T(tg, "points")
                }
            };

            return records.ToArray();
        }

        /// <summary>
        ///     Sets the default parameters for an index instrument.
        /// </summary>
        /// <param name="symbol">The instrument symbol.</param>
        /// <param name="instrType">The instrument type: Index or CFD</param>
        private void SetDefaultIndexParams(string symbol, InstrumentType instrType)
        {
            Symbol = symbol;
            InstrType = instrType;
            Comment = symbol + " " + instrType;
            Digits = 2;
            LotSize = 100;
            Spread = 4;
            SwapType = CommissionType.Percents;
            SwapLong = -5.0;
            SwapShort = -1.0;
            CommissionType = CommissionType.Percents;
            CommissionScope = CommissionScope.Deal;
            CommissionTime = CommissionTime.OpenClose;
            Commission = 0.25;
            Slippage = 0;
            PriceIn = "USD";
            RateToUSD = 1;
            RateToEUR = 1;
            RateToGBP = 1;
            RateToJPY = 0.01;
            BaseFileName = symbol;
            LotSize = 10000;
            StopLevel = 5;
            TickValue = LotSize*Point;
            MinLot = 0.01;
            MaxLot = 100;
            LotStep = 0.01;
            MarginRequired = 1000;
        }

        /// <summary>
        ///     Sets the default parameters for an forex type instrument.
        /// </summary>
        /// <param name="symbol">The instrument symbol.</param>
        private void SetDefaultForexParams(string symbol)
        {
            Symbol = symbol;
            InstrType = InstrumentType.Forex;
            Comment = symbol.Substring(0, 3) + " vs " + symbol.Substring(3, 3);
            Digits = (symbol.Contains("JPY") ? 3 : 5);
            LotSize = 100000;
            Spread = 20;
            SwapType = CommissionType.Points;
            SwapLong = -2.0;
            SwapShort = -2.0;
            CommissionType = CommissionType.Points;
            CommissionScope = CommissionScope.Lot;
            CommissionTime = CommissionTime.OpenClose;
            Commission = 0;
            Slippage = 0;
            PriceIn = symbol.Substring(3, 3);
            RateToUSD = (symbol.Contains("JPY") ? 100 : 1);
            RateToEUR = (symbol.Contains("JPY") ? 100 : 1);
            RateToGBP = (symbol.Contains("JPY") ? 100 : 1);
            RateToJPY = (symbol.Contains("JPY") ? 1 : 0.01);
            BaseFileName = symbol;
            LotSize = 10000;
            StopLevel = 5;
            TickValue = LotSize*Point;
            MinLot = 0.01;
            MaxLot = 100;
            LotStep = 0.01;
            MarginRequired = 1000;
        }
    }
}