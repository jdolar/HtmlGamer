namespace HtmlGamer.Core.Data.Models.OutPut;

public sealed class JsonFile
{
    public DateTime Created { get; set; }
    public string Owner { get; set; } = string.Empty;
    public List<MemberGuildType> Content { get; set; } = [];
}
