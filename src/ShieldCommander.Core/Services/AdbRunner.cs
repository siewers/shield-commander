using System.Text;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Commands;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Services;

public sealed class AdbRunner : IAdbRunner
{
    private readonly AdbClient _client = new();
    private readonly IAdbPathProvider _pathProvider;
    private readonly AdbServer _server;
    private DeviceData? _device;

    public AdbRunner(IAdbPathProvider pathProvider)
    {
        _pathProvider = pathProvider;
        _server = new AdbServer(_client);
    }

    public event Action? SessionLost;

    public void OpenSession(string serial)
    {
        // Address the device by serial rather than transport id, which changes every time it reconnects
        _device = new DeviceData { Serial = serial, State = DeviceState.Online };
    }

    public void CloseSession()
    {
        _device = null;
    }

    public async Task<string> RunShellAsync(string command, CancellationToken ct = default)
    {
        if (_device is not { } device)
        {
            return string.Empty;
        }

        // The library's shell service runs commands on a terminal, which merges stderr into the output and makes
        // tools like ls print columns. Discard stderr and pipe stdout so the parsers see what a plain adb shell gives.
        // Commands that need stderr redirect it to stdout themselves.
        var wrapped = $"{{ {command}\n}} 2>/dev/null | cat";
        var output = new StringBuilder();

        try
        {
            await _client.ExecuteShellCommandAsync(device, wrapped, line =>
            {
                output.AppendLine(line);
                return true;
            }, ct);

            // Cancellation stops reading without throwing, so the output may be incomplete
            return ct.IsCancellationRequested ? string.Empty : output.ToString().TrimEnd('\r', '\n');
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
        catch
        {
            // Only the first failing command reports the loss, even when several run concurrently
            if (Interlocked.CompareExchange(ref _device, null, device) == device)
            {
                SessionLost?.Invoke();
            }

            return string.Empty;
        }
    }

    public async Task<AdbResult> RunServerAsync(Func<IAdbClient, CancellationToken, Task<string>> action, CancellationToken ct = default)
    {
        try
        {
            await EnsureServerAsync(ct);
            var output = await action(_client, ct);

            // The server reports some failures, such as "failed to connect", as a normal response
            var success = !output.Contains("failed", StringComparison.OrdinalIgnoreCase)
                       && !output.Contains("error", StringComparison.OrdinalIgnoreCase);
            return new AdbResult(success, output.Trim());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new AdbResult(false, "", ex.Message.Trim());
        }
    }

    public Task<AdbResult> RunOnDeviceAsync(Func<IAdbClient, DeviceData, CancellationToken, Task<string>> action, CancellationToken ct = default)
    {
        return _device is { } device
            ? RunServerAsync((client, token) => action(client, device, token), ct)
            : Task.FromResult(new AdbResult(false, "", "No device connected"));
    }

    public async Task<T> QueryServerAsync<T>(Func<IAdbClient, CancellationToken, Task<T>> query, T fallback, CancellationToken ct = default)
    {
        try
        {
            await EnsureServerAsync(ct);
            return await query(_client, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return fallback;
        }
    }

    internal Task<AdbResult> ExecuteAsync(IAdbCommand command, CancellationToken ct) => command.ExecuteAsync(this, ct);

    internal Task<T> ExecuteAsync<T>(IAdbQuery<T> query, CancellationToken ct) => query.ExecuteAsync(this, ct);

    private async Task EnsureServerAsync(CancellationToken ct)
    {
        // Start the server from this process so it inherits the app's local network access on macOS (issue #35)
        if (!(await _server.GetStatusAsync(ct)).IsRunning)
        {
            await _server.StartServerAsync(_pathProvider.CurrentPath, false, ct);
        }
    }
}
