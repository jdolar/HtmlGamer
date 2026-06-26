using HtmlGamer.Core.Data.Interfaces;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
using HtmlGamer.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Collections;
using System.Reflection;
using System.Text;
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
    public static string ToTextString(this Account account)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Name: {account.UserName}");
        builder.AppendLine($"eMail: {account.Email}");
        builder.AppendLine($"PassWord: {Constants.Unsorted.PasswordMask}");
        builder.AppendLine($"VpnProfile: {account.Vpn}");

        return builder.ToString();
    }
    public static string ToTextString(this Execute execute)
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
    public static string ToTextString(this Models.InPut.Scenario input)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Name: {input.Name}");
        builder.AppendLine("Steps:");

        foreach (var step in input.Steps)
            builder.AppendLine($"  - {step}");

        if (input.Properties is null || input.Properties.Count == 0)
            return builder.ToString().TrimEnd();

        foreach (var (category, properties) in input.Properties)
        {
            builder.AppendLine($"[{category}]");

            if (properties.Count == 0)
            {
                builder.AppendLine("  <empty>");
                continue;
            }

            foreach (var property in properties)
                builder.AppendLine($"  {property.Name}: {property.Value}");

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }
    internal static string ToTextString(this Dictionary<(Enums.AppSettingsKeys, string), string> fields,
        string? preSpace = Constants.Unsorted.PreFixTab,
        string? tailingSpace = Constants.Unsorted.TailingTab,
        int? spaceBetween = null)
    {
        if (fields.Count == 0)
            return string.Empty;


        //int labelWidth = fields.Max(f => f.Key.Length);
        //int valueWidth = fields.Max(f => f.Value.Length);

        //StringBuilder sb = new();
        //foreach (var (key, value) in fields)
        //{
        //    sb.Append("\n" + preSpace + key.PadRight(labelWidth) + value.PadLeft(valueWidth) + tailingSpace);
        //}
        //return sb.ToString();

        return string.Empty;
    }
    public static Dictionary<(Enums.AppSettingsKeys, string), string> ToDictionary(this AppSettings settings)
    {
        Dictionary<(Enums.AppSettingsKeys, string), string> results = [];
        results.Add((Enums.AppSettingsKeys.Root, nameof(settings.AutoClose)), settings.AutoClose.ToString());

        results.Add((Enums.AppSettingsKeys.Folders, nameof(settings.Folders.Data)), settings.Folders.Data);
        results.Add((Enums.AppSettingsKeys.Folders, nameof(settings.Folders.Analyzed)), settings.Folders.Analyzed);
        results.Add((Enums.AppSettingsKeys.Folders, nameof(settings.Folders.Parsed)), settings.Folders.Parsed);
        results.Add((Enums.AppSettingsKeys.Folders, nameof(settings.Folders.ToParse)), settings.Folders.ToParse);

        return results;
    }  
    public static async Task<IHost> Validate(this IHost host)
    {
        ValidateConfigs(host);
        await ValidatePlaywright(host);
        await ValidateVpnClient(host);
        
        return host;
    }
    public static IHost ValidateConfigs(this IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<Runner>>();
        
        Loggers.LogAs.Debug(logger, "Validating configurations...");

        AppSettings? appSettings = host.Services.GetRequiredService<AppSettings>();
        Dictionary<(Enums.AppSettingsKeys, string), string>? appSettingsFields = appSettings.ToDictionary();
        Loggers.LogAs.Debug(logger, appSettingsFields.ToTextString()!);

        if (appSettings != null)
            Loggers.LogAs.Debug(logger, appSettings.ToTextString()!);
        else
            Loggers.LogAs.Error(logger, "AppSettings is null");

        var scenarios = host.Services.GetRequiredService<IReadOnlyList<Data.Models.InPut.Scenario>>();
        if (scenarios != null)
            Loggers.LogAs.Debug(logger, "Scenarios:\n" + string.Join("\n", scenarios.Select(scenario => scenario.ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No scenarios found");

        var accounts = host.Services.GetRequiredService<IReadOnlyList<Account>>();
        if (accounts != null)
            Loggers.LogAs.Debug(logger, "Accounts:\n" + string.Join("\n", accounts.Select(account => account.ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No accounts found");

        var executes = host.Services.GetRequiredService<IReadOnlyList<Models.InPut.Execute>>();
        if (executes != null)
            Loggers.LogAs.Debug(logger, "Executes:\n" + string.Join("\n", executes.Select(execute => execute.ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No executes found");

        var constants = host.Services.GetRequiredService<IReadOnlyDictionary<string, string>>();
        if (constants != null)
            Loggers.LogAs.Debug(logger, "Constants:\n" + string.Join("\n", constants.Select(constant => constant.ToTextString())));
        else
            Loggers.LogAs.Error(logger, "No constants found");

        var vpnProvider = host.Services.GetRequiredService<IVpnProvider>;
        if (vpnProvider != null)
            Loggers.LogAs.Debug(logger, "VpnProvider: ", vpnProvider.GetType().Name);
        else
            Loggers.LogAs.Error(logger, "No VpnProvider found");

        var vpnClient = host.Services.GetRequiredService<IVpnClient>;
        if (vpnClient != null)
            Loggers.LogAs.Debug(logger, "VpnClient:", vpnClient.GetType().Name);
        else
            Loggers.LogAs.Error(logger, "No VpnClient found");

        return host;
    }
    public static async Task ValidatePlaywright(this IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<Runner>>();
        Loggers.LogAs.Debug(logger, "Validating Playwright...");

        try
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await playwright.Chromium.LaunchAsync();

            Loggers.LogAs.Debug(logger, "Playwright is working correctly.");
        }
        catch (PlaywrightException ex)
            when (ex.Message.Contains("Executable doesn't exist"))
        {
            Loggers.LogAs.Error(logger, "Playwright is not properly installed. Installing now...", ex);
            Program.Main(new[] { "install", "chromium" });
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(logger, "Playwright is not properly installed. Installing now...", ex);
            Program.Main(new[] { "install", "chromium" });
        }
    }
    public static async Task ValidateVpnClient(this IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<Runner>>();      
        Loggers.LogAs.Debug(logger, "Validating VPN client...");

        try
        {
            var client = host.Services.GetRequiredService<IVpnClient>();
            await client.EnsureInstalledAsync();
            Loggers.LogAs.Debug(logger, "VPN client is installed correctly.");
        }
        catch (Exception ex)
        {
            
            Loggers.LogAs.Error(logger, "VPN client is not properly installed. Installing now...", ex);
            Program.Main(new[] { "install", "vpn" });
        }
    }
    public static Runner GetRunner(this IHost host)
    {
        try
        {
            return host.Services.GetRequiredService<Runner>();
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Runner>>();
            logger.LogError(ex, "Failed to get Runner service.");
            throw;
        }
    }
   
}
