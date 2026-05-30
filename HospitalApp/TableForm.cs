using Npgsql;
using System.Data;

namespace HospitalApp;

public sealed class TableForm : Form
{
    private readonly TableConfig _config;

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public TableForm(TableConfig config)
    {
        _config = config;

        Text = _config.Title;
        Width = 1000;
        Height = 650;
        StartPosition = FormStartPosition.CenterParent;

        var title = new Label
        {
            Text = _config.Title,
            Dock = DockStyle.Top,
            Height = 45,
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var addButton = new Button
        {
            Text = "Додати",
            Width = 130,
            Enabled = _config.AllowAdd
        };

        var editButton = new Button
        {
            Text = "Редагувати",
            Width = 130,
            Enabled = _config.AllowEdit
        };

        var deleteButton = new Button
        {
            Text = "Видалити",
            Width = 130,
            Enabled = _config.AllowDelete
        };

        var refreshButton = new Button
        {
            Text = "Оновити",
            Width = 130
        };

        var backButton = new Button
        {
            Text = "Повернутись до меню",
            Width = 180
        };

        addButton.Click += (_, _) => AddRow();
        editButton.Click += (_, _) => EditRow();
        deleteButton.Click += (_, _) => DeleteRow();
        refreshButton.Click += (_, _) => LoadData();
        backButton.Click += (_, _) => Close();

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(10),
            FlowDirection = FlowDirection.LeftToRight
        };

        buttons.Controls.Add(addButton);
        buttons.Controls.Add(editButton);
        buttons.Controls.Add(deleteButton);
        buttons.Controls.Add(refreshButton);
        buttons.Controls.Add(backButton);

        Controls.Add(_grid);
        Controls.Add(buttons);
        Controls.Add(title);

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            _grid.DataSource = Db.GetTable(_config.SelectSql);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.GetFriendlyError(ex), "Помилка завантаження даних");
        }
    }

    private DataRowView? GetSelectedRow()
    {
        if (_grid.CurrentRow?.DataBoundItem is DataRowView row)
            return row;

        MessageBox.Show("Виберіть рядок у таблиці.");
        return null;
    }

    private void AddRow()
    {
        if (_config.TableName == "departments")
        {
            using var departmentDialog = new DepartmentWithHeadForm();

            if (departmentDialog.ShowDialog(this) == DialogResult.OK)
                LoadData();

            return;
        }

        try
        {
            using var dialog = new RowEditForm(_config);

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var values = dialog.Values;

            var columns = _config.Columns;
            var tableName = QuoteIdentifier(_config.TableName);
            var columnNames = string.Join(", ", columns.Select(column => QuoteIdentifier(column.Name)));
            var parameterNames = string.Join(", ", columns.Select(column => $"@p_{column.Name}"));

            var sql = $"""
                INSERT INTO {tableName} ({columnNames})
                VALUES ({parameterNames})
                """;

            var parameters = columns
                .Select(column => CreateParameter($"p_{column.Name}", values[column.Name]))
                .ToArray();

            Db.Execute(sql, parameters);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.GetFriendlyError(ex), "Помилка додавання");
        }
    }

    private void EditRow()
    {
        var selectedRow = GetSelectedRow();
        if (selectedRow == null)
            return;

        try
        {
            using var dialog = new RowEditForm(_config, selectedRow);

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var values = dialog.Values;

            var updateColumns = _config.Columns
                .Where(column => !column.IsPrimaryKey)
                .ToList();

            if (updateColumns.Count == 0)
            {
                MessageBox.Show("У цій таблиці немає полів для редагування.");
                return;
            }

            var tableName = QuoteIdentifier(_config.TableName);
            var setPart = string.Join(", ", updateColumns.Select(column => $"{QuoteIdentifier(column.Name)} = @p_{column.Name}"));
            var wherePart = string.Join(" AND ", _config.PrimaryKeys.Select(column => $"{QuoteIdentifier(column.Name)} = @k_{column.Name}"));

            var sql = $"""
                UPDATE {tableName}
                SET {setPart}
                WHERE {wherePart}
                """;

            var parameters = new List<NpgsqlParameter>();

            foreach (var column in updateColumns)
                parameters.Add(CreateParameter($"p_{column.Name}", values[column.Name]));

            foreach (var keyColumn in _config.PrimaryKeys)
                parameters.Add(CreateParameter($"k_{keyColumn.Name}", selectedRow.Row[keyColumn.Name]));

            Db.Execute(sql, parameters.ToArray());
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.GetFriendlyError(ex), "Помилка редагування");
        }
    }

    private void DeleteRow()
    {
        var selectedRow = GetSelectedRow();
        if (selectedRow == null)
            return;

        var result = MessageBox.Show(
            "Видалити вибраний запис?",
            "Підтвердження",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
            return;

        try
        {
            var tableName = QuoteIdentifier(_config.TableName);
            var wherePart = string.Join(" AND ", _config.PrimaryKeys.Select(column => $"{QuoteIdentifier(column.Name)} = @k_{column.Name}"));

            var sql = $"""
                DELETE FROM {tableName}
                WHERE {wherePart}
                """;

            var parameters = _config.PrimaryKeys
                .Select(column => CreateParameter($"k_{column.Name}", selectedRow.Row[column.Name]))
                .ToArray();

            Db.Execute(sql, parameters);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.GetFriendlyError(ex), "Помилка видалення");
        }
    }

    private static NpgsqlParameter CreateParameter(string name, object? value)
    {
        if (value == DBNull.Value)
            value = null;

        return new NpgsqlParameter(name, value ?? DBNull.Value);
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
