using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
using HtmlGamer.Core.Data.Models.OutPut;
using Microsoft.Extensions.Logging;
using System.Text;
namespace HtmlGamer.Core.Services;
public sealed class Mapper
{
    private readonly ILogger<Mapper> _logger;
    private readonly string _memberUrl;
    private readonly string _guildUrl;
    public Mapper(ILogger<Mapper> logger, IReadOnlyDictionary<string,string> config)
    {
        _logger = logger;
        _memberUrl = config[Constants.EncryptKeys.MemberSelector];
        _guildUrl = config[Constants.EncryptKeys.GuildSelector];

        Loggers.LogAs.Init(_logger);
    }
    internal string DrawFields(List<MemberGuild> fields, string? preSpace = Constants.Unsorted.PreFixTab, string? tailingSpace = Constants.Unsorted.TailingTab, int? spaceBetween = null, bool? includeGuilds = null, bool? groupByGuild = null)
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
    internal string MapToPersonalLog(ParsedData parsedData, bool? ignoreGuilds = null, int? minMemberPerGuild = 2)
    {
        StringBuilder sb = new();

        if (ignoreGuilds.HasValue && ignoreGuilds.Value)
        {
            foreach (var content in parsedData.MebersGuilds.Values)
            {
                Member? member = parsedData.Members.Values.Where(x => x.Id == content.MemberId).FirstOrDefault();
                if (member is not null && member.Id != 0)
                    sb.Append(_memberUrl).Append(member.Id).Append(Constants.Html.BlankTarget).Append('>').Append(member.Name).Append(Constants.Html.UrlEnd).AppendLine();
            }
            return sb.ToString();
        }

        foreach (var guild in parsedData.Guilds.Values)
        {
            var contentData = parsedData.MebersGuilds.Values.Where(x => x.GuildId == guild.Id);
            int minimumCount = minMemberPerGuild.HasValue ? minMemberPerGuild.Value : 0;
            if (contentData.Count() >= minimumCount)
            {
                sb.Append(_guildUrl).Append(guild.Id).Append(Constants.Html.BlankTarget).Append("> [ ").Append(guild.Name).Append(" ]").Append(Constants.Html.UrlEnd).AppendLine();
                foreach (var content in contentData)
                {
                    Member? member = parsedData.Members.Values.Where(x => x.Id == content.MemberId).FirstOrDefault();
                    if (member is not null && member.Id != 0)
                        sb.Append(_memberUrl).Append(member.Id).Append(Constants.Html.BlankTarget).Append('>').Append(member.Name).Append(Constants.Html.UrlEnd).AppendLine();
                }
            }
        }

        return sb.ToString();
    }
    internal JsonFile MapToJson(ParsedData parsedData)
    {
        JsonFile jsonFile = new();
        jsonFile.Created = DateTime.UtcNow;
        jsonFile.Owner = Environment.UserName;

        var contentList = new List<MemberGuildType>();

        foreach (var content in parsedData.MebersGuilds.Values)
        {
            Member? member = parsedData.Members.Values.Where(x => x.Id == content.MemberId).FirstOrDefault();
            Guild? guild = parsedData.Guilds.Values.Where(x => x.Id == content.GuildId).FirstOrDefault();

            jsonFile.Content.Add(new MemberGuildType
            {
                Member = member ?? new Member { Id = (int)content.MemberId, Name = $"Unknown Member {content.MemberId}" },
                Guild = guild ?? (content.GuildId != null ? new Guild { Id = (int)content.GuildId, Name = $"Unknown Guild {content.GuildId}" } : null),
                Type = Data.Enums.Type.Member
            });
        }

        return jsonFile;
    }
}
