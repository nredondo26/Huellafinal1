using System;
using System.Windows.Forms;
using DPFP.Capture;

namespace Huella
{
    public class FormularioConLector : Form, DPFP.Capture.EventHandler
    {
        protected DPFP.Capture.Capture Capturer;

        public FormularioConLector()
        {
            this.Load += new System.EventHandler(FormularioConLector_Load);
            this.FormClosing += new FormClosingEventHandler(FormularioConLector_Closing);
        }

        private void FormularioConLector_Load(object sender, EventArgs e)
        {
            ReiniciarLector(); // ✅ Se ejecuta al abrir cualquier formulario que herede de esta clase
        }

        private void FormularioConLector_Closing(object sender, FormClosingEventArgs e)
        {
            DetenerYLiberarLector(); // ✅ Se ejecuta al cerrar cualquier formulario que herede de esta clase
        }

        private void ReiniciarLector()
        {
            try
            {
                if (Capturer != null)
                {
                    Capturer.StopCapture();
                    Capturer.EventHandler = null;
                }

                Capturer = new DPFP.Capture.Capture();
                Capturer.EventHandler = this;
                Capturer.StartCapture();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reiniciar el lector: " + ex.Message);
            }
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
                MessageBox.Show("Error al detener el lector: " + ex.Message);
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
