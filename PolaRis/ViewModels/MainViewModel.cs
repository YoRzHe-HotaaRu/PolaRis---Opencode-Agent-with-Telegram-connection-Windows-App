using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PolaRis.Models;
using PolaRis.Services;

namespace PolaRis.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ProcessService _opencodeService;
    private readonly ProcessService _telegramService;
    private readonly AutoStartService _autoStartService;
    private readonly System.Threading.Timer _uptimeTimer;

    [ObservableProperty]
    private ServiceInfo _opencodeInfo = new() { Name = "Opencode Server", Port = 4096 };

    [ObservableProperty]
    private ServiceInfo _telegramInfo = new() { Name = "Telegram Bot" };

    [ObservableProperty]
    private ObservableCollection<string> _opencodeLogs = new();

    [ObservableProperty]
    private ObservableCollection<string> _telegramLogs = new();

    [ObservableProperty]
    private int _opencodePort = 4096;

    [ObservableProperty]
    private bool _autoStartEnabled;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private string _telegramBotName = "Not connected";

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _currentView = "Dashboard";

    public TrayService TrayService { get; }

    public MainViewModel()
    {
        _opencodeService = new ProcessService("Opencode", "powershell.exe", "-NoProfile -Command \"opencode serve --port 4096\"");
        _telegramService = new ProcessService("Telegram", "powershell.exe", "-NoProfile -Command \"opencode-telegram start\"");
        _autoStartService = new AutoStartService();
        TrayService = new TrayService();

        _autoStartEnabled = _autoStartService.IsEnabled;

        _opencodeService.OutputReceived += (s, msg) => AppendLog(OpencodeLogs, msg);
        _opencodeService.ErrorReceived += (s, msg) => AppendLog(OpencodeLogs, $"[ERR] {msg}");
        _opencodeService.StatusChanged += (s, status) =>
        {
            OpencodeInfo.Status = status;
            OpencodeInfo.StartTime = _opencodeService.StartTime;
            OnPropertyChanged(nameof(OpencodeInfo));
        };

        _telegramService.OutputReceived += (s, msg) =>
        {
            AppendLog(TelegramLogs, msg);
            TryParseBotName(msg);
        };
        _telegramService.ErrorReceived += (s, msg) => AppendLog(TelegramLogs, $"[ERR] {msg}");
        _telegramService.StatusChanged += (s, status) =>
        {
            TelegramInfo.Status = status;
            TelegramInfo.StartTime = _telegramService.StartTime;
            OnPropertyChanged(nameof(TelegramInfo));
        };

        TrayService.StartAllRequested += async (s, e) => await StartAllAsync();
        TrayService.StopAllRequested += async (s, e) => await StopAllAsync();

        _uptimeTimer = new System.Threading.Timer(_ => RefreshUptime(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void AppendLog(ObservableCollection<string> logs, string message)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            logs.Add($"[{timestamp}] {message}");

            while (logs.Count > 500)
                logs.RemoveAt(0);
        });
    }

    private void TryParseBotName(string message)
    {
        // Common patterns: "Bot name: @MyBot", "Bot: @MyBot", "Connected as @MyBot", "Running as @MyBot"
        string[] patterns = ["Bot name:", "Bot:", "Connected as", "Running as", "Bot username:", "Username:"];
        foreach (var pattern in patterns)
        {
            var idx = message.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var name = message[(idx + pattern.Length)..].Trim().TrimStart('@').Trim();
                if (!string.IsNullOrEmpty(name) && name != TelegramBotName)
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        TelegramBotName = name;
                    });
                }
                return;
            }
        }

        // Also check if message contains "@username" pattern after "started" or "connected"
        if (message.Contains("started", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("connected", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("listening", StringComparison.OrdinalIgnoreCase))
        {
            var atIdx = message.IndexOf('@');
            if (atIdx >= 0)
            {
                var rest = message[(atIdx + 1)..];
                var spaceIdx = rest.IndexOf(' ');
                var name = spaceIdx >= 0 ? rest[..spaceIdx].Trim() : rest.Trim();
                if (!string.IsNullOrEmpty(name) && name != TelegramBotName)
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        TelegramBotName = name;
                    });
                }
            }
        }
    }

    private void RefreshUptime()
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            OnPropertyChanged(nameof(OpencodeInfo));
            OnPropertyChanged(nameof(TelegramInfo));
        });
    }

    [RelayCommand]
    private async Task StartOpencodeAsync()
    {
        _opencodeService.Dispose();
        var newService = new ProcessService("Opencode", "powershell.exe", $"-NoProfile -Command \"opencode serve --port {OpencodePort}\"");
        CopyServiceEvents(newService, _opencodeService, OpencodeLogs, OpencodeInfo);
        await newService.StartAsync();
    }

    [RelayCommand]
    private async Task StopOpencodeAsync()
    {
        await _opencodeService.StopAsync();
    }

    [RelayCommand]
    private async Task RestartOpencodeAsync()
    {
        await _opencodeService.RestartAsync();
    }

    [RelayCommand]
    private async Task StartTelegramAsync()
    {
        await _telegramService.StartAsync();
    }

    [RelayCommand]
    private async Task StopTelegramAsync()
    {
        await _telegramService.StopAsync();
    }

    [RelayCommand]
    private async Task RestartTelegramAsync()
    {
        await _telegramService.RestartAsync();
    }

    [RelayCommand]
    private async Task StartAllAsync()
    {
        await _opencodeService.StartAsync();
        await Task.Delay(500);
        await _telegramService.StartAsync();
    }

    [RelayCommand]
    private async Task StopAllAsync()
    {
        await _telegramService.StopAsync();
        await _opencodeService.StopAsync();
    }

    [RelayCommand]
    private void ClearOpencodeLogs()
    {
        OpencodeLogs.Clear();
    }

    [RelayCommand]
    private void ClearTelegramLogs()
    {
        TelegramLogs.Clear();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentView = "Dashboard";
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentView = "Settings";
    }

    partial void OnAutoStartEnabledChanged(bool value)
    {
        if (value)
            _autoStartService.Enable();
        else
            _autoStartService.Disable();
    }

    private void CopyServiceEvents(ProcessService newService, ProcessService oldService,
        ObservableCollection<string> logs, ServiceInfo info)
    {
        newService.OutputReceived += (s, msg) => AppendLog(logs, msg);
        newService.ErrorReceived += (s, msg) => AppendLog(logs, $"[ERR] {msg}");
        newService.StatusChanged += (s, status) =>
        {
            info.Status = status;
            info.StartTime = newService.StartTime;
            OnPropertyChanged(nameof(info));
        };
    }

    public async Task ShutdownAsync()
    {
        _uptimeTimer.Dispose();
        await _telegramService.StopAsync();
        await _opencodeService.StopAsync();
        _opencodeService.Dispose();
        _telegramService.Dispose();
    }
}
