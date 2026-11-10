using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager;

/// <summary>
/// Головне вікно. Обробники подій лише передають дії користувача в TaskList
/// і показують результат; список і лічильник оновлюються через прив'язки.
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
    }

    private void AddButton_Click(object? sender, RoutedEventArgs e) => AddFromInput();

    private void TitleBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AddFromInput();
    }

    private void DeleteButton_Click(object? sender, RoutedEventArgs e) => DeleteSelected();

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

    internal void DeleteSelected()
    {
        if (TasksList.SelectedItem is TaskItem selected)
        {
            _tasks.Remove(selected);
            ShowError(null);
        }
        else
        {
            ShowError("Виберіть завдання у списку, щоб його видалити.");
        }
    }

    private void ShowError(string? message)
    {
        ErrorText.Text = message;
        ErrorText.IsVisible = message is not null;
    }

    internal TextBox Input => TitleBox;
    internal ListBox List => TasksList;
}
