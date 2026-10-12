# Shield Commander

<p align="center">
  <img src="src/ShieldCommander.UI/Assets/app-icon.png" width="128" alt="Shield Commander icon" />
</p>

<p align="center">
  <strong>Monitoring and app management for your NVIDIA Shield TV</strong>
</p>

Shield Commander connects to your NVIDIA Shield TV over Wi-Fi and gives you full visibility into what's happening on your device — real-time performance charts, running processes, and complete control over installed apps.

## Features

**Activity Monitor** — Live charts for CPU, memory, disk I/O, network, and thermals with configurable refresh rates.

**Processes** — See every running process sorted by CPU usage, updated in real time. Right-click or double-click any process to view detailed info from `/proc` and `dumpsys package`.

**App Management** — Browse installed apps, uninstall what you don't need, and install APKs with drag-and-drop. Double-click any app for full package details including version, SDK levels, install dates, and size.

**System Info** — Device name, model, Android version, IP address, uptime, RAM, storage, and more at a glance.

**Device Discovery** — Automatically finds Shield devices on your network. Saves your devices for quick reconnection, with optional auto-connect on startup.

## Screenshots

<p>
  <a href="assets/screenshots/cpu.jpg"><img src="assets/screenshots/cpu.jpg" width="400" alt="CPU" /></a>
  <a href="assets/screenshots/apps.jpg"><img src="assets/screenshots/apps.jpg" width="400" alt="Apps" /></a>
  <a href="assets/screenshots/thermals.jpg"><img src="assets/screenshots/thermals.jpg" width="400" alt="Thermals" /></a>
  <a href="assets/screenshots/processes.jpg"><img src="assets/screenshots/processes.jpg" width="400" alt="Processes" /></a>
</p>

## Installation

Download the [latest release](https://github.com/siewers/shield-commander/releases/latest) for your platform:

| Platform | File |
| --- | --- |
| macOS (Apple Silicon) | `ShieldCommander-macos-arm64.dmg` |
| macOS (Intel) | `ShieldCommander-macos-x64.dmg` |
| Windows | `ShieldCommander-windows-x64.zip` |
| Linux | `ShieldCommander-linux-x64.tar.gz` or `ShieldCommander-linux-arm64.tar.gz` |

Shield Commander is in beta while it heads for 1.0. Versions follow [semantic versioning](https://semver.org/), and older and prerelease builds are on the [Releases page](https://github.com/siewers/shield-commander/releases).

### macOS

Open the DMG and drag Shield Commander to Applications.

The app is not signed with an Apple Developer certificate, so macOS blocks it the first time you open it. To allow it:

1. Open Terminal
2. Run: `xattr -cr "/Applications/Shield Commander.app"`
3. Open the app normally

The first time you connect, macOS asks whether Shield Commander may find and connect to devices on your local network. Click **Allow**; without it the app can't reach your Shield. If you clicked **Don't Allow**, turn it on under **System Settings → Privacy & Security → Local Network**.

### Windows

Extract the zip and run `ShieldCommander.exe`. The app is not code signed, so Windows SmartScreen may show "Windows protected your PC" the first time. Click **More info**, then **Run anyway**.

### Linux

Extract the archive and run `./ShieldCommander`.

## Requirements

- macOS, Windows, or Linux
- [ADB (Android Debug Bridge)](https://developer.android.com/tools/adb) installed
- An NVIDIA Shield TV on the same network, with **Network debugging** enabled

### Installing ADB

Shield Commander requires ADB (Android Debug Bridge) to communicate with your Shield TV. Install it for your platform:

**Windows** — Install via [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/):
```
winget install Google.PlatformTools
```

**macOS** — Install via [Homebrew](https://brew.sh/):
```
brew install android-platform-tools
```

**Linux (Debian/Ubuntu):**
```
sudo apt install android-tools-adb
```

You can also [download the SDK Platform Tools](https://developer.android.com/tools/releases/platform-tools#downloads) directly from Google and extract them anywhere.

Shield Commander auto-detects ADB from common install locations. If it can't find it, click **Configure ADB** in the connection dialog, or open **Settings → ADB Configuration…**, and set the path manually.

### Enabling network debugging on the Shield

1. On the Shield, open **Settings → Device Preferences → About**.
2. Select **Build** seven times until you see that you're now a developer.
3. Go back to **Device Preferences → Developer options** and turn on **Network debugging**.

The first time you connect, the Shield asks you to allow USB debugging from your computer. Check **Always allow from this computer** and select **OK**. Shield Commander waits for you to accept.

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (the exact version is pinned in `global.json`).

```
dotnet run --project src/ShieldCommander.UI
```

Run the tests with:

```
dotnet run --project src/ShieldCommander.Core.Tests
```

## Built With

- [Avalonia UI](https://avaloniaui.net/) with [Fluent Avalonia](https://github.com/amwx/FluentAvalonia)
- [LiveCharts2](https://livecharts.dev/) for real-time charting
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) for MVVM

## License

[MIT](LICENSE)
