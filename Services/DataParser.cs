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
using System.IO;
using System.Text.RegularExpressions;
using ForexStrategyBuilder.Infrastructure.Entities;

namespace ForexStrategyBuilder.Services
{
    public class DataParser
    {
        private const string SpacePattern  = @"[\t ;,]";
        private const string DatePattern   = @"\d{1,4}[\./-]\d{1,4}[\./-]\d{1,4}";
        private const string TimePattern   = @"\d{1,2}(:\d{1,2}){1,2}";
        private const string PricePattern  = @"\d+([\.,]\d+)?";
        private const string VolumePattern = @"\d{1,10}";

        private static bool isOptionalDataFile;

        /// <summary>
        ///     Gets a compiled general data file regex.
        /// </summary>
        private static Regex GeneralDataFileRegex
        {
            get
            {
                // Patten for Data Files that include a Time Field
                const string pattern1 = "^" + // Start of the string
                                        SpacePattern  + "*" + // Zero or more white spaces
                                        DatePattern   + // Valid date pattern
                                        SpacePattern  + "+" + // One or more spaces
                                        TimePattern   + // Valid time pattern
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        VolumePattern + // Optional volume
                                        SpacePattern  + "*"; // Zero or more white spaces

                // A data line has to start with date and has an optional time string
                var regex = new Regex(pattern1, RegexOptions.Compiled);

                return regex;
            }
        }

        /// <summary>
        ///     Gets a compiled optional data file regex.
        /// </summary>
        private static Regex OptionalDataFileRegex
        {
            get
            {
                // Pattern for Data Files that do NOT include a Time Field
                const string pattern2 = "^" + // Start of the string
                                        SpacePattern  + "*" + // Zero or more white spaces
                                        DatePattern   + // Valid date pattern
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        PricePattern  + // Price
                                        SpacePattern  + "+" + // One or more spaces
                                        VolumePattern + // Optional volume
                                        SpacePattern  + "*"; // Zero or more white spaces

                // A data line has to start with date and has an optional time string
                var regex = new Regex(pattern2, RegexOptions.Compiled);

                return regex;
            }
        }

        #region IDataParser Members

        /// <summary>
        ///     Gets the the data array
        /// </summary>
        public List<Bar> Bar { get; private set; }

        /// <summary>
        ///     Parses the input data string.
        /// </summary>
        /// <param name="dataString">The input data string.</param>
        /// <param name="period">Data file period.</param>
        /// <returns>The number of parsed bars.</returns>
        public int Parse(string dataString, int period)
        {
            int bars = 0;

            try
            {
                Regex regexDataString = AnalyzeInput(dataString);
                Bar = ParseInput(dataString, regexDataString, period);
                bars = Bar.Count;
            }
            catch (Exception exception)
            {
                Console.WriteLine("DataParser.Parse: " + exception.Message);
            }

            return bars;
        }

        public string LoadingNote { get; private set; }

        #endregion

        /// <summary>
        ///     Analyzes the input data string.
        /// </summary>
        /// <param name="dataString">The input data string.</param>
        /// <returns>Matched regex for the data string.</returns>
        private Regex AnalyzeInput(string dataString)
        {
            string datePattern  = GetDateMatchPattern(dataString);
            string timePattern  = GetTimeMatchPattern(dataString);
            string pricePattern = PriceMatchPattern(dataString);

            if (!string.IsNullOrEmpty(timePattern))
                timePattern += @"[\t ;,]+";

            string dataMatchPattern = string.Format("^[\t ;,]*{0}[\\t ;,]+{1}{2}[\\t ;,]+(?<volume>\\d+)[\\t ;,]*$",
                datePattern, timePattern, pricePattern);

            return new Regex(dataMatchPattern, RegexOptions.Compiled);
        }

        /// <summary>
        ///     Gets the date regex pattern that matches the data file.
        /// </summary>
        /// <param name="dataString">The data file content.</param>
        /// <returns>Date regex pattern.</returns>
        private string GetDateMatchPattern(string dataString)
        {
            string line;
            int yearPos = 0;
            int monthPos = 0;
            int dayPos = 0;
            const string datePattern = @"(?<1>\d{1,4})[\./-](?<2>\d{1,4})[\./-](?<3>\d{1,4})";
            var regexDate = new Regex(datePattern, RegexOptions.Compiled);

            var stringReader = new StringReader(dataString);
            while ((line = stringReader.ReadLine()) != null)
            {
                Match matchDate = regexDate.Match(line);

                if (!matchDate.Success)
                    continue;

                int pos1 = int.Parse(matchDate.Result("$1"));
                int pos2 = int.Parse(matchDate.Result("$2"));
                int pos3 = int.Parse(matchDate.Result("$3"));

                // Determines the year index
                if (yearPos == 0)
                {
                    if (pos1 > 31)
                    {
                        yearPos = 1;
                        monthPos = 2;
                        dayPos = 3;
                        break;
                    }
                    if (pos3 > 31)
                    {
                        yearPos = 3;
                    }
                }

                // Determines the day index
                if (dayPos == 0 && yearPos > 0)
                {
                    if (yearPos == 1)
                    {
                        dayPos = 2;
                        monthPos = 3;
                        break;
                    }
                    if (yearPos == 3)
                    {
                        if (pos1 > 12)
                        {
                            dayPos = 1;
                            monthPos = 2;
                            break;
                        }
                        if (pos2 > 12)
                        {
                            monthPos = 1;
                            dayPos = 2;
                            break;
                        }
                    }
                }

                // Determines the month index
                if (dayPos > 0 && yearPos > 0)
                {
                    if (yearPos != 1 && dayPos != 1)
                        monthPos = 1;
                    else if (yearPos != 2 && dayPos != 2)
                        monthPos = 2;
                    else if (yearPos != 3 && dayPos != 3)
                        monthPos = 3;
                }

                if (yearPos > 0 && monthPos > 0 && dayPos > 0)
                    break;
            }
            stringReader.Close();

            // If the date format is not recognized we try to find the number of changes
            if (yearPos == 0 || monthPos == 0 || dayPos == 0)
            {
                int old1 = 0;
                int old2 = 0;
                int old3 = 0;

                int changes1 = -1;
                int changes2 = -1;
                int changes3 = -1;

                stringReader = new StringReader(dataString);
                while ((line = stringReader.ReadLine()) != null)
                {
                    Match matchDate = regexDate.Match(line);

                    if (!matchDate.Success)
                        continue;

                    int pos1 = int.Parse(matchDate.Result("$1"));
                    int pos2 = int.Parse(matchDate.Result("$2"));
                    int pos3 = int.Parse(matchDate.Result("$3"));

                    if (pos1 != old1)
                    {
                        // pos1 has changed
                        old1 = pos1;
                        changes1++;
                    }
                    if (pos2 != old2)
                    {
                        // pos2 has changed
                        old2 = pos2;
                        changes2++;
                    }
                    if (pos3 != old3)
                    {
                        // date2 has changed
                        old3 = pos3;
                        changes3++;
                    }


                    // Check number of changes
                    if (changes1 > changes2 && changes1 > changes2)
                    {
                        dayPos   = 1;
                        monthPos = 2;
                        yearPos  = 3;
                        break;
                    }
                    if (changes3 > changes1 && changes3 > changes2)
                    {
                        dayPos   = 3;
                        monthPos = 2;
                        yearPos  = 1;
                        break;
                    }
                    if (changes2 > changes1 && changes2 > changes3)
                    {
                        yearPos  = 3;
                        monthPos = 1;
                        dayPos   = 2;
                        break;
                    }
                }
                stringReader.Close();

                if (yearPos > 0)
                {
                    // The year position is known
                    if (yearPos == 1)
                    {
                        if (changes3 > changes2)
                        {
                            monthPos = 2;
                            dayPos   = 3;
                        }
                        else if (changes2 > changes3)
                        {
                            monthPos = 3;
                            dayPos   = 2;
                        }
                    }
                    else if (yearPos == 3)
                    {
                        if (changes2 > changes1)
                        {
                            monthPos = 1;
                            dayPos   = 2;
                        }
                        else if (changes1 > changes2)
                        {
                            monthPos = 2;
                            dayPos   = 1;
                        }
                    }
                }

                // If we don't know the year position but know that the day is somewhere in the end.
                // The year must be on the other end of the pattern because the year doesn't stay in the middle.
                if (yearPos == 0 && dayPos == 1)
                {
                    yearPos = 3;
                    monthPos = 2;
                }
                if (yearPos == 0 && dayPos == 3)
                {
                    yearPos = 1;
                    monthPos = 2;
                }

                if (yearPos == 0)
                {
                    // The year position is unknown
                    if (changes1 >= 0 && changes2 > changes1 && changes3 > changes2)
                    {
                        yearPos = 1;
                        monthPos = 2;
                        dayPos = 3;
                    }
                    else if (changes1 >= 0 && changes3 > changes1 && changes2 > changes3)
                    {
                        yearPos = 1;
                        monthPos = 3;
                        dayPos = 2;
                    }
                    else if (changes2 >= 0 && changes1 > changes2 && changes3 > changes1)
                    {
                        yearPos = 2;
                        monthPos = 1;
                        dayPos = 3;
                    }
                    else if (changes2 >= 0 && changes3 > changes2 && changes1 > changes3)
                    {
                        yearPos = 2;
                        monthPos = 3;
                        dayPos = 1;
                    }
                    else if (changes3 >= 0 && changes1 > changes3 && changes2 > changes1)
                    {
                        yearPos = 3;
                        monthPos = 1;
                        dayPos = 2;
                    }
                    else if (changes3 >= 0 && changes2 > changes3 && changes1 > changes2)
                    {
                        yearPos = 3;
                        monthPos = 2;
                        dayPos = 1;
                    }
                }
            }

            string dateMatchPattern = "";
            if (yearPos * monthPos * dayPos > 0)
            {
                if (yearPos == 1 && monthPos == 2 && dayPos == 3)
                    dateMatchPattern = @"(?<year>\d{1,4})[\./-](?<month>\d{1,4})[\./-](?<day>\d{1,4})";
                else if (yearPos == 3 && monthPos == 1 && dayPos == 2)
                    dateMatchPattern = @"(?<month>\d{1,4})[\./-](?<day>\d{1,4})[\./-](?<year>\d{1,4})";
                else if (yearPos == 3 && monthPos == 2 && dayPos == 1)
                    dateMatchPattern = @"(?<day>\d{1,4})[\./-](?<month>\d{1,4})[\./-](?<year>\d{1,4})";
            }
            else
            {
                throw new Exception("Could not determine the date format!");
            }

            return dateMatchPattern;
        }

        /// <summary>
        ///     Gets the time regex pattern than matches the data file.
        /// </summary>
        /// <param name="dataString">The data file content.</param>
        /// <returns>Time regex pattern.</returns>
        private string GetTimeMatchPattern(string dataString)
        {
            Regex regexGeneral = GeneralDataFileRegex;
            Regex regexOptional = OptionalDataFileRegex;
            string timeMatchPattern = null;
            string line;
            var stringReader = new StringReader(dataString);

            while ((line = stringReader.ReadLine()) != null)
            {
                if (regexGeneral.IsMatch(line))
                {
                    timeMatchPattern = @"(?<hour>\d{1,2}):(?<min>\d{1,2})(:(?<sec>\d{1,2}))?";
                    isOptionalDataFile = false;
                    break;
                }

                if (regexOptional.IsMatch(line))
                {
                    timeMatchPattern = "";
                    isOptionalDataFile = true;
                    break;
                }
            }

            stringReader.Close();

            if (timeMatchPattern == null)
                throw new Exception("Could not determine the time field format!");

            return timeMatchPattern;
        }

        /// <summary>
        ///     Determines the price pattern.
        /// </summary>
        /// <param name="dataString">The data file content.</param>
        /// <returns>Price match pattern.</returns>
        private string PriceMatchPattern(string dataString)
        {
            Regex regexGeneral = isOptionalDataFile ? OptionalDataFileRegex : GeneralDataFileRegex;
            const string columnSeparator = @"[\t ;,]+";
            string priceMatchPattern = "";
            string line;
            var stringReader = new StringReader(dataString);

            while ((line = stringReader.ReadLine()) != null)
            {
                if (!regexGeneral.IsMatch(line))
                    continue;

                Match matchPrice = Regex.Match(line,
                    string.Format(
                        "{0}(?<1>\\d+([\\.,]\\d+)?){0}(?<2>\\d+([\\.,]\\d+)?){0}(?<3>\\d+([\\.,]\\d+)?){0}(?<4>\\d+([\\.,]\\d+)?){0}",
                        columnSeparator));

                double price2 = ParseDouble(matchPrice.Result("$2"));
                double price3 = ParseDouble(matchPrice.Result("$3"));

                const double epsilon = 0.000001;
                if (price2 > price3 + epsilon)
                {
                    priceMatchPattern =
                        string.Format(
                            @"(?<open>\d+([\.,]\d+)?){0}(?<high>\d+([\.,]\d+)?){0}(?<low>\d+([\.,]\d+)?){0}(?<close>\d+([\.,]\d+)?)",
                            columnSeparator);
                    break;
                }
                if (price3 > price2 + epsilon)
                {
                    priceMatchPattern =
                        string.Format(
                            @"(?<open>\d+([\.,]\d+)?){0}(?<low>\d+([\.,]\d+)?){0}(?<high>\d+([\.,]\d+)?){0}(?<close>\d+([\.,]\d+)?)",
                            columnSeparator);
                    break;
                }
            }
            stringReader.Close();

            if (priceMatchPattern == "")
                throw new Exception("Could not determine the price columns order!");

            return priceMatchPattern;
        }

        /// <summary>
        ///     Parses the input data file.
        /// </summary>
        /// <param name="dataFile">The data file as string.</param>
        /// <param name="regexDataFile">The compiled regex.</param>
        /// <param name="period">The period of data file.</param>
        /// <returns>Returns a parsed bar array.</returns>
        private List<Bar> ParseInput(string dataFile, Regex regexDataFile, int period)
        {
            var barList = new List<Bar>();

            string line;
            var stringReader = new StringReader(dataFile);
            int cutWrongTimeBars = 0;
            LoadingNote = string.Empty;

            while ((line = stringReader.ReadLine()) != null)
            {
                Match match = regexDataFile.Match(line);
                if (!match.Success) continue;

                DateTime time = ParseTime(match);

                if (!CheckTimePeriod(time, period))
                {
                    cutWrongTimeBars++;
                    continue;
                }

                var bar = new Bar
                {
                    Time   = time,
                    Open   = ParseDouble(match.Groups["open"].Value),
                    High   = ParseDouble(match.Groups["high"].Value),
                    Low    = ParseDouble(match.Groups["low"].Value),
                    Close  = ParseDouble(match.Groups["close"].Value),
                    Volume = int.Parse(match.Groups["volume"].Value)
                };
                barList.Add(bar);
            }

            stringReader.Close();

            if (barList.Count == 0)
                throw new Exception("Could not count the data bars!");

            if (cutWrongTimeBars > 0)
                LoadingNote += $"Cut off {cutWrongTimeBars} bars with wrong time.\r\n";

            return barList;
        }

        private DateTime ParseTime(Match match)
        {
            int year = int.Parse(match.Groups["year"].Value);
            year = CorrectProblemYear2000(year);
            int month = int.Parse(match.Groups["month"].Value);
            int day = int.Parse(match.Groups["day"].Value);
            int hour = 0;
            int min = 0;
            int sec = 0;

            if (!isOptionalDataFile)
            {
                hour = int.Parse(match.Groups["hour"].Value);
                min = int.Parse(match.Groups["min"].Value);
                string seconds = match.Groups["sec"].Value;
                sec = (seconds == "" ? 0 : int.Parse(seconds));
            }

            return new DateTime(year, month, day, hour, min, sec);
        }

        private bool CheckTimePeriod(DateTime time, int period)
        {
            if (period == 1)
                return true;
            if (period < 60)
                return time.Minute % period == 0;
            if (time.Minute != 0)
                return false;
            if (period == 240)
                return time.Hour % 4 == 0;
            if (period >= 1440)
                return time.Hour == 0;
            return true;
        }

        /// <summary>
        ///     Parses a value as double from a string.
        /// </summary>
        /// <param name="input">String to parse.</param>
        /// <returns>A value as double.</returns>
        private double ParseDouble(string input)
        {
            string separator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

            if (separator == "." && input.Contains(","))
                input = input.Replace(",", separator);
            else if (separator == "," && input.Contains("."))
                input = input.Replace(".", separator);

            return double.Parse(input);
        }

        /// <summary>
        ///     Fixes wrong year interpretation.
        ///     For example 08 must be 2008 instead of 8.
        /// </summary>
        private int CorrectProblemYear2000(int year)
        {
            if (year < 100)
                year += 2000;
            if (year > DateTime.Now.Year)
                year -= 100;
            return year;
        }
    }
}