using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
namespace HtmlGamer.Core.Data;
internal static class ModelExtensions
{
    internal static void AddMemberData (this MemberGuildType parsed, int id, string name, string? guild, string? title)
    {
        parsed = parsed ?? new MemberGuildType ();
        parsed.Type = Enums.Type.Member;

        string guildName = string.IsNullOrEmpty(guild) ? string.Empty : guild;

        parsed.Member = new Member
        {
            Id = id,
            Name = name,
            Title = title ?? string.Empty
        };

        if (!string.IsNullOrEmpty(guild))
        {
            parsed.Guild = new Guild
            {
                Name = guildName
            };
        }
    }
    internal static void AddGuildData(this MemberGuildType parsed, int id, string name, string leaderName, int leaderId, int membersCount)
    {
        parsed = parsed ?? new MemberGuildType();
        parsed.Type = Enums.Type.Guild;

        parsed.Guild = new Guild
        {
            Id = id,
            Name = name,
            MemberCount = membersCount
        };

        parsed.Member = new Member
        {
            Id= leaderId,
            Name = leaderName,
        };
    }
    internal static string ToStringText(this Guild guild) => $"{guild.Name} [Id={guild.Id} MemberCount={guild.MemberCount}]";
    internal static string ToStringText(this Member member) => $"{member.Name} [Id={member.Id}]";
    internal static string ToStringText(this MemberGuildType data) => $"{data.Member.ToStringText()} [Guild={data.Guild?.ToStringText() ?? " / "}]";
}
