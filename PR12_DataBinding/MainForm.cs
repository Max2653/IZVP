using System.ComponentModel;

namespace PR12_DataBinding;

/// <summary>
/// Практична робота 12: Data Binding у Windows Forms.
/// Список студентів (BindingList) → BindingSource → DataGridView + поля редагування.
/// Жодного ручного присвоєння значень елементам: усе через DataBindings.
/// </summary>
public class MainForm : Form
{
    private readonly BindingList<Student> _students = new()
    {
        new Student { Name = "Іваненко Олена", Age = 19, Grade = 88 },
        new Student { Name = "Петренко Андрій", Age = 20, Grade = 74 },
        new Student { Name = "Коваль Марія", Age = 18, Grade = 95 }
    };

    private readonly BindingSource _source = new();

    private readonly DataGridView _grid = new()
    {
        Location = new Point(12, 12),
        Size = new Size(430, 200),
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AutoGenerateColumns = false
    };

    private readonly TextBox _nameBox = new() { Width = 220 };
    private readonly TextBox _ageBox = new() { Width = 220 };
    private readonly TextBox _gradeBox = new() { Width = 220 };
    private readonly Label _summaryLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
    private readonly Label _statusLabel = new() { AutoSize = true, ForeColor = Color.DarkRed, MaximumSize = new Size(430, 0) };
    private readonly Button _bonusButton = new() { Text = "+5 балів (зміна в моделі)", Width = 220 };
    private readonly Button _addButton = new() { Text = "Додати студента", Width = 220 };
    private readonly ErrorProvider _errorProvider = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };

    public MainForm()
    {
        Text = "ПР12 — Data Binding (Student)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(700, 330);
        Font = new Font("Segoe UI", 9.5f);

        // 1. Джерело даних
        _source.DataSource = _students;

        // 2. Таблиця прив'язана до того ж BindingSource
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Student.Name), HeaderText = "Ім'я" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Student.Age), HeaderText = "Вік" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Student.Grade), HeaderText = "Оцінка" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Student.GradeLetter), HeaderText = "ECTS", ReadOnly = true });
        _grid.DataSource = _source;

        // 3. Двостороннє зв'язування полів з поточним студентом
        _nameBox.DataBindings.Add(nameof(TextBox.Text), _source, nameof(Student.Name), true, DataSourceUpdateMode.OnPropertyChanged);
        _ageBox.DataBindings.Add(nameof(TextBox.Text), _source, nameof(Student.Age), true, DataSourceUpdateMode.OnValidation);
        _gradeBox.DataBindings.Add(nameof(TextBox.Text), _source, nameof(Student.Grade), true, DataSourceUpdateMode.OnValidation);

        // 4. Одностороннє зв'язування (модель → UI) для обчислюваної властивості
        _summaryLabel.DataBindings.Add(nameof(Label.Text), _source, nameof(Student.Summary));

        // 5. ErrorProvider бере помилки з IDataErrorInfo моделі
        _errorProvider.DataSource = _source;

        // некоректний формат (наприклад, "abc" у полі віку) — повідомлення з BindingComplete
        foreach (var binding in new[] { _ageBox.DataBindings[0], _gradeBox.DataBindings[0] })
            binding.BindingComplete += OnBindingComplete;

        _source.CurrentItemChanged += (_, _) => ShowModelErrors();
        _bonusButton.Click += (_, _) => AddBonus();
        _addButton.Click += (_, _) => _source.Add(new Student { Name = "Новий студент", Age = 18, Grade = 60 });

        var editor = new FlowLayoutPanel
        {
            Location = new Point(455, 12),
            Size = new Size(240, 310),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        editor.Controls.AddRange(new Control[]
        {
            new Label { Text = "Ім'я:", AutoSize = true }, _nameBox,
            new Label { Text = "Вік:", AutoSize = true }, _ageBox,
            new Label { Text = "Оцінка (0–100):", AutoSize = true }, _gradeBox,
            _bonusButton, _addButton
        });

        var bottom = new FlowLayoutPanel
        {
            Location = new Point(12, 220),
            Size = new Size(430, 100),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        bottom.Controls.AddRange(new Control[] { new Label { Text = "Поточний запис (Summary):", AutoSize = true }, _summaryLabel, _statusLabel });

        Controls.Add(_grid);
        Controls.Add(editor);
        Controls.Add(bottom);
    }

    internal TextBox GradeBox => _gradeBox;
    internal BindingSource Source => _source;

    /// <summary>Зміна даних у моделі з коду — інтерфейс оновлюється сам через PropertyChanged.</summary>
    private void AddBonus()
    {
        if (_source.Current is Student s)
            s.Grade = Math.Min(100, s.Grade + 5);
    }

    private void OnBindingComplete(object? sender, BindingCompleteEventArgs e)
    {
        if (e.BindingCompleteContext != BindingCompleteContext.DataSourceUpdate) return;

        if (e.Binding?.Control is not Control control) return;
        if (e.BindingCompleteState == BindingCompleteState.Success)
        {
            ShowModelErrors();
            return;
        }

        // значення не вдалося перетворити на число — скасовуємо і показуємо помилку
        _errorProvider.SetError(control, "Введіть ціле число");
        _statusLabel.Text = $"Некоректне значення «{control.Text}»: очікується ціле число.";
        e.Cancel = true;
    }

    private void ShowModelErrors()
    {
        if (_source.Current is not Student s) return;
        _errorProvider.UpdateBinding();
        _statusLabel.Text = s.IsValid ? string.Empty : "Помилка: " + s.Error;
    }
}
