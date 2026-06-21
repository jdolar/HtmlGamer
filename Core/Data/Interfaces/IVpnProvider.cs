namespace HtmlGamer.Core.Data.Interfaces;
public interface IVpnProvider
{
    string Name { get; }
    Task ConnectAsync(string profileName, CancellationToken cancellationToken = default);
    Task DisconnectAsync(string profileName, CancellationToken cancellationToken = default);
}
