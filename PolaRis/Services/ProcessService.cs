using System.Diagnostics;
using PolaRis.Models;

namespace PolaRis.Services;

public class ProcessService : IDisposable
{
    private Process? _process;
    private readonly string _fileName;
    private readonly string _arguments;
    private readonly string _serviceName;
    private readonly string? _workingDirectory;

    public event EventHandler<string>? OutputReceived;
    public event EventHandler<string>? ErrorReceived;
    public event EventHandler<ServiceStatus>? StatusChanged;

    public ServiceStatus CurrentStatus { get; private set; } = ServiceStatus.Stopped;
    public DateTime? StartTime { get; private set; }
    public string LastError { get; private set; } = string.Empty;

    public ProcessService(string serviceName, string fileName, string arguments, string? workingDirectory = null)
    {
        _serviceName = serviceName;
        _fileName = fileName;
        _arguments = arguments;
        _workingDirectory = workingDirectory;
    }

    public async Task StartAsync()
    {
        if (_process != null && !_process.HasExited)
            return;

        CurrentStatus = ServiceStatus.Starting;
        StatusChanged?.Invoke(this, CurrentStatus);

        try
        {
            _process = new Process();
            _process.StartInfo = new ProcessStartInfo
            {
                FileName = _fileName,
                Arguments = _arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WorkingDirectory = _workingDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            _process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    OutputReceived?.Invoke(this, e.Data);
            };

            _process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    ErrorReceived?.Invoke(this, e.Data);
                    LastError = e.Data;
                }
            };

            _process.Exited += (s, e) =>
            {
                CurrentStatus = ServiceStatus.Stopped;
                StatusChanged?.Invoke(this, CurrentStatus);
            };

            _process.EnableRaisingEvents = true;
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            StartTime = DateTime.Now;
            CurrentStatus = ServiceStatus.Running;
            StatusChanged?.Invoke(this, CurrentStatus);

            OutputReceived?.Invoke(this, $"[{_serviceName}] started successfully");
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            CurrentStatus = ServiceStatus.Error;
            StatusChanged?.Invoke(this, CurrentStatus);
            ErrorReceived?.Invoke(this, $"Failed to start: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_process == null || _process.HasExited)
        {
            CurrentStatus = ServiceStatus.Stopped;
            StatusChanged?.Invoke(this, CurrentStatus);
            return;
        }

        CurrentStatus = ServiceStatus.Stopping;
        StatusChanged?.Invoke(this, CurrentStatus);

        try
        {
            OutputReceived?.Invoke(this, $"[{_serviceName}] stopping...");

            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            ErrorReceived?.Invoke(this, $"Stop error: {ex.Message}");
        }
        finally
        {
            CurrentStatus = ServiceStatus.Stopped;
            StartTime = null;
            StatusChanged?.Invoke(this, CurrentStatus);
            OutputReceived?.Invoke(this, $"[{_serviceName}] stopped");
        }
    }

    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(1000);
        await StartAsync();
    }

    public bool IsRunning => _process != null && !_process.HasExited;

    public void Dispose()
    {
        if (_process != null)
        {
            try
            {
                if (!_process.HasExited)
                    _process.Kill(entireProcessTree: true);
            }
            catch { }
            _process.Dispose();
            _process = null;
        }
    }
}
