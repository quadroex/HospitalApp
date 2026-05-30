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
        Text = config.Title;
        Width = 1100;
        Height = 650;
        StartPosition = FormStartPosition.CenterParent;

        var title = new Label
        {
            Text = config.Title,
            Dock = DockStyle.Top,
            Height = 45,
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var status = new Label
        {
            Text = data.Rows.Count == 0
                ? "Запит виконано, але рядків не знайдено."
                : $"Знайдено рядків: {data.Rows.Count}",
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };

        var closeButton = new Button
        {
            Text = "Повернутись до меню запитів",
            Width = 230,
            Height = 35
        };

        closeButton.Click += (_, _) => Close();

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12)
        };

        buttons.Controls.Add(closeButton);

        _grid.DataSource = data;

        Controls.Add(_grid);
        Controls.Add(status);
        Controls.Add(buttons);
        Controls.Add(title);
    }
}
