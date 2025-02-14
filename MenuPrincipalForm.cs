using MySql.Data.MySqlClient;
using DPFP.Capture;
using System;
using System.Windows.Forms;
using Capture = DPFP.Capture.Capture;

namespace Huella
{
    public partial class MenuPrincipalForm : Form, DPFP.Capture.EventHandler
    {
        private Button btnRegistrarEmpleados, btnRegistrarAsistencia, btnVerReportes;
        private Button btnEstadoDB, btnEstadoLector;
        private DPFP.Capture.Capture Capturer;

        public MenuPrincipalForm()
        {
            InitializeComponent();
            InitializeComponent2();
            VerificarConexionDB();
            VerificarConexionLector();
        }

        private void InitializeComponent2()
        {
            this.Text = "Menú Principal";
            this.Size = new System.Drawing.Size(400, 300);

            btnEstadoDB = new Button { Text = "Base de Datos", Left = 50, Top = 30, Width = 120, Height = 40 };
            btnEstadoLector = new Button { Text = "Lector Biométrico", Left = 200, Top = 30, Width = 120, Height = 40 };

            btnRegistrarEmpleados = new Button { Text = "Registrar Empleados", Left = 50, Top = 90, Width = 270, Height = 40 };
            btnRegistrarEmpleados.Click += (sender, e) => new RegistroEmpleadosForm().ShowDialog();

            btnRegistrarAsistencia = new Button { Text = "Registrar Asistencia", Left = 50, Top = 140, Width = 270, Height = 40 };
            btnRegistrarAsistencia.Click += (sender, e) => new ModuloAsistenciaForm().ShowDialog();

            btnVerReportes = new Button { Text = "Ver Reportes", Left = 50, Top = 190, Width = 270, Height = 40 };
            btnVerReportes.Click += (sender, e) => new ModuloReportesForm().ShowDialog();

            this.Controls.Add(btnEstadoDB);
            this.Controls.Add(btnEstadoLector);
            this.Controls.Add(btnRegistrarEmpleados);
            this.Controls.Add(btnRegistrarAsistencia);
            this.Controls.Add(btnVerReportes);
        }

        private void VerificarConexionDB()
        {
            try
            {
                using (MySqlConnection conn = new DatabaseConnection().GetConnection())
                {
                    conn.Open();
                    btnEstadoDB.BackColor = System.Drawing.Color.Green;
                }
            }
            catch
            {
                btnEstadoDB.BackColor = System.Drawing.Color.Red;
            }
        }

        private void VerificarConexionLector()
        {
            try
            {
                Capturer = new Capture();
                Capturer.EventHandler = this;
                Capturer.StartCapture();
                btnEstadoLector.BackColor = System.Drawing.Color.Green;
            }
            catch
            {
                btnEstadoLector.BackColor = System.Drawing.Color.Red;
            }
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            this.Invoke((MethodInvoker)delegate
            {
                btnEstadoLector.BackColor = System.Drawing.Color.Green;
            });
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            this.Invoke((MethodInvoker)delegate
            {
                btnEstadoLector.BackColor = System.Drawing.Color.Red;
            });
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample) { }
        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback) { }
    }
}
