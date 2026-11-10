using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace TaskManager.Controls;

/// <summary>
/// Повторно використовуваний компонент «рядок завдання»: назва, позначка «Виконано», бейдж статусу і кнопка видалення.
/// Вхідні дані — властивості Title, IsDone (двостороння), DeleteText.
/// Взаємодія назовні — маршрутизована подія DeleteRequested.
/// </summary>
public partial class TaskItemControl : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TaskItemControl, string>(nameof(Title), string.Empty);

    /// <summary>Двостороння за замовчуванням: зміна позначки записується назад у модель завдання.</summary>
    public static readonly StyledProperty<bool> IsDoneProperty =
        AvaloniaProperty.Register<TaskItemControl, bool>(nameof(IsDone), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> DeleteTextProperty =
        AvaloniaProperty.Register<TaskItemControl, string>(nameof(DeleteText), "Видалити");

    /// <summary>Подія «користувач хоче видалити це завдання» — спливає до головного вікна.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> DeleteRequestedEvent =
        RoutedEvent.Register<TaskItemControl, RoutedEventArgs>(nameof(DeleteRequested), RoutingStrategies.Bubble);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsDone
    {
        get => GetValue(IsDoneProperty);
        set => SetValue(IsDoneProperty, value);
    }

    public string DeleteText
    {
        get => GetValue(DeleteTextProperty);
        set => SetValue(DeleteTextProperty, value);
    }

    public event EventHandler<RoutedEventArgs> DeleteRequested
    {
        add => AddHandler(DeleteRequestedEvent, value);
        remove => RemoveHandler(DeleteRequestedEvent, value);
    }

    public TaskItemControl()
    {
        InitializeComponent();
    }

    private void DeleteButton_Click(object? sender, RoutedEventArgs e) => RequestDelete();

    internal void RequestDelete() => RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent, this));
}
