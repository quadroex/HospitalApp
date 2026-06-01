namespace HospitalApp;

public static class UiTheme
{
    public static readonly Color AppBackground = Color.FromArgb(248, 250, 252);
    public static readonly Color MainBack = Color.FromArgb(248, 250, 252);
    public static readonly Color SidebarBack = Color.FromArgb(17, 24, 39);
    public static readonly Color SidebarBackground = SidebarBack;
    public static readonly Color CardBack = Color.White;
    public static readonly Color CardSecondary = Color.FromArgb(241, 245, 249);
    public static readonly Color InputBack = Color.White;
    public static readonly Color CardBorder = Color.FromArgb(203, 213, 225);
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentHover = Color.FromArgb(29, 78, 216);
    public static readonly Color TextDark = Color.FromArgb(17, 24, 39);
    public static readonly Color TextLight = Color.FromArgb(249, 250, 251);
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
    public static readonly Color MutedText = TextMuted;
    public static readonly Color Border = CardBorder;
    public static readonly Color Surface = CardBack;
    public static readonly Color SurfaceAlt = CardSecondary;
    public static readonly Color SurfaceDark = Color.FromArgb(226, 232, 240);
    public static readonly Color Text = TextDark;
    public static readonly Color Danger = Color.FromArgb(249, 115, 22);
    public static readonly Color DangerHover = Color.FromArgb(234, 88, 12);
    public static readonly Color Success = Color.FromArgb(34, 197, 94);

    public static readonly Font TextFont = new("Segoe UI", 10.5F, FontStyle.Regular);
    public static readonly Font ButtonFont = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font HeaderFont = new("Segoe UI", 18F, FontStyle.Bold);
    public static readonly Font MainHeaderFont = new("Segoe UI", 24F, FontStyle.Bold);
    public static readonly Font SidebarTitleFont = new("Segoe UI", 16F, FontStyle.Bold);
    public static readonly Font SubHeaderFont = new("Segoe UI", 11F, FontStyle.Regular);
    public static readonly Font SectionFont = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font GroupFont = new("Segoe UI", 9.5F, FontStyle.Bold);

    public const int ButtonHeight = 42;
    public const int ControlHeight = 34;
    public const int Spacing = 12;
    public const int FormPadding = 18;

    public static void ApplyFormTheme(Form form)
    {
        form.BackColor = MainBack;
        form.ForeColor = TextDark;
        form.Font = TextFont;
        form.AutoScaleMode = AutoScaleMode.Font;
    }

    public static Panel CreateSidebarPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = SidebarBack,
            Padding = new Padding(20)
        };
    }

    public static Label CreateSidebarTitleLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 36,
            Font = SidebarTitleFont,
            ForeColor = TextLight,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static Label CreateSidebarSubtitleLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 24,
            Font = TextFont,
            ForeColor = Color.FromArgb(148, 163, 184),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static Label CreateSidebarGroupLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Width = 240,
            Height = 26,
            Font = GroupFont,
            ForeColor = Color.FromArgb(125, 211, 252),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 14, 0, 6)
        };
    }

    public static Button CreateSidebarButton(string text)
    {
        var button = new Button
        {
            Text = text
        };
        StyleSidebarButton(button);
        return button;
    }

    public static Panel CreateCardPanel()
    {
        var panel = new Panel();
        StylePanelCard(panel);
        return panel;
    }

    public static Panel CreateMainCard()
    {
        return CreateCardPanel();
    }

    public static void StylePanelCard(Panel panel)
    {
        panel.BackColor = CardBack;
        panel.ForeColor = TextDark;
        panel.BorderStyle = BorderStyle.FixedSingle;
        panel.Padding = new Padding(18);
        panel.Margin = new Padding(Spacing);
    }

    public static Label CreateHeaderLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = HeaderFont,
            ForeColor = TextDark,
            TextAlign = ContentAlignment.MiddleCenter
        };
    }

    public static Label CreateMainHeaderLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = MainHeaderFont,
            ForeColor = TextDark,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static Label CreateSubHeaderLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = SubHeaderFont,
            ForeColor = TextMuted,
            TextAlign = ContentAlignment.MiddleCenter
        };
    }

    public static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = SectionFont,
            ForeColor = TextDark,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(2, 0, 0, 0)
        };
    }

    public static Label CreateSectionTitle(string text)
    {
        return CreateSectionLabel(text);
    }

    public static Label CreateSmallMutedLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = TextFont,
            ForeColor = TextMuted,
            TextAlign = ContentAlignment.TopLeft
        };
    }

    public static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = TextFont,
            ForeColor = TextDark,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    public static Panel CreateHeroCard(string title, string subtitle, string body)
    {
        var card = CreateMainCard();
        card.Padding = new Padding(24, 18, 24, 18);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        ApplyTableLayoutDefaults(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var titleLabel = CreateMainHeaderLabel(title);
        var subtitleLabel = CreateSubHeaderLabel(subtitle);
        subtitleLabel.TextAlign = ContentAlignment.MiddleLeft;
        var bodyLabel = CreateSmallMutedLabel(body);

        layout.Controls.Add(titleLabel, 0, 0);
        layout.Controls.Add(subtitleLabel, 0, 1);
        layout.Controls.Add(bodyLabel, 0, 2);
        card.Controls.Add(layout);

        return card;
    }

    public static Panel CreateFeatureCard(string title, string body)
    {
        var card = CreateDashboardCard(title, body);
        card.Height = 130;
        return card;
    }

    public static Panel CreateDashboardCard(string title, string body)
    {
        var card = CreateMainCard();
        card.Padding = new Padding(20);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        ApplyTableLayoutDefaults(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Font = SectionFont,
            ForeColor = TextDark,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var bodyLabel = new Label
        {
            Text = body,
            Dock = DockStyle.Fill,
            Font = TextFont,
            ForeColor = TextMuted,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = false
        };

        layout.Controls.Add(titleLabel, 0, 0);
        layout.Controls.Add(bodyLabel, 0, 1);
        card.Controls.Add(layout);

        return card;
    }

    public static void ApplyTableLayoutDefaults(TableLayoutPanel panel)
    {
        panel.BackColor = Color.Transparent;
        panel.ForeColor = TextDark;
        panel.Padding = new Padding(0);
        panel.Margin = new Padding(0);
        panel.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
    }

    public static void ApplyFlowLayoutDefaults(FlowLayoutPanel panel)
    {
        panel.BackColor = Color.Transparent;
        panel.ForeColor = TextDark;
        panel.Margin = new Padding(0);
        panel.WrapContents = true;
    }

    public static void StylePrimaryButton(Button button)
    {
        StyleButton(button, Primary, Color.White, PrimaryHover, Primary);
    }

    public static void StyleSecondaryButton(Button button)
    {
        StyleButton(button, CardBack, TextDark, CardSecondary, CardBorder);
    }

    public static void StyleDangerButton(Button button)
    {
        StyleButton(button, Danger, Color.White, DangerHover, Danger);
    }

    public static void StyleTextButton(Button button)
    {
        StyleSecondaryButton(button);
    }

    public static void StyleSidebarButton(Button button)
    {
        button.BackColor = Color.FromArgb(31, 41, 55);
        button.ForeColor = Color.FromArgb(229, 231, 235);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = PrimaryHover;
        button.FlatAppearance.MouseOverBackColor = Primary;
        button.Font = ButtonFont;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Width = 240;
        button.Height = ButtonHeight;
        button.MinimumSize = new Size(240, ButtonHeight);
        button.MaximumSize = new Size(240, ButtonHeight);
        button.Padding = new Padding(14, 0, 10, 0);
        button.Margin = new Padding(0, 0, 0, 8);
        button.UseVisualStyleBackColor = false;
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = InputBack;
        textBox.ForeColor = TextDark;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = TextFont;
        textBox.MinimumSize = new Size(0, ControlHeight);
        textBox.Margin = new Padding(0, 4, 0, 4);
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = InputBack;
        comboBox.ForeColor = TextDark;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Font = TextFont;
        comboBox.MinimumSize = new Size(0, ControlHeight);
        comboBox.Margin = new Padding(0, 4, 0, 4);
    }

    public static void StyleDateTimePicker(DateTimePicker picker)
    {
        picker.BackColor = InputBack;
        picker.ForeColor = TextDark;
        picker.Font = TextFont;
        picker.CalendarForeColor = TextDark;
        picker.CalendarMonthBackground = InputBack;
        picker.CalendarTitleBackColor = CardSecondary;
        picker.CalendarTitleForeColor = TextDark;
        picker.MinimumSize = new Size(0, ControlHeight);
        picker.Margin = new Padding(0, 4, 0, 4);
    }

    public static void StyleNumericUpDown(NumericUpDown numeric)
    {
        numeric.BackColor = InputBack;
        numeric.ForeColor = TextDark;
        numeric.BorderStyle = BorderStyle.FixedSingle;
        numeric.Font = TextFont;
        numeric.MinimumSize = new Size(0, ControlHeight);
        numeric.Margin = new Padding(0, 4, 0, 4);
    }

    public static void StyleInput(Control control)
    {
        switch (control)
        {
            case TextBox textBox:
                StyleTextBox(textBox);
                break;
            case ComboBox comboBox:
                StyleComboBox(comboBox);
                break;
            case DateTimePicker picker:
                StyleDateTimePicker(picker);
                break;
            case NumericUpDown numeric:
                StyleNumericUpDown(numeric);
                break;
        }
    }

    public static void StyleDataGridView(DataGridView grid)
    {
        grid.BackgroundColor = CardBack;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = CardBorder;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 232, 240);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextDark;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextDark;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font(TextFont, FontStyle.Bold);
        grid.DefaultCellStyle.BackColor = CardBack;
        grid.DefaultCellStyle.ForeColor = TextDark;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(191, 219, 254);
        grid.DefaultCellStyle.SelectionForeColor = TextDark;
        grid.DefaultCellStyle.Font = TextFont;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.AlternatingRowsDefaultCellStyle.ForeColor = TextDark;
        grid.RowsDefaultCellStyle.BackColor = CardBack;
        grid.RowsDefaultCellStyle.ForeColor = TextDark;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.RowTemplate.Height = 30;
        grid.ColumnHeadersHeight = 38;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
    }

    private static void StyleButton(Button button, Color backColor, Color foreColor, Color hoverColor, Color borderColor)
    {
        button.BackColor = backColor;
        button.ForeColor = foreColor;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseDownBackColor = hoverColor;
        button.FlatAppearance.MouseOverBackColor = hoverColor;
        button.Font = ButtonFont;
        button.Height = ButtonHeight;
        button.MinimumSize = new Size(110, ButtonHeight);
        button.Padding = new Padding(10, 0, 10, 0);
        button.Margin = new Padding(6);
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseVisualStyleBackColor = false;
    }
}
