using System.Data;

namespace HospitalApp;

public sealed class QueryResultForm : Form
{
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

    public QueryResultForm(QueryConfig config, DataTable data)
    {
        UiTheme.ApplyFormTheme(this);
        UiTheme.StyleDataGridView(_grid);

        Text = config.Title;
        Width = 1120;
        Height = 700;
        MinimumSize = new Size(920, 620);
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
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

        var headerCard = UiTheme.CreateCardPanel();
        headerCard.Dock = DockStyle.Fill;
        headerCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        headerCard.Padding = new Padding(14, 6, 14, 6);
        headerCard.Controls.Add(UiTheme.CreateHeaderLabel(config.Title));

        var gridCard = UiTheme.CreateCardPanel();
        gridCard.Dock = DockStyle.Fill;
        gridCard.Margin = new Padding(0);
        gridCard.Padding = new Padding(1);
        _grid.DataSource = data;
        gridCard.Controls.Add(_grid);

        var status = new Label
        {
            Text = data.Rows.Count == 0
                ? "Запит виконано, але рядків не знайдено."
                : $"Знайдено рядків: {data.Rows.Count}",
            Dock = DockStyle.Fill,
            ForeColor = data.Rows.Count == 0 ? UiTheme.Danger : UiTheme.MutedText,
            Font = UiTheme.TextFont,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0)
        };

        var closeButton = new Button
        {
            Text = "Повернутись до меню запитів",
            Width = 270
        };
        UiTheme.StyleTextButton(closeButton);
        closeButton.Click += (_, _) => Close();

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
        buttons.Controls.Add(closeButton);
        buttonsCard.Controls.Add(buttons);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(gridCard, 0, 1);
        root.Controls.Add(status, 0, 2);
        root.Controls.Add(buttonsCard, 0, 3);
        Controls.Add(root);
    }
}
