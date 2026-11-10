using System.ComponentModel;
using System.Runtime.CompilerServices;
using TodoMvvm.Models;

namespace TodoMvvm.ViewModels;

/// <summary>
/// VIEWMODEL — єдине джерело даних для вікна і вся логіка дій:
/// додавання (з перевіркою назви), зміна статусу, видалення.
/// Нічого не знає про конкретні елементи Windows Forms.
/// </summary>
public class TaskListViewModel : INotifyPropertyChanged
{
    private string _newTitle = string.Empty;
    private TaskItem? _selectedTask;
    private string _message = string.Empty;
    private bool _isError;

    public TaskListViewModel()
    {
        AddCommand = new RelayCommand(AddTask);
        ToggleStatusCommand = new RelayCommand(ToggleStatus, () => SelectedTask is not null);
        DeleteCommand = new RelayCommand(DeleteTask, () => SelectedTask is not null);
    }

    /// <summary>Колекція завдань. BindingList повідомляє View про додавання/видалення/зміну елементів.</summary>
    public BindingList<TaskItem> Tasks { get; } = new();

    public RelayCommand AddCommand { get; }
    public RelayCommand ToggleStatusCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public string NewTitle
    {
        get => _newTitle;
        set => SetField(ref _newTitle, value ?? string.Empty);
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (SetField(ref _selectedTask, value))
            {
                ToggleStatusCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Повідомлення для користувача (результат останньої дії або помилка).</summary>
    public string Message
    {
        get => _message;
        private set => SetField(ref _message, value);
    }

    public bool IsError
    {
        get => _isError;
        private set => SetField(ref _isError, value);
    }

    private void AddTask()
    {
        string title = NewTitle.Trim();
        if (title.Length == 0)
        {
            ShowMessage("Назва завдання не може бути порожньою.", isError: true);
            return;
        }

        var task = new TaskItem(title);
        Tasks.Add(task);
        NewTitle = string.Empty;
        SelectedTask = task;
        ShowMessage($"Додано: {task}");
    }

    private void ToggleStatus()
    {
        if (SelectedTask is null) return;
        SelectedTask.IsDone = !SelectedTask.IsDone; // BindingList побачить PropertyChanged і оновить View
        ShowMessage($"Статус змінено: {SelectedTask}");
    }

    private void DeleteTask()
    {
        if (SelectedTask is null) return;
        var removed = SelectedTask;
        Tasks.Remove(removed);
        SelectedTask = Tasks.FirstOrDefault();
        ShowMessage($"Видалено: «{removed.Title}»");
    }

    private void ShowMessage(string text, bool isError = false)
    {
        IsError = isError;
        Message = text;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
