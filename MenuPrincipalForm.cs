using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using DPFP.Capture;
using System.Text.RegularExpressions;


namespace Huella
{
    public partial class MenuPrincipalForm : Form, DPFP.Capture.EventHandler
    {
        private Button btnEstadoDB;
        private Button btnEstadoLector;
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
            this.Size = new System.Drawing.Size(400, 250);

            btnEstadoDB = new Button { Text = "Base de Datos", Left = 50, Top = 50, Width = 120, Height = 50 };
            btnEstadoLector = new Button { Text = "Lector Biométrico", Left = 200, Top = 50, Width = 120, Height = 50 };

            this.Controls.Add(btnEstadoDB);
            this.Controls.Add(btnEstadoLector);
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
                Capturer = new DPFP.Capture.Capture();
                Capturer.EventHandler = this;
                Capturer.StartCapture();
                btnEstadoLector.BackColor = System.Drawing.Color.Green;
            }
            catch
            {
                btnEstadoLector.BackColor = System.Drawing.Color.Red;
            }
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample) { }
        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback) { }
    }
}
