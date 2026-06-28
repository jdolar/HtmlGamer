using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Interfaces;
using HtmlGamer.Core.Data.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.ServiceProcess;
namespace HtmlGamer.Core.Vpn.Clients;
public sealed class WireGuard(ILogger<WireGuard> logger, AppSettings settings) : IVpnClient
{
    private readonly HttpClient _httpClient = new();
    private readonly string _checkIpUrl = "https://api.ipify.org";
    public Task EnsureInstalledAsync(CancellationToken cancellationToken = default)
    {
        string executablePath = GetExecutablePath();
        if (!File.Exists(GetExecutablePath()))
        {
            Loggers.LogAs.Error(logger, "WireGuard is not installed. ExecutablePath: ", executablePath);
            return Task.CompletedTask;
        }
        Loggers.LogAs.Debug(logger, "WireGuard is installed. ExecutablePath: ", executablePath);
        return Task.CompletedTask;
    }
    public async Task ConnectAsync(string tunnelName, CancellationToken cancellationToken = default)
    {
        string ip = await GetIp(cancellationToken);
        await Connect(tunnelName, cancellationToken);
        await EnsureIpChange(ip);
        await EnsureTunnelRunning(tunnelName);
    }
    public async Task DisconnectAsync(string tunnelName, CancellationToken cancellationToken = default)
    {
        Loggers.LogAs.Debug(logger, "Disconnecting tunnel: ", tunnelName);
        await ExecuteAsync($"/uninstalltunnelservice \"{tunnelName}\"", cancellationToken);
        Loggers.LogAs.Debug(logger, "Disconnected tunnel: ", tunnelName);
    }
    public async Task<string> GetIp(CancellationToken cancellationToken = default)
    {
        try
        {
            string ip = await _httpClient.GetStringAsync(_checkIpUrl);
            return ip.Trim();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
    public async Task ClearTunnels(CancellationToken cancellationToken = default)
    {
        Loggers.LogAs.Debug(logger, "Clearing all WireGuard tunnels.");
        var services = ServiceController.GetServices();
        foreach (var service in services)
        {
            if (!service.ServiceName.StartsWith("WireGuardTunnel$"))
                continue;

            Loggers.LogAs.Debug(logger, "Uninstalling tunnel service: ", service.ServiceName);
            await ExecuteAsync($"/uninstalltunnelservice {service.ServiceName["WireGuardTunnel$".Length..]}", cancellationToken);
            Loggers.LogAs.Debug(logger, "Uninstalled tunnel service: ", service.ServiceName);
        }
    }
    private async Task Connect(string tunnelName, CancellationToken cancellationToken = default)
    {
        string configPath = Path.Combine(Environment.CurrentDirectory, Constants.Folders.Config, Constants.Folders.Vpn, $"{tunnelName}.conf");
        Loggers.LogAs.Debug(logger, "Connecting to VPN tunnel: ", tunnelName);
        await ExecuteAsync($"/installtunnelservice \"{configPath}\"", cancellationToken);
        Loggers.LogAs.Debug(logger, "Connected to VPN tunnel: ", tunnelName);
    }
    private async Task EnsureIpChange(string previousIp, int timeoutMs = 15000)
    {
        Loggers.LogAs.Debug(logger, "Ensuring IP change. Previous IP: ", previousIp);

        var start = DateTime.UtcNow;
        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            await Task.Delay(500);
            string ip = await GetIp();
            if (ip != previousIp && !string.IsNullOrEmpty(ip))
            {
                Loggers.LogAs.Debug(logger, "IP changed successfully. New IP: ", ip);
                return;
            }
        }

        Loggers.LogAs.Error(logger, "IP did not change within the timeout period.", $"Previous IP: {previousIp}, Timeout: {timeoutMs}ms");
    }
    private async Task EnsureTunnelRunning(string tunnelName, CancellationToken cancellationToken = default)
    {
        string serviceName = $"WireGuardTunnel${tunnelName}";
        Loggers.LogAs.Debug(logger, "Ensuring WireGuard tunnel service is running: ", serviceName);
        var service = ServiceController
            .GetServices()
            .FirstOrDefault(s =>
                s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (service == null)
        {
            Loggers.LogAs.Error(logger, $"WireGuard service not found: {serviceName}");
            return;
        }

        Loggers.LogAs.Debug(logger, "Refreshing service: ", serviceName);
        service.Refresh();

        try
        {
            Loggers.LogAs.Debug(logger, "Current service status: ", service.Status.ToString());
            switch (service.Status)
            {
                case ServiceControllerStatus.Running:
                    return;

                case ServiceControllerStatus.StartPending:
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    return;

                case ServiceControllerStatus.Stopped:
                    Loggers.LogAs.Debug(logger, "Starting service: ", serviceName);
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    return;

                case ServiceControllerStatus.StopPending:
                    Loggers.LogAs.Debug(logger, "Service is stopping. Waiting for it to stop: ", serviceName);
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    Loggers.LogAs.Debug(logger, "Starting service: ", serviceName);
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    return;

                default:
                    Loggers.LogAs.Debug(logger, "Refreshing service: ", serviceName);
                    service.Refresh();
                    Loggers.LogAs.Debug(logger, "Starting service: ", serviceName);
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    return;
            }
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            Loggers.LogAs.Error(logger, $"Failed to start WireGuard tunnel: {serviceName}. Are you running as Administrator?", ex.Message);
        }
    }
    private string GetExecutablePath()
    {
        return Path.Combine(settings.Folders.WireGuard, "wireguard.exe");
    }
    private async Task ExecuteAsync(string arguments, CancellationToken cancellationToken)
    {
        var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = GetExecutablePath(),
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas"
            });

        Loggers.LogAs.Debug(logger, "Executing Process with arguments:", process!.ToString());

        if (process is null)
        {
            Loggers.LogAs.Error(logger, "Failed to start WireGuard process.", $"Arguments: {arguments}");
        }

        Loggers.LogAs.Debug(logger, "Executing WireGuard with arguments:", arguments);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            Loggers.LogAs.Error(logger, $"WireGuard exited with code {process.ExitCode}.", $"Arguments: {arguments}");
        }
    }
}