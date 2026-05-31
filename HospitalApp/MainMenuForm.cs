namespace HospitalApp;

public sealed class MainMenuForm : Form
{
    public MainMenuForm()
    {
        UiTheme.ApplyFormTheme(this);

        Text = "Госпіталь - головне меню";
        Width = 1040;
        Height = 740;
        MinimumSize = new Size(1000, 720);
        StartPosition = FormStartPosition.CenterScreen;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 1,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 148));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(CreateHeaderCard(), 0, 0);
        root.Controls.Add(CreateDashboard(), 0, 1);

        Controls.Add(root);
    }

    private static Control CreateHeaderCard()
    {
        var card = UiTheme.CreateCardPanel();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 0, 14);
        card.Padding = new Padding(24, 16, 24, 16);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        UiTheme.ApplyTableLayoutDefaults(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = UiTheme.CreateHeaderLabel("База даних госпіталя");
        title.TextAlign = ContentAlignment.MiddleLeft;

        var subtitle = UiTheme.CreateSubHeaderLabel("Навчальний застосунок для роботи з PostgreSQL БД");
        subtitle.TextAlign = ContentAlignment.MiddleLeft;

        var description = new Label
        {
            Text = "Форми введення даних, зв'язки між сутностями, параметризовані запити та запити з множинними порівняннями.",
            Dock = DockStyle.Fill,
            Font = UiTheme.TextFont,
            ForeColor = UiTheme.MutedText,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(description, 0, 2);
        card.Controls.Add(layout);

        return card;
    }

    private Control CreateDashboard()
    {
        var dashboard = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3
        };
        UiTheme.ApplyTableLayoutDefaults(dashboard);
        dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

        dashboard.Controls.Add(CreateNavigationCard(
            "Основні дані",
            new[] { "departments", "doctors", "patients", "rooms" },
            primary: false), 0, 0);

        dashboard.Controls.Add(CreateNavigationCard(
            "Медичні події та зв'язки",
            new[] { "visit_records", "appointments", "doctor_consultations" },
            primary: false), 1, 0);

        dashboard.Controls.Add(CreateNavigationCard(
            "Спеціалізації лікарів",
            new[] { "therapists", "surgeons" },
            primary: false), 0, 1);

        dashboard.Controls.Add(CreateQueriesCard(), 1, 1);

        var systemCard = CreateSystemCard();
        dashboard.Controls.Add(systemCard, 0, 2);
        dashboard.SetColumnSpan(systemCard, 2);

        return dashboard;
    }

    private Control CreateNavigationCard(string title, IEnumerable<string> tableNames, bool primary)
    {
        var card = CreateDashboardCard(title);
        var panel = CreateButtonPanel();

        foreach (var tableName in tableNames)
        {
            var config = TableConfigs.All.First(item => item.TableName == tableName);
            var button = CreateDashboardButton(config.Title, primary);

            button.Click += (_, _) =>
            {
                using var form = new TableForm(config);
                form.ShowDialog(this);
            };

            panel.Controls.Add(button);
        }

        card.Controls.Add(panel);
        return card;
    }

    private Control CreateQueriesCard()
    {
        var card = CreateDashboardCard("Аналітика");
        var panel = CreateButtonPanel();

        var queriesButton = CreateDashboardButton("Запити", primary: true);
        queriesButton.Click += (_, _) =>
        {
            using var form = new QueriesForm();
            form.ShowDialog(this);
        };

        panel.Controls.Add(queriesButton);
        card.Controls.Add(panel);
        return card;
    }

    private Control CreateSystemCard()
    {
        var card = CreateDashboardCard("Система");
        card.Margin = new Padding(0, 10, 0, 0);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 12, 0, 0)
        };
        UiTheme.ApplyFlowLayoutDefaults(panel);

        var exitButton = CreateDashboardButton("Вихід", primary: false);
        UiTheme.StyleDangerButton(exitButton);
        exitButton.Click += (_, _) => Close();

        panel.Controls.Add(exitButton);
        card.Controls.Add(panel);
        return card;
    }

    private static Panel CreateDashboardCard(string title)
    {
        var card = UiTheme.CreateCardPanel();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 14, 14);
        card.Padding = new Padding(18, 48, 18, 16);

        var titleLabel = UiTheme.CreateSectionLabel(title);
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Height = 36;
        titleLabel.Padding = new Padding(18, 4, 0, 0);
        card.Controls.Add(titleLabel);

        return card;
    }

    private static FlowLayoutPanel CreateButtonPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = false,
            Padding = new Padding(0)
        };
        UiTheme.ApplyFlowLayoutDefaults(panel);
        return panel;
    }

    private static Button CreateDashboardButton(string text, bool primary)
    {
        var button = new Button
        {
            Text = text,
            Width = 210,
            Height = UiTheme.ButtonHeight
        };

        if (primary)
            UiTheme.StylePrimaryButton(button);
        else
            UiTheme.StyleSecondaryButton(button);

        return button;
    }
}
