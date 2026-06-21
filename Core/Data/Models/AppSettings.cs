using static HtmlGamer.Core.Data.Models.Files;

namespace HtmlGamer.Core.Data.Models;
public sealed class Folders
{
    public string Data { get; set; } = nameof(Data);
    public string Config { get; set; } = nameof(Config);
    public string ToParse { get; set; } = nameof(ToParse);
    public string Parsed { get; set; } = nameof(Parsed);
    public string Analyzed { get; set; } = nameof(Analyzed);
    public string WireGuard { get; set; } = nameof(WireGuard);
    public string Vpn { get; set; } = Path.Combine(Constants.Folders.Config, Constants.Folders.Vpn);
}
public class Files
{
    public class Config
    {
        public string Accounts { get; set; } = $"{Constants.Files.Accounts}.json";
        public string Scenarios { get; set; } = $"{Constants.Files.Scenarios}.json";
        public string Execute { get; set; } = $"{Constants.Files.Execute}.json";
    }
}
public sealed class AppSettings
{
    public bool AutoClose { get; set; } = true;
    public Folders Folders { get; set; } = new();
    public Generation Generation { get; set; } = new();
    public Config Config { get; set; } = new();
}
public sealed class Generation
{
    public bool IgnoreGuilds { get; set; } = true;
    public int PageStart { get; set; } = 1;
    public int PageEnd { get; set; } = 5;
}