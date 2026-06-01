using Npgsql;

namespace HospitalApp;

public static class QueryUi
{
    public static Control CreateQueryCard(QueryConfig query, Action<QueryConfig> runAction)
    {
        var container = UiTheme.CreateCardPanel();
        container.Dock = DockStyle.Top;
        container.Height = 116;
        container.Margin = new Padding(0);
        container.Padding = new Padding(14);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(layout);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = query.Title,
            Dock = DockStyle.Fill,
            Font = UiTheme.SectionFont,
            ForeColor = UiTheme.TextDark,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var description = new Label
        {
            Text = query.Description,
            Dock = DockStyle.Fill,
            Font = UiTheme.TextFont,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        };

        var button = new Button
        {
            Text = "Виконати",
            Width = 120,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        UiTheme.StylePrimaryButton(button);
        button.Click += (_, _) => runAction(query);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(description, 0, 1);
        layout.Controls.Add(button, 1, 0);
        layout.SetRowSpan(button, 2);

        container.Controls.Add(layout);

        var wrapper = new Panel
        {
            Dock = DockStyle.Top,
            Height = 126,
            Padding = new Padding(0, 0, 0, 10),
            BackColor = Color.Transparent
        };
        wrapper.Controls.Add(container);

        return wrapper;
    }

    public static void RunQuery(Form owner, QueryConfig query)
    {
        try
        {
            var values = new Dictionary<string, object?>();

            if (query.Parameters.Count > 0)
            {
                using var parameterForm = new QueryParameterForm(query);

                if (parameterForm.ShowDialog(owner) != DialogResult.OK)
                    return;

                values = parameterForm.Values;
            }

            var parameters = query.Parameters
                .Select(parameter => new NpgsqlParameter(parameter.Name, values[parameter.Name] ?? DBNull.Value))
                .ToArray();

            var data = Db.GetTable(query.Sql, parameters);

            using var resultForm = new QueryResultForm(query, data);
            resultForm.ShowDialog(owner);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.GetFriendlyError(ex), "Помилка виконання запиту");
        }
    }
}
