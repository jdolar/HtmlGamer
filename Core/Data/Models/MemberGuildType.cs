using HtmlGamer.Core.Data.Models.InPut;
namespace HtmlGamer.Core.Data.Models;
internal sealed class MemberGuildType
{
    internal Member Member { get; set; } = new();
    internal Guild? Guild { get; set; } = null;
    internal Enums.Type Type { get; set; }
}
