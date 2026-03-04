using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using ShieldCommander.Core.Services;

namespace ShieldCommander.UI.Dialogs;

internal static class AdbConfigDialog
{
    public static async Task ShowAsync(IAdbConfigService adbConfig)
    {
        var isAvailable = adbConfig.IsAdbAvailable(null);
        var currentPath = adbConfig.CurrentPath ?? adbConfig.ResolveAdbPath();

        var statusText = new TextBlock
        {
            Text = isAvailable
                ? $"ADB found at {currentPath}"
                : "ADB not found",
            Foreground = isAvailable
                ? new SolidColorBrush(Color.Parse("#76B900"))
                : new SolidColorBrush(Color.Parse("#FF6B6B")),
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 12),
        };

        var instructionsHeader = new TextBlock
        {
            Text = "Install via terminal:",
            Foreground = Brushes.Gray,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var instructionsText = new TextBox
        {
            Text = adbConfig.GetInstallInstructions(),
            IsReadOnly = true,
            FontFamily = new FontFamily("Cascadia Code,Consolas,Menlo,monospace"),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 16),
        };

        var pathLabel = new TextBlock
        {
            Text = "Or specify the path to ADB manually:",
            Foreground = Brushes.Gray,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var pathBox = new TextBox
        {
            Text = adbConfig.CurrentPath ?? string.Empty,
            Watermark = adbConfig.ResolveAdbPath(),
            Padding = new Thickness(8, 4),
        };

        var browseButton = new Button
        {
            Content = "Browse",
            Padding = new Thickness(8, 4),
        };

        var pathRow = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"),
            ColumnSpacing = 4,
        };

        Grid.SetColumn(pathBox, 0);
        Grid.SetColumn(browseButton, 1);
        pathRow.Children.Add(pathBox);
        pathRow.Children.Add(browseButton);

        var panel = new StackPanel
        {
            Spacing = 0,
            MinWidth = 400,
            Children =
            {
                statusText,
                instructionsHeader,
                instructionsText,
                pathLabel,
                pathRow,
            },
        };

        var dialog = new ContentDialog
        {
            Title = "ADB Configuration",
            Content = panel,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
        };

        browseButton.Click += async (_, _) =>
        {
            var topLevel = TopLevel.GetTopLevel(dialog);
            if (topLevel is null)
                return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select ADB executable",
                AllowMultiple = false,
            });

            if (files.Count > 0)
            {
                var selectedPath = files[0].Path.LocalPath;
                pathBox.Text = selectedPath;
                adbConfig.SetAdbPath(selectedPath);
                UpdateStatus(statusText, adbConfig);
            }
        };

        pathBox.LostFocus += (_, _) =>
        {
            var path = string.IsNullOrWhiteSpace(pathBox.Text) ? null : pathBox.Text;
            adbConfig.SetAdbPath(path);
            UpdateStatus(statusText, adbConfig);
        };

        await dialog.ShowAsync();
    }

    private static void UpdateStatus(TextBlock statusText, IAdbConfigService adbConfig)
    {
        var isAvailable = adbConfig.IsAdbAvailable(null);
        var currentPath = adbConfig.CurrentPath ?? adbConfig.ResolveAdbPath();
        statusText.Text = isAvailable
            ? $"ADB found at {currentPath}"
            : "ADB not found";
        statusText.Foreground = isAvailable
            ? new SolidColorBrush(Color.Parse("#76B900"))
            : new SolidColorBrush(Color.Parse("#FF6B6B"));
    }
}
