namespace HtmlGamer.Core.Data.Interfaces;
public interface IVpnClient
{
    Task EnsureInstalledAsync(CancellationToken cancellationToken = default);
    Task ConnectAsync(string tunnelName, CancellationToken cancellationToken = default);
    Task DisconnectAsync(string tunnelName,CancellationToken cancellationToken = default);
    Task<string> GetIp(CancellationToken cancellationToken = default);
    Task ClearTunnels(CancellationToken cancellationToken = default);
}
