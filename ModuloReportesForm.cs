using MySql.Data.MySqlClient;
using System.Data;


namespace Huella
{
    public partial class ModuloReportesForm : Form
    {
        private DataGridView dgvReportes;
        private Button btnCargarReportes;

        public ModuloReportesForm()
        {
            InitializeComponent();
            InitializeComponent2();
        }

        private void InitializeComponent2()
        {
            this.dgvReportes = new DataGridView { Left = 50, Top = 50, Width = 500, Height = 300 };
            this.btnCargarReportes = new Button { Text = "Cargar Reportes", Left = 50, Top = 370, Width = 200 };
            this.btnCargarReportes.Click += new EventHandler(this.BtnCargarReportes_Click);

            this.Controls.Add(dgvReportes);
            this.Controls.Add(btnCargarReportes);
        }

        private void BtnCargarReportes_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new DatabaseConnection().GetConnection())
            {
                conn.Open();
                string query = "SELECT cedula, nombre, apellido, fecha, tipo FROM asistencia";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgvReportes.DataSource = table;
            }
        }
    }
}
