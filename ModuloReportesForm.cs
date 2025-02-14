using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

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
            CargarReportes(); // 🚀 Cargar reportes automáticamente al abrir el formulario
        }

        private void InitializeComponent2()
        {
            this.Text = "Reportes de Asistencia";
            this.Size = new System.Drawing.Size(650, 500);

            dgvReportes = new DataGridView { Left = 50, Top = 50, Width = 550, Height = 300 };
            btnCargarReportes = new Button { Text = "Cargar Reportes", Left = 50, Top = 370, Width = 200 };
            btnCargarReportes.Click += new EventHandler(this.BtnCargarReportes_Click);

            this.Controls.Add(dgvReportes);
            this.Controls.Add(btnCargarReportes);
        }

        private void BtnCargarReportes_Click(object sender, EventArgs e)
        {
            CargarReportes();
        }

        private void CargarReportes()
        {
            try
            {
                using (MySqlConnection conn = new DatabaseConnection().GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT e.cedula, e.nombre, e.apellido, a.fecha, a.tipo 
                        FROM asistencia a
                        INNER JOIN empleados e ON a.empleado_id = e.id
                        ORDER BY a.fecha DESC";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dgvReportes.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reportes: " + ex.Message);
            }
        }
    }
}
