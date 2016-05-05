using System;
using System.IO;
using System.Linq;
using System.Text;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Services
{
    public class IndicatorsJson
    {
        public void GenerateJsonObject()
        {
            var repo = new Repository();
            {
                var path = @"d://IndicatorsOpenPoint.json";
                var json = GenerateJsonString(repo.GetIndicators().Values.ToArray(), SlotTypes.Open);
                File.WriteAllText(path, json);
            }
            {
                var path = @"d://IndicatorsOpenFilter.json";
                var json = GenerateJsonString(repo.GetIndicators().Values.ToArray(), SlotTypes.OpenFilter);
                File.WriteAllText(path, json);
            }
            {
                var path = @"d://IndicatorsClosePoint.json";
                var json = GenerateJsonString(repo.GetIndicators().Values.ToArray(), SlotTypes.Close);
                File.WriteAllText(path, json);
            }
            {
                var path = @"d://IndicatorsCloseFilter.json";
                var json = GenerateJsonString(repo.GetIndicators().Values.ToArray(), SlotTypes.CloseFilter);
                File.WriteAllText(path, json);
            }
        }

        private string GenerateJsonString(IIndicator[] indicatorsList, SlotTypes slotType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            foreach (var indicator in indicatorsList)
            {
                if (!indicator.TestPossibleSlot(slotType)) continue;
                indicator.Initialize(slotType);
                sb.AppendLine("    {");
                sb.AppendLine($"      \"indicatorName\": \"{indicator.IndicatorName}\",");
                //sb.AppendLine("      \"PossibleSlots\": [");
                //sb.AppendLine(GetPossibleSlotsString(indicator));
                //sb.AppendLine("      ],");
                sb.AppendLine("      \"listParams\": [");
                sb.AppendLine(GetListParamsString(indicator));
                sb.AppendLine("      ],");
                sb.AppendLine("      \"numParams\": [");
                sb.AppendLine(GetNumParamsString(indicator));
                sb.AppendLine("      ],");
                sb.AppendLine("      \"checkParams\": [");
                sb.AppendLine(GetCheckParamsString(indicator));
                sb.AppendLine("      ]");
                sb.AppendLine("    },");
            }
            sb.AppendLine("]");
            var output = sb.ToString();
            var idx = output.LastIndexOf(",", StringComparison.Ordinal);
            output = output.Remove(idx, 1);
            return output;
        }

        private string GetPossibleSlotsString(IIndicator indicator)
        {
            var sb = new StringBuilder();
            if (indicator.TestPossibleSlot(SlotTypes.Open))
                sb.AppendLine("        \"Open\",");
            if (indicator.TestPossibleSlot(SlotTypes.OpenFilter))
                sb.AppendLine("        \"OpenFilter\",");
            if (indicator.TestPossibleSlot(SlotTypes.Close))
                sb.AppendLine("        \"Close\",");
            if (indicator.TestPossibleSlot(SlotTypes.CloseFilter))
                sb.AppendLine("        \"CloseFilter\",");
            var output = sb.ToString();
            return output.Remove(output.Length - 2);
        }

        private string GetListParamsString(IIndicator indicator)
        {
            var sb = new StringBuilder();
            foreach (var param in indicator.IndParam.ListParam)
            {
                if (!param.Enabled) continue;
                sb.AppendLine("        {");
                sb.AppendLine($"          \"caption\": \"{param.Caption}\",");
                sb.AppendLine("          \"itemList\": [");
                for (var i = 0; i < param.ItemList.Length; i++)
                {
                    var item = param.ItemList[i];
                    sb.Append($"            \"{item}\"");
                    sb.AppendLine(i < param.ItemList.Length - 1 ? "," : "");
                }
                sb.AppendLine("          ],");
                sb.AppendLine($"          \"index\": {param.Index},");
                sb.AppendLine($"          \"text\": \"{param.Text}\",");
                sb.AppendLine($"          \"enabled\": {param.Enabled.ToString().ToLower()},");
                sb.AppendLine($"          \"toolTip\": \"{param.ToolTip}\"");
                sb.AppendLine("        },");
            }
            var output = sb.ToString();
            if (string.IsNullOrEmpty(output))
                return string.Empty;
            return output.Remove(output.Length - 3);
        }

        private string GetNumParamsString(IIndicator indicator)
        {
            var sb = new StringBuilder();
            foreach (var param in indicator.IndParam.NumParam)
            {
                if (!param.Enabled) continue;
                sb.AppendLine("        {");
                sb.AppendLine($"          \"caption\": \"{param.Caption}\",");
                sb.AppendLine($"          \"value\": {param.Value},");
                sb.AppendLine($"          \"min\": {param.Min},");
                sb.AppendLine($"          \"max\": {param.Max},");
                sb.AppendLine($"          \"point\": {param.Point},");
                sb.AppendLine($"          \"enabled\": {param.Enabled.ToString().ToLower()},");
                sb.AppendLine($"          \"toolTip\": \"{param.ToolTip}\"");
                sb.AppendLine("        },");
            }
            var output = sb.ToString();
            if (string.IsNullOrEmpty(output))
                return string.Empty;
            return output.Remove(output.Length - 3);
        }

        private string GetCheckParamsString(IIndicator indicator)
        {
            var sb = new StringBuilder();
            foreach (var param in indicator.IndParam.CheckParam)
            {
                if (!param.Enabled) continue;
                sb.AppendLine("        {");
                sb.AppendLine($"          \"caption\": \"{param.Caption}\",");
                sb.AppendLine($"          \"checked\": {param.Checked.ToString().ToLower()},");
                sb.AppendLine($"          \"enabled\": {param.Enabled.ToString().ToLower()},");
                sb.AppendLine($"          \"toolTip\": \"{param.ToolTip}\"");
                sb.AppendLine("        },");
            }
            var output = sb.ToString();
            if (string.IsNullOrEmpty(output))
                return string.Empty;
            return output.Remove(output.Length - 3);
        }
    }
}