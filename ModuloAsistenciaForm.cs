using MySql.Data.MySqlClient;
using DPFP.Capture;


namespace Huella
{
    public partial class ModuloAsistenciaForm : Form, DPFP.Capture.EventHandler
    {
        private Label lblStatus;
        private Button btnVerificar;
        private Capture Capturer;
        private DPFP.Verification.Verification Verifier;

        public ModuloAsistenciaForm()
        {
            InitializeComponent();
            InitializeComponent2();
            Capturer = new Capture();
            Capturer.EventHandler = this;
            Verifier = new DPFP.Verification.Verification();
        }

        private void InitializeComponent2()
        {
            this.lblStatus = new Label { Text = "Coloque su huella", Left = 50, Top = 50, Width = 200 };
            this.btnVerificar = new Button { Text = "Verificar", Left = 50, Top = 100, Width = 200 };
            this.btnVerificar.Click += new System.EventHandler(this.BtnVerificar_Click);

            this.Controls.Add(lblStatus);
            this.Controls.Add(btnVerificar);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DetenerYLiberarLector(); // ✅ Liberamos el lector antes de cerrar el formulario
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
                    Thread.Sleep(500); // ✅ Pequeña pausa para liberar completamente el lector
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al detener el lector: " + ex.Message);
            }
        }

        private void BtnVerificar_Click(object sender, EventArgs e)
        {
            try
            {
                Capturer.StartCapture();
                this.Invoke((MethodInvoker)(() => lblStatus.Text = "Escaneando huella..."));
            }
            catch
            {
                MessageBox.Show("Error al iniciar la captura.");
            }
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            DPFP.Processing.FeatureExtraction extractor = new DPFP.Processing.FeatureExtraction();
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            CaptureFeedback feedback = CaptureFeedback.None;
            extractor.CreateFeatureSet(Sample, DPFP.Processing.DataPurpose.Verification, ref feedback, ref features);

            if (feedback == CaptureFeedback.Good)
            {
                this.Invoke((MethodInvoker)(() => IdentificarEmpleado(features)));
            }
            else
            {
                this.Invoke((MethodInvoker)(() => MessageBox.Show("Huella no válida. Intente nuevamente.")));
            }
        }

        private void IdentificarEmpleado(DPFP.FeatureSet features)
        {
            using (MySqlConnection conn = new DatabaseConnection().GetConnection())
            {
                conn.Open();
                string query = "SELECT id, nombre, apellido, huella FROM empleados";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    byte[] huellaData = (byte[])reader["huella"];
                    DPFP.Template template = new DPFP.Template(new MemoryStream(huellaData));

                    DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();
                    Verifier.Verify(features, template, ref result);

                    if (result.Verified)
                    {
                        int empleadoId = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        string apellido = reader["apellido"].ToString();

                        RegistrarAsistencia(empleadoId);

                        DialogResult respuesta = MessageBox.Show(
                            $"Bienvenido {nombre} {apellido}. Asistencia registrada.\n\n¿Desea verificar otro empleado?",
                            "Asistencia Registrada",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (respuesta == DialogResult.Yes)
                        {
                            ReiniciarProceso();
                            BtnVerificar_Click(null, EventArgs.Empty); // ✅ Llamar automáticamente al botón
                        }
                        else
                        {
                            this.Close(); // Volver al menú principal
                        }

                        return;
                    }
                }

                MessageBox.Show("Huella no reconocida.");
                ReiniciarProceso();
                BtnVerificar_Click(null, EventArgs.Empty); // ✅ Llamar automáticamente al botón si la huella no es reconocida
            }
        }


        private void RegistrarAsistencia(int empleadoId)
        {
            using (MySqlConnection conn = new DatabaseConnection().GetConnection())
            {
                conn.Open();

                string tipoRegistro = DeterminarTipoRegistro(empleadoId);
                string query = "INSERT INTO asistencia (empleado_id, tipo) VALUES (@empleado_id, @tipo)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@empleado_id", empleadoId);
                cmd.Parameters.AddWithValue("@tipo", tipoRegistro);
                cmd.ExecuteNonQuery();
            }
        }

        private string DeterminarTipoRegistro(int empleadoId)
        {
            using (MySqlConnection conn = new DatabaseConnection().GetConnection())
            {
                conn.Open();

                string query = "SELECT tipo FROM asistencia WHERE empleado_id = @empleado_id ORDER BY fecha DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@empleado_id", empleadoId);
                object result = cmd.ExecuteScalar();

                return (result != null && result.ToString() == "entrada") ? "salida" : "entrada";
            }
        }

        private void ReiniciarProceso()
        {
            try
            {
                Capturer.StopCapture();
                Capturer.EventHandler = null;
                Capturer = new Capture();
                Capturer.EventHandler = this;
                Capturer.StartCapture();

                this.Invoke((MethodInvoker)(() => lblStatus.Text = "Coloque su huella"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reiniciar el lector: " + ex.Message);
            }
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback) { }

    }
}