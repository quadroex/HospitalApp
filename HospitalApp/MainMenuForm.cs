namespace HospitalApp;

public sealed class MainMenuForm : Form
{
    public MainMenuForm()
    {
        Text = "Госпіталь — головне меню";
        Width = 520;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;

        var title = new Label
        {
            Text = "База даних госпіталя",
            Dock = DockStyle.Top,
            Height = 60,
            Font = new Font(Font.FontFamily, 18, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(30)
        };

        foreach (var config in TableConfigs.All)
        {
            var button = new Button
            {
                Text = config.Title,
                Width = 430,
                Height = 42,
                Margin = new Padding(0, 0, 0, 10)
            };

            button.Click += (_, _) =>
            {
                using var form = new TableForm(config);
                form.ShowDialog(this);
            };

            panel.Controls.Add(button);
        }

        var queriesButton = new Button
        {
            Text = "Запити",
            Width = 430,
            Height = 42,
            Margin = new Padding(0, 15, 0, 10)
        };

        queriesButton.Click += (_, _) =>
        {
            using var form = new QueriesForm();
            form.ShowDialog(this);
        };

        var exitButton = new Button
        {
            Text = "Вихід",
            Width = 430,
            Height = 42,
            Margin = new Padding(0, 10, 0, 0)
        };

        exitButton.Click += (_, _) => Close();

        panel.Controls.Add(queriesButton);
        panel.Controls.Add(exitButton);

        Controls.Add(panel);
        Controls.Add(title);
    }
}
