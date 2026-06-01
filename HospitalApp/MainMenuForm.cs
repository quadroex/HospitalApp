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

        navigation.Controls.Add(UiTheme.CreateSidebarGroupLabel("АНАЛІТИКА"));
        var queriesButton = CreateSidebarNavButton("SQL-запити", primary: true);
        queriesButton.Click += (_, _) =>
        {
            using var form = new QueriesForm();
            form.ShowDialog(this);
        };
        navigation.Controls.Add(queriesButton);

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

        var subtitle = UiTheme.CreateSidebarSubtitleLabel("PostgreSQL БД");
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

    private static Control CreateContentArea()
    {
        var content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.MainBack,
            Padding = new Padding(28)
        };

        var note = CreateInstructionCard();
        note.Dock = DockStyle.Top;
        note.Height = 64;
        note.Margin = new Padding(0, 18, 0, 0);

        var featureGrid = CreateFeatureGrid();
        featureGrid.Dock = DockStyle.Top;
        featureGrid.Height = 300;
        featureGrid.Margin = new Padding(0, 18, 0, 0);

        var hero = UiTheme.CreateHeroCard(
            "База даних госпіталя",
            "Навчальний застосунок для роботи з PostgreSQL БД",
            "Форми введення даних, вибір зовнішніх ключів через списки, параметризовані SQL-запити та запити з множинними порівняннями.");
        hero.Dock = DockStyle.Top;
        hero.Height = 160;
        hero.Margin = new Padding(0);

        content.Controls.Add(note);
        content.Controls.Add(featureGrid);
        content.Controls.Add(hero);

        return content;
    }

    private static Control CreateFeatureGrid()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        UiTheme.ApplyTableLayoutDefaults(grid);
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));

        AddFeatureCard(grid, 0, 0, "Форми введення", "Додавання, редагування та видалення записів через користувацькі форми.");
        AddFeatureCard(grid, 1, 0, "Зовнішні ключі", "Пов'язані значення вибираються зі списків ComboBox.");
        AddFeatureCard(grid, 0, 1, "Складні зв'язки", "Тернарний зв'язок, рекурсивний зв'язок, IS-A підтипи та слабка сутність.");
        AddFeatureCard(grid, 1, 1, "SQL-запити", "5 параметризованих запитів і 2 запити з множинними порівняннями.");

        return grid;
    }

    private static void AddFeatureCard(TableLayoutPanel grid, int column, int row, string title, string body)
    {
        var card = UiTheme.CreateFeatureCard(title, body);
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(
            column == 0 ? 0 : 8,
            row == 0 ? 0 : 8,
            column == 0 ? 8 : 0,
            row == 0 ? 8 : 0);

        grid.Controls.Add(card, column, row);
    }

    private static Control CreateInstructionCard()
    {
        var card = UiTheme.CreateMainCard();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0);
        card.Padding = new Padding(20, 12, 20, 12);

        var label = new Label
        {
            Text = "Оберіть розділ у лівому меню, щоб відкрити таблицю або запити.",
            Dock = DockStyle.Fill,
            Font = UiTheme.TextFont,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        };

        card.Controls.Add(label);
        return card;
    }
}
