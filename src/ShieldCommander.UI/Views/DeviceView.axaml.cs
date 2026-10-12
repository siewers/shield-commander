using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services;
using ShieldCommander.UI.Dialogs;
using ShieldCommander.UI.ViewModels;

namespace ShieldCommander.UI.Views;

public sealed partial class DeviceView : UserControl
{
    private EventHandler<SelectionChangedEventArgs>? _selectionHandler;

    public DeviceView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_selectionHandler != null)
        {
            IpAutoComplete.SelectionChanged -= _selectionHandler;
        }

        if (DataContext is DeviceViewModel vm)
        {
            vm.ShowAuthorizationDialog = ShowAuthorizationDialogAsync;
            _selectionHandler = (_, args) =>
            {
                if (args.AddedItems is [DeviceSuggestion suggestion])
                {
                    vm.SetPendingDeviceName(suggestion.DisplayName);
                }
            };
            IpAutoComplete.SelectionChanged += _selectionHandler;
            await vm.InitializeAsync();
        }
    }

    private async Task ShowAuthorizationDialogAsync(CancellationToken ct)
    {
        var dialog = new FAContentDialog
        {
            Title = "Waiting for Device",
            Content = "Please check your TV and accept the connection.\n\n" +
                      "If your device is off, turn it on — the connection will be " +
                      "established automatically.\n\n" +
                      "A dialog should appear on your device asking you to allow USB debugging. " +
                      "Select \"Always allow from this computer\" and tap OK.",
            CloseButtonText = "Cancel",
        };

        // Auto-close when authorization is detected (token cancelled by VM)
        ct.Register(() =>
        {
            Dispatcher.UIThread.Post(() => dialog.Hide());
        });

        await dialog.ShowAsync();
    }

    private async void DropdownButton_Click(object? sender, RoutedEventArgs e)
    {
        var autoComplete = this.FindControl<AutoCompleteBox>("IpAutoComplete");
        if (autoComplete is null)
        {
            return;
        }

        autoComplete.Text = string.Empty;
        autoComplete.Focus();

        // Small delay so the focus and text change settle before opening
        await Task.Delay(50);
        autoComplete.IsDropDownOpen = true;
    }

    private async void ConfigureAdbButton_Click(object? sender, RoutedEventArgs e)
    {
        var adbConfig = App.Services.GetRequiredService<IAdbConfigService>();
        await AdbConfigDialog.ShowAsync(adbConfig);

        if (DataContext is DeviceViewModel vm)
        {
            vm.RefreshAdbStatus();
        }
    }
}
