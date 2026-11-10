using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TaskManager.Services;

namespace TaskManager;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var tasks = new TaskList();
            var window = new MainWindow(tasks);

            // TaskManager.exe --screenshot → зберігає знімки працюючого інтерфейсу
            if (desktop.Args?.Contains("--screenshot") == true)
                ScreenshotDemo.Attach(window, tasks, Path.Combine(AppContext.BaseDirectory, "screenshots"));

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
