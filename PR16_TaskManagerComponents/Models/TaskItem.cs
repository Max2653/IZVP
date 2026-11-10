using System.ComponentModel;

namespace TaskManager.Models;

/// <summary>Одне завдання. Повідомляє інтерфейс про зміну статусу.</summary>
public class TaskItem : INotifyPropertyChanged
{
    private bool _isDone;

    public TaskItem(string title) => Title = title;

    public string Title { get; }

    public bool IsDone
    {
        get => _isDone;
        set
        {
            if (_isDone == value) return;
            _isDone = value;
            OnPropertyChanged(nameof(IsDone));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText => IsDone ? "Виконано" : "Не виконано";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
