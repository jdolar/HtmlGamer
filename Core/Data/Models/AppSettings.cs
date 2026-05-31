namespace HtmlGamer.Core.Data.Models;
public sealed class Folders
{
    public string Data { get; set; } = nameof(Data);
    public string ToParse { get; set; } = nameof(ToParse);
    public string Parsed { get; set; } = nameof(Parsed);
    public string Analyzed { get; set; } = nameof(Analyzed);
}
public sealed class AppSettings
{
    public bool AutoClose { get; set; } = true;
    public Folders Folders { get; set; } = new();
    public Generation Generation { get; set; } = new();
    public Execute Execute { get; set; } = new();
}
public sealed class Generation
{
    public bool IgnoreGuilds { get; set; } = true;
    public int PageStart { get; set; } = 1;
    public int PageEnd { get; set; } = 5;
}
public sealed class Execute
{
    public bool Scrapper { get; set; } = false;
    public bool Parser { get; set; } = false;
    public bool Analyzer { get; set; } = false;
}