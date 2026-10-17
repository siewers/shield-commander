using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services;
using ShieldCommander.Core.Services.Commands;

namespace ShieldCommander.UI.ViewModels;

public sealed partial class InstallViewModel : ViewModelBase
{
    private readonly IAdbExecutor _adb;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isProgressIndeterminate;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _selectedApkPath = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    public InstallViewModel(IAdbExecutor adb)
    {
        _adb = adb;
    }

    public ObservableCollection<string> ApkQueue { get; } = [];

    public bool DidInstall { get; private set; }

    public void ResetDidInstall() => DidInstall = false;

    public void AddApkFiles(IEnumerable<string> filePaths)
    {
        foreach (var path in filePaths)
        {
            if (path.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) && !ApkQueue.Contains(path))
            {
                ApkQueue.Add(path);
            }
        }
    }

    [RelayCommand]
    private void RemoveApk(string path)
    {
        ApkQueue.Remove(path);
    }

    [RelayCommand]
    private void ClearQueue()
    {
        ApkQueue.Clear();
        StatusText = string.Empty;
    }

    // IncludeCancelCommand generates InstallAllCancelCommand, which cancels the token passed in here
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task InstallAllAsync(CancellationToken ct)
    {
        if (ApkQueue.Count == 0)
        {
            StatusText = "No APKs to install";
            return;
        }

        IsBusy = true;
        var total = ApkQueue.Count;
        var succeeded = 0;

        try
        {
            for (var i = 0; i < ApkQueue.Count; i++)
            {
                var apk = ApkQueue[i];
                var fileName = Path.GetFileName(apk);
                var label = $"{fileName} ({i + 1}/{total})";
                var start = new InstallProgress(InstallPhase.Transferring, 0);
                ShowProgress(label, start);

                var progress = new Progress<InstallProgress>(p => ShowProgress(label, p));
                var install = new InstallPackageCommand(apk, progress);
                var result = await _adb.ExecuteAsync(install, ct);
                if (result.Success)
                {
                    succeeded++;
                }
                else
                {
                    StatusText = $"Failed: {fileName} - {(string.IsNullOrEmpty(result.Error) ? result.Output : result.Error)}";
                }
            }

            StatusText = $"Done: {succeeded}/{total} installed successfully";
        }
        catch (OperationCanceledException)
        {
            StatusText = $"Cancelled: {succeeded}/{total} installed";
        }

        if (succeeded > 0)
        {
            DidInstall = true;
        }

        if (succeeded == total)
        {
            ApkQueue.Clear();
        }

        IsBusy = false;
        IsProgressIndeterminate = false;
    }

    private void ShowProgress(string label, InstallProgress progress)
    {
        // The package manager gives no progress while installing, so the bar only moves during the transfer
        IsProgressIndeterminate = progress.Phase == InstallPhase.Installing;
        ProgressPercent = progress.Percent ?? 0;
        StatusText = progress.Phase == InstallPhase.Installing
            ? $"Installing {label}..."
            : $"Transferring {label}... {progress.Percent}%";
    }
}
