using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using DPFP;
using DPFP.Capture;
using System.IO;

namespace Huella
{
    public partial class RegistroEmpleadosForm : Form, DPFP.Capture.EventHandler
    {
        private TextBox txtCedula, txtNombre, txtApellido;
        private Button btnCapturarHuella, btnRegistrar;
        private ProgressBar progressBar;
        private Label lblProgreso;
        private DPFP.Capture.Capture Capturer;
        private DPFP.Template HuellaTemplate;
        private DPFP.Processing.Enrollment Enrollment;

        public RegistroEmpleadosForm()
        {
            InitializeComponent();
            InitializeComponent2();
            Capturer = new DPFP.Capture.Capture();
            Capturer.EventHandler = this;
            Enrollment = new DPFP.Processing.Enrollment();
        }

        private void InitializeComponent2()
        {
            this.Text = "Registro de Empleados";
            this.Size = new System.Drawing.Size(400, 350);

            txtCedula = new TextBox { PlaceholderText = "Cédula", Left = 100, Top = 30, Width = 200 };
            txtNombre = new TextBox { PlaceholderText = "Nombre", Left = 100, Top = 70, Width = 200 };
            txtApellido = new TextBox { PlaceholderText = "Apellido", Left = 100, Top = 110, Width = 200 };

            btnCapturarHuella = new Button { Text = "Iniciar Captura", Left = 100, Top = 150, Width = 200 };
            btnCapturarHuella.Click += new System.EventHandler(BtnCapturarHuella_Click);

            progressBar = new ProgressBar { Left = 100, Top = 200, Width = 200, Height = 20, Minimum = 0, Maximum = 4 };
            lblProgreso = new Label { Text = "Capturas: 0/4", Left = 100, Top = 230, Width = 200 };

            btnRegistrar = new Button { Text = "Registrar Empleado", Left = 100, Top = 260, Width = 200, Enabled = false };
            btnRegistrar.Click += new System.EventHandler(BtnRegistrar_Click);

            this.Controls.Add(txtCedula);
            this.Controls.Add(txtNombre);
            this.Controls.Add(txtApellido);
            this.Controls.Add(btnCapturarHuella);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblProgreso);
            this.Controls.Add(btnRegistrar);
        }

        private bool huellaCapturada = false; // ✅ Variable para evitar reinicio de captura

        private void BtnCapturarHuella_Click(object sender, EventArgs e)
        {
            if (huellaCapturada)
            {
                MessageBox.Show("La huella ya ha sido capturada.");
                return;
            }

            try
            {
                Enrollment.Clear();
                progressBar.Value = 0;
                lblProgreso.Text = "Capturas: 0/4";
                Capturer.StartCapture();
                MessageBox.Show("Coloque su dedo en el lector. La captura se realizará automáticamente.");
            }
            catch
            {
                MessageBox.Show("Error iniciando la captura de huella.");
            }
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            DPFP.Processing.FeatureExtraction extractor = new DPFP.Processing.FeatureExtraction();
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;

            extractor.CreateFeatureSet(Sample, DPFP.Processing.DataPurpose.Enrollment, ref feedback, ref features);

            if (feedback == DPFP.Capture.CaptureFeedback.Good)
            {
                Enrollment.AddFeatures(features);

                this.Invoke((MethodInvoker)delegate
                {
                    int capturasRealizadas = (int)(Enrollment.FeaturesNeeded > 0 ? (5 - Enrollment.FeaturesNeeded) : 4);
                    progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(progressBar.Maximum, capturasRealizadas));
                    lblProgreso.Text = $"Capturas: {capturasRealizadas}/4";

                    if (Enrollment.TemplateStatus == DPFP.Processing.Enrollment.Status.Ready)
                    {
                        Capturer.StopCapture(); // ✅ DETIENE LA CAPTURA
                        Capturer.EventHandler = null; // ✅ ELIMINA EVENTOS PARA EVITAR QUE SE VUELVA A EJECUTAR
                        huellaCapturada = true; // ✅ EVITA QUE SE REINICIE LA CAPTURA

                        HuellaTemplate = Enrollment.Template;
                        lblProgreso.Text = "Huella capturada correctamente.";
                        progressBar.Value = 4;
                        btnRegistrar.Enabled = true;

                        Task.Run(async () =>
                        {
                            MessageBox.Show("Huella capturada correctamente.");
                            await Task.Delay(500);
                            SendKeys.SendWait("{ENTER}"); // Cierra el MessageBox sin intervención del usuario
                        });
                    }
                });
            }
        }


        private async void MostrarMensajeTemporal(string mensaje)
        {
            await Task.Run(() =>
            {
                MessageBox.Show(mensaje);
                Task.Delay(2000).Wait(); // Espera 2 segundos antes de cerrar
                SendKeys.SendWait("{ENTER}"); // Cierra el MessageBox automáticamente
            });
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback) { }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            if (HuellaTemplate == null)
            {
                MessageBox.Show("Debe capturar la huella antes de registrar al empleado.");
                return;
            }

            string cedula = txtCedula.Text.Trim();
            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();

            using (MySqlConnection conn = new DatabaseConnection().GetConnection())
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM empleados WHERE cedula = @cedula";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@cedula", cedula);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Error: La cédula ya está registrada.");
                    return;
                }

                string query = "INSERT INTO empleados (cedula, nombre, apellido, huella) VALUES (@cedula, @nombre, @apellido, @huella)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cedula", cedula);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@apellido", apellido);

                using (MemoryStream ms = new MemoryStream())
                {
                    HuellaTemplate.Serialize(ms);
                    byte[] huellaData = ms.ToArray();
                    cmd.Parameters.AddWithValue("@huella", huellaData);
                }

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    DialogResult respuesta = MessageBox.Show(
                        "Empleado registrado exitosamente.\n¿Desea agregar otro empleado?",
                        "Registro Exitoso",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (respuesta == DialogResult.Yes)
                    {
                        LimpiarCampos(); // ✅ Si elige "Sí", limpiamos y permitimos otro registro
                    }
                    else
                    {
                        this.Close(); // ✅ Si elige "No", volvemos al menú principal
                    }
                }
                else
                {
                    MessageBox.Show("Error al registrar empleado.");
                }
            }
        }


        private void LimpiarCampos()
        {
            txtCedula.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            progressBar.Value = 0;
            lblProgreso.Text = "Capturas: 0/4";
            btnRegistrar.Enabled = false;
            huellaCapturada = false; // ✅ Permite capturar una nueva huella
            HuellaTemplate = null;   // ✅ Reseteamos la huella
            Enrollment.Clear();      // ✅ Reinicia la captura de huellas
            ReiniciarLector();       // ✅ Vuelve a habilitar el lector de huellas
        }

        private void ReiniciarLector()
        {
            try
            {
                if (Capturer != null)
                {
                    Capturer.StopCapture();
                    Capturer.EventHandler = null;
                    Capturer = new DPFP.Capture.Capture();
                    Capturer.EventHandler = this;
                    Capturer.StartCapture();
                    MessageBox.Show("Lector de huellas listo para un nuevo registro.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reiniciar el lector: " + ex.Message);
            }
        }



    }
}
