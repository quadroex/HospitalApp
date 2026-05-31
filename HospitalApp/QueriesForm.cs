using Npgsql;

namespace HospitalApp;

public sealed class QueriesForm : Form
{
    public QueriesForm()
    {
        UiTheme.ApplyFormTheme(this);

        Text = "Запити";
        Width = 1040;
        Height = 760;
        MinimumSize = new Size(880, 640);
        StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(UiTheme.FormPadding),
            ColumnCount = 1,
            RowCount = 3
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

        var headerCard = UiTheme.CreateCardPanel();
        headerCard.Dock = DockStyle.Fill;
        headerCard.Margin = new Padding(0, 0, 0, UiTheme.Spacing);
        headerCard.Padding = new Padding(14, 6, 14, 6);
        headerCard.Controls.Add(UiTheme.CreateHeaderLabel("Запити до бази даних"));

        var listCard = UiTheme.CreateCardPanel();
        listCard.Dock = DockStyle.Fill;
        listCard.Margin = new Padding(0);
        listCard.Padding = new Padding(12);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        UiTheme.ApplyFlowLayoutDefaults(panel);

        var queries = QueryConfigs.All;
        for (var index = 0; index < queries.Count; index++)
        {
            if (index == 0)
                panel.Controls.Add(CreateSectionHeader("Параметризовані запити"));
            else if (index == 5)
                panel.Controls.Add(CreateSectionHeader("Запити з множинними порівняннями"));

            panel.Controls.Add(CreateQueryPanel(queries[index]));
        }

        listCard.Controls.Add(panel);

        var closeButton = new Button
        {
            Text = "Повернутись до головного меню",
            Width = 280
        };
        UiTheme.StyleTextButton(closeButton);
        closeButton.Click += (_, _) => Close();

        var bottomCard = UiTheme.CreateCardPanel();
        bottomCard.Dock = DockStyle.Fill;
        bottomCard.Margin = new Padding(0, UiTheme.Spacing, 0, 0);
        bottomCard.Padding = new Padding(10, 8, 10, 8);

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        UiTheme.ApplyFlowLayoutDefaults(bottomPanel);
        bottomPanel.Controls.Add(closeButton);
        bottomCard.Controls.Add(bottomPanel);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(listCard, 0, 1);
        root.Controls.Add(bottomCard, 0, 2);
        Controls.Add(root);
    }

    private static Control CreateSectionHeader(string text)
    {
        var label = UiTheme.CreateSectionLabel(text);
        label.Width = 930;
        label.Height = 34;
        label.Margin = new Padding(4, 4, 4, 8);
        return label;
    }

    private Control CreateQueryPanel(QueryConfig query)
    {
        var container = UiTheme.CreateCardPanel();
        container.Width = 930;
        container.Height = 132;
        container.Margin = new Padding(4, 0, 4, 12);
        container.Padding = new Padding(14);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(layout);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = query.Title,
            Dock = DockStyle.Fill,
            Font = UiTheme.SectionFont,
            ForeColor = UiTheme.Text,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var description = new Label
        {
            Text = query.Description,
            Dock = DockStyle.Fill,
            Font = UiTheme.TextFont,
            ForeColor = UiTheme.MutedText,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        };

        var button = new Button
        {
            Text = "Виконати",
            Width = 160,
            Anchor = AnchorStyles.Right | AnchorStyles.Top
        };
        UiTheme.StylePrimaryButton(button);
        button.Click += (_, _) => RunQuery(query);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(description, 0, 1);
        layout.Controls.Add(button, 1, 0);
        layout.SetRowSpan(button, 2);

        container.Controls.Add(layout);
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
