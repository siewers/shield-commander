using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services;
using ShieldCommander.Core.Services.Commands;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.UI.ViewModels;

public sealed partial class PackageRow : ObservableObject
{
    [ObservableProperty]
    private long? _codeSize;

    [ObservableProperty]
    private bool _isSizeLoading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionName))]
    private InstalledPackage _package;

    public PackageRow(InstalledPackage package)
    {
        _package = package;
        Update(package);
    }

    public string PackageName => Package.PackageName;

    public string? VersionName => Package.VersionName;

    public void Update(InstalledPackage package)
    {
        Package = package;
        CodeSize = package.CodeSize;
        IsSizeLoading = package.CodeSize is null;
    }
}

public sealed partial class AppsViewModel(IAdbExecutor adb) : ViewModelBase
{
    private static readonly InstalledPackagesQuery PackagesQuery = new();

    private static readonly SemaphoreSlim SizeSemaphore = new(1);

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private PackageRow? _selectedPackage;

    [ObservableProperty]
    private string _statusText = string.Empty;

    public IAdbExecutor Adb { get; } = adb;

    public ObservableCollection<PackageRow> Packages { get; } = [];

    public void Clear()
    {
        Packages.Clear();
        SelectedPackage = null;
        StatusText = string.Empty;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusText = "Loading packages...";

        var packages = await Adb.QueryAsync(PackagesQuery);
        var incoming = packages.ToDictionary(p => p.PackageName);
        var existing = Packages.ToDictionary(r => r.PackageName);

        // Remove packages no longer installed
        for (var i = Packages.Count - 1; i >= 0; i--)
        {
            if (!incoming.ContainsKey(Packages[i].PackageName))
            {
                Packages.RemoveAt(i);
            }
        }

        // Add new packages and update the ones that changed, for example after an upgrade
        var changedRows = new List<PackageRow>();
        foreach (var pkg in packages)
        {
            if (!existing.TryGetValue(pkg.PackageName, out var row))
            {
                row = new PackageRow(pkg);
                Packages.Add(row);
                changedRows.Add(row);
            }
            else if (row.Package != pkg)
            {
                row.Update(pkg);
                changedRows.Add(row);
            }
        }

        StatusText = $"{Packages.Count} packages found";
        IsBusy = false;

        if (changedRows.Count > 0)
        {
            _ = LoadSizesAsync(changedRows);
        }
    }

    private async Task LoadSizesAsync(IEnumerable<PackageRow> rows)
    {
        var tasks = rows.Select(row => LoadSizeForPackageAsync(row));
        await Task.WhenAll(tasks);
    }

    private async Task LoadSizeForPackageAsync(PackageRow row)
    {
        await SizeSemaphore.WaitAsync();
        try
        {
            var query = new PackageInfoQuery(row.PackageName);
            var detailed = await Adb.QueryAsync(query);
            row.CodeSize = detailed.CodeSize;
        }
        catch
        {
            // Size loading is best-effort; skip failures
        }
        finally
        {
            row.IsSizeLoading = false;
            SizeSemaphore.Release();
        }
    }

    [RelayCommand]
    private async Task UninstallAsync(PackageRow row)
    {
        IsBusy = true;
        StatusText = $"Uninstalling {row.PackageName}...";

        var uninstall = new UninstallPackageCommand(row.PackageName);
        var result = await Adb.ExecuteAsync(uninstall);

        if (result.Success)
        {
            Packages.Remove(row);
            StatusText = $"Uninstalled {row.PackageName}";
        }
        else
        {
            StatusText = $"Failed to uninstall: {(string.IsNullOrEmpty(result.Error) ? result.Output : result.Error)}";
        }

        IsBusy = false;
    }
}
