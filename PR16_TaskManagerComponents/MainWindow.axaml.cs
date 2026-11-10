using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TaskManager.Controls;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager;

/// <summary>
/// Головне вікно. Список рендериться компонентами TaskItemControl;
/// подія DeleteRequested від будь-якої картки обробляється тут один раз.
/// </summary>
public partial class MainWindow : Window
{
    private readonly TaskList _tasks;

    public MainWindow() : this(new TaskList()) { }

    public MainWindow(TaskList tasks)
    {
        _tasks = tasks;
        InitializeComponent();
        DataContext = _tasks;

        // підписка на подію компонента: спрацьовує для будь-якої картки у вікні
        AddHandler(TaskItemControl.DeleteRequestedEvent, OnTaskDeleteRequested);
    }

    private void AddButton_Click(object? sender, RoutedEventArgs e) => AddFromInput();

    private void TitleBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AddFromInput();
    }

    /// <summary>Картка повідомила про видалення — DataContext картки і є відповідне завдання.</summary>
    private void OnTaskDeleteRequested(object? sender, RoutedEventArgs e)
    {
        if (e.Source is TaskItemControl { DataContext: TaskItem task })
        {
            _tasks.Remove(task);
            ShowError(null);
        }
    }

    internal void AddFromInput()
    {
        if (_tasks.TryAdd(TitleBox.Text, out string error))
        {
            TitleBox.Text = string.Empty;
            ShowError(null);
        }
        else
        {
            ShowError(error);
        }
        TitleBox.Focus();
    }

    private void ShowError(string? message)
    {
        ErrorText.Text = message;
        ErrorText.IsVisible = message is not null;
    }

    internal TextBox Input => TitleBox;
}
