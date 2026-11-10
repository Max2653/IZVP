using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PR12_DataBinding;

/// <summary>
/// Модель даних. INotifyPropertyChanged повідомляє GUI про зміни (модель → UI),
/// IDataErrorInfo віддає повідомлення про некоректні значення (для ErrorProvider).
/// </summary>
public class Student : INotifyPropertyChanged, IDataErrorInfo
{
    private string _name = string.Empty;
    private int _age;
    private int _grade;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetField(ref _age, value);
    }

    /// <summary>Оцінка за 100-бальною шкалою.</summary>
    public int Grade
    {
        get => _grade;
        set => SetField(ref _grade, value);
    }

    /// <summary>Обчислювана властивість — показує, що UI оновлюється автоматично.</summary>
    public string Summary =>
        string.IsNullOrWhiteSpace(Name)
            ? "—"
            : $"{Name}, {Age} р., оцінка {Grade} ({GradeLetter})";

    public string GradeLetter => Grade switch
    {
        >= 90 => "A",
        >= 82 => "B",
        >= 74 => "C",
        >= 64 => "D",
        >= 60 => "E",
        _ => "FX"
    };

    // ---- IDataErrorInfo: перевірка значень ----
    public string Error => string.Join("; ", new[] { this[nameof(Name)], this[nameof(Age)], this[nameof(Grade)] }
        .Where(e => !string.IsNullOrEmpty(e)));

    public string this[string columnName] => columnName switch
    {
        nameof(Name) when string.IsNullOrWhiteSpace(Name) => "Ім'я не може бути порожнім",
        nameof(Age) when Age is < 16 or > 100 => "Вік має бути в межах 16–100",
        nameof(Grade) when Grade is < 0 or > 100 => "Оцінка має бути в межах 0–100",
        _ => string.Empty
    };

    public bool IsValid => string.IsNullOrEmpty(Error);

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
        // залежні властивості теж змінились
        OnPropertyChanged(nameof(Summary));
        if (propertyName == nameof(Grade))
            OnPropertyChanged(nameof(GradeLetter));
    }

    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
