using Microsoft.Playwright;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace HtmlGamer.Core.Data;
internal sealed class Constants
{
    private static readonly string _userName = Environment.UserName;
    internal sealed class Browsers
    {
        internal static readonly BrowserTypeLaunchOptions LaunchOptions = new()
        {
            Headless = false
        };
    }
    internal sealed class Json
    {
        internal static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    internal sealed class Folders
    {
        internal const string Vpn = nameof(Vpn);
        internal const string InputData = "ToScrap";
        internal const string OutputData = "Scrapped";
        internal const string Config = nameof(Config);
    }
    internal sealed class Files
    {
        internal const string Accounts = "accounts";
        internal const string Execute = "execute";
        internal const string Scenarios = "scenarios";
        internal const string Encrypt = nameof(Encrypt);
        internal const string Encrypted = nameof(Encrypted);
        internal const string AppSettings = "appsettings";
    }
    internal sealed class Html
    {
        internal const string UrlStart = "<a href=";
        internal const string UrlEnd = "</a><br />";
        internal const string BlankTarget = " target=_blank";
    }
    internal sealed class EncryptKeys
    {
        internal const string MemberSelector = nameof(MemberSelector);
        internal const string GuildSelector = nameof(GuildSelector);
        internal const string MasterPageSelector = nameof(MasterPageSelector);
        internal const string MasterUrl = nameof(MasterUrl);
        internal const string SlaveUrl = nameof(SlaveUrl);
        internal const string GuildsHtml = nameof(GuildsHtml);
    }
    internal sealed class Unsorted
    {
        internal const string PreFixTab = "  ";
        internal const string TailingTab = " ";
        internal const int SpaceBetween = 10;
        internal const string PasswordMask = "**********";
    }
    internal static string GetFileName(string? name = null, string? extension = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = _userName;

        if (string.IsNullOrWhiteSpace(extension))
            return name;

        return name.Contains(extension) ? name : $"{name}.{extension}";
    }
}