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
        UiTheme.ApplyFormTheme(this);
        _config = config;
        _editMode = existingRow != null;

        Text = _editMode ? $"Редагування: {_config.Title}" : $"Додавання: {_config.Title}";
        Width = 660;
        Height = 560;
        MinimumSize = new Size(580, 480);
        StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(UiTheme.FormPadding),
            ColumnCount = 1,
            RowCount = 3
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));

        var headerCard = UiTheme.CreateCardPanel();
        headerCard.Dock = DockStyle.Fill;
        headerCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        headerCard.Padding = new Padding(14, 6, 14, 6);
        headerCard.Controls.Add(UiTheme.CreateHeaderLabel(Text));

        var fieldsCard = UiTheme.CreateCardPanel();
        fieldsCard.Dock = DockStyle.Fill;
        fieldsCard.Margin = new Padding(0);
        fieldsCard.Padding = new Padding(18);

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true
        };
        UiTheme.ApplyTableLayoutDefaults(panel);

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        var row = 0;
        foreach (var column in _config.Columns)
        {
            var label = UiTheme.CreateFieldLabel(column.Label);

            var control = CreateControl(column, existingRow);
            control.Dock = DockStyle.Fill;
            UiTheme.StyleInput(control);

            if (_editMode && column.IsPrimaryKey)
                control.Enabled = false;

            _controls[column.Name] = control;

            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            panel.Controls.Add(label, 0, row);
            panel.Controls.Add(control, 1, row);
            row++;
        }

        var saveButton = new Button
        {
            Text = "Зберегти",
            Width = 130
        };
        UiTheme.StylePrimaryButton(saveButton);

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
            Width = 130,
            DialogResult = DialogResult.Cancel
        };
        UiTheme.StyleSecondaryButton(cancelButton);

        var buttonsCard = UiTheme.CreateCardPanel();
        buttonsCard.Dock = DockStyle.Fill;
        buttonsCard.Margin = new Padding(0, UiTheme.Spacing, 0, 0);
        buttonsCard.Padding = new Padding(10, 8, 10, 8);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
        };
        UiTheme.ApplyFlowLayoutDefaults(buttons);

        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        fieldsCard.Controls.Add(panel);
        buttonsCard.Controls.Add(buttons);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(fieldsCard, 0, 1);
        root.Controls.Add(buttonsCard, 0, 2);
        Controls.Add(root);

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
