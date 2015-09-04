using System.IO;
using System.Linq;
using System.Text;
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Services
{
    public class DocGenerator
    {
        public void GenerateDocs()
        {
            var repo = new Repository();
            var indicators = repo.GetIndicators();
            var baseDir = @"d://Docs/Indicators/";

            // Generate and save docs
            indicators.ToList().ForEach(pair =>
            {
                IIndicator indicator = pair.Value;
                string info = GenerateIndicatorDoc(indicator);
                string fileName = GetFileName(indicator.IndicatorName) + ".txt";
                string path = $@"{baseDir}{fileName}";
                SaveFile(info, path);
            });
            
            // Generate and save Sidebar
            var names = indicators.Select(pair => pair.Key).ToArray();
            var sidebar = GenerateSideBar(names);
            string sidebarPath = baseDir + @"sidebar.txt";
            SaveFile(sidebar, sidebarPath);
        }

        private string GenerateSideBar(string[] indicators)
        {
            var sb = new StringBuilder();

            sb.AppendLine("====== Indicator List ======");
            sb.AppendLine();
            indicators.ToList().ForEach(i => sb.AppendLine($"  * [[indicator:{GetFileName(i)}|{i}]]"));

            return sb.ToString();
        }

        private string GenerateIndicatorDoc(IIndicator indicator)
        {
            var slotTypes = new[] {SlotTypes.Open, SlotTypes.OpenFilter, SlotTypes.Close, SlotTypes.CloseFilter};
            var csBaseUrl = @"https://github.com/PopovMP/FSB_Pro_Indicators/blob/master/Indicators/Store/";
            var mqlBaseUrl = @"https://github.com/PopovMP/FSB_Expert_Advisor_Code/blob/master/Forexsb.com/Indicators/";
            var className = indicator.IndicatorName
                .Replace(" of ", "Of").Replace("-", "_").Replace(".", "_")
                .Replace(" ", string.Empty).Replace("'", string.Empty);

            var sb = new StringBuilder();

            sb.AppendLine($"====== {indicator.IndicatorName} ======");
            sb.AppendLine();

            sb.AppendLine("FIXME");
            sb.AppendLine();

            sb.AppendLine("===== Parameters =====");
            sb.AppendLine();
           
            // Logic rules
            slotTypes.ToList()
                .Where(indicator.TestPossibleSlot).ToList()
                .ForEach(s => SetLogicalRules(sb, indicator, s));
            sb.AppendLine();
            
            // List parameters
            if (indicator.IndParam.ListParam.Length > 1)
            {
                sb.AppendLine("**List box parameters**");
                indicator.IndParam.ListParam.ToList()
                    .Where(param => !param.Caption.Contains("Logic"))
                    .Where(param => !string.IsNullOrEmpty(param.Caption)).ToList()
                    .ForEach(param => SetListParam(sb, param));
            }
            sb.AppendLine();
            
            // Numeric parameters
            if (indicator.IndParam.NumParam.Length > 0)
            {
                sb.AppendLine("**Numeric box parameters**");
                indicator.IndParam.NumParam.ToList()
                    .Where(param => !string.IsNullOrEmpty(param.Caption)).ToList()
                    .ForEach(param => SetNumParam(sb, param));
            }
            sb.AppendLine();

            sb.AppendLine("===== Implementation =====");
            sb.AppendLine();
            sb.AppendLine($"C# source code: [[{csBaseUrl}{className}.cs|{indicator.IndicatorName}]].");
            sb.AppendLine();
            sb.AppendLine($"MQL source code: [[{mqlBaseUrl}{className}.mqh|{indicator.IndicatorName}]].");

            return sb.ToString();
        }

        private void SetLogicalRules(StringBuilder sb, IIndicator indicator, SlotTypes slotType)
        {
            sb.AppendLine();
            sb.AppendLine($"**Logical rules for {SlotTypeToString(slotType)} slot.**");
            indicator.Initialize(slotType);
            indicator.IndParam.ListParam[0].ItemList.ToList()
                .ForEach(text => sb.AppendLine($"  * {text};"));
        }

        private void SetListParam(StringBuilder sb, ListParam param)
        {
            var options = param.ItemList
                .Select(item => item).ToArray()
                .Aggregate(string.Empty, (p, a) => p + ", " + a, s => s.Remove(0, 2));
            sb.AppendLine($"  * **{param.Caption}** ({param.Text}). {param.ToolTip} Options: {options}.");
        }

        private void SetNumParam(StringBuilder sb, NumericParam param)
        {
            sb.AppendLine($"  * **{param.Caption}** ({param.Value}). {param.ToolTip} Minimum: {param.Min}, Maximum: {param.Max}.");
        }

        private string SlotTypeToString(SlotTypes slotType)
        {
            var stringCaptionText = "Not Defined";
            switch (slotType)
            {
                case SlotTypes.Open:
                    stringCaptionText = "Opening Point of the Position";
                    break;
                case SlotTypes.OpenFilter:
                    stringCaptionText = "Opening Logic Condition";
                    break;
                case SlotTypes.Close:
                    stringCaptionText = "Closing Point of the Position";
                    break;
                case SlotTypes.CloseFilter:
                    stringCaptionText = "Closing Logic Condition";
                    break;
            }

            return stringCaptionText;
        }

        private string GetFileName(string name)
        {
            return name
                .Replace(" ", "_").Replace("-", "_")
                .Replace(".", "_").Replace("'", string.Empty).ToLower();
        }

        private void SaveFile(string info, string path)
        {
            File.WriteAllText(path, info);
        }
    }
}
