using HtmlGamer.Core.Data.Enums;
namespace HtmlGamer.Core.Data.Models.InPut;
public sealed class Scenario
{
    public string Name { get; set; } = string.Empty;
    public List<Step> Steps { get; set; } = [];
    public Dictionary<string, List<Property>>? Properties { get; set; } = null;
}
