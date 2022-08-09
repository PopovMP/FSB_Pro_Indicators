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
using System.IO;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;
using Newtonsoft.Json;

namespace ForexStrategyBuilder.Services
{
    public class DataLoader
    {
        public IDataSet LoadDataSet(string dataSourceName, string symbol, DataPeriod period)
        {
            DataId      dataId     = new DataId(dataSourceName, symbol, period);
            IDataSource dataSource = LoadDataSource(dataId.Source);
            DataParams  dataParams = GetDataParams(dataId, UserFilesFolder, dataSource);
            IDataSet    dataSet    = LoadCsvFile(dataParams);
            
            dataSet.Properties = dataSource.InstrumentProperties[symbol];

            return dataSet;
        }

        private DataParams GetDataParams(DataId dataId, string folder, IDataSource dataSource)
        {
            var fileName = $"{dataId.Symbol}{(int)dataId.Period}.csv";
            var path     = Path.Combine(folder, "Data", dataId.Source, fileName);

            var dataParams = new DataParams
            {
                DataSourceName     = dataId.Source,
                Symbol             = dataId.Symbol,
                Period             = dataId.Period,
                DataId             = dataId,
                Path               = path,
                StartDate          = dataSource.StartDate,
                EndDate            = dataSource.EndDate,
                IsUseStartDate     = dataSource.IsUseStartDate,
                IsUseEndDate       = dataSource.IsUseEndDate,
                MaximumBars        = dataSource.MaximumBars,
                MinimumBars        = dataSource.MinimumBars,
            };

            return dataParams;
        }

        private IDataSource LoadDataSource(string dataSourceName)
        {
            string fileName = "DataSource_" + dataSourceName + ".json";
            var path = Path.Combine(UserFilesFolder, "System", fileName);
            if (!FileExists(path))
            {
                Console.WriteLine("File does not exist \"{0}\".", path);
                return null;
            }

            IDataSource dataSource = new DataSource();

            try
            {
                var json = ReadTextFile(path);
                dataSource = JsonConvert.DeserializeObject<DataSource>(json);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Data source loading: " + exception.Message);
            }

            return dataSource;
        }

        private IDataSet LoadCsvFile(DataParams dataParams)
        {
            var dataFile = LoadDataFile(dataParams.Path);

            if (string.IsNullOrEmpty(dataFile))
                return null;

            var dataParser = new DataParser();
            var bars = dataParser.Parse(dataFile, (int) dataParams.Period);
            if (bars == 0)
                return null;

            IDataSet dataSet = new DataSet(dataParams.Symbol, dataParams.Period, bars);
            dataSet.DataId      = dataParams.DataId;
            dataSet.DataParams  = dataParams;
            dataSet.LoadingNote = dataParser.LoadingNote;

            for (var bar = 0; bar < bars; bar++)
                dataSet.UpdateBar(bar, dataParser.Bar[bar]);

            return dataSet;
        }

        private string LoadDataFile(string path)
        {
            if (!FileExists(path))
            {
                Console.WriteLine("Could not find {0} file.", path);
                return string.Empty;
            }

            try
            {
                return ReadTextFile(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }
        }

        private bool FileExists(string path)
        {
            return File.Exists(path);
        }

        private string ReadTextFile(string path)
        {
            return File.ReadAllText(path);
        }

        public string UserFilesFolder { get; set; }
    }
}