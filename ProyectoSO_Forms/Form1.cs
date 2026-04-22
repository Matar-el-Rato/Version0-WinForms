using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoSO_Forms
{
    public partial class Form1 : Form
    {
        public bool LoggedOut { get; private set; } = false;

        private readonly int    _userId;
        private readonly string _username;
        private readonly int    _skinId;
        private const string ServerHost = "bolty.website";
        private const int    ServerPort = 8888;

        private int _currentRoom = 0;

        // Room controls (index 1-3)
        private readonly Label[]   _roomStatusLabels = new Label[4];
        private readonly ListBox[] _roomPlayerLists  = new ListBox[4];
        private readonly Button[]  _roomJoinButtons  = new Button[4];
        private readonly Button[]  _roomReadyButtons = new Button[4];
        private readonly bool[]    _roomReady        = new bool[4];

        // Parchis UI (index 1-4 for players)
        private readonly Panel[][]   _playerShells   = new Panel[5][];
        private readonly Label[][]   _playerItemInd  = new Label[5][];
        private readonly GroupBox[]  _playerPanels   = new GroupBox[5];
        private Label                _lblP1TurnStatus;
        private Button[]             _itemButtons;   // [0..4] = Handcuffs, Axe, AmpGlass, Smoke, Makarov
        private Button               _btnRollDice;
        private Label                _lblTurnStatus;

        private static readonly string[] ItemNames      = { "Handcuffs", "Axe", "Amp. Glass", "Smoke", "Makarov" };
        private static readonly string[] ItemNamesShort = { "Cuffs", "Axe", "A.Glass", "Smoke", "Mak." };
        private static readonly Color[]  PlayerColors   = { Color.Black, Color.SteelBlue, Color.Firebrick, Color.SeaGreen, Color.DarkOrange };

        public Form1(int userId, string username, int skinId)
        {
            _userId   = userId;
            _username = username;
            _skinId   = skinId;
            InitializeComponent();
            BuildRoomPanels();
            BuildParchisUI();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblWelcome.Text   = $"Logged in as {_username} (ID {_userId})";
            numericSkin.Value = Math.Min(Math.Max(_skinId, (int)numericSkin.Minimum), (int)numericSkin.Maximum);
            lblStatus.Text    = $"Current skin: {_skinId}";

            LiveConnectionManager.OnUserListUpdated     += OnUserListUpdated;
            LiveConnectionManager.OnChatMessageReceived += OnChatMessageReceived;
            LiveConnectionManager.OnRoomStateUpdated    += OnRoomStateUpdated;
            LiveConnectionManager.OnCountdownTick       += OnCountdownTick;
            LiveConnectionManager.OnGameStartReceived   += OnGameStartReceived;
            LiveConnectionManager.Connect(ServerHost, ServerPort, _username, _userId);

            var cached = LiveConnectionManager.LastKnownPlayers;
            if (cached.Count > 0) ApplyPlayerList(cached);

            for (int r = 1; r <= 3; r++)
            {
                var rp = LiveConnectionManager.RoomPlayers[r];
                if (rp != null && rp.Length > 0) ApplyRoomState(r, rp);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LiveConnectionManager.OnUserListUpdated     -= OnUserListUpdated;
            LiveConnectionManager.OnChatMessageReceived -= OnChatMessageReceived;
            LiveConnectionManager.OnRoomStateUpdated    -= OnRoomStateUpdated;
            LiveConnectionManager.OnCountdownTick       -= OnCountdownTick;
            LiveConnectionManager.OnGameStartReceived   -= OnGameStartReceived;
            LiveConnectionManager.Disconnect();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            LoggedOut = true;
            LiveConnectionManager.SendLogout();
            LiveConnectionManager.OnUserListUpdated     -= OnUserListUpdated;
            LiveConnectionManager.OnChatMessageReceived -= OnChatMessageReceived;
            LiveConnectionManager.OnRoomStateUpdated    -= OnRoomStateUpdated;
            LiveConnectionManager.OnCountdownTick       -= OnCountdownTick;
            LiveConnectionManager.OnGameStartReceived   -= OnGameStartReceived;
            LiveConnectionManager.Disconnect();
            this.Close();
        }

        // ── Room panels (left panel) ──────────────────────────────────────────────

        private void BuildRoomPanels()
        {
            groupBoxRooms.Controls.Clear();

            for (int r = 1; r <= 3; r++)
            {
                int roomId = r;
                int yBase  = 8 + (r - 1) * 90;

                var grp = new GroupBox
                {
                    Text     = $"Room {r}",
                    Location = new Point(6, yBase),
                    Size     = new Size(368, 86),
                    Font     = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold)
                };

                var lblStatus = new Label
                {
                    Text      = "AVAILABLE",
                    Location  = new Point(8, 18),
                    Size      = new Size(200, 16),
                    Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                    ForeColor = Color.Green
                };

                var lblPlayers = new Label
                {
                    Text     = "Players:",
                    Location = new Point(8, 40),
                    Size     = new Size(55, 16),
                    Font     = new Font("Microsoft Sans Serif", 7.5f)
                };

                var listPlayers = new ListBox
                {
                    Location      = new Point(65, 37),
                    Size          = new Size(210, 44),
                    Font          = new Font("Consolas", 8f),
                    SelectionMode = SelectionMode.None,
                    BorderStyle   = BorderStyle.FixedSingle
                };

                var btnReady = new Button
                {
                    Text      = "READY",
                    Location  = new Point(282, 16),
                    Size      = new Size(78, 18),
                    Font      = new Font("Microsoft Sans Serif", 7.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(40, 130, 40),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Visible   = false
                };
                btnReady.Click += (s, e) => OnReadyButtonClicked(roomId);

                var btnJoin = new Button
                {
                    Text      = "Join",
                    Location  = new Point(282, 37),
                    Size      = new Size(78, 44),
                    Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(60, 160, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnJoin.Click += (s, e) => OnRoomButtonClicked(roomId);

                grp.Controls.Add(lblStatus);
                grp.Controls.Add(lblPlayers);
                grp.Controls.Add(listPlayers);
                grp.Controls.Add(btnReady);
                grp.Controls.Add(btnJoin);
                groupBoxRooms.Controls.Add(grp);

                _roomStatusLabels[r] = lblStatus;
                _roomPlayerLists[r]  = listPlayers;
                _roomJoinButtons[r]  = btnJoin;
                _roomReadyButtons[r] = btnReady;
            }
        }

        private void OnRoomButtonClicked(int roomId)
        {
            if (_currentRoom == roomId)
                LiveConnectionManager.SendLeaveRoom();
            else if (_currentRoom == 0)
                LiveConnectionManager.SendJoinRoom(roomId);
            else
            {
                LiveConnectionManager.SendLeaveRoom();
                LiveConnectionManager.SendJoinRoom(roomId);
            }
        }

        // ── Parchis UI (right panel) ──────────────────────────────────────────────

        private void BuildParchisUI()
        {
            groupBoxParchis.Controls.Clear();

            // Central board
            var boardPanel = new Panel
            {
                Location    = new Point(194, 105),
                Size        = new Size(360, 360),
                BorderStyle = BorderStyle.Fixed3D
            };
            string texturePath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "texture.png");
            if (System.IO.File.Exists(texturePath))
            {
                boardPanel.BackgroundImage       = Image.FromFile(texturePath);
                boardPanel.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                boardPanel.BackColor = Color.FromArgb(235, 215, 170);
                boardPanel.Controls.Add(new Label
                {
                    Text      = "PARCHIS\nBOARD",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock      = DockStyle.Fill,
                    Font      = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 65, 20)
                });
            }
            groupBoxParchis.Controls.Add(boardPanel);

            // Player areas around the board
            // P2 top (wide/short), P3 left, P4 right (narrow/tall), P1 us (bottom strip)
            BuildPlayerArea(2, "Player 2", new Point(194, 18),  new Size(360, 83));
            BuildPlayerArea(3, "Player 3", new Point(5,   105), new Size(185, 360));
            BuildPlayerArea(4, "Player 4", new Point(559, 105), new Size(184, 360));
            BuildOurPlayerArea(1, new Point(194, 469), new Size(360, 52));

            // Action bar at the bottom
            BuildActionBar(new Point(5, 525));
        }

        private void BuildPlayerArea(int playerIndex, string name, Point location, Size size)
        {
            var grp = new GroupBox
            {
                Text      = name,
                Location  = location,
                Size      = size,
                Font      = new Font("Microsoft Sans Serif", 8f, FontStyle.Bold),
                ForeColor = PlayerColors[playerIndex]
            };

            // Wide-short (P2 top): lives + items on one row.
            // Narrow-tall (P3/P4 sides): lives row then items stacked.
            bool singleRow = (size.Height <= 90);

            grp.Controls.Add(new Label
            {
                Text      = "Lives:",
                Location  = new Point(5, 22),
                Size      = new Size(38, 15),
                Font      = new Font("Microsoft Sans Serif", 7.5f),
                ForeColor = Color.Black
            });

            _playerShells[playerIndex] = new Panel[3];
            for (int i = 0; i < 3; i++)
            {
                var shell = new Panel
                {
                    Location    = new Point(46 + i * 18, 23),
                    Size        = new Size(13, 13),
                    BackColor   = Color.LimeGreen,
                    BorderStyle = BorderStyle.FixedSingle
                };
                _playerShells[playerIndex][i] = shell;
                grp.Controls.Add(shell);
            }

            int itemLblY = singleRow ? 22 : 42;
            int itemLblX = singleRow ? 103 : 5;
            grp.Controls.Add(new Label
            {
                Text      = "Items:",
                Location  = new Point(itemLblX, itemLblY),
                Size      = new Size(38, 15),
                Font      = new Font("Microsoft Sans Serif", 7.5f),
                ForeColor = Color.Black
            });

            _playerItemInd[playerIndex] = new Label[5];
            for (int i = 0; i < 5; i++)
            {
                Point pos  = singleRow ? new Point(144 + i * 41, 21) : new Point(5, 60 + i * 22);
                Size  sz   = singleRow ? new Size(38, 18)             : new Size(170, 18);

                var ind = new Label
                {
                    Text        = ItemNamesShort[i],
                    Location    = pos,
                    Size        = sz,
                    Font        = new Font("Consolas", 6.5f),
                    ForeColor   = Color.DarkGray,
                    BackColor   = Color.FromArgb(230, 230, 230),
                    BorderStyle = BorderStyle.FixedSingle,
                    TextAlign   = ContentAlignment.MiddleCenter
                };
                _playerItemInd[playerIndex][i] = ind;
                grp.Controls.Add(ind);
            }

            groupBoxParchis.Controls.Add(grp);
            _playerPanels[playerIndex] = grp;
        }

        private void BuildOurPlayerArea(int playerIndex, Point location, Size size)
        {
            var grp = new GroupBox
            {
                Text      = $"You ({_username})",
                Location  = location,
                Size      = size,
                Font      = new Font("Microsoft Sans Serif", 8f, FontStyle.Bold),
                ForeColor = PlayerColors[playerIndex]
            };

            var lblLives = new Label
            {
                Text      = "Lives:",
                Location  = new Point(6, 20),
                Size      = new Size(40, 15),
                Font      = new Font("Microsoft Sans Serif", 7.5f),
                ForeColor = Color.Black
            };
            grp.Controls.Add(lblLives);

            _playerShells[playerIndex] = new Panel[3];
            for (int i = 0; i < 3; i++)
            {
                var shell = new Panel
                {
                    Location    = new Point(50 + i * 20, 21),
                    Size        = new Size(14, 14),
                    BackColor   = Color.LimeGreen,
                    BorderStyle = BorderStyle.FixedSingle
                };
                _playerShells[playerIndex][i] = shell;
                grp.Controls.Add(shell);
            }

            _lblP1TurnStatus = new Label
            {
                Text      = "Waiting for turn...",
                Location  = new Point(108, 20),
                Size      = new Size(240, 15),
                Font      = new Font("Microsoft Sans Serif", 7.5f, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            grp.Controls.Add(_lblP1TurnStatus);

            groupBoxParchis.Controls.Add(grp);
            _playerPanels[playerIndex] = grp;
        }

        private void BuildActionBar(Point origin)
        {
            // Separator
            var sep = new Panel
            {
                Location  = new Point(origin.X, origin.Y),
                Size      = new Size(735, 1),
                BackColor = SystemColors.ControlDark
            };
            groupBoxParchis.Controls.Add(sep);

            // Items section
            var lblYourItems = new Label
            {
                Text      = "Your Items:",
                Location  = new Point(origin.X, origin.Y + 6),
                Size      = new Size(75, 16),
                Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                ForeColor = Color.Black
            };
            groupBoxParchis.Controls.Add(lblYourItems);

            _itemButtons = new Button[5];
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text      = ItemNames[i],
                    Location  = new Point(origin.X + i * 120, origin.Y + 26),
                    Size      = new Size(115, 34),
                    Font      = new Font("Microsoft Sans Serif", 8f, FontStyle.Bold),
                    BackColor = Color.FromArgb(210, 210, 210),
                    ForeColor = Color.DimGray,
                    FlatStyle = FlatStyle.Flat,
                    Enabled   = false
                };
                btn.Click += (s, e) => OnItemButtonClicked(idx);
                _itemButtons[i] = btn;
                groupBoxParchis.Controls.Add(btn);
            }

            // Turn actions section
            var lblTurnActions = new Label
            {
                Text      = "Turn Actions:",
                Location  = new Point(origin.X, origin.Y + 68),
                Size      = new Size(90, 16),
                Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                ForeColor = Color.Black
            };
            groupBoxParchis.Controls.Add(lblTurnActions);

            _btnRollDice = new Button
            {
                Text      = "Roll Dice",
                Location  = new Point(origin.X, origin.Y + 88),
                Size      = new Size(115, 34),
                Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 120, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled   = false
            };
            _btnRollDice.Click += OnRollDiceClicked;
            groupBoxParchis.Controls.Add(_btnRollDice);

            _lblTurnStatus = new Label
            {
                Text      = "Not your turn",
                Location  = new Point(origin.X + 125, origin.Y + 96),
                Size      = new Size(200, 18),
                Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            groupBoxParchis.Controls.Add(_lblTurnStatus);
        }

        // ── Parchis state helpers ─────────────────────────────────────────────────

        public void SetPlayerLives(int playerIndex, int lives)
        {
            if (playerIndex < 1 || playerIndex > 4) return;
            var shells = _playerShells[playerIndex];
            if (shells == null) return;
            for (int i = 0; i < 3; i++)
                shells[i].BackColor = (i < lives) ? Color.LimeGreen : Color.DarkGray;
        }

        public void SetPlayerItems(int playerIndex, bool[] held)
        {
            if (playerIndex < 1 || playerIndex > 4) return;
            var inds = _playerItemInd[playerIndex];
            if (inds == null) return;
            for (int i = 0; i < 5 && i < held.Length; i++)
            {
                inds[i].ForeColor = held[i] ? Color.Black : Color.DarkGray;
                inds[i].BackColor = held[i] ? Color.FromArgb(255, 240, 180) : Color.FromArgb(230, 230, 230);
            }
        }

        public void SetOurItems(bool[] held)
        {
            if (_itemButtons == null) return;
            for (int i = 0; i < 5 && i < held.Length; i++)
            {
                _itemButtons[i].Enabled   = held[i];
                _itemButtons[i].BackColor = held[i] ? Color.FromArgb(255, 220, 80)  : Color.FromArgb(210, 210, 210);
                _itemButtons[i].ForeColor = held[i] ? Color.Black : Color.DimGray;
            }
        }

        public void SetOurTurn(bool isOurTurn)
        {
            if (_btnRollDice == null) return;
            _btnRollDice.Enabled       = isOurTurn;
            _btnRollDice.BackColor     = isOurTurn ? Color.FromArgb(60, 120, 200) : Color.FromArgb(140, 140, 140);
            _lblTurnStatus.Text        = isOurTurn ? "YOUR TURN!" : "Not your turn";
            _lblTurnStatus.ForeColor   = isOurTurn ? Color.DarkGreen : Color.Gray;
            _lblTurnStatus.Font        = new Font("Microsoft Sans Serif", 8.5f,
                isOurTurn ? FontStyle.Bold : FontStyle.Italic);
            if (_lblP1TurnStatus != null)
            {
                _lblP1TurnStatus.Text      = isOurTurn ? "YOUR TURN!" : "Waiting for turn...";
                _lblP1TurnStatus.ForeColor = isOurTurn ? Color.DarkGreen : Color.Gray;
            }
        }

        private void OnItemButtonClicked(int itemIndex)
        {
            // Stub — wire to server when game protocol is ready
        }

        private void OnRollDiceClicked(object sender, EventArgs e)
        {
            // Stub — wire to server when game protocol is ready
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
            foreach (var user in users) listPlayers.Items.Add(user);
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
            foreach (var p in players) playerList.Items.Add(p);

            bool isFull  = players.Length >= 4;
            bool weAreIn = Array.IndexOf(players, _username) >= 0;

            var readyBtn = _roomReadyButtons[roomId];

            if (weAreIn)
            {
                _currentRoom        = roomId;
                statusLbl.Text      = "IN ROOM";
                statusLbl.ForeColor = Color.Orange;
                joinBtn.Text        = "Exit";
                joinBtn.BackColor   = Color.FromArgb(180, 60, 40);
                if (readyBtn != null) readyBtn.Visible = true;
            }
            else if (_currentRoom == roomId)
            {
                _currentRoom        = 0;
                _roomReady[roomId]  = false;
                statusLbl.Text      = players.Length == 0 ? "AVAILABLE" : $"{players.Length}/4";
                statusLbl.ForeColor = Color.Green;
                joinBtn.Text        = "Join";
                joinBtn.BackColor   = Color.FromArgb(60, 160, 60);
                if (readyBtn != null) { readyBtn.Visible = false; readyBtn.Text = "READY"; readyBtn.BackColor = Color.FromArgb(40, 130, 40); readyBtn.Enabled = true; }
            }
            else
            {
                statusLbl.Text      = isFull ? "FULL" : (players.Length == 0 ? "AVAILABLE" : $"{players.Length}/4");
                statusLbl.ForeColor = isFull ? Color.Red : Color.Green;
                joinBtn.Text        = "Join";
                joinBtn.BackColor   = isFull ? Color.FromArgb(100, 100, 100) : Color.FromArgb(60, 160, 60);
                joinBtn.Enabled     = !isFull;
            }

            for (int r = 1; r <= 3; r++)
            {
                if (r == roomId || _roomJoinButtons[r] == null) continue;
                _roomJoinButtons[r].Enabled = _currentRoom == 0 &&
                    Array.IndexOf(LiveConnectionManager.RoomPlayers[r] ?? new string[0], _username) < 0 &&
                    (LiveConnectionManager.RoomPlayers[r]?.Length ?? 0) < 4;
            }
        }

        // ── Skin ──────────────────────────────────────────────────────────────────

        private void btnUpdateSkin_Click(object sender, EventArgs e)
        {
            var skinId = (int)numericSkin.Value;
            lblStatus.Text = "Updating skin...";
            var result = ServerProtocol.ChangeSkin(ServerHost, ServerPort, _userId, skinId);
            lblStatus.Text = result.IsSuccess ? $"Skin updated to {skinId}!" : "Skin update failed: " + result.Message;
        }

        // ── Chat ──────────────────────────────────────────────────────────────────

        private void OnChatMessageReceived(string sender, string message) => AppendChat(sender, message);

        private void AppendChat(string sender, string message)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string, string>(AppendChat), sender, message); return; }
            var time   = DateTime.Now.ToString("HH:mm");
            var prefix = _currentRoom > 0 ? $"[Room {_currentRoom}]" : "[Lobby]";
            txtChatHistory.AppendText($"{prefix} [{time}] {sender}: {message}{Environment.NewLine}");
            txtChatHistory.ScrollToCaret();
        }

        private void SendChat()
        {
            var text = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            LiveConnectionManager.SendChatMessage(text);
            txtChatInput.Clear();
        }

        private void btnSendChat_Click(object sender, EventArgs e) { SendChat(); txtChatInput.Focus(); }

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendChat(); }
        }

        // ── Ready / countdown ─────────────────────────────────────────────────────

        private void OnReadyButtonClicked(int roomId)
        {
            if (_currentRoom != roomId) return;
            _roomReady[roomId] = !_roomReady[roomId];
            var btn = _roomReadyButtons[roomId];
            if (_roomReady[roomId])
            {
                LiveConnectionManager.SendReady();
                if (btn != null) { btn.Text = "CANCEL"; btn.BackColor = Color.FromArgb(140, 60, 60); }
            }
            else
            {
                LiveConnectionManager.SendUnready();
                if (btn != null) { btn.Text = "READY"; btn.BackColor = Color.FromArgb(40, 130, 40); }
            }
        }

        private void OnCountdownTick(int roomId, int seconds)
        {
            if (InvokeRequired) { BeginInvoke(new Action<int, int>(OnCountdownTick), roomId, seconds); return; }
            if (roomId < 1 || roomId > 3) return;
            var statusLbl = _roomStatusLabels[roomId];
            if (statusLbl != null)
            {
                statusLbl.Text      = seconds.ToString();
                statusLbl.ForeColor = Color.OrangeRed;
            }
            var readyBtn = _roomReadyButtons[roomId];
            if (readyBtn != null) readyBtn.Enabled = false;
        }

        private void OnGameStartReceived(int roomId)
        {
            if (InvokeRequired) { BeginInvoke(new Action<int>(OnGameStartReceived), roomId); return; }
            if (roomId < 1 || roomId > 3) return;
            var statusLbl = _roomStatusLabels[roomId];
            if (statusLbl != null)
            {
                statusLbl.Text      = "IN GAME";
                statusLbl.ForeColor = Color.MediumPurple;
            }
            var readyBtn = _roomReadyButtons[roomId];
            if (readyBtn != null) { readyBtn.Visible = false; readyBtn.Text = "READY"; readyBtn.BackColor = Color.FromArgb(40, 130, 40); readyBtn.Enabled = true; }
            _roomReady[roomId] = false;
        }
    }
}
