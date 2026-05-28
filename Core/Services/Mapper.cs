using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
using HtmlGamer.Core.Data.Models.OutPut;
using Microsoft.Extensions.Logging;
using System.Text;
namespace HtmlGamer.Core.Services;
public class Mapper
{
    private readonly ILogger<Mapper> _logger;
    internal Dictionary<string, Member> _members = new();
    internal Dictionary<string, Guild> _guilds = new();
    public Dictionary<string, MemberGuild> _content = new();
    private readonly string _memberUrl;
    private readonly string _guildUrl;
    public Mapper(ILogger<Mapper> logger, IReadOnlyDictionary<string,string> config)
    {
        _logger = logger;
        _memberUrl = config[Constants.EncryptKeys.MemberSelector];
        _guildUrl = config[Constants.EncryptKeys.GuildSelector];

        Loggers.LogAs.Init(_logger);
    }
    public void Merge(MemberGuildType entry)
    {
        var memberKey = entry.Member.Name;

        if (!_members.TryGetValue(memberKey, out var member))
        {
            member = entry.Member;
            _members[memberKey] = member;
        }
        else
        {
            if (member.Id == 0 && entry.Member.Id != 0)
                member.Id = entry.Member.Id;

            if (string.IsNullOrEmpty(member.Title) && !string.IsNullOrEmpty(entry.Member.Title))
                member.Title = entry.Member.Title;
        }

        Guild? guild = null;
        if (!string.IsNullOrEmpty(entry.Guild?.Name))
        {
            var guildKey = entry.Guild.Name ?? "";

            if (!_guilds.TryGetValue(guildKey, out guild))
            {
                guild = entry.Guild;
                _guilds[guildKey] = guild;
            }
            else
            {
                if (guild.Id == 0 && entry.Guild.Id != 0)
                    guild.Id = entry.Guild.Id;

                if (guild.MemberCount == 0 && entry.Guild.MemberCount != 0)
                    guild.MemberCount = entry.Guild.MemberCount;
            }
        }

        var contentKey = member.Id != 0
            ? $"p:{member.Id}"
            : $"n:{member.Name}";

        if (!_content.TryGetValue(contentKey, out var content))
        {
            content = new MemberGuild
            {
                MemberId = member.Id,
                GuildId = guild?.Id
            };

            _content[contentKey] = content;
        }
        else
        {
            if (content.MemberId == 0 && member.Id != 0)
                content.MemberId = member.Id;

            if (content.GuildId == 0 && guild?.Id != 0)
                content.GuildId = guild?.Id;
        }
    }
    public string DrawFields(List<MemberGuild> fields, string? preSpace = Constants.Unsorted.PreFixTab, string? tailingSpace = Constants.Unsorted.TailingTab, int? spaceBetween = null, bool? includeGuilds = null, bool? groupByGuild = null)
    {
        if (fields.Count == 0)
            return string.Empty;

        if (includeGuilds.HasValue && includeGuilds.Value)
        {
            var guildGroups = fields.Where(f => f.GuildId != null).GroupBy(f => f.GuildId);
            StringBuilder sbGuilds = new();
            foreach (var guildGroup in guildGroups)
            {
                sbGuilds.AppendLine($"Guild ID: {guildGroup.Key}");
                foreach (var field in guildGroup)
                {
                    sbGuilds.AppendLine(preSpace + field.MemberId.ToString());
                }
            }
            return sbGuilds.ToString();
        }

        int labelWidth = fields.Max(f => f.MemberId.ToString().Length);
        int valueWidth = fields.Max(f => f.GuildId?.ToString().Length ?? 0);

        StringBuilder sb = new();
        sb.AppendLine();
        foreach (MemberGuild field in fields)
        {
            if (field.GuildId == null)
            {
                sb.AppendLine(preSpace + field.MemberId.ToString().PadRight(labelWidth));
            }
            else
            {
                sb.AppendLine(preSpace + field.MemberId.ToString().PadRight(labelWidth + (spaceBetween ??= Constants.Unsorted.SpaceBetween)) + field.GuildId?.ToString().PadLeft(valueWidth) + tailingSpace);
            }
        }
        return sb.ToString();
    }
    public string MapToPersonalLog(bool? ignoreGuilds = null, int? minMemberPerGuild = 2)
    {
        StringBuilder sb = new();

        if (ignoreGuilds.HasValue && ignoreGuilds.Value)
        {
            foreach (var content in _content)
            {
                Member? member = _members.Values.Where(x => x.Id == content.Value.MemberId).FirstOrDefault();
                if (member is not null && member.Id != 0)
                    sb.Append(_memberUrl).Append(member.Id).Append(Constants.Html.BlankTarget).Append('>').Append(member.Name).Append(Constants.Html.UrlEnd).AppendLine();
            }
            return sb.ToString();
        }

        foreach (var guild in _guilds)
        {
            var contentData = _content.Where(x => x.Value.GuildId == guild.Value.Id);
            int minimumCount = minMemberPerGuild.HasValue ? minMemberPerGuild.Value : 0;
            if (contentData.Count() >= minimumCount)
            {
                sb.Append(_guildUrl).Append(guild.Value.Id).Append(Constants.Html.BlankTarget).Append("> [ ").Append(guild.Value.Name).Append(" ]").Append(Constants.Html.UrlEnd).AppendLine();
                foreach (var content in contentData)
                {
                    Member? member = _members.Values.Where(x => x.Id == content.Value.MemberId).FirstOrDefault();
                    if (member is not null && member.Id != 0)
                        sb.Append(_memberUrl).Append(member.Id).Append(Constants.Html.BlankTarget).Append('>').Append(member.Name).Append(Constants.Html.UrlEnd).AppendLine();
                }
            }
        }

        return sb.ToString();
    }
    public JsonFile MapToJson()
    {
        JsonFile jsonFile = new();
        jsonFile.Created = DateTime.UtcNow;
        jsonFile.Owner = Environment.UserName;

        var contentList = new List<MemberGuildType>();

        foreach (var content in _content)
        {
            Member? member = _members.Values.Where(x => x.Id == content.Value.MemberId).FirstOrDefault();
            Guild? guild = _guilds.Values.Where(x => x.Id == content.Value.GuildId).FirstOrDefault();

            jsonFile.Content.Add(new MemberGuildType
            {
                Member = member ?? new Member { Id = (int)content.Value.MemberId, Name = $"Unknown Member {content.Value.MemberId}" },
                Guild = guild ?? (content.Value.GuildId != null ? new Guild { Id = (int)content.Value.GuildId.Value, Name = $"Unknown Guild {content.Value.GuildId.Value}" } : null),
                Type = Data.Enums.Type.Member
            });
        }

        return jsonFile;
    }
}
