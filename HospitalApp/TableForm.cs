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
        UiTheme.ApplyFormTheme(this);
        _config = config;

        Text = _config.Title;
        Width = 1000;
        Height = 650;
        MinimumSize = new Size(850, 520);
        StartPosition = FormStartPosition.CenterParent;

        UiTheme.StyleDataGridView(_grid);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(UiTheme.FormPadding),
            ColumnCount = 1,
            RowCount = 3
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var headerCard = UiTheme.CreateCardPanel();
        headerCard.Dock = DockStyle.Fill;
        headerCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        headerCard.Padding = new Padding(24, 6, 24, 6);

        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(headerLayout);
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = UiTheme.CreateHeaderLabel(_config.Title);
        title.TextAlign = ContentAlignment.MiddleLeft;
        var subtitle = UiTheme.CreateSubHeaderLabel("Перегляд і редагування даних таблиці");
        subtitle.TextAlign = ContentAlignment.MiddleLeft;

        headerLayout.Controls.Add(title, 0, 0);
        headerLayout.Controls.Add(subtitle, 0, 1);
        headerCard.Controls.Add(headerLayout);

        var gridCard = UiTheme.CreateCardPanel();
        gridCard.Dock = DockStyle.Fill;
        gridCard.Margin = new Padding(0);
        gridCard.Padding = new Padding(1);
        gridCard.Controls.Add(_grid);

        var addButton = new Button
        {
            Text = "Додати",
            Width = 135,
            Enabled = _config.AllowAdd
        };
        UiTheme.StylePrimaryButton(addButton);

        var editButton = new Button
        {
            Text = "Редагувати",
            Width = 135,
            Enabled = _config.AllowEdit
        };
        UiTheme.StyleSecondaryButton(editButton);

        var deleteButton = new Button
        {
            Text = "Видалити",
            Width = 135,
            Enabled = _config.AllowDelete
        };
        UiTheme.StyleDangerButton(deleteButton);

        var refreshButton = new Button
        {
            Text = "Оновити",
            Width = 135
        };
        UiTheme.StyleSecondaryButton(refreshButton);

        var backButton = new Button
        {
            Text = "Повернутись до меню",
            Width = 210
        };
        UiTheme.StyleTextButton(backButton);

        addButton.Click += (_, _) => AddRow();
        editButton.Click += (_, _) => EditRow();
        deleteButton.Click += (_, _) => DeleteRow();
        refreshButton.Click += (_, _) => LoadData();
        backButton.Click += (_, _) => Close();

        var toolbarCard = UiTheme.CreateCardPanel();
        toolbarCard.Dock = DockStyle.Fill;
        toolbarCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        toolbarCard.Padding = new Padding(20, 0, 20, 0);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true
        };
        UiTheme.ApplyFlowLayoutDefaults(buttons);

        buttons.Controls.Add(addButton);
        buttons.Controls.Add(editButton);
        buttons.Controls.Add(deleteButton);
        buttons.Controls.Add(refreshButton);
        buttons.Controls.Add(backButton);

        toolbarCard.Controls.Add(buttons);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(toolbarCard, 0, 1);
        root.Controls.Add(gridCard, 0, 2);
        Controls.Add(root);

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
