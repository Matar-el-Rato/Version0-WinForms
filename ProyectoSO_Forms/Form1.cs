using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoSO_Forms
{
    public partial class Form1 : Form
    {
        private readonly int    _userId;
        private readonly string _username;
        private readonly int    _skinId;
        private const string ServerHost = "bolty.website";
        private const int    ServerPort = 8888;

        private int _currentRoom = 0;

        // Per-room controls (index 1-3)
        private readonly Label[]    _roomStatusLabels  = new Label[4];
        private readonly ListBox[]  _roomPlayerLists   = new ListBox[4];
        private readonly Button[]   _roomJoinButtons   = new Button[4];

        public Form1(int userId, string username, int skinId)
        {
            _userId   = userId;
            _username = username;
            _skinId   = skinId;
            InitializeComponent();
            BuildRoomPanels();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblWelcome.Text   = $"Logged in as {_username} (ID {_userId})";
            numericSkin.Value = Math.Min(Math.Max(_skinId, (int)numericSkin.Minimum), (int)numericSkin.Maximum);
            lblStatus.Text    = $"Current skin: {_skinId}";

            LiveConnectionManager.OnUserListUpdated    += OnUserListUpdated;
            LiveConnectionManager.OnChatMessageReceived += OnChatMessageReceived;
            LiveConnectionManager.OnRoomStateUpdated   += OnRoomStateUpdated;
            LiveConnectionManager.Connect(ServerHost, ServerPort, _username, _userId);

            var cached = LiveConnectionManager.LastKnownPlayers;
            if (cached.Count > 0)
                ApplyPlayerList(cached);

            // Apply any already-known room states (reconnect case)
            for (int r = 1; r <= 3; r++)
            {
                var rp = LiveConnectionManager.RoomPlayers[r];
                if (rp != null && rp.Length > 0)
                    ApplyRoomState(r, rp);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LiveConnectionManager.OnUserListUpdated    -= OnUserListUpdated;
            LiveConnectionManager.OnChatMessageReceived -= OnChatMessageReceived;
            LiveConnectionManager.OnRoomStateUpdated   -= OnRoomStateUpdated;
            LiveConnectionManager.Disconnect();
        }

        // ── Room panel construction ───────────────────────────────────────────────

        private void BuildRoomPanels()
        {
            groupBoxGame.Text     = "Rooms";
            groupBoxGame.Controls.Clear();

            for (int r = 1; r <= 3; r++)
            {
                int roomId = r;
                int yBase  = 10 + (r - 1) * 148;

                var grp = new GroupBox
                {
                    Text     = $"Room {r}",
                    Location = new Point(8, yBase),
                    Size     = new Size(430, 140),
                    Font     = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold)
                };

                var lblStatus = new Label
                {
                    Text     = "AVAILABLE",
                    Location = new Point(10, 22),
                    Size     = new Size(200, 18),
                    Font     = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold),
                    ForeColor = Color.Green
                };

                var lblPlayers = new Label
                {
                    Text     = "Players:",
                    Location = new Point(10, 45),
                    Size     = new Size(60, 16),
                    Font     = new Font("Microsoft Sans Serif", 8f)
                };

                var listPlayers = new ListBox
                {
                    Location          = new Point(10, 63),
                    Size              = new Size(310, 64),
                    Font              = new Font("Consolas", 8.5f),
                    SelectionMode     = SelectionMode.None,
                    BorderStyle       = BorderStyle.FixedSingle
                };

                var btnJoin = new Button
                {
                    Text     = "Join",
                    Location = new Point(330, 63),
                    Size     = new Size(88, 28),
                    Font     = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(60, 160, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnJoin.Click += (s, e) => OnRoomButtonClicked(roomId);

                grp.Controls.Add(lblStatus);
                grp.Controls.Add(lblPlayers);
                grp.Controls.Add(listPlayers);
                grp.Controls.Add(btnJoin);
                groupBoxGame.Controls.Add(grp);

                _roomStatusLabels[r] = lblStatus;
                _roomPlayerLists[r]  = listPlayers;
                _roomJoinButtons[r]  = btnJoin;
            }

            // Expand form to fit 3 rooms
            groupBoxGame.Size = new Size(450, 460);
            this.ClientSize   = new Size(this.ClientSize.Width, Math.Max(this.ClientSize.Height, 490));
        }

        private void OnRoomButtonClicked(int roomId)
        {
            if (_currentRoom == roomId)
            {
                // Exit current room
                LiveConnectionManager.SendLeaveRoom();
            }
            else if (_currentRoom == 0)
            {
                // Join a room
                LiveConnectionManager.SendJoinRoom(roomId);
            }
            else
            {
                // Already in a different room — leave first, then join
                LiveConnectionManager.SendLeaveRoom();
                LiveConnectionManager.SendJoinRoom(roomId);
            }
        }

        // ── Player list ───────────────────────────────────────────────────────────

        private void OnUserListUpdated(List<string> users)
        {
            if (InvokeRequired) { BeginInvoke(new Action<List<string>>(ApplyPlayerList), users); return; }
            ApplyPlayerList(users);
        }

        private void ApplyPlayerList(List<string> users)
        {
            listPlayers.Items.Clear();
            foreach (var user in users)
                listPlayers.Items.Add(user);
            lblPlayersCount.Text = $"Connected Players: {users.Count} / 64";
        }

        // ── Room state ────────────────────────────────────────────────────────────

        private void OnRoomStateUpdated(int roomId, string[] players)
        {
            if (InvokeRequired) { BeginInvoke(new Action<int, string[]>(ApplyRoomState), roomId, players); return; }
            ApplyRoomState(roomId, players);
        }

        private void ApplyRoomState(int roomId, string[] players)
        {
            if (roomId < 1 || roomId > 3) return;

            var statusLbl  = _roomStatusLabels[roomId];
            var playerList = _roomPlayerLists[roomId];
            var joinBtn    = _roomJoinButtons[roomId];
            if (statusLbl == null) return;

            playerList.Items.Clear();
            foreach (var p in players)
                playerList.Items.Add(p);

            bool isFull    = players.Length >= 4;
            bool weAreIn   = Array.IndexOf(players, _username) >= 0;

            if (weAreIn)
            {
                _currentRoom      = roomId;
                statusLbl.Text    = "IN GAME";
                statusLbl.ForeColor = Color.Orange;
                joinBtn.Text      = "Exit";
                joinBtn.BackColor = Color.FromArgb(180, 60, 40);
            }
            else if (_currentRoom == roomId)
            {
                // We just left this room
                _currentRoom = 0;
                statusLbl.Text    = players.Length == 0 ? "AVAILABLE" : $"{players.Length}/4";
                statusLbl.ForeColor = Color.Green;
                joinBtn.Text      = "Join";
                joinBtn.BackColor = Color.FromArgb(60, 160, 60);
            }
            else
            {
                statusLbl.Text    = isFull ? "FULL" : (players.Length == 0 ? "AVAILABLE" : $"{players.Length}/4");
                statusLbl.ForeColor = isFull ? Color.Red : Color.Green;
                joinBtn.Text      = "Join";
                joinBtn.BackColor = isFull
                    ? Color.FromArgb(100, 100, 100)
                    : Color.FromArgb(60, 160, 60);
                joinBtn.Enabled = !isFull;
            }

            // Refresh join button states on all other rooms so only one is "Exit"
            for (int r = 1; r <= 3; r++)
            {
                if (r == roomId || _roomJoinButtons[r] == null) continue;
                _roomJoinButtons[r].Enabled = _currentRoom == 0 && Array.IndexOf(
                    LiveConnectionManager.RoomPlayers[r] ?? new string[0], _username) < 0
                    && (LiveConnectionManager.RoomPlayers[r]?.Length ?? 0) < 4;
            }
        }

        // ── Skin ──────────────────────────────────────────────────────────────────

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

        // ── Chat ──────────────────────────────────────────────────────────────────

        private void OnChatMessageReceived(string sender, string message)
        {
            AppendChat(sender, message);
        }

        private void AppendChat(string sender, string message)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string, string>(AppendChat), sender, message); return; }
            var time = DateTime.Now.ToString("HH:mm");
            var prefix = _currentRoom > 0 ? $"[Room {_currentRoom}]" : "[Lobby]";
            txtChatHistory.AppendText($"{prefix} [{time}] {sender}: {message}{System.Environment.NewLine}");
            txtChatHistory.ScrollToCaret();
        }

        private void SendChat()
        {
            var text = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            LiveConnectionManager.SendChatMessage(text);
            txtChatInput.Clear();
        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            SendChat();
            txtChatInput.Focus();
        }

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendChat();
            }
        }
    }
}
