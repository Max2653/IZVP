using Microsoft.Data.Sqlite;

namespace PR10_SqliteProducts;

/// <summary>
/// Практична робота 10: інтеграція GUI-застосунку з локальною базою SQLite.
/// 1) одна таблиця Products у файлі products.db;
/// 2) таблиця підключена до DataGridView;
/// 3) додавання запису;
/// 4) показ записів;
/// 5) редагування та видалення вибраного запису.
/// </summary>
public class MainForm : Form
{
    private readonly ProductRepository _repo =
        new(Path.Combine(AppContext.BaseDirectory, "products.db"));

    private readonly DataGridView _grid = new()
    {
        Location = new Point(12, 12),
        Size = new Size(460, 300),
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private readonly TextBox _nameBox = new() { Width = 200 };
    private readonly NumericUpDown _priceNumeric = new() { Width = 200, DecimalPlaces = 2, Maximum = 1_000_000 };
    private readonly NumericUpDown _quantityNumeric = new() { Width = 200, Maximum = 100_000 };

    private readonly Button _addButton = new() { Text = "Додати", Width = 200 };
    private readonly Button _updateButton = new() { Text = "Зберегти зміни", Width = 200 };
    private readonly Button _deleteButton = new() { Text = "Видалити", Width = 200 };
    private readonly Button _clearButton = new() { Text = "Очистити поля", Width = 200 };

    public MainForm()
    {
        Text = "ПР10 — Товари (SQLite)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(700, 330);
        Font = new Font("Segoe UI", 9.5f);

        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Location = new Point(485, 12),
            Size = new Size(210, 310),
            WrapContents = false
        };
        panel.Controls.AddRange(new Control[]
        {
            new Label { Text = "Назва:", AutoSize = true }, _nameBox,
            new Label { Text = "Ціна:", AutoSize = true }, _priceNumeric,
            new Label { Text = "Кількість:", AutoSize = true }, _quantityNumeric,
            _addButton, _updateButton, _deleteButton, _clearButton
        });

        Controls.Add(_grid);
        Controls.Add(panel);

        _addButton.Click += (_, _) => AddProduct();
        _updateButton.Click += (_, _) => UpdateProduct();
        _deleteButton.Click += (_, _) => DeleteProduct();
        _clearButton.Click += (_, _) => ClearInputs();
        _grid.SelectionChanged += (_, _) => FillInputsFromSelection();

        Load += (_, _) => LoadProducts();
    }

    private Product? SelectedProduct => _grid.CurrentRow?.DataBoundItem as Product;

    private void LoadProducts()
    {
        try
        {
            _grid.DataSource = _repo.GetAll();
            SetHeader(nameof(Product.Id), "Id");
            SetHeader(nameof(Product.Name), "Назва");
            SetHeader(nameof(Product.Price), "Ціна");
            SetHeader(nameof(Product.Quantity), "Кількість");
        }
        catch (SqliteException ex)
        {
            ShowError(ex);
        }
    }

    private void SetHeader(string column, string header)
    {
        if (_grid.Columns[column] is { } col)
            col.HeaderText = header;
    }

    private bool TryReadInputs(out Product product)
    {
        product = new Product
        {
            Name = _nameBox.Text.Trim(),
            Price = _priceNumeric.Value,
            Quantity = (int)_quantityNumeric.Value
        };

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            MessageBox.Show("Введіть назву товару", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private void AddProduct()
    {
        if (!TryReadInputs(out var product)) return;
        try
        {
            _repo.Add(product);
            LoadProducts();
            ClearInputs();
        }
        catch (SqliteException ex)
        {
            ShowError(ex);
        }
    }

    private void UpdateProduct()
    {
        var selected = SelectedProduct;
        if (selected is null)
        {
            MessageBox.Show("Оберіть запис у таблиці", "Редагування", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!TryReadInputs(out var product)) return;

        product.Id = selected.Id;
        try
        {
            _repo.Update(product);
            LoadProducts();
        }
        catch (SqliteException ex)
        {
            ShowError(ex);
        }
    }

    private void DeleteProduct()
    {
        var selected = SelectedProduct;
        if (selected is null)
        {
            MessageBox.Show("Оберіть запис у таблиці", "Видалення", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var answer = MessageBox.Show($"Видалити \"{selected.Name}\"?", "Підтвердження",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        try
        {
            _repo.Delete(selected.Id);
            LoadProducts();
            ClearInputs();
        }
        catch (SqliteException ex)
        {
            ShowError(ex);
        }
    }

    private void FillInputsFromSelection()
    {
        if (SelectedProduct is not { } p) return;
        _nameBox.Text = p.Name;
        _priceNumeric.Value = Math.Min(p.Price, _priceNumeric.Maximum);
        _quantityNumeric.Value = Math.Min(p.Quantity, (int)_quantityNumeric.Maximum);
    }

    private void ClearInputs()
    {
        _nameBox.Clear();
        _priceNumeric.Value = 0;
        _quantityNumeric.Value = 0;
        _grid.ClearSelection();
    }

    private static void ShowError(Exception ex) =>
        MessageBox.Show($"Помилка роботи з базою даних:\n{ex.Message}", "Помилка",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
}
