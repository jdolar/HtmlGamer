using HtmlGamer.Core.Data.Interfaces;
namespace HtmlGamer.Core.Vpn.Providers;

public sealed class SurfShark(IVpnClient client) : IVpnProvider
{
    public string Name => "Surfshark";
    public async Task ConnectAsync(string profileName, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(profileName))
        {
            await client.EnsureInstalledAsync(ct);
            await client.ConnectAsync(profileName, ct);
        }
    }
    public Task DisconnectAsync(string profileName, CancellationToken ct = default) => client.DisconnectAsync(profileName, ct);
}
