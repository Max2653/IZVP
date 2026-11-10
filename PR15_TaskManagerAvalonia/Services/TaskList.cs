using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using TaskManager.Models;

namespace TaskManager.Services;

/// <summary>
/// Логіка «Менеджера завдань», відокремлена від інтерфейсу:
/// колекція завдань, перевірка введення, видалення і лічильник невиконаних.
/// </summary>
public class TaskList : INotifyPropertyChanged
{
    public TaskList()
    {
        Items.CollectionChanged += OnItemsChanged;
    }

    public ObservableCollection<TaskItem> Items { get; } = new();

    public int PendingCount => Items.Count(t => !t.IsDone);

    public string PendingText => $"Невиконаних завдань: {PendingCount} з {Items.Count}";

    /// <summary>Додає завдання; повертає false і текст помилки, якщо назва порожня.</summary>
    public bool TryAdd(string? title, out string error)
    {
        string trimmed = (title ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            error = "Введіть назву завдання — порожнє завдання додати не можна.";
            return false;
        }

        Items.Add(new TaskItem(trimmed));
        error = string.Empty;
        return true;
    }

    public void Remove(TaskItem item) => Items.Remove(item);

    // підписка на зміну статусу кожного завдання — для автоматичного лічильника
    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (TaskItem t in e.NewItems) t.PropertyChanged += OnTaskChanged;
        if (e.OldItems is not null)
            foreach (TaskItem t in e.OldItems) t.PropertyChanged -= OnTaskChanged;
        RaiseCounters();
    }

    private void OnTaskChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TaskItem.IsDone))
            RaiseCounters();
    }

    private void RaiseCounters()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingText)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
