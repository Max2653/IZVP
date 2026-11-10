using OrderCore;

namespace OrderApp;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var form = new MainForm(new OrderService());

        // OrderApp.exe --screenshots → знімки GUI для правильних і помилкових даних
        if (args.Contains("--screenshots"))
            ScreenshotDemo.Attach(form, Path.Combine(AppContext.BaseDirectory, "screenshots"));

        Application.Run(form);
    }
}
