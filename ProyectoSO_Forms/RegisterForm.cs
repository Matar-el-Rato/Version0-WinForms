using System;
using System.Windows.Forms;

namespace ProyectoSO_Forms
{
    public partial class RegisterForm : Form
    {
        private const string ServerHost = "bolty.website";
        private const int ServerPort = 8888;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
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

            btnRegister.Enabled = false;
            lblStatus.Text = "Registering...";
            Application.DoEvents();

            var result = ServerProtocol.RegisterUser(ServerHost, ServerPort, username, password);
            btnRegister.Enabled = true;

            if (!result.IsSuccess)
            {
                lblStatus.Text = "Register failed: " + result.Message;
                return;
            }

            MessageBox.Show("Registration successful. You can now login.", "Register", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
