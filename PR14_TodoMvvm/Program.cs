using TodoMvvm.ViewModels;
using TodoMvvm.Views;

namespace TodoMvvm;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var viewModel = new TaskListViewModel();
        var view = new MainForm(viewModel);

        // PR14_TodoMvvm.exe --check → виконує 4 обов'язкові перевірки, пише журнал і знімки вікна
        if (args.Contains("--check"))
            CheckScenario.Attach(view, viewModel, Path.Combine(AppContext.BaseDirectory, "check"));

        Application.Run(view);
    }
}
