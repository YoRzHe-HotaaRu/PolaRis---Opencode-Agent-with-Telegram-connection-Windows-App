# PolaRis

A modern Windows desktop app for managing [Opencode](https://opencode.ai/) server and [Opencode Telegram Bot](https://github.com/grinev/opencode-telegram-bot) from a single interface.

<p align="center">
  <img src="PolaRis/Resources/Icons/logo.svg" alt="PolaRis" width="248">
</p>

## Features

- **Unified Dashboard** — Monitor Opencode server and Telegram bot status in real-time
- **One-click Controls** — Start, stop, and restart services individually or all at once
- **Live Logs** — View real-time logs from both services with auto-scroll
- **System Tray** — Minimize to tray with quick-access context menu
- **Auto Start** — Optionally launch with Windows on startup
- **Dark Theme** — Modern dark UI with gradient accents

## Prerequisites

Before using PolaRis, ensure the following are installed:

| Requirement | Link |
|-------------|------|
| **.NET 8 SDK** | [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) |
| **Opencode** | [opencode.ai](https://opencode.ai/) |
| **Opencode Telegram Bot** | [github.com/grinev/opencode-telegram-bot](https://github.com/grinev/opencode-telegram-bot) |

## Installation

### Pre-built Executable (No Build Required)

Download [PolaRis.exe](release/PolaRis.exe) directly from the `release` folder — no build needed.

### Build from Source

```bash
git clone https://github.com/YoRzHe-HotaaRu/PolaRis---Opencode-Agent-with-Telegram-connection-Windows-App.git
cd PolaRis
dotnet restore
dotnet build --configuration Release
```

The built executable will be at:

```
PolaRis\bin\Release\net8.0-windows\PolaRis.exe
```

## Usage

1. Launch PolaRis
2. Click **Start All** to run both services, or start them individually
3. Use the **Dashboard** to monitor status and view logs
4. Open **Settings** to configure:
   - **Server Port** — Change the Opencode server port (default: 4096)
   - **Auto Start with Windows** — Enable/disable launch on startup
   - **Minimize to Tray** — Close button minimizes to tray instead of exiting

### System Tray

Right-click the tray icon for quick access:
- **Show PolaRis** — Restore the window
- **Start All / Stop All** — Control services
- **Exit** — Fully close the application

## Architecture

```
PolaRis/
├── Models/          # Data models (ServiceStatus, ServiceInfo)
├── Services/        # Process, AutoStart, and Tray services
├── ViewModels/      # MVVM ViewModels (MainViewModel)
├── Views/           # XAML views (Dashboard, Settings)
├── Resources/
│   ├── Icons/       # App logo and icon
│   └── Themes/      # Dark theme styles
├── MainWindow.xaml  # Main window with navigation
└── App.xaml         # Application entry point
```

**Tech Stack:**
- .NET 8.0 / WPF
- MVVM pattern with [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) for system tray

## Configuration

Settings are stored in the application and persisted automatically:
- **OpencodePort** — Port for the Opencode server
- **AutoStartEnabled** — Windows auto-start toggle
- **MinimizeToTray** — Behavior on window close

## License

MIT License — see [LICENSE](LICENSE) for details.
