namespace HospitalApp;

public sealed class MainMenuForm : Form
{
    public MainMenuForm()
    {
        UiTheme.ApplyFormTheme(this);

        Text = "Госпіталь - головне меню";
        Width = 1200;
        Height = 760;
        MinimumSize = new Size(1050, 680);
        StartPosition = FormStartPosition.CenterScreen;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0)
        };
        UiTheme.ApplyTableLayoutDefaults(root);
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.Controls.Add(CreateSidebar(), 0, 0);
        root.Controls.Add(CreateContentArea(), 1, 0);

        Controls.Add(root);
    }

    private Control CreateSidebar()
    {
        var sidebar = UiTheme.CreateSidebarPanel();
        sidebar.Padding = new Padding(20);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));

        var navigation = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0)
        };
        UiTheme.ApplyFlowLayoutDefaults(navigation);

        navigation.Controls.Add(CreateSidebarHeader());

        AddNavigationGroup(navigation, "ОСНОВНІ ДАНІ", new[]
        {
            "departments",
            "doctors",
            "patients",
            "rooms"
        });

        AddNavigationGroup(navigation, "СПЕЦІАЛІЗАЦІЇ", new[]
        {
            "therapists",
            "surgeons"
        });

        AddNavigationGroup(navigation, "МЕДИЧНІ ЗВ'ЯЗКИ", new[]
        {
            "visit_records",
            "appointments",
            "doctor_consultations"
        });

        var exitButton = CreateSidebarNavButton("Вихід", primary: false);
        UiTheme.StyleDangerButton(exitButton);
        exitButton.Width = 240;
        exitButton.Height = UiTheme.ButtonHeight;
        exitButton.MinimumSize = new Size(240, UiTheme.ButtonHeight);
        exitButton.MaximumSize = new Size(240, UiTheme.ButtonHeight);
        exitButton.TextAlign = ContentAlignment.MiddleLeft;
        exitButton.Padding = new Padding(14, 0, 10, 0);
        exitButton.Margin = new Padding(0);
        exitButton.Click += (_, _) => Close();

        layout.Controls.Add(navigation, 0, 0);
        layout.Controls.Add(exitButton, 0, 1);
        sidebar.Controls.Add(layout);

        return sidebar;
    }

    private static Control CreateSidebarHeader()
    {
        var header = new Panel
        {
            Width = 240,
            Height = 92,
            Margin = new Padding(0, 0, 0, 16),
            BackColor = Color.Transparent
        };

        var accent = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 3,
            BackColor = UiTheme.Primary,
            Margin = new Padding(0, 10, 0, 0)
        };

        var subtitle = UiTheme.CreateSidebarSubtitleLabel("Powered by PostgreSQL.");
        var title = UiTheme.CreateSidebarTitleLabel("HospitalApp");

        header.Controls.Add(accent);
        header.Controls.Add(subtitle);
        header.Controls.Add(title);

        return header;
    }

    private void AddNavigationGroup(FlowLayoutPanel parent, string title, IEnumerable<string> tableNames)
    {
        parent.Controls.Add(UiTheme.CreateSidebarGroupLabel(title));

        foreach (var tableName in tableNames)
        {
            var config = FindConfig(tableName);
            var button = CreateSidebarNavButton(config.Title, primary: false);

            button.Click += (_, _) =>
            {
                using var form = new TableForm(config);
                form.ShowDialog(this);
            };

            parent.Controls.Add(button);
        }
    }

    private static Button CreateSidebarNavButton(string text, bool primary)
    {
        var button = new Button
        {
            Text = text
        };

        if (primary)
        {
            UiTheme.StylePrimaryButton(button);
            button.Width = 240;
            button.Height = UiTheme.ButtonHeight;
            button.MinimumSize = new Size(240, UiTheme.ButtonHeight);
            button.MaximumSize = new Size(240, UiTheme.ButtonHeight);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 10, 0);
            button.Margin = new Padding(0, 0, 0, 8);
        }
        else
        {
            UiTheme.StyleSidebarButton(button);
        }

        return button;
    }

    private static TableConfig FindConfig(string tableName)
    {
        return TableConfigs.All.First(config => config.TableName == tableName);
    }

    private Control CreateContentArea()
    {
        var content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.MainBack,
            Padding = new Padding(28)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreateWorkspaceHeader(), 0, 0);
        layout.Controls.Add(CreateQueryWorkspace(), 0, 1);
        content.Controls.Add(layout);

        return content;
    }

    private static Control CreateWorkspaceHeader()
    {
        var card = UiTheme.CreateMainCard();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 0, 12);
        card.Padding = new Padding(22, 12, 22, 12);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        UiTheme.ApplyTableLayoutDefaults(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = UiTheme.CreateMainHeaderLabel("База даних госпіталя");
        var subtitle = UiTheme.CreateSubHeaderLabel("  Супер пупер дупер крутий госпіталь.");
        subtitle.TextAlign = ContentAlignment.MiddleLeft;
        var line = UiTheme.CreateSmallMutedLabel("");

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(line, 0, 2);

        card.Controls.Add(layout);
        return card;
    }

    private Control CreateQueryWorkspace()
    {
        var card = UiTheme.CreateMainCard();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0);
        card.Padding = new Padding(12);

        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Transparent
        };

        var cards = QueryConfigs.All
            .Select(query => QueryUi.CreateQueryCard(query, queryConfig => QueryUi.RunQuery(this, queryConfig)))
            .ToList();

        for (var index = cards.Count - 1; index >= 0; index--)
            scrollPanel.Controls.Add(cards[index]);

        card.Controls.Add(scrollPanel);
        return card;
    }
}
