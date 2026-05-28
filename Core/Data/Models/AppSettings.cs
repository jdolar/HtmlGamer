namespace HtmlGamer.Core.Data.Models;
public sealed class Folders
{
    public string Data { get; set; } = string.Empty;
    public string InPut { get; set; } = string.Empty;
    public string OutPut { get; set; } = string.Empty;
    public string ToEncrypt { get; set; } = string.Empty;
}
public sealed class AppSettings
{
    public bool AutoClose { get; set; } = true;
    public Folders Folders { get; set; } = new();
    public Generation Generation { get; set; } = new();
}
public sealed class Generation
{
    public bool? IgnoreGuilds { get; set; } = true;
}