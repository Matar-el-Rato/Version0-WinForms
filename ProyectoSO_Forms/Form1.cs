using System;
using System.Windows.Forms;

namespace ProyectoSO_Forms
{
    public partial class Form1 : Form
    {
        private readonly int _userId;
        private readonly string _username;
        private const string ServerHost = "bolty.website";
        private const int ServerPort = 8888;

        public Form1(int userId, string username)
        {
            _userId = userId;
            _username = username;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = $"Logged in as {_username} (ID {_userId})";
            numericSkin.Value = 0;
            lblStatus.Text = "Select a skin ID and click Update Skin.";
        }

        private void btnUpdateSkin_Click(object sender, EventArgs e)
        {
            var skinId = (int)numericSkin.Value;
            lblStatus.Text = "Updating skin...";

            var result = ServerProtocol.ChangeSkin(ServerHost, ServerPort, _userId, skinId);

            if (!result.IsSuccess)
            {
                lblStatus.Text = "Skin update failed: " + result.Message;
                return;
            }

            lblStatus.Text = result.Message;
        }
    }
}
