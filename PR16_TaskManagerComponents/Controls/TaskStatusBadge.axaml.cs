using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TaskManager.Controls;

/// <summary>
/// Повторно використовуваний компонент «бейдж статусу».
/// Вхідні дані: IsDone (стан) і підписи DoneText / NotDoneText.
/// </summary>
public partial class TaskStatusBadge : UserControl
{
    public static readonly StyledProperty<bool> IsDoneProperty =
        AvaloniaProperty.Register<TaskStatusBadge, bool>(nameof(IsDone));

    public static readonly StyledProperty<string> DoneTextProperty =
        AvaloniaProperty.Register<TaskStatusBadge, string>(nameof(DoneText), "Виконано");

    public static readonly StyledProperty<string> NotDoneTextProperty =
        AvaloniaProperty.Register<TaskStatusBadge, string>(nameof(NotDoneText), "Не виконано");

    public bool IsDone
    {
        get => GetValue(IsDoneProperty);
        set => SetValue(IsDoneProperty, value);
    }

    public string DoneText
    {
        get => GetValue(DoneTextProperty);
        set => SetValue(DoneTextProperty, value);
    }

    public string NotDoneText
    {
        get => GetValue(NotDoneTextProperty);
        set => SetValue(NotDoneTextProperty, value);
    }

    private static readonly IBrush DoneBrush = new SolidColorBrush(Color.Parse("#2E7D32"));
    private static readonly IBrush NotDoneBrush = new SolidColorBrush(Color.Parse("#C62828"));

    public TaskStatusBadge()
    {
        InitializeComponent();
        UpdateVisual();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDoneProperty || change.Property == DoneTextProperty || change.Property == NotDoneTextProperty)
            UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (BadgeText is null || BadgeBorder is null) return;
        BadgeText.Text = IsDone ? DoneText : NotDoneText;
        BadgeBorder.Background = IsDone ? DoneBrush : NotDoneBrush;
    }
}
