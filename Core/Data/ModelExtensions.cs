using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
using HtmlGamer.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualBasic;
using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
namespace HtmlGamer.Core.Data;

public static class ModelExtensions
{
    internal static void AddMemberData(this MemberGuildType parsed, int id, string name, string? guild, string? title)
    {
        parsed = parsed ?? new MemberGuildType();
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
            Id = leaderId,
            Name = leaderName,
        };
    }
    internal static string ToStringText(this Guild guild) => $"{guild.Name} [Id={guild.Id} MemberCount={guild.MemberCount}]";
    internal static string ToStringText(this Member member) => $"{member.Name} [Id={member.Id}]";
    internal static string ToStringText(this MemberGuildType data) => $"{data.Member.ToStringText()} [Guild={data.Guild?.ToStringText() ?? " / "}]";
    public static string ToTextString(this object obj)
    {
        var sb = new StringBuilder();
        AppendObject(sb, obj, 0);
        return sb.ToString();
    }
    private static void AppendObject(StringBuilder sb, object? obj, int indent)
    {
        if (obj is null)
        {
            sb.AppendLine($"{Indent(indent)}null");
            return;
        }

        var type = obj.GetType();

        if (IsSimpleType(type))
        {
            sb.AppendLine($"{Indent(indent)}{obj}");
            return;
        }

        if (obj is IEnumerable enumerable && obj is not string)
        {
            foreach (var item in enumerable)
            {
                AppendObject(sb, item, indent + 2);
            }

            return;
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = prop.GetValue(obj);

            if (value is null || IsSimpleType(prop.PropertyType))
            {
                sb.AppendLine($"{Indent(indent)}{prop.Name}: {value}");
            }
            else
            {
                sb.AppendLine($"{Indent(indent)}{prop.Name}:");
                AppendObject(sb, value, indent + 2);
            }
        }
    }
    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid)
            || type == typeof(TimeSpan);
    }
    private static string Indent(int count) => new(' ', count);
    public static string ToTextString(this Scenario scenario)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Name: {scenario.Name}");
        builder.AppendLine("Steps:");
        
        foreach (var step in scenario.Steps)
            builder.AppendLine($"  - {step}");

        return builder.ToString();
    }
    public static string ToTextString(this Account account)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Name: {account.UserName}");
        builder.AppendLine($"eMail: {account.Email}");
        builder.AppendLine($"PassWord: ********** ");
        builder.AppendLine($"VpnProfile: {account.VpnProfile}");

        return builder.ToString();
    }
    public static string ToTextString(this Models.InPut.Execute execute)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Player [UserName]: {execute.UserName}");
        builder.AppendLine($"Scenario [Name]: {execute.ScenarioName}");
        
        return builder.ToString();
    }
    public static string ToTextString(this KeyValuePair<string, string> constant)
    {
        return $" * {constant.Key}: {constant.Value}";
    }
    public static IHost ValidateConfigs(this IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<Runner>>();

        Loggers.LogAs.Debug(logger, "Validating configurations...");

        AppSettings? appSettings = host.Services.GetRequiredService<AppSettings>();
        if (appSettings != null)
            Loggers.LogAs.Debug(logger, appSettings.ToTextString()!);
        else
            Loggers.LogAs.Error(logger, "AppSettings is null");

        var scenarios = host.Services.GetRequiredService<IReadOnlyList<Scenario>>();
        if (scenarios != null)
            Loggers.LogAs.Debug(logger, "Scenarios\n" + string.Join("\n", scenarios.Select(scenario => scenario.ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No scenarios found");

        var accounts = host.Services.GetRequiredService<IReadOnlyList<Account>>();
        if (accounts != null)
            Loggers.LogAs.Debug(logger, "Accounts\n" + string.Join("\n", accounts.Select(account => account .ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No accounts found");

        var executes = host.Services.GetRequiredService<IReadOnlyList<Models.InPut.Execute>>();
        if (executes != null)
            Loggers.LogAs.Debug(logger, "Executes\n" + string.Join("\n", executes.Select(execute => execute.ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No executes found");

        var constants = host.Services.GetRequiredService<IReadOnlyDictionary<string, string>>();
        if (constants != null)
            Loggers.LogAs.Debug(logger, "Constans\n" + string.Join("\n", constants.Select(constant => constant.ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No constants found");

        return host;
    }
    public static IHost ValidatePlaywright(this IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<Runner>>();
        Loggers.LogAs.Debug(logger, "Validating Playwright...");
        try
        {
            using var playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
            Loggers.LogAs.Debug(logger, "Playwright is working correctly.");
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(logger, "Playwright validation failed.", ex);
            throw;
        }
        return host;
    }
}
