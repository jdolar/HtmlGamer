using HtmlGamer.Core.Data.Models.InPut;
namespace HtmlGamer.Core.Data.Models;
public sealed class MemberGuildType
{
    public Member Member { get; set; } = new();
    public Guild? Guild { get; set; } = null;
    public Enums.Type Type { get; set; }
}
