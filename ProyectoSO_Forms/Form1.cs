using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ProyectoSO_Forms
{
    public partial class Form1 : Form
    {
        private readonly int _userId;
        private readonly string _username;
        private readonly int _skinId;
        private const string ServerHost = "bolty.website";
        private const int ServerPort = 8888;

        public Form1(int userId, string username, int skinId)
        {
            _userId = userId;
            _username = username;
            _skinId = skinId;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = $"Logged in as {_username} (ID {_userId})";
            numericSkin.Value = Math.Min(Math.Max(_skinId, (int)numericSkin.Minimum), (int)numericSkin.Maximum);
            lblStatus.Text = $"Current skin: {_skinId}";

            LiveConnectionManager.OnUserListUpdated += OnUserListUpdated;
            LiveConnectionManager.Connect(ServerHost, ServerPort, _username, _userId);

            // Apply any cached players from before the form loaded
            var cached = LiveConnectionManager.LastKnownPlayers;
            if (cached.Count > 0)
                ApplyPlayerList(cached);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LiveConnectionManager.OnUserListUpdated -= OnUserListUpdated;
            LiveConnectionManager.Disconnect();
        }

        private void OnUserListUpdated(List<string> users)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<List<string>>(ApplyPlayerList), users);
                return;
            }
            ApplyPlayerList(users);
        }

        private void ApplyPlayerList(List<string> users)
        {
            listPlayers.Items.Clear();
            foreach (var user in users)
                listPlayers.Items.Add(user);
            lblPlayersCount.Text = $"Connected Players: {users.Count} / 64";
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

            lblStatus.Text = $"Skin updated to {skinId}!";
        }
    }
}
