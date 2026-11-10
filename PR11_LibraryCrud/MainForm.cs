using Microsoft.Data.Sqlite;

namespace PR11_LibraryCrud;

/// <summary>
/// Практична робота 11: GUI-застосунок «Облік книг у бібліотеці» з повним CRUD над SQLite.
/// Поля введення + кнопки + таблиця; після кожної операції таблиця перечитується з БД.
/// </summary>
public class MainForm : Form
{
    private readonly BookRepository _repo =
        new(Path.Combine(AppContext.BaseDirectory, "library.db"));

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private readonly TextBox _searchBox = new() { PlaceholderText = "Пошук за назвою або автором", Width = 300 };
    private readonly TextBox _titleBox = new() { Width = 220 };
    private readonly TextBox _authorBox = new() { Width = 220 };
    private readonly NumericUpDown _yearNumeric = new() { Width = 220, Minimum = 1450, Maximum = DateTime.Now.Year, Value = DateTime.Now.Year };
    private readonly ComboBox _genreCombo = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly CheckBox _availableCheck = new() { Text = "Є в наявності", Checked = true, AutoSize = true };

    private readonly Button _addButton = new() { Text = "Додати", Width = 220 };
    private readonly Button _updateButton = new() { Text = "Редагувати вибрану", Width = 220 };
    private readonly Button _deleteButton = new() { Text = "Видалити вибрану", Width = 220 };
    private readonly Button _clearButton = new() { Text = "Очистити поля", Width = 220 };
    private readonly ToolStripStatusLabel _status = new();

    private readonly ErrorProvider _errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };

    public MainForm()
    {
        Text = "ПР11 — Облік книг у бібліотеці (CRUD + SQLite)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(900, 460);
        MinimumSize = new Size(800, 420);
        Font = new Font("Segoe UI", 9.5f);

        _genreCombo.Items.AddRange(new object[] { "Роман", "Фантастика", "Детектив", "Наукова", "Навчальна", "Поезія" });

        var editor = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 250,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10)
        };
        editor.Controls.AddRange(new Control[]
        {
            new Label { Text = "Назва *", AutoSize = true }, _titleBox,
            new Label { Text = "Автор *", AutoSize = true }, _authorBox,
            new Label { Text = "Рік видання", AutoSize = true }, _yearNumeric,
            new Label { Text = "Жанр", AutoSize = true }, _genreCombo,
            _availableCheck,
            _addButton, _updateButton, _deleteButton, _clearButton
        });

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(8, 6, 0, 0) };
        top.Controls.Add(new Label { Text = "Пошук:", AutoSize = true, Margin = new Padding(0, 4, 4, 0) });
        top.Controls.Add(_searchBox);

        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_status);

        Controls.Add(_grid);
        Controls.Add(editor);
        Controls.Add(top);
        Controls.Add(statusStrip);

        _addButton.Click += (_, _) => CreateBook();
        _updateButton.Click += (_, _) => UpdateBook();
        _deleteButton.Click += (_, _) => DeleteBook();
        _clearButton.Click += (_, _) => ClearInputs();
        _searchBox.TextChanged += (_, _) => RefreshGrid();
        _grid.SelectionChanged += (_, _) => FillInputs();

        Load += (_, _) => RefreshGrid();
    }

    private Book? Selected => _grid.CurrentRow?.DataBoundItem as Book;

    /// <summary>Read: перечитує дані з БД і оновлює таблицю.</summary>
    private void RefreshGrid(int? selectId = null)
    {
        try
        {
            var books = _repo.Read(_searchBox.Text);
            _grid.DataSource = books;
            SetHeader(nameof(Book.Id), "Id");
            SetHeader(nameof(Book.Title), "Назва");
            SetHeader(nameof(Book.Author), "Автор");
            SetHeader(nameof(Book.Year), "Рік");
            SetHeader(nameof(Book.Genre), "Жанр");
            SetHeader(nameof(Book.IsAvailable), "В наявності");

            if (selectId is int id)
            {
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.DataBoundItem is Book b && b.Id == id)
                    {
                        row.Selected = true;
                        _grid.CurrentCell = row.Cells[0];
                        break;
                    }
                }
            }
            _status.Text = $"Записів: {books.Count}";
        }
        catch (SqliteException ex)
        {
            ShowDbError(ex);
        }
    }

    private void SetHeader(string column, string header)
    {
        if (_grid.Columns[column] is { } col)
            col.HeaderText = header;
    }

    /// <summary>Перевірка введених значень. Повертає null, якщо дані некоректні.</summary>
    private Book? ReadInputs()
    {
        _errors.Clear();
        bool ok = true;

        string title = _titleBox.Text.Trim();
        string author = _authorBox.Text.Trim();

        if (title.Length < 2)
        {
            _errors.SetError(_titleBox, "Назва має містити щонайменше 2 символи");
            ok = false;
        }
        if (author.Length < 2)
        {
            _errors.SetError(_authorBox, "Вкажіть автора (щонайменше 2 символи)");
            ok = false;
        }
        else if (author.Any(char.IsDigit))
        {
            _errors.SetError(_authorBox, "Ім'я автора не повинно містити цифр");
            ok = false;
        }

        if (!ok) return null;

        return new Book
        {
            Title = title,
            Author = author,
            Year = (int)_yearNumeric.Value,
            Genre = _genreCombo.Text.Trim(),
            IsAvailable = _availableCheck.Checked
        };
    }

    private void CreateBook()
    {
        if (ReadInputs() is not { } book) return;
        try
        {
            int id = _repo.Create(book);
            RefreshGrid(id);
            _status.Text = $"Додано книгу «{book.Title}»";
        }
        catch (SqliteException ex)
        {
            ShowDbError(ex);
        }
    }

    private void UpdateBook()
    {
        if (Selected is not { } current)
        {
            MessageBox.Show("Спочатку виберіть книгу в таблиці", "Редагування", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (ReadInputs() is not { } book) return;

        book.Id = current.Id;
        try
        {
            _repo.Update(book);
            RefreshGrid(book.Id);
            _status.Text = $"Зміни збережено (Id = {book.Id})";
        }
        catch (SqliteException ex)
        {
            ShowDbError(ex);
        }
    }

    private void DeleteBook()
    {
        if (Selected is not { } current)
        {
            MessageBox.Show("Спочатку виберіть книгу в таблиці", "Видалення", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show($"Видалити книгу «{current.Title}»?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            _repo.Delete(current.Id);
            RefreshGrid();
            ClearInputs();
            _status.Text = "Запис видалено";
        }
        catch (SqliteException ex)
        {
            ShowDbError(ex);
        }
    }

    private void FillInputs()
    {
        if (Selected is not { } b) return;
        _errors.Clear();
        _titleBox.Text = b.Title;
        _authorBox.Text = b.Author;
        _yearNumeric.Value = Math.Clamp(b.Year, (int)_yearNumeric.Minimum, (int)_yearNumeric.Maximum);
        _genreCombo.Text = b.Genre;
        _availableCheck.Checked = b.IsAvailable;
    }

    private void ClearInputs()
    {
        _errors.Clear();
        _titleBox.Clear();
        _authorBox.Clear();
        _yearNumeric.Value = DateTime.Now.Year;
        _genreCombo.Text = string.Empty;
        _availableCheck.Checked = true;
        _grid.ClearSelection();
        _titleBox.Focus();
    }

    private static void ShowDbError(Exception ex) =>
        MessageBox.Show($"Помилка бази даних:\n{ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
