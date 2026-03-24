using System.Windows;

namespace PolaRis;

public partial class App : Application
{
    public static bool StartMinimized { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Contains("--minimized"))
            StartMinimized = true;

        base.OnStartup(e);
    }
}
