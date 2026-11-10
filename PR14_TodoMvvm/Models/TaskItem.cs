using System.ComponentModel;

namespace TodoMvvm.Models;

/// <summary>
/// MODEL — дані одного завдання. Не знає ні про вікно, ні про ViewModel.
/// INotifyPropertyChanged потрібен, щоб зміна статусу автоматично відображалась у списку.
/// </summary>
public class TaskItem : INotifyPropertyChanged
{
    private bool _isDone;

    public TaskItem(string title)
    {
        Title = title;
    }

    public string Title { get; }

    public bool IsDone
    {
        get => _isDone;
        set
        {
            if (_isDone == value) return;
            _isDone = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDone)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    public string Status => IsDone ? "Виконано" : "Не виконано";

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString() => $"«{Title}» — {Status}";
}
