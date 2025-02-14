using MySql.Data.MySqlClient;
using Capture = DPFP.Capture.Capture;

namespace Huella
{
    public partial class MenuPrincipalForm : Form, DPFP.Capture.EventHandler
    {
        private Button btnRegistrarEmpleados, btnRegistrarAsistencia, btnVerReportes, btnCerrarSesion;
        private Button btnEstadoDB, btnEstadoLector;
        private Capture Capturer;

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
            this.Size = new System.Drawing.Size(400, 350);

            btnEstadoDB = new Button { Text = "Base de Datos", Left = 50, Top = 30, Width = 120, Height = 40 };
            btnEstadoLector = new Button { Text = "Lector Biométrico", Left = 200, Top = 30, Width = 120, Height = 40 };

            btnRegistrarEmpleados = new Button { Text = "Registrar Empleados", Left = 50, Top = 90, Width = 270, Height = 40 };
            btnRegistrarEmpleados.Click += BtnRegistrarEmpleados_Click;

            btnRegistrarAsistencia = new Button { Text = "Registrar Asistencia", Left = 50, Top = 140, Width = 270, Height = 40 };
            btnRegistrarAsistencia.Click += BtnRegistrarAsistencia_Click;

            btnVerReportes = new Button { Text = "Ver Reportes", Left = 50, Top = 190, Width = 270, Height = 40 };
            btnVerReportes.Click += (sender, e) => new ModuloReportesForm().ShowDialog();

            btnCerrarSesion = new Button { Text = "Cerrar Sesión", Left = 50, Top = 240, Width = 270, Height = 40 };
            btnCerrarSesion.Click += BtnCerrarSesion_Click;

            this.Controls.Add(btnEstadoDB);
            this.Controls.Add(btnEstadoLector);
            this.Controls.Add(btnRegistrarEmpleados);
            this.Controls.Add(btnRegistrarAsistencia);
            this.Controls.Add(btnVerReportes);
            this.Controls.Add(btnCerrarSesion);
        }


        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            VerificarConexionLector(); // ✅ Cada vez que el menú recupera el foco, reactiva el lector
        }

      
        private void BtnRegistrarEmpleados_Click(object sender, EventArgs e)
        {
            DetenerYLiberarLector(); // ✅ Detenemos el lector antes de abrir RegistroEmpleadosForm

            using (RegistroEmpleadosForm empleadosForm = new RegistroEmpleadosForm())
            {
                empleadosForm.ShowDialog();
            }

            VerificarConexionLector(); // ✅ Reactivamos el lector después de cerrar RegistroEmpleadosForm
        }



        private void VerificarConexionLector()
        {
            try
            {
                if (Capturer == null)
                {
                    Capturer = new Capture();
                    Capturer.EventHandler = this;
                    Capturer.StartCapture(); // ✅ Reactivamos el lector si no está en uso
                    btnEstadoLector.BackColor = Color.Green;
                }
            }
            catch
            {
                btnEstadoLector.BackColor = Color.Red;
            }
        }

        private void BtnRegistrarAsistencia_Click(object sender, EventArgs e)
        {
            DetenerYLiberarLector(); // ✅ Detener el lector antes de abrir ModuloAsistenciaForm

            using (ModuloAsistenciaForm asistenciaForm = new ModuloAsistenciaForm())
            {
                asistenciaForm.ShowDialog();
            }

            VerificarConexionLector(); // ✅ Reiniciar el lector al volver al menú
        }


        private void DetenerYLiberarLector()
        {
            try
            {
                if (Capturer != null)
                {
                    Capturer.StopCapture();
                    Capturer.EventHandler = null;
                    Capturer = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al detener el lector en el menú: " + ex.Message);
            }
        }
       
        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm login = new LoginForm();
            login.ShowDialog();
            this.Close();
        }

        private void VerificarConexionDB()
        {
            try
            {
                using (MySqlConnection conn = new DatabaseConnection().GetConnection())
                {
                    conn.Open();
                    btnEstadoDB.BackColor = Color.Green;
                }
            }
            catch
            {
                btnEstadoDB.BackColor = Color.Red;
            }
        }


        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            this.Invoke((MethodInvoker)delegate
            {
                btnEstadoLector.BackColor = Color.Red;
            });
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            this.Invoke((MethodInvoker)delegate
            {
                btnEstadoLector.BackColor = Color.Green;
            });
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample) { }
        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback) { }
    }
}
