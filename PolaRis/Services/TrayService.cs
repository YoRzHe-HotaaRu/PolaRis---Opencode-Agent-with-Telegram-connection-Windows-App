using PolaRis.Models;

namespace PolaRis.Services;

public class TrayService
{
    public event EventHandler? ShowWindowRequested;
    public event EventHandler? StartAllRequested;
    public event EventHandler? StopAllRequested;
    public event EventHandler? ExitRequested;

    public void OnShowWindow() => ShowWindowRequested?.Invoke(this, EventArgs.Empty);
    public void OnStartAll() => StartAllRequested?.Invoke(this, EventArgs.Empty);
    public void OnStopAll() => StopAllRequested?.Invoke(this, EventArgs.Empty);
    public void OnExit() => ExitRequested?.Invoke(this, EventArgs.Empty);

    public string GetTooltip(ServiceInfo opencodeInfo, ServiceInfo telegramInfo)
    {
        return $"PolaRis\nOpencode: {opencodeInfo.StatusText}\nTelegram: {telegramInfo.StatusText}";
    }
}
