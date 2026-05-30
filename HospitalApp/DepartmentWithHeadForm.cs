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
        Text = "Додати відділення із завідувачем";
        Width = 620;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;

        var title = new Label
        {
            Text = "Нове відділення та його завідувач",
            Dock = DockStyle.Top,
            Height = 45,
            Font = new Font(Font.FontFamily, 13, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true,
            Padding = new Padding(12)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        AddField(panel, "Назва відділення", _departmentNameBox);
        AddField(panel, "Поверх", _floorBox);
        AddSeparator(panel, "Завідувач");
        AddField(panel, "Паспорт", _passportBox);
        AddField(panel, "Прізвище", _lastNameBox);
        AddField(panel, "Ім'я", _firstNameBox);
        AddField(panel, "По батькові", _middleNameBox);
        AddField(panel, "Спеціалізація", _specializationBox);

        var saveButton = new Button
        {
            Text = "Зберегти",
            Width = 120,
            Height = 35
        };

        saveButton.Click += (_, _) => Save();

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
        Controls.Add(title);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static void AddField(TableLayoutPanel panel, string labelText, Control control)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true
        };

        control.Dock = DockStyle.Fill;

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(label);
        panel.Controls.Add(control);
    }

    private static void AddSeparator(TableLayoutPanel panel, string text)
    {
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 4)
        };

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(label);
        panel.SetColumnSpan(label, 2);
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
