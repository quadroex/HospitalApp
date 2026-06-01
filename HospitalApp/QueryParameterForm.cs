using System.Data;

namespace HospitalApp;

public sealed class QueryParameterForm : Form
{
    private readonly QueryConfig _config;
    private readonly Dictionary<string, Control> _controls = new();

    public Dictionary<string, object?> Values { get; private set; } = new();

    public QueryParameterForm(QueryConfig config)
    {
        UiTheme.ApplyFormTheme(this);
        _config = config;

        Text = _config.Title;
        Width = 620;
        Height = 420;
        MinimumSize = new Size(560, 340);
        StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(UiTheme.FormPadding),
            ColumnCount = 1,
            RowCount = 3
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

        var headerCard = UiTheme.CreateCardPanel();
        headerCard.Dock = DockStyle.Fill;
        headerCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        headerCard.Padding = new Padding(14, 6, 14, 6);
        headerCard.Controls.Add(UiTheme.CreateHeaderLabel("Параметри запиту"));

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
        foreach (var parameter in _config.Parameters)
        {
            var label = UiTheme.CreateFieldLabel(parameter.Label);

            var control = CreateControl(parameter);
            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            UiTheme.StyleInput(control);
            _controls[parameter.Name] = control;

            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            panel.Controls.Add(label, 0, row);
            panel.Controls.Add(control, 1, row);
            row++;
        }

        fieldsCard.Controls.Add(panel);

        var runButton = new Button
        {
            Text = "Виконати",
            Width = 130
        };
        UiTheme.StylePrimaryButton(runButton);

        runButton.Click += (_, _) =>
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
            FlowDirection = FlowDirection.RightToLeft
        };
        UiTheme.ApplyFlowLayoutDefaults(buttons);

        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(runButton);
        buttonsCard.Controls.Add(buttons);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(fieldsCard, 0, 1);
        root.Controls.Add(buttonsCard, 0, 2);
        Controls.Add(root);

        AcceptButton = runButton;
        CancelButton = cancelButton;
    }

    private static Control CreateControl(QueryParameter parameter)
    {
        return parameter.Kind switch
        {
            QueryParameterKind.Integer => new NumericUpDown
            {
                Minimum = 0,
                Maximum = 150,
                DecimalPlaces = 0
            },
            QueryParameterKind.Date => new DateTimePicker
            {
                Format = DateTimePickerFormat.Short
            },
            QueryParameterKind.ForeignKey => CreateForeignKeyControl(parameter),
            _ => new TextBox()
        };
    }

    private static Control CreateForeignKeyControl(QueryParameter parameter)
    {
        if (parameter.Lookup == null)
            throw new InvalidOperationException($"Для параметра {parameter.Name} не задано Lookup.");

        var data = Db.GetTable(parameter.Lookup.Sql);

        if (parameter.Lookup.DisplayMember == "doctor_display") AddDoctorDisplayColumn(data);

        var comboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = data,
            ValueMember = parameter.Lookup.ValueMember,
            DisplayMember = parameter.Lookup.DisplayMember
        };

        return comboBox;
    }

    private static void AddDoctorDisplayColumn(DataTable table)
    {
        if (table.Columns.Contains("doctor_display"))
            return;

        if (!table.Columns.Contains("passport") || !table.Columns.Contains("last_name"))
            return;

        table.Columns.Add("doctor_display", typeof(string));

        foreach (DataRow row in table.Rows)
        {
            var passport = row["passport"]?.ToString() ?? "";
            var lastName = row["last_name"]?.ToString() ?? "";
            var firstName = table.Columns.Contains("first_name")
                ? row["first_name"]?.ToString() ?? ""
                : "";

            row["doctor_display"] = $"{passport} - {lastName} {firstName}".Trim();
        }
    }

    private Dictionary<string, object?> CollectValues()
    {
        var result = new Dictionary<string, object?>();

        foreach (var parameter in _config.Parameters)
        {
            var control = _controls[parameter.Name];

            object? value = parameter.Kind switch
            {
                QueryParameterKind.Integer => Convert.ToInt32(((NumericUpDown)control).Value),
                QueryParameterKind.Date => ((DateTimePicker)control).Value.Date,
                QueryParameterKind.ForeignKey => ((ComboBox)control).SelectedValue,
                _ => ((TextBox)control).Text.Trim()
            };

            if (value is DataRowView)
                value = null;

            if (value is string text && string.IsNullOrWhiteSpace(text))
                value = null;

            if (parameter.IsRequired && value == null)
                throw new InvalidOperationException($"Параметр \"{parameter.Label}\" є обов'язковим.");

            result[parameter.Name] = value;
        }

        return result;
    }
}
