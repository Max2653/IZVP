namespace PR12_DataBinding;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var form = new MainForm();

        // dotnet run -- --screenshots  → зберігає 3 знімки екрана для звіту
        if (args.Contains("--screenshots"))
            ScreenshotDemo.Attach(form, Path.Combine(AppContext.BaseDirectory, "screenshots"));

        Application.Run(form);
    }
}
