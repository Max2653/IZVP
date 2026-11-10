using System.Text.Json;

namespace PR09_JsonNotes;

/// <summary>
/// Практична робота 9: серіалізація, збереження та відновлення даних у JSON.
/// 1) простий клас Note;
/// 2) збереження 2–3 об'єктів у notes.json;
/// 3) читання JSON назад;
/// 4) показ відновлених даних у GUI (ListBox + деталі).
/// </summary>
public class MainForm : Form
{
    private readonly NoteStorage _storage =
        new(Path.Combine(AppContext.BaseDirectory, "notes.json"));

    private List<Note> _notes = new();

    private readonly TextBox _titleBox = new() { PlaceholderText = "Заголовок", Width = 300 };
    private readonly TextBox _textBox = new() { PlaceholderText = "Текст нотатки", Width = 300, Height = 80, Multiline = true };
    private readonly Button _addButton = new() { Text = "Додати", Width = 145 };
    private readonly Button _sampleButton = new() { Text = "Створити 3 приклади", Width = 145 };
    private readonly Button _saveButton = new() { Text = "Зберегти у JSON", Width = 145 };
    private readonly Button _loadButton = new() { Text = "Прочитати JSON", Width = 145 };
    private readonly Button _clearButton = new() { Text = "Очистити список", Width = 145 };
    private readonly ListBox _list = new() { Width = 300, Height = 200 };
    private readonly Label _details = new() { Width = 300, Height = 90, BorderStyle = BorderStyle.FixedSingle };
    private readonly Label _status = new() { AutoSize = true, ForeColor = Color.DimGray };

    public MainForm()
    {
        Text = "ПР9 — Нотатки (JSON)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(640, 420);
        Font = new Font("Segoe UI", 9.5f);

        var left = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Location = new Point(12, 12),
            Size = new Size(310, 400),
            WrapContents = false
        };
        var buttons = new FlowLayoutPanel { Width = 310, Height = 100 };
        buttons.Controls.AddRange(new Control[] { _addButton, _sampleButton, _saveButton, _loadButton, _clearButton });
        left.Controls.AddRange(new Control[] { new Label { Text = "Нова нотатка:", AutoSize = true }, _titleBox, _textBox, buttons, _status });

        var right = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Location = new Point(325, 12),
            Size = new Size(310, 400),
            WrapContents = false
        };
        right.Controls.AddRange(new Control[] { new Label { Text = "Нотатки:", AutoSize = true }, _list, new Label { Text = "Деталі:", AutoSize = true }, _details });

        Controls.Add(left);
        Controls.Add(right);

        _addButton.Click += (_, _) => AddNote();
        _sampleButton.Click += (_, _) => CreateSamples();
        _saveButton.Click += (_, _) => SaveNotes();
        _loadButton.Click += (_, _) => LoadNotes();
        _clearButton.Click += (_, _) => { _notes.Clear(); RefreshList(); SetStatus("Список у пам'яті очищено"); };
        _list.SelectedIndexChanged += (_, _) => ShowDetails();

        // автоматичне відновлення даних під час запуску
        Load += (_, _) => LoadNotes();
    }

    private void AddNote()
    {
        string title = _titleBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Введіть заголовок нотатки", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _notes.Add(new Note
        {
            Id = _notes.Count == 0 ? 1 : _notes.Max(n => n.Id) + 1,
            Title = title,
            Text = _textBox.Text.Trim(),
            CreatedAt = DateTime.Now
        });
        _titleBox.Clear();
        _textBox.Clear();
        RefreshList();
        SetStatus("Нотатку додано (ще не збережено)");
    }

    private void CreateSamples()
    {
        _notes = new List<Note>
        {
            new() { Id = 1, Title = "Покупки", Text = "Хліб, молоко, сир", CreatedAt = DateTime.Now.AddDays(-2) },
            new() { Id = 2, Title = "Навчання", Text = "Здати практичну роботу з ІЗВП", CreatedAt = DateTime.Now.AddDays(-1) },
            new() { Id = 3, Title = "Ідея", Text = "Додати пошук по нотатках", CreatedAt = DateTime.Now }
        };
        RefreshList();
        SetStatus("Створено 3 об'єкти Note");
    }

    private void SaveNotes()
    {
        try
        {
            _storage.Save(_notes);
            SetStatus($"Збережено {_notes.Count} об'єкт(и) у {Path.GetFileName(_storage.FilePath)}");
            MessageBox.Show("Дані збережено!", "JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Не вдалося зберегти файл:\n{ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadNotes()
    {
        try
        {
            _notes = _storage.Load();
            RefreshList();
            SetStatus(File.Exists(_storage.FilePath)
                ? $"Відновлено {_notes.Count} об'єкт(и) з JSON"
                : "Файл notes.json ще не існує");
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            MessageBox.Show($"Не вдалося прочитати JSON:\n{ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshList()
    {
        _list.DataSource = null;
        _list.DataSource = _notes;
        ShowDetails();
    }

    private void ShowDetails()
    {
        _details.Text = _list.SelectedItem is Note n
            ? $"Id: {n.Id}\nЗаголовок: {n.Title}\nТекст: {n.Text}\nСтворено: {n.CreatedAt:dd.MM.yyyy HH:mm:ss}"
            : string.Empty;
    }

    private void SetStatus(string message) => _status.Text = message;
}
