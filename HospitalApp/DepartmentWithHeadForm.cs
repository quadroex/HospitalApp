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
        Width = 760;
        Height = 680;
        MinimumSize = new Size(720, 620);
        StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(UiTheme.FormPadding),
            ColumnCount = 1,
            RowCount = 4
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));

        var headerCard = UiTheme.CreateMainCard();
        headerCard.Dock = DockStyle.Fill;
        headerCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        headerCard.Padding = new Padding(18, 8, 18, 8);

        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(headerLayout);
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = UiTheme.CreateHeaderLabel("Нове відділення із завідувачем");
        title.TextAlign = ContentAlignment.MiddleLeft;
        var subtitle = UiTheme.CreateSubHeaderLabel("Створення взаємозалежних записів в одній транзакції");
        subtitle.TextAlign = ContentAlignment.MiddleLeft;

        headerLayout.Controls.Add(title, 0, 0);
        headerLayout.Controls.Add(subtitle, 0, 1);
        headerCard.Controls.Add(headerLayout);

        var departmentPanel = CreateFieldsPanel(2);
        AddField(departmentPanel, "Назва відділення", _departmentNameBox, 0);
        AddField(departmentPanel, "Поверх", _floorBox, 1);
        var departmentCard = CreateSectionCard("Відділення", departmentPanel);

        var doctorPanel = CreateFieldsPanel(5);
        AddField(doctorPanel, "Паспорт", _passportBox, 0);
        AddField(doctorPanel, "Прізвище", _lastNameBox, 1);
        AddField(doctorPanel, "Ім'я", _firstNameBox, 2);
        AddField(doctorPanel, "По батькові", _middleNameBox, 3);
        AddField(doctorPanel, "Спеціалізація", _specializationBox, 4);
        var doctorCard = CreateSectionCard("Завідувач-лікар", doctorPanel);

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

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 14, 0, 0)
        };
        UiTheme.ApplyFlowLayoutDefaults(buttons);
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(departmentCard, 0, 1);
        root.Controls.Add(doctorCard, 0, 2);
        root.Controls.Add(buttons, 0, 3);
        Controls.Add(root);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static Panel CreateSectionCard(string title, Control content)
    {
        var card = UiTheme.CreateMainCard();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        card.Padding = new Padding(18);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var titleLabel = UiTheme.CreateSectionLabel(title);
        titleLabel.Padding = new Padding(0, 0, 0, 0);

        content.Dock = DockStyle.Fill;
        layout.Controls.Add(titleLabel, 0, 0);
        layout.Controls.Add(content, 0, 1);
        card.Controls.Add(layout);

        return card;
    }

    private static TableLayoutPanel CreateFieldsPanel(int fieldCount)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = fieldCount + 1
        };
        UiTheme.ApplyTableLayoutDefaults(panel);
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        for (var index = 0; index < fieldCount; index++)
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        return panel;
    }

    private static void AddField(TableLayoutPanel panel, string labelText, Control control, int row)
    {
        var label = UiTheme.CreateFieldLabel(labelText);

        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        UiTheme.StyleInput(control);
        control.Height = 28;
        control.MinimumSize = new Size(0, 28);

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
        var floor = Convert.ToInt32(_floorBox.Value);

        ValidationHelper.ValidateText("Назва відділення", departmentName, required: true, minLength: 2, maxLength: 100);
        ValidationHelper.ValidateInteger("Поверх", floor, minValue: 1, maxValue: 200);
        ValidationHelper.ValidateText("Паспорт", passport, required: true, minLength: 3, maxLength: 20);
        ValidationHelper.ValidateText("Прізвище", lastName, required: true, minLength: 2, maxLength: 50);
        ValidationHelper.ValidateText("Ім'я", firstName, required: true, minLength: 2, maxLength: 50);
        ValidationHelper.ValidateText("По батькові", middleName, required: false, minLength: 2, maxLength: 50);
        ValidationHelper.ValidateText("Спеціалізація", specialization, required: true, minLength: 2, maxLength: 100);

        return new DepartmentWithHeadValues(
            departmentName,
            floor,
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
