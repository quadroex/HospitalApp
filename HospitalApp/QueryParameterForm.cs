using System.Data;

namespace HospitalApp;

public sealed class QueryParameterForm : Form
{
    private readonly QueryConfig _config;
    private readonly Dictionary<string, Control> _controls = new();

    public Dictionary<string, object?> Values { get; private set; } = new();

    public QueryParameterForm(QueryConfig config)
    {
        _config = config;

        Text = _config.Title;
        Width = 560;
        Height = 360;
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

        foreach (var parameter in _config.Parameters)
        {
            var label = new Label
            {
                Text = parameter.Label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };

            var control = CreateControl(parameter);
            control.Dock = DockStyle.Fill;
            _controls[parameter.Name] = control;

            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(label);
            panel.Controls.Add(control);
        }

        var runButton = new Button
        {
            Text = "Виконати",
            Width = 120,
            Height = 35
        };

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
        buttons.Controls.Add(runButton);

        Controls.Add(panel);
        Controls.Add(buttons);

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

        var comboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = data,
            ValueMember = parameter.Lookup.ValueMember,
            DisplayMember = parameter.Lookup.DisplayMember
        };

        if (data.Rows.Count > 0)
            comboBox.SelectedIndex = 0;

        return comboBox;
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
