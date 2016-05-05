using System.IO;
using System.Linq;
using System.Text;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Services
{
    public class IndicatorToTs
    {
        public static void ConvertIndicators()
        {
            var repo = new Repository();
            var indicators = repo.GetIndicators();
            var baseDir = @"d://IndicatorsTs/";

            // Generate and save docs
            indicators.ToList().ForEach(pair =>
            {
                var indicator = pair.Value;
                if (indicator.TestPossibleSlot(SlotTypes.OpenFilter))
                {
                    indicator.Initialize(SlotTypes.OpenFilter);
                    var info = ConvertIndicator(indicator);
                    var fileName = GetFileName(indicator.IndicatorName);
                    string path = $@"{baseDir}{fileName}";
                    SaveFile(info, path);
                }
            });
        }

        private static string ConvertIndicator(IIndicator indicator)
        {
            var sb = new StringBuilder();

            sb.AppendLine("\"use strict\";");
            sb.AppendLine();

            sb.AppendLine("class XXX extends Indicator implements IIndicator {".Replace("XXX",
                GetClassName(indicator.IndicatorName)));
            sb.AppendLine("    indicatorName = \"XXX\";".Replace("XXX", indicator.IndicatorName));
            sb.AppendLine();

            sb.AppendLine("    initialize(slotType:SlotTypes):void {");
            sb.AppendLine("        this.slotType = slotType;");

            sb.AppendLine(GetListParamsString(indicator));

            sb.AppendLine(GetNumParamsString(indicator));
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    calculate(data:DataSet):void {");
            sb.AppendLine("        let logic:string = this.listParam[0].text;");
            sb.AppendLine("        let maMethod:MaMethod = MaMethod[<string>this.listParam[1].text];");
            sb.AppendLine("        let basePrice:BasePrice = BasePrice[<string>this.listParam[2].text];");
            sb.AppendLine("        let period:number = this.numParam[0].value;");
            sb.AppendLine("        let level:number = this.numParam[1].value;");
            sb.AppendLine("        let previous:number = this.usePreviousBarValue ? 1 : 0;");
            sb.AppendLine("        var firstBar:number = period + 2;");
            sb.AppendLine();
            sb.AppendLine("        var arr1:number[] = new Array(data.bars);");
            sb.AppendLine("        var arr2:number[] = new Array(data.bars);");
            sb.AppendLine();
            sb.AppendLine("        var ind:number[] = new Array(data.bars);");
            sb.AppendLine();
            sb.AppendLine(@"
        let logicRule:IndicatorLogic = IndicatorLogic.It_does_not_act_as_a_filter;
        if (logic === 'XXXX rises') {
            logicRule = IndicatorLogic.The_indicator_rises;
        } else if (logic === 'XXXX falls') {
            logicRule = IndicatorLogic.The_indicator_falls;
        } else if (logic === 'XXXX is higher than the Level line') {
            logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
        } else if (logic === 'XXXX is lower than the Level line') {
            logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
        } else if (logic === 'XXXX crosses the Level line upward') {
            logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
        } else if (logic === 'XXXX crosses the Level line downward') {
            logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
        } else if (logic === 'XXXX changes its direction upward') {
            logicRule = IndicatorLogic.The_indicator_changes_its_direction_upward;
        } else if (logic === 'XXXX changes its direction downward') {
            logicRule = IndicatorLogic.The_indicator_changes_its_direction_downward;
        }
                ".Replace("'", "\""));
            sb.AppendLine();
            sb.AppendLine(
                "        let signals = IndicatorHelper.oscillatorLogic(firstBar, previous, ind, level, -level, logicRule);");
            sb.AppendLine();
            sb.AppendLine("        this.longSignal = signals.longSignal;");
            sb.AppendLine("        this.shortSignal = signals.shortSignal;");
            sb.AppendLine("        this.firstBar = firstBar;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();

            return sb.ToString();
        }


        private static string GetListParamsString(IIndicator indicator)
        {
            var sb = new StringBuilder();
            var p = 0;
            foreach (var param in indicator.IndParam.ListParam)
            {
                if (!param.Enabled) continue;
                sb.AppendLine();
                sb.AppendLine($"        this.listParam[{p}].caption = \"{param.Caption}\";");
                if (param.Caption == "Logic")
                {
                    sb.AppendLine($"        this.listParam[{p}].itemList = [");
                    for (var i = 0; i < param.ItemList.Length; i++)
                    {
                        var item = param.ItemList[i];
                        sb.Append($"            \"{item}\"");
                        sb.AppendLine(i < param.ItemList.Length - 1 ? "," : "");
                    }
                    sb.AppendLine("        ];");
                }
                else
                {
                    sb.Append($"        this.listParam[{p}].itemList = [");
                    for (var i = 0; i < param.ItemList.Length; i++)
                    {
                        var item = param.ItemList[i];
                        sb.Append($"\"{item}\"");
                        sb.Append(i < param.ItemList.Length - 1 ? "," : "");
                    }
                    sb.AppendLine("];");
                }
                sb.AppendLine($"        this.listParam[{p}].index = {param.Index};");
                sb.AppendLine($"        this.listParam[{p}].text = \"{param.Text}\";");
                sb.AppendLine($"        this.listParam[{p}].enabled = true;");
                sb.AppendLine($"        this.listParam[{p}].toolTip = \"{param.ToolTip}\";");
                p++;
            }
            var output = sb.ToString();
            if (string.IsNullOrEmpty(output))
                return string.Empty;
            return output;
        }

        private static string GetNumParamsString(IIndicator indicator)
        {
            var sb = new StringBuilder();
            var p = 0;
            foreach (var param in indicator.IndParam.NumParam)
            {
                if (!param.Enabled) continue;
                sb.AppendLine();
                sb.AppendLine($"        this.numParam[{p}].caption = \"{param.Caption}\";");
                sb.AppendLine($"        this.numParam[{p}].value = {param.Value};");
                sb.AppendLine($"        this.numParam[{p}].min = {param.Min};");
                sb.AppendLine($"        this.numParam[{p}].max = {param.Max};");
                sb.AppendLine($"        this.numParam[{p}].point = {param.Point};");
                sb.AppendLine($"        this.numParam[{p}].enabled = {param.Enabled.ToString().ToLower()};");
                sb.AppendLine($"        this.numParam[{p}].toolTip = \"{param.ToolTip}\";");
                p++;
            }
            var output = sb.ToString();
            if (string.IsNullOrEmpty(output))
                return string.Empty;
            return output.Remove(output.Length - 3);
        }

        private static string GetClassName(string name)
        {
            return name
                .Replace(" ", string.Empty).Replace("-", string.Empty)
                .Replace(".", string.Empty).Replace("'", string.Empty);
        }

        private static string GetFileName(string name)
        {
            return name.Replace(" ", "-").Replace(".", "-").Replace("'", string.Empty).ToLower() + ".ts";
        }

        private static void SaveFile(string info, string path)
        {
            File.WriteAllText(path, info);
        }
    }
}