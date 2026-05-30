using Npgsql;

namespace HospitalApp;

public sealed class QueriesForm : Form
{
    public QueriesForm()
    {
        Text = "Запити";
        Width = 780;
        Height = 680;
        StartPosition = FormStartPosition.CenterParent;

        var title = new Label
        {
            Text = "Запити до бази даних",
            Dock = DockStyle.Top,
            Height = 55,
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(16)
        };

        foreach (var query in QueryConfigs.All)
            panel.Controls.Add(CreateQueryPanel(query));

        var closeButton = new Button
        {
            Text = "Повернутись до головного меню",
            Width = 260,
            Height = 38
        };

        closeButton.Click += (_, _) => Close();

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12)
        };

        bottomPanel.Controls.Add(closeButton);

        Controls.Add(panel);
        Controls.Add(bottomPanel);
        Controls.Add(title);
    }

    private Control CreateQueryPanel(QueryConfig query)
    {
        var container = new Panel
        {
            Width = 720,
            Height = 110,
            Margin = new Padding(0, 0, 0, 10),
            BorderStyle = BorderStyle.FixedSingle
        };

        var button = new Button
        {
            Text = query.Title,
            Width = 260,
            Height = 82,
            Left = 12,
            Top = 12
        };

        var description = new Label
        {
            Text = query.Description,
            Left = 285,
            Top = 14,
            Width = 415,
            Height = 80,
            AutoEllipsis = true
        };

        button.Click += (_, _) => RunQuery(query);

        container.Controls.Add(button);
        container.Controls.Add(description);

        return container;
    }

    private void RunQuery(QueryConfig query)
    {
        try
        {
            var values = new Dictionary<string, object?>();

            if (query.Parameters.Count > 0)
            {
                using var parameterForm = new QueryParameterForm(query);

                if (parameterForm.ShowDialog(this) != DialogResult.OK)
                    return;

                values = parameterForm.Values;
            }

            var parameters = query.Parameters
                .Select(parameter => new NpgsqlParameter(parameter.Name, values[parameter.Name] ?? DBNull.Value))
                .ToArray();

            var data = Db.GetTable(query.Sql, parameters);

            using var resultForm = new QueryResultForm(query, data);
            resultForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.GetFriendlyError(ex), "Помилка виконання запиту");
        }
    }
}
