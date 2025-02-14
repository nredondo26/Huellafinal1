using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Huella
{
    public partial class RegistroEmpleadosForm : Form, DPFP.Capture.EventHandler
    {
        private TextBox txtCedula, txtNombre, txtApellido;
        private Button btnCapturarHuella, btnRegistrar;
        private DPFP.Capture.Capture Capturer;
        private DPFP.Template HuellaTemplate;

        public RegistroEmpleadosForm()
        {
            InitializeComponent();
            InitializeComponent2();
            Capturer = new DPFP.Capture.Capture();
            Capturer.EventHandler = this;
        }

        private void InitializeComponent2()
        {
            this.Text = "Registro de Empleados";
            this.Size = new System.Drawing.Size(350, 300);

            txtCedula = new TextBox { PlaceholderText = "Cédula", Left = 100, Top = 30, Width = 200 };
            txtNombre = new TextBox { PlaceholderText = "Nombre", Left = 100, Top = 70, Width = 200 };
            txtApellido = new TextBox { PlaceholderText = "Apellido", Left = 100, Top = 110, Width = 200 };

            btnCapturarHuella = new Button { Text = "Capturar Huella", Left = 100, Top = 150, Width = 200 };
            btnCapturarHuella.Click += new EventHandler(BtnCapturarHuella_Click);

            btnRegistrar = new Button { Text = "Registrar Empleado", Left = 100, Top = 190, Width = 200 };
            btnRegistrar.Click += new EventHandler(BtnRegistrar_Click);

            this.Controls.Add(txtCedula);
            this.Controls.Add(txtNombre);
            this.Controls.Add(txtApellido);
            this.Controls.Add(btnCapturarHuella);
            this.Controls.Add(btnRegistrar);
        }

        private void BtnCapturarHuella_Click(object sender, EventArgs e)
        {
            try
            {
                Capturer.StartCapture();
                MessageBox.Show("Coloque su dedo en el lector.");
            }
            catch
            {
                MessageBox.Show("Error iniciando la captura de huella.");
            }
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            // Convertir la muestra en una plantilla
            DPFP.Processing.FeatureExtraction extractor = new DPFP.Processing.FeatureExtraction();
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;

            extractor.CreateFeatureSet(Sample, DPFP.Processing.DataPurpose.Enrollment, ref feedback, ref features);

            if (feedback == DPFP.Capture.CaptureFeedback.Good)
            {
                DPFP.Processing.Enrollment enrollment = new DPFP.Processing.Enrollment();
                enrollment.AddFeatures(features);

                if (enrollment.TemplateStatus == DPFP.Processing.Enrollment.Status.Ready)
                {
                    HuellaTemplate = enrollment.Template;
                    MessageBox.Show("Huella capturada correctamente.");
                    Capturer.StopCapture();
                }
            }
            else
            {
                MessageBox.Show("Muestra de huella no válida. Intente nuevamente.");
            }
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

            string cedula = txtCedula.Text;
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;

            using (MemoryStream ms = new MemoryStream())
            {
                HuellaTemplate.Serialize(ms);
                byte[] huellaData = ms.ToArray();

                using (MySqlConnection conn = new DatabaseConnection().GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO empleados (cedula, nombre, apellido, huella) VALUES (@cedula, @nombre, @apellido, @huella)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@cedula", cedula);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@apellido", apellido);
                    cmd.Parameters.AddWithValue("@huella", huellaData);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Empleado registrado exitosamente.");
                    }
                    else
                    {
                        MessageBox.Show("Error al registrar empleado.");
                    }
                }
            }
        }
    }
}
