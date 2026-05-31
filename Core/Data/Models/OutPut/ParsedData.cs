using HtmlGamer.Core.Data.Models.InPut;
namespace HtmlGamer.Core.Data.Models.OutPut;
public sealed class ParsedData
{
    internal Dictionary<string, Member> Members = new();
    internal Dictionary<string, Guild> Guilds = new();
    internal Dictionary<string, MemberGuild> MebersGuilds = new();
}
