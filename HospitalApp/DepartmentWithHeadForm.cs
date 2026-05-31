using Npgsql;

namespace HospitalApp;

public sealed class DepartmentWithHeadForm : Form
{
    private readonly TextBox _departmentNameBox = new();
    private readonly NumericUpDown _floorBox = new()
    {
        Minimum = 1,
        Maximum = 200,
        DecimalPlaces = 0
    };

    private readonly TextBox _passportBox = new();
    private readonly TextBox _lastNameBox = new();
    private readonly TextBox _firstNameBox = new();
    private readonly TextBox _middleNameBox = new();
    private readonly TextBox _specializationBox = new();

    public DepartmentWithHeadForm()
    {
        UiTheme.ApplyFormTheme(this);

        Text = "Додати відділення із завідувачем";
        Width = 720;
        Height = 640;
        MinimumSize = new Size(640, 560);
        StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(UiTheme.FormPadding),
            ColumnCount = 1,
            RowCount = 4
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

        var headerCard = UiTheme.CreateCardPanel();
        headerCard.Dock = DockStyle.Fill;
        headerCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        headerCard.Padding = new Padding(14, 6, 14, 6);
        headerCard.Controls.Add(UiTheme.CreateHeaderLabel("Нове відділення та його завідувач"));

        var departmentCard = CreateSectionCard("Відділення");
        var departmentPanel = CreateFieldsPanel();
        AddField(departmentPanel, "Назва відділення", _departmentNameBox, 0);
        AddField(departmentPanel, "Поверх", _floorBox, 1);
        departmentCard.Controls.Add(departmentPanel);

        var doctorCard = CreateSectionCard("Завідувач-лікар");
        var doctorPanel = CreateFieldsPanel();
        AddField(doctorPanel, "Паспорт", _passportBox, 0);
        AddField(doctorPanel, "Прізвище", _lastNameBox, 1);
        AddField(doctorPanel, "Ім'я", _firstNameBox, 2);
        AddField(doctorPanel, "По батькові", _middleNameBox, 3);
        AddField(doctorPanel, "Спеціалізація", _specializationBox, 4);
        doctorCard.Controls.Add(doctorPanel);

        var saveButton = new Button
        {
            Text = "Зберегти",
            Width = 130
        };
        UiTheme.StylePrimaryButton(saveButton);
        saveButton.Click += (_, _) => Save();

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
        buttons.Controls.Add(saveButton);
        buttonsCard.Controls.Add(buttons);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(departmentCard, 0, 1);
        root.Controls.Add(doctorCard, 0, 2);
        root.Controls.Add(buttonsCard, 0, 3);
        Controls.Add(root);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static Panel CreateSectionCard(string title)
    {
        var card = UiTheme.CreateCardPanel();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        card.Padding = new Padding(18, 42, 18, 14);

        var titleLabel = UiTheme.CreateSectionLabel(title);
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Height = 30;
        titleLabel.Padding = new Padding(18, 0, 0, 0);
        card.Controls.Add(titleLabel);

        return card;
    }

    private static TableLayoutPanel CreateFieldsPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(panel);
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        return panel;
    }

    private static void AddField(TableLayoutPanel panel, string labelText, Control control, int row)
    {
        var label = UiTheme.CreateFieldLabel(labelText);

        control.Dock = DockStyle.Fill;
        UiTheme.StyleInput(control);

        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        panel.Controls.Add(label, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private void Save()
    {
        try
        {
            var values = CollectValues();
            SaveDepartmentWithHead(values);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.GetFriendlyError(ex), "Помилка додавання відділення");
        }
    }

    private DepartmentWithHeadValues CollectValues()
    {
        var departmentName = RequiredText(_departmentNameBox, "Назва відділення");
        var passport = RequiredText(_passportBox, "Паспорт");
        var lastName = RequiredText(_lastNameBox, "Прізвище");
        var firstName = RequiredText(_firstNameBox, "Ім'я");
        var specialization = RequiredText(_specializationBox, "Спеціалізація");
        var middleName = _middleNameBox.Text.Trim();

        return new DepartmentWithHeadValues(
            departmentName,
            Convert.ToInt32(_floorBox.Value),
            passport,
            lastName,
            firstName,
            string.IsNullOrWhiteSpace(middleName) ? null : middleName,
            specialization);
    }

    private static string RequiredText(TextBox textBox, string label)
    {
        var value = textBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Поле \"{label}\" є обов'язковим.");

        return value;
    }

    private static void SaveDepartmentWithHead(DepartmentWithHeadValues values)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            Execute(
                connection,
                transaction,
                "SET CONSTRAINTS ALL DEFERRED");

            Execute(
                connection,
                transaction,
                """
                INSERT INTO departments(name, floor, head_passport)
                VALUES (@department_name, @floor, @head_passport)
                """,
                new NpgsqlParameter("department_name", values.DepartmentName),
                new NpgsqlParameter("floor", values.Floor),
                new NpgsqlParameter("head_passport", values.Passport));

            Execute(
                connection,
                transaction,
                """
                INSERT INTO doctors(passport, last_name, first_name, middle_name, specialization, department_name)
                VALUES (@passport, @last_name, @first_name, @middle_name, @specialization, @department_name)
                """,
                new NpgsqlParameter("passport", values.Passport),
                new NpgsqlParameter("last_name", values.LastName),
                new NpgsqlParameter("first_name", values.FirstName),
                new NpgsqlParameter("middle_name", values.MiddleName ?? (object)DBNull.Value),
                new NpgsqlParameter("specialization", values.Specialization),
                new NpgsqlParameter("department_name", values.DepartmentName));

            transaction.Commit();
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Preserve the original database error for the user.
            }

            throw;
        }
    }

    private static void Execute(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string sql,
        params NpgsqlParameter[] parameters)
    {
        using var command = new NpgsqlCommand(sql, connection, transaction);

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        command.ExecuteNonQuery();
    }

    private sealed record DepartmentWithHeadValues(
        string DepartmentName,
        int Floor,
        string Passport,
        string LastName,
        string FirstName,
        string? MiddleName,
        string Specialization);
}
