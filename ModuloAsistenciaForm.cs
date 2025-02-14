using MySql.Data.MySqlClient;
using DPFP.Capture;


namespace Huella
{
    public partial class ModuloAsistenciaForm : Form, DPFP.Capture.EventHandler
    {
        private Label lblStatus;
        private Button btnVerificar;
        private DPFP.Capture.Capture Capturer;

        public ModuloAsistenciaForm()
        {
            InitializeComponent();
            InitializeComponent2();
            Capturer = new DPFP.Capture.Capture();
            Capturer.EventHandler = this;
        }

        private void InitializeComponent2()
        {
            this.lblStatus = new Label { Text = "Coloque su huella", Left = 50, Top = 50, Width = 200 };
            this.btnVerificar = new Button { Text = "Verificar", Left = 50, Top = 100, Width = 200 };
            this.btnVerificar.Click += new System.EventHandler(this.BtnVerificar_Click);

            this.Controls.Add(lblStatus);
            this.Controls.Add(btnVerificar);
        }

        private void BtnVerificar_Click(object sender, EventArgs e)
        {
            Capturer.StartCapture();
            lblStatus.Text = "Escaneando huella...";
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            // Convertir la muestra a un template
            DPFP.Processing.FeatureExtraction extractor = new DPFP.Processing.FeatureExtraction();
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            extractor.CreateFeatureSet(Sample, DPFP.Processing.DataPurpose.Verification, ref feedback, ref features);

            if (feedback == DPFP.Capture.CaptureFeedback.Good)
            {
                using (MySqlConnection conn = new DatabaseConnection().GetConnection())
                {
                    conn.Open();
                    string query = "SELECT nombre FROM empleados WHERE huella=@huella";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@huella", features.Bytes);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        string nombre = result.ToString();
                        MessageBox.Show("Bienvenido, " + nombre);
                    }
                    else
                    {
                        MessageBox.Show("Huella no reconocida");
                    }
                }
            }
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback) { }
    }
}
