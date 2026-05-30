namespace HospitalApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.Add(grid);

            try
            {
                grid.DataSource = Db.GetTable("SELECT * FROM doctors ORDER BY passport");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database connection error");
            }
        }
    }
}
