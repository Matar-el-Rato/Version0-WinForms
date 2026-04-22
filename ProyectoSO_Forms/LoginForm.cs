using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ProyectoSO_Forms
{
    public partial class LoginForm : Form
    {
        private const string ServerHost = "bolty.website";
        private const int ServerPort = 8888;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text.Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Please enter both username and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (username.Length > 12 || password.Length > 12)
            {
                MessageBox.Show("User and password must be at most 12 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Enabled = false;
            btnRegister.Enabled = false;
            lblStatus.Text = "Connecting...";
            Application.DoEvents();

            var result = ServerProtocol.LoginUser(ServerHost, ServerPort, username, password);

            btnLogin.Enabled = true;
            btnRegister.Enabled = true;

            if (!result.IsSuccess)
            {
                lblStatus.Text = "Login failed: " + result.Message;
                return;
            }

            lblStatus.Text = "Login successful!";
            var mainForm = new Form1(result.UserId, username, result.SkinId);
            this.Hide();
            mainForm.ShowDialog();

            if (mainForm.LoggedOut)
            {
                lblStatus.Text = "";
                txtUsername.Clear();
                txtPassword.Clear();
                this.Show();
                return;
            }

            this.Close();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using (var registerForm = new RegisterForm())
            {
                registerForm.ShowDialog();
            }
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            var username = "admin-forms";
            var password = "forms";
            var result = ServerProtocol.LoginUser(ServerHost, ServerPort, username, password);

            btnLogin.Enabled = true;
            btnRegister.Enabled = true;

            if (!result.IsSuccess)
            {
                lblStatus.Text = "Login failed: " + result.Message;
                return;
            }

            lblStatus.Text = "Login successful!";
            var mainForm = new Form1(result.UserId, username, result.SkinId);
            this.Hide();
            mainForm.ShowDialog();

            if (mainForm.LoggedOut)
            {
                lblStatus.Text = "";
                txtUsername.Clear();
                txtPassword.Clear();
                this.Show();
                return;
            }

            this.Close();

        }
    }
}
