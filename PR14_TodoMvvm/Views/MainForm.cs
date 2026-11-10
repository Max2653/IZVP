using TodoMvvm.Models;
using TodoMvvm.ViewModels;

namespace TodoMvvm.Views;

/// <summary>
/// VIEW — лише елементи інтерфейсу та прив'язки до ViewModel.
/// Список завдань тут не зберігається, логіки додавання/статусу/видалення немає:
/// кнопки викликають команди ViewModel, таблиця і поля прив'язані через Data Binding.
/// </summary>
public class MainForm : Form
{
    private readonly TaskListViewModel _vm;
    private readonly BindingSource _tasksSource = new();

    private readonly TextBox _titleBox = new() { Width = 300, PlaceholderText = "Назва нового завдання" };
    private readonly Button _addButton = new() { Text = "Додати", Width = 110 };
    private readonly Button _toggleButton = new() { Text = "Виконано / Не виконано", Width = 200 };
    private readonly Button _deleteButton = new() { Text = "Видалити", Width = 110 };
    private readonly Label _messageLabel = new() { AutoSize = true, MaximumSize = new Size(520, 0) };

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AutoGenerateColumns = false,
        AllowUserToAddRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public MainForm(TaskListViewModel viewModel)
    {
        _vm = viewModel;

        Text = "ПР14 — Список завдань (MVVM)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(560, 380);
        Font = new Font("Segoe UI", 10f);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(TaskItem.Title), HeaderText = "Завдання", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(TaskItem.Status), HeaderText = "Статус", FillWeight = 30 });

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6) };
        top.Controls.AddRange(new Control[] { _titleBox, _addButton });

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(6), FlowDirection = FlowDirection.LeftToRight };
        bottom.Controls.AddRange(new Control[] { _toggleButton, _deleteButton });
        bottom.SetFlowBreak(_deleteButton, true);
        bottom.Controls.Add(_messageLabel);

        Controls.Add(_grid);
        Controls.Add(top);
        Controls.Add(bottom);

        BindToViewModel();
    }

    private void BindToViewModel()
    {
        // таблиця ← колекція ViewModel
        _tasksSource.DataSource = _vm.Tasks;
        _grid.DataSource = _tasksSource;

        // поле введення ↔ NewTitle
        _titleBox.DataBindings.Add(nameof(TextBox.Text), _vm, nameof(TaskListViewModel.NewTitle), false, DataSourceUpdateMode.OnPropertyChanged);

        // повідомлення ← Message
        _messageLabel.DataBindings.Add(nameof(Label.Text), _vm, nameof(TaskListViewModel.Message));

        // кнопки → команди ViewModel (Button.Command також керує Enabled через CanExecute)
        _addButton.Command = _vm.AddCommand;
        _toggleButton.Command = _vm.ToggleStatusCommand;
        _deleteButton.Command = _vm.DeleteCommand;
        AcceptButton = _addButton;

        // вибір рядка в таблиці → SelectedTask; SelectedTask у ViewModel → поточний рядок
        _tasksSource.CurrentChanged += (_, _) => _vm.SelectedTask = _tasksSource.Current as TaskItem;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TaskListViewModel.SelectedTask) && _vm.SelectedTask is { } t)
                _tasksSource.Position = _vm.Tasks.IndexOf(t);
            if (e.PropertyName == nameof(TaskListViewModel.IsError))
                _messageLabel.ForeColor = _vm.IsError ? Color.DarkRed : Color.DarkGreen;
        };
    }
}
