namespace PolaRis.Models;

public enum ServiceStatus
{
    Stopped,
    Starting,
    Running,
    Error,
    Stopping
}

public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; } = ServiceStatus.Stopped;
    public int? Port { get; set; }
    public DateTime? StartTime { get; set; }
    public string LastError { get; set; } = string.Empty;

    public string Uptime
    {
        get
        {
            if (StartTime == null || Status != ServiceStatus.Running)
                return "--";

            var elapsed = DateTime.Now - StartTime.Value;
            if (elapsed.TotalDays >= 1)
                return $"{(int)elapsed.TotalDays}d {elapsed.Hours}h {elapsed.Minutes}m";
            if (elapsed.TotalHours >= 1)
                return $"{elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s";
            if (elapsed.TotalMinutes >= 1)
                return $"{elapsed.Minutes}m {elapsed.Seconds}s";
            return $"{elapsed.Seconds}s";
        }
    }

    public string StatusColor => Status switch
    {
        ServiceStatus.Running => "#4ADE80",
        ServiceStatus.Starting => "#FBBF24",
        ServiceStatus.Stopping => "#FBBF24",
        ServiceStatus.Error => "#F87171",
        _ => "#6B7280"
    };

    public string StatusText => Status switch
    {
        ServiceStatus.Running => "Running",
        ServiceStatus.Starting => "Starting...",
        ServiceStatus.Stopping => "Stopping...",
        ServiceStatus.Error => "Error",
        _ => "Stopped"
    };
}
