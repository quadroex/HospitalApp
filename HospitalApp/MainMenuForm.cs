namespace HospitalApp;

public sealed class MainMenuForm : Form
{
    public MainMenuForm()
    {
        UiTheme.ApplyFormTheme(this);

        Text = "Госпіталь - головне меню";
        Width = 920;
        Height = 760;
        MinimumSize = new Size(820, 680);
        StartPosition = FormStartPosition.CenterScreen;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28),
            ColumnCount = 1,
            RowCount = 1
        };
        UiTheme.ApplyTableLayoutDefaults(root);

        var card = UiTheme.CreateCardPanel();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7
        };
        UiTheme.ApplyTableLayoutDefaults(content);
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        var title = UiTheme.CreateHeaderLabel("База даних госпіталя");
        var subtitle = UiTheme.CreateSubHeaderLabel("Навчальний застосунок для роботи з PostgreSQL БД");
        var dataSection = UiTheme.CreateSectionLabel("Дані");
        var querySection = UiTheme.CreateSectionLabel("Запити");

        var tableButtons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true,
            Padding = new Padding(0, 4, 0, 4)
        };
        UiTheme.ApplyTableLayoutDefaults(tableButtons);
        tableButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tableButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var tableIndex = 0;
        foreach (var config in TableConfigs.All)
        {
            var button = new Button
            {
                Text = config.Title,
                Dock = DockStyle.Fill,
                Height = UiTheme.ButtonHeight
            };
            UiTheme.StyleSecondaryButton(button);

            button.Click += (_, _) =>
            {
                using var form = new TableForm(config);
                form.ShowDialog(this);
            };

            var row = tableIndex / 2;
            var column = tableIndex % 2;

            if (tableButtons.RowStyles.Count <= row)
                tableButtons.RowStyles.Add(new RowStyle(SizeType.Absolute, UiTheme.ButtonHeight + 14));

            tableButtons.Controls.Add(button, column, row);
            tableIndex++;
        }

        var queriesButton = new Button
        {
            Text = "Запити",
            Dock = DockStyle.Fill
        };
        UiTheme.StylePrimaryButton(queriesButton);

        queriesButton.Click += (_, _) =>
        {
            using var form = new QueriesForm();
            form.ShowDialog(this);
        };

        var systemPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        UiTheme.ApplyFlowLayoutDefaults(systemPanel);

        var exitButton = new Button
        {
            Text = "Вихід",
            Width = 180
        };
        UiTheme.StyleDangerButton(exitButton);
        exitButton.Click += (_, _) => Close();
        systemPanel.Controls.Add(exitButton);

        content.Controls.Add(title, 0, 0);
        content.Controls.Add(subtitle, 0, 1);
        content.Controls.Add(dataSection, 0, 2);
        content.Controls.Add(tableButtons, 0, 3);
        content.Controls.Add(querySection, 0, 4);
        content.Controls.Add(queriesButton, 0, 5);
        content.Controls.Add(systemPanel, 0, 6);

        card.Controls.Add(content);
        root.Controls.Add(card, 0, 0);
        Controls.Add(root);
    }
}
