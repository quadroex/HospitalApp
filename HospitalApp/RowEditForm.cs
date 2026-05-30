using System.Data;

namespace HospitalApp;

public sealed class RowEditForm : Form
{
    private readonly TableConfig _config;
    private readonly Dictionary<string, Control> _controls = new();
    private readonly bool _editMode;

    public Dictionary<string, object?> Values { get; private set; } = new();

    public RowEditForm(TableConfig config, DataRowView? existingRow = null)
    {
        _config = config;
        _editMode = existingRow != null;

        Text = _editMode ? $"Редагування: {_config.Title}" : $"Додавання: {_config.Title}";
        Width = 600;
        Height = 520;
        StartPosition = FormStartPosition.CenterParent;

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true,
            Padding = new Padding(12)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        foreach (var column in _config.Columns)
        {
            var label = new Label
            {
                Text = column.Label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };

            var control = CreateControl(column, existingRow);
            control.Dock = DockStyle.Fill;

            if (_editMode && column.IsPrimaryKey)
                control.Enabled = false;

            _controls[column.Name] = control;

            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(label);
            panel.Controls.Add(control);
        }

        var saveButton = new Button
        {
            Text = "Зберегти",
            Width = 120,
            Height = 35
        };

        saveButton.Click += (_, _) =>
        {
            try
            {
                Values = CollectValues();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка введення");
            }
        };

        var cancelButton = new Button
        {
            Text = "Скасувати",
            Width = 120,
            Height = 35,
            DialogResult = DialogResult.Cancel
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12)
        };

        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        Controls.Add(panel);
        Controls.Add(buttons);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private Control CreateControl(ColumnConfig column, DataRowView? existingRow)
    {
        object? value = null;

        if (existingRow != null)
        {
            value = existingRow.Row[column.Name];
            if (value == DBNull.Value)
                value = null;
        }

        return column.Kind switch
        {
            FieldKind.Integer => CreateIntegerControl(value),
            FieldKind.Date => CreateDateControl(value),
            FieldKind.Time => CreateTimeControl(value),
            FieldKind.ForeignKey => CreateForeignKeyControl(column, value),
            _ => CreateTextControl(value)
        };
    }

    private static Control CreateTextControl(object? value)
    {
        return new TextBox
        {
            Text = value?.ToString() ?? ""
        };
    }

    private static Control CreateIntegerControl(object? value)
    {
        var numeric = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1_000_000,
            DecimalPlaces = 0
        };

        if (value != null)
            numeric.Value = Convert.ToDecimal(value);

        return numeric;
    }

    private static Control CreateDateControl(object? value)
    {
        var picker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short
        };

        if (value is DateTime date)
            picker.Value = date;

        return picker;
    }

    private static Control CreateTimeControl(object? value)
    {
        var picker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true
        };

        if (value is TimeSpan time)
            picker.Value = DateTime.Today.Add(time);
        else if (value is DateTime dateTime)
            picker.Value = dateTime;

        return picker;
    }

    private static Control CreateForeignKeyControl(ColumnConfig column, object? value)
    {
        if (column.Lookup == null)
            throw new InvalidOperationException($"Для поля {column.Name} не задано Lookup.");

        var comboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        comboBox.DataSource = Db.GetTable(column.Lookup.Sql);
        comboBox.ValueMember = column.Lookup.ValueMember;
        comboBox.DisplayMember = column.Lookup.DisplayMember;

        if (value != null)
            comboBox.SelectedValue = value;

        return comboBox;
    }

    private Dictionary<string, object?> CollectValues()
    {
        var result = new Dictionary<string, object?>();

        foreach (var column in _config.Columns)
        {
            var control = _controls[column.Name];

            object? value = column.Kind switch
            {
                FieldKind.Integer => Convert.ToInt32(((NumericUpDown)control).Value),
                FieldKind.Date => ((DateTimePicker)control).Value.Date,
                FieldKind.Time => ((DateTimePicker)control).Value.TimeOfDay,
                FieldKind.ForeignKey => ((ComboBox)control).SelectedValue,
                _ => ((TextBox)control).Text.Trim()
            };

            if (value is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    if (column.IsRequired)
                        throw new InvalidOperationException($"Поле \"{column.Label}\" є обов'язковим.");

                    value = null;
                }
            }

            if (column.Kind == FieldKind.ForeignKey && value == null && column.IsRequired)
                throw new InvalidOperationException($"Поле \"{column.Label}\" є обов'язковим.");

            result[column.Name] = value;
        }

        return result;
    }
}