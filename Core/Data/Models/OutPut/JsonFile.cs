namespace HtmlGamer.Core.Data.Models.OutPut;

internal sealed class JsonFile
{
    internal DateTime Created { get; set; }
    internal string Owner { get; set; } = string.Empty;
    internal List<MemberGuildType> Content { get; set; } = [];
}
