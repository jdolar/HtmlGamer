using AngleSharp;
using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
using HtmlGamer.Core.Data.Models.OutPut;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
namespace HtmlGamer.Core.Services;
public sealed class Parser
{
    private readonly ILogger<Parser> _logger;
    private readonly string _guildsHtml;
    private readonly string _guildSelector;
    private readonly string _memberSelector;

    private Dictionary<string, Member> _members = new();
    private Dictionary<string, Guild> _guilds = new();
    private Dictionary<string, MemberGuild> _content = new();
    public Parser(ILogger<Parser> logger, IReadOnlyDictionary<string, string> config)
    {
        _logger = logger;

        _guildsHtml = config[Constants.EncryptKeys.GuildsHtml];
        _guildSelector = config[Constants.EncryptKeys.GuildSelector];
        _memberSelector = config[Constants.EncryptKeys.MemberSelector];

        Loggers.LogAs.Init(_logger);
    }
    internal async Task<(List<MemberGuildType>, long)> ParseHtml(string html)
    {
        if (html.Contains(_guildsHtml))
            return await ParseGuilds(html);

        return await ParseMembers(html);
    }
    internal async Task<(List<MemberGuildType>, long)> ParseMembers(string html)
    {
        var sw = Stopwatch.StartNew();
        var result = new List<MemberGuildType>();

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        var links = document.QuerySelectorAll(_memberSelector);

        foreach (var link in links)
        {
            string name = Clean(link.TextContent);

            var href = link.GetAttribute("href") ?? "";
            string idStr = href.Split("id=").LastOrDefault() ?? "";
            int id = int.TryParse(idStr, out var parsedId) ? parsedId : 0;

            var tr1 = link.Closest("tr");
            var tr2 = tr1?.NextElementSibling;

            string guild = "";
            string title = "";

            if (tr2 != null)
            {
                string detail = Clean(tr2.TextContent);

                int aStart = detail.IndexOf('[');
                int aEnd = detail.IndexOf(']');

                if (aStart != -1 && aEnd > aStart)
                {
                    var inside = detail[(aStart + 1)..aEnd];

                    int dash = inside.IndexOf('-');

                    if (dash != -1)
                    {
                        guild = Clean(inside[..dash]);
                        title = Clean(inside[(dash + 1)..]);
                    }
                    else
                    {
                        guild = Clean(inside);
                    }
                }
            }

            MemberGuildType parsedData = new();
            parsedData.AddMemberData(id, name, guild, title);

            result.Add(parsedData);
        }

        sw.Stop();
        return (result, sw.ElapsedMilliseconds);
    }
    internal async Task<(List<MemberGuildType>, long)> ParseGuilds(string html)
    {
        var timer = Stopwatch.StartNew();
        var result = new List<MemberGuildType>();

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        foreach (var row in document.QuerySelectorAll("tr"))
        {
            var firstCell = row.QuerySelector("td");
            if (firstCell == null)
                continue;

            var guildLink = firstCell.QuerySelector(_guildSelector);
            if (guildLink == null)
                continue;

            string guildName = Clean(guildLink.TextContent);

            var href = guildLink.GetAttribute("href") ?? "";
            int.TryParse(href.Split("id=").LastOrDefault(), out int guildId);

            var tds = row.QuerySelectorAll("td");
            var leaderLink = tds[1].QuerySelector(_memberSelector);

            string leaderName = "";
            int leaderId = 0;

            if (leaderLink != null)
            {
                leaderName = Clean(leaderLink.TextContent);

                var url = leaderLink.GetAttribute("href") ?? "";
                var idStr = url.Split("id=").LastOrDefault();
                int.TryParse(idStr, out leaderId);
            }
            int memberCount = 0;

            if (tds.Length > 0)
            {
                string raw = Clean(tds[^1].TextContent);
                int p = raw.IndexOf('(');
                if (p > 0)
                    raw = raw[..p];

                raw = new string(raw.Where(char.IsDigit).ToArray());

                int.TryParse(raw, out memberCount);
            }

            MemberGuildType parsedData = new();
            parsedData.AddGuildData(guildId, guildName, leaderName, leaderId, memberCount);

            result.Add(parsedData);
        }

        timer.Stop();
        return (result, timer.ElapsedMilliseconds);
    }
    internal string Clean(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "-1";

        return NormalizeSpaces(System.Net.WebUtility.HtmlDecode(input));
    }
    internal string NormalizeSpaces(string input)
    {
        Span<char> buffer = stackalloc char[input.Length];

        int idx = 0;
        bool lastWasSpace = false;

        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace && idx > 0)
                {
                    buffer[idx++] = ' ';
                    lastWasSpace = true;
                }
            }
            else
            {
                buffer[idx++] = c;
                lastWasSpace = false;
            }
        }

        return new string(buffer[..idx]).Trim();
    }
    internal void Merge(MemberGuildType entry)
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
    internal ParsedData GetParsedData()
    {
        return new ParsedData
        {
            Members = _members,
            Guilds = _guilds,
            MebersGuilds = _content
        };
    }
}
