using System.Collections.ObjectModel;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services;

namespace ShieldCommander.UI.ViewModels;

public sealed partial class DeviceViewModel : ViewModelBase
{
    private readonly IDeviceConnectionService _connectionService;

    [ObservableProperty]
    private string _connectedDeviceName = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private bool _isAdbAvailable;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusText = "Not connected";

    public DeviceViewModel(IDeviceConnectionService connectionService)
    {
        _connectionService = connectionService;
        _isAdbAvailable = connectionService.IsAdbAvailable();

        LoadSavedDevices();
        RefreshSuggestions();
    }

    public async Task InitializeAsync()
    {
        await ScanForSuggestionsAsync();
    }

    public void RefreshAdbStatus()
    {
        IsAdbAvailable = _connectionService.IsAdbAvailable();
    }

    public ObservableCollection<ShieldDevice> ConnectedDevices { get; } = [];

    public ObservableCollection<SavedDevice> SavedDevices { get; } = [];

    public ObservableCollection<DeviceSuggestion> DeviceSuggestions { get; } = [];

    /// Raised when the UI should show the "waiting for authorization" dialog.
    /// The func receives a CancellationToken (cancelled when the user clicks Cancel)
    /// and should return only after the dialog is closed.
    public Func<CancellationToken, Task>? ShowAuthorizationDialog { get; set; }

    private void LoadSavedDevices()
    {
        SavedDevices.Clear();
        foreach (var device in _connectionService.GetSavedDevices().OrderByDescending(d => d.LastConnected))
        {
            SavedDevices.Add(device);
        }
    }

    private bool CanConnect() => IPAddress.TryParse(IpAddress, out _);

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        IsBusy = true;
        StatusText = $"Connecting to {IpAddress}...";

        var result = await _connectionService.ConnectAsync(IpAddress);

        if (result.Status == ConnectionStatus.Connected)
        {
            OnConnected(IpAddress, result.DeviceName);
            IsBusy = false;
            return;
        }

        if (result.Status == ConnectionStatus.AwaitingAuthorization && ShowAuthorizationDialog != null)
        {
            using var cts = new CancellationTokenSource();
            var dialogTask = ShowAuthorizationDialog(cts.Token);
            var pollTask = _connectionService.WaitForAuthorizationAsync(IpAddress, cts.Token);

            var completed = await Task.WhenAny(dialogTask, pollTask);

            if (completed == pollTask && await pollTask)
            {
                await cts.CancelAsync();
                await RefreshDevicesAsync();
                var device = ConnectedDevices.FirstOrDefault(d => d.IpAddress.StartsWith(IpAddress));
                OnConnected(IpAddress, device?.DeviceName);
            }
            else
            {
                await cts.CancelAsync();
                StatusText = "Connection cancelled";
                IsConnected = false;
                await _connectionService.DisconnectAsync(IpAddress);
            }
        }
        else
        {
            StatusText = "Failed to connect";
        }

        IsBusy = false;
    }

    private void OnConnected(string ipAddress, string? deviceName)
    {
        StatusText = $"Connected to {ipAddress}";
        IsConnected = true;
        ConnectedDeviceName = deviceName ?? "";
        _ = RefreshDevicesAsync();
        LoadSavedDevices();
        RefreshSuggestions();
    }

    [RelayCommand]
    private void ToggleAutoConnect(SavedDevice device)
    {
        _connectionService.SetAutoConnect(device.IpAddress, !device.AutoConnect);
        LoadSavedDevices();
    }

    public async Task<bool> AutoConnectAsync()
    {
        var device = _connectionService.GetAutoConnectDevice();
        if (device == null)
        {
            return false;
        }

        IpAddress = device.IpAddress;
        await ConnectAsync();
        return IsConnected;
    }

    [RelayCommand]
    private async Task ConnectToSavedAsync(SavedDevice device)
    {
        IpAddress = device.IpAddress;
        await ConnectAsync();
    }

    [RelayCommand]
    private void RemoveSavedDevice(SavedDevice device)
    {
        _connectionService.RemoveDevice(device.IpAddress);
        LoadSavedDevices();
    }

    private void RefreshSuggestions()
    {
        DeviceSuggestions.Clear();
        foreach (var suggestion in _connectionService.GetSavedSuggestions())
        {
            DeviceSuggestions.Add(suggestion);
        }
    }

    private async Task ScanForSuggestionsAsync()
    {
        IsScanning = true;
        var existingIps = DeviceSuggestions.Select(s => s.IpAddress).ToHashSet();
        var discovered = await _connectionService.ScanForSuggestionsAsync(existingIps);
        foreach (var suggestion in discovered)
        {
            DeviceSuggestions.Add(suggestion);
        }

        IsScanning = false;
    }

    [RelayCommand]
    private async Task RescanAsync()
    {
        RefreshSuggestions();
        await ScanForSuggestionsAsync();
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        IsBusy = true;
        await _connectionService.DisconnectAllAsync();
        IsConnected = false;
        ConnectedDeviceName = string.Empty;
        StatusText = "Disconnected";
        ConnectedDevices.Clear();
        IsBusy = false;
    }

    [RelayCommand]
    private async Task RefreshDevicesAsync()
    {
        var devices = await _connectionService.GetConnectedDevicesAsync();
        ConnectedDevices.Clear();
        foreach (var device in devices)
        {
            ConnectedDevices.Add(device);
        }
    }
}
