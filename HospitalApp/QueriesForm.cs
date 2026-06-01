namespace HospitalApp;

public sealed class QueriesForm : Form
{
    public QueriesForm()
    {
        UiTheme.ApplyFormTheme(this);

        Text = "SQL-запити";
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
        headerCard.Controls.Add(CreateHeader());

        var listCard = UiTheme.CreateCardPanel();
        listCard.Dock = DockStyle.Fill;
        listCard.Margin = new Padding(0);
        listCard.Padding = new Padding(12);

        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Transparent
        };
        AddQueryCards(scrollPanel);
        listCard.Controls.Add(scrollPanel);

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
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        UiTheme.ApplyFlowLayoutDefaults(bottomPanel);
        bottomPanel.Controls.Add(closeButton);
        bottomCard.Controls.Add(bottomPanel);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(listCard, 0, 1);
        root.Controls.Add(bottomCard, 0, 2);
        Controls.Add(root);
    }

    private static Control CreateHeader()
    {
        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(headerLayout);
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = UiTheme.CreateHeaderLabel("SQL-запити");
        title.TextAlign = ContentAlignment.MiddleLeft;
        var subtitle = UiTheme.CreateSubHeaderLabel("Параметризовані запити та запити з множинними порівняннями");
        subtitle.TextAlign = ContentAlignment.MiddleLeft;

        headerLayout.Controls.Add(title, 0, 0);
        headerLayout.Controls.Add(subtitle, 0, 1);
        return headerLayout;
    }

    private void AddQueryCards(Panel scrollPanel)
    {
        var controls = new List<Control>();
        var queries = QueryConfigs.All;

        controls.Add(CreateSectionHeader("Параметризовані запити"));
        for (var index = 0; index < 5 && index < queries.Count; index++)
            controls.Add(QueryUi.CreateQueryCard(queries[index], query => QueryUi.RunQuery(this, query)));

        controls.Add(CreateSectionHeader("Запити з множинними порівняннями"));
        for (var index = 5; index < queries.Count; index++)
            controls.Add(QueryUi.CreateQueryCard(queries[index], query => QueryUi.RunQuery(this, query)));

        for (var index = controls.Count - 1; index >= 0; index--)
            scrollPanel.Controls.Add(controls[index]);
    }

    private static Control CreateSectionHeader(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 36,
            Font = UiTheme.SectionFont,
            ForeColor = UiTheme.TextDark,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(2, 0, 0, 0)
        };
    }

}
