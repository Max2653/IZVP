using OrderCore;

namespace OrderApp;

/// <summary>
/// Інтерфейс «Розрахунок замовлення». Форма лише:
/// 1) читає текст із полів, 2) викликає OrderService, 3) показує результат або помилку.
/// Жодних обчислень і перевірок у коді кнопки немає.
/// </summary>
public class MainForm : Form
{
    private readonly OrderService _service;

    private readonly TextBox _amountBox = new() { Width = 180 };
    private readonly TextBox _discountBox = new() { Width = 180 };
    private readonly Button _calculateButton = new() { Text = "Розрахувати", Width = 180, Height = 30 };
    private readonly Label _resultLabel = new() { AutoSize = true, MaximumSize = new Size(340, 0), Font = new Font("Segoe UI", 11f, FontStyle.Bold) };

    public MainForm(OrderService service)
    {
        _service = service;

        Text = "ПР13 — Розрахунок замовлення";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(380, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 10f);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 4
        };
        layout.Controls.Add(new Label { Text = "Сума, грн:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(_amountBox, 1, 0);
        layout.Controls.Add(new Label { Text = "Знижка, %:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(_discountBox, 1, 1);
        layout.Controls.Add(_calculateButton, 1, 2);
        layout.Controls.Add(_resultLabel, 0, 3);
        layout.SetColumnSpan(_resultLabel, 2);
        Controls.Add(layout);

        AcceptButton = _calculateButton;
        _calculateButton.Click += (_, _) => OnCalculateClick();
    }

    internal TextBox AmountBox => _amountBox;
    internal TextBox DiscountBox => _discountBox;
    internal Button CalculateButton => _calculateButton;
    internal bool ShowMessageBoxes = true;

    private void OnCalculateClick()
    {
        // GUI → сервіс: передаємо те, що ввів користувач
        OrderResult result = _service.Calculate(_amountBox.Text, _discountBox.Text);

        // сервіс → GUI: показуємо результат
        if (result.Success)
        {
            _resultLabel.ForeColor = Color.DarkGreen;
            _resultLabel.Text = $"До сплати: {result.Total:0.##} грн\n(знижка {result.DiscountValue:0.##} грн)";
        }
        else
        {
            _resultLabel.ForeColor = Color.DarkRed;
            _resultLabel.Text = result.Error;
            if (ShowMessageBoxes)
                MessageBox.Show(result.Error, "Неправильні дані", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
