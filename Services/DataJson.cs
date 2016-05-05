using System;
using System.Globalization;
using System.Text;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Services
{
    public class DataJson
    {
        public string GetDataJson(IDataSet dataSet)
        {
            var sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine("   \"time\": " + ConvertTimeToJson(dataSet.Time) + ",");
            sb.AppendLine("   \"open\": " + ConvertPriceToJson(dataSet.Open) + ",");
            sb.AppendLine("   \"high\": " + ConvertPriceToJson(dataSet.High) + ",");
            sb.AppendLine("   \"low\": " + ConvertPriceToJson(dataSet.Low) + ",");
            sb.AppendLine("   \"close\": " + ConvertPriceToJson(dataSet.Close) + ",");
            sb.AppendLine("   \"volume\": " + ConvertVolumeToJson(dataSet.Volume));
            sb.AppendLine("}");

            return sb.ToString();
        }


        private string ConvertTimeToJson(DateTime[] time)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (var i = 0; i < time.Length; i++)
            {
                string text = ((int) (time[i] - new DateTime(1970, 1, 1)).TotalMinutes).ToString();
                if (i == time.Length - 1)
                    sb.Append(text);
                else
                    sb.Append(text + ",");
            }
            sb.Append("]");

            return sb.ToString();

        }
        
        private string ConvertPriceToJson(double[] data)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (var i = 0; i < data.Length; i++)
            {
                if (i == data.Length - 1)
                    sb.Append(data[i].ToString(CultureInfo.InvariantCulture));
                else
                    sb.Append(data[i].ToString(CultureInfo.InvariantCulture) + ",");
            }
            sb.Append("]");

            return sb.ToString();
        }

        private string ConvertVolumeToJson(int[] data)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (var i = 0; i < data.Length; i++)
            {
                if (i == data.Length - 1)
                    sb.Append(data[i].ToString(CultureInfo.InvariantCulture));
                else
                    sb.Append(data[i].ToString(CultureInfo.InvariantCulture) + ",");
            }
            sb.Append("]");

            return sb.ToString();
        }

    }
}