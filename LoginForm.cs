using MySql.Data.MySqlClient;

namespace Huella
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;

        public LoginForm()
        {
            InitializeComponent();
            InitializeComponent2();
        }

        private void InitializeComponent2()
        {
            this.txtUsername = new TextBox { PlaceholderText = "Usuario", Left = 50, Top = 50, Width = 200 };
            this.txtPassword = new TextBox { PlaceholderText = "Contraseña", Left = 50, Top = 100, Width = 200, UseSystemPasswordChar = true };
            this.btnLogin = new Button { Text = "Iniciar Sesión", Left = 50, Top = 150, Width = 200 };
            this.btnLogin.Click += new EventHandler(this.BtnLogin_Click);

            this.Controls.Add(txtUsername);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            using (MySqlConnection conn = new DatabaseConnection().GetConnection())
            {
                conn.Open();
                string query = "SELECT role FROM usuarios WHERE username=@username AND password=@password";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string role = result.ToString();
                    MessageBox.Show("Bienvenido " + role);
                    if (role == "admin")
                    {
                        new RegistroEmpleadosForm().Show();
                        this.Hide();
                    }
                }
                else
                {
                    MessageBox.Show("Credenciales incorrectas");
                }
            }
        }
    }
}
