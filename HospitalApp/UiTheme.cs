namespace HospitalApp;

public static class UiTheme
{
    public static readonly Color Background = Color.FromArgb(15, 23, 42);
    public static readonly Color Surface = Color.FromArgb(17, 24, 39);
    public static readonly Color SurfaceAlt = Color.FromArgb(30, 41, 59);
    public static readonly Color SurfaceDark = Color.FromArgb(2, 6, 23);
    public static readonly Color Accent = Color.FromArgb(56, 189, 248);
    public static readonly Color AccentHover = Color.FromArgb(14, 165, 233);
    public static readonly Color Text = Color.FromArgb(229, 231, 235);
    public static readonly Color MutedText = Color.FromArgb(148, 163, 184);
    public static readonly Color Border = Color.FromArgb(51, 65, 85);
    public static readonly Color Danger = Color.FromArgb(249, 115, 22);
    public static readonly Color DangerHover = Color.FromArgb(234, 88, 12);

    public static readonly Font TextFont = new("Segoe UI", 10.5F, FontStyle.Regular);
    public static readonly Font ButtonFont = new("Segoe UI", 10.5F, FontStyle.Bold);
    public static readonly Font HeaderFont = new("Segoe UI", 20F, FontStyle.Bold);
    public static readonly Font SubHeaderFont = new("Segoe UI", 11F, FontStyle.Regular);
    public static readonly Font SectionFont = new("Segoe UI", 12F, FontStyle.Bold);

    public const int ButtonHeight = 42;
    public const int ControlHeight = 34;
    public const int Spacing = 12;
    public const int FormPadding = 18;

    public static void ApplyFormTheme(Form form)
    {
        form.BackColor = Background;
        form.ForeColor = Text;
        form.Font = TextFont;
        form.AutoScaleMode = AutoScaleMode.Font;
    }

    public static Panel CreateCardPanel()
    {
        return new Panel
        {
            BackColor = Surface,
            ForeColor = Text,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(16),
            Margin = new Padding(Spacing)
        };
    }

    public static Label CreateHeaderLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = HeaderFont,
            ForeColor = Text,
            TextAlign = ContentAlignment.MiddleCenter
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
            ForeColor = MutedText,
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
            ForeColor = Accent,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(2, 0, 0, 0)
        };
    }

    public static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = TextFont,
            ForeColor = Text,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    public static void ApplyTableLayoutDefaults(TableLayoutPanel panel)
    {
        panel.BackColor = Color.Transparent;
        panel.ForeColor = Text;
        panel.Padding = new Padding(0);
        panel.Margin = new Padding(0);
        panel.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
    }

    public static void ApplyFlowLayoutDefaults(FlowLayoutPanel panel)
    {
        panel.BackColor = Color.Transparent;
        panel.ForeColor = Text;
        panel.Margin = new Padding(0);
        panel.WrapContents = true;
    }

    public static void StylePrimaryButton(Button button)
    {
        StyleButton(button, Accent, SurfaceDark, AccentHover, Accent);
    }

    public static void StyleSecondaryButton(Button button)
    {
        StyleButton(button, SurfaceAlt, Text, Border, Accent);
    }

    public static void StyleDangerButton(Button button)
    {
        StyleButton(button, Danger, Color.White, DangerHover, Danger);
    }

    public static void StyleTextButton(Button button)
    {
        StyleButton(button, Surface, Text, SurfaceAlt, Border);
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = SurfaceDark;
        textBox.ForeColor = Text;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = TextFont;
        textBox.MinimumSize = new Size(0, ControlHeight);
        textBox.Margin = new Padding(0, 4, 0, 4);
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = SurfaceDark;
        comboBox.ForeColor = Text;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Font = TextFont;
        comboBox.MinimumSize = new Size(0, ControlHeight);
        comboBox.Margin = new Padding(0, 4, 0, 4);
    }

    public static void StyleDateTimePicker(DateTimePicker picker)
    {
        picker.BackColor = SurfaceDark;
        picker.ForeColor = Text;
        picker.Font = TextFont;
        picker.CalendarForeColor = Text;
        picker.CalendarMonthBackground = SurfaceDark;
        picker.CalendarTitleBackColor = SurfaceAlt;
        picker.CalendarTitleForeColor = Text;
        picker.MinimumSize = new Size(0, ControlHeight);
        picker.Margin = new Padding(0, 4, 0, 4);
    }

    public static void StyleNumericUpDown(NumericUpDown numeric)
    {
        numeric.BackColor = SurfaceDark;
        numeric.ForeColor = Text;
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
        grid.BackgroundColor = SurfaceDark;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Border;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font(TextFont, FontStyle.Bold);
        grid.DefaultCellStyle.BackColor = SurfaceDark;
        grid.DefaultCellStyle.ForeColor = Text;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(12, 74, 110);
        grid.DefaultCellStyle.SelectionForeColor = Text;
        grid.DefaultCellStyle.Font = TextFont;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(8, 13, 29);
        grid.AlternatingRowsDefaultCellStyle.ForeColor = Text;
        grid.RowsDefaultCellStyle.BackColor = SurfaceDark;
        grid.RowsDefaultCellStyle.ForeColor = Text;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.RowTemplate.Height = 30;
        grid.ColumnHeadersHeight = 36;
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
        button.UseVisualStyleBackColor = false;
    }
}
