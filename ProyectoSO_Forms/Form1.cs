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
        private ComboBox             _cmbPiece;
        private Button               _btnMovePiece;
        private Label                _lblTurnStatus;
        private Label                _lblDiceResult;

        // Piece movement state
        private string               _myColor        = null;
        private readonly int[]       _myPositions    = new int[4];   // piece_positions for our color
        private readonly List<int>   _moveablePieces = new List<int>();
        private bool                 _diceRolled     = false;

        // user_id → username, populated from chair_taken events.
        private readonly Dictionary<int, string> _usernameByUserId = new Dictionary<int, string>();

        // user_id → player panel index (2–4), assigned in chair_taken order for remote players.
        private readonly Dictionary<int, int> _playerPanelByUserId = new Dictionary<int, int>();
        private int _nextRemotePanel = 2;

        // Chair selection display (index 0-3 = yellow/red/green/blue)
        // Order matches game activation: 2p = yellow+red, 3p adds green, 4p adds blue.
        private static readonly string[] ChairColorKeys  = { "yellow", "red", "green", "blue" };
        private static readonly Color[]  ChairLabelColors =
        {
            Color.FromArgb(200, 170, 0),
            Color.FromArgb(200, 50,  50),
            Color.FromArgb(40,  160, 60),
            Color.FromArgb(60,  130, 220)
        };
        private readonly Button[] _chairButtons = new Button[4];

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
            LiveConnectionManager.OnGameActionReceived  += OnGameActionReceived;
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
            LiveConnectionManager.OnGameActionReceived  -= OnGameActionReceived;
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
            LiveConnectionManager.OnGameActionReceived  -= OnGameActionReceived;
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

            // Chair selection panel (top-left gap, above P3)
            BuildChairPanel(new Point(5, 18));

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

            _cmbPiece = new ComboBox
            {
                Location      = new Point(origin.X + 123, origin.Y + 88),
                Size          = new Size(78, 28),
                Font          = new Font("Microsoft Sans Serif", 8.5f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled       = false
            };
            _cmbPiece.Items.AddRange(new object[] { "Piece 0", "Piece 1", "Piece 2", "Piece 3" });
            _cmbPiece.SelectedIndex = 0;
            _cmbPiece.SelectedIndexChanged += (s, e) => UpdateMovePieceButton();
            groupBoxParchis.Controls.Add(_cmbPiece);

            _btnMovePiece = new Button
            {
                Text      = "Move Piece",
                Location  = new Point(origin.X + 209, origin.Y + 88),
                Size      = new Size(100, 34),
                Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(140, 80, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled   = false
            };
            _btnMovePiece.Click += OnMovePieceClicked;
            groupBoxParchis.Controls.Add(_btnMovePiece);

            _lblTurnStatus = new Label
            {
                Text      = "Not your turn",
                Location  = new Point(origin.X + 125, origin.Y + 96),
                Size      = new Size(200, 18),
                Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            groupBoxParchis.Controls.Add(_lblTurnStatus);

            _lblDiceResult = new Label
            {
                Text      = "",
                Location  = new Point(origin.X + 340, origin.Y + 96),
                Size      = new Size(240, 18),
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.DarkOrange
            };
            groupBoxParchis.Controls.Add(_lblDiceResult);
        }

        private void BuildChairPanel(Point location)
        {
            var grp = new GroupBox
            {
                Text     = "Chairs",
                Location = location,
                Size     = new Size(185, 90),
                Font     = new Font("Microsoft Sans Serif", 8f, FontStyle.Bold)
            };

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text      = $"{ChairColorKeys[i].ToUpper()}: --",
                    Location  = new Point(6, 16 + i * 17),
                    Size      = new Size(172, 16),
                    Font      = new Font("Consolas", 7f),
                    ForeColor = Color.White,
                    BackColor = ChairLabelColors[i],
                    FlatStyle = FlatStyle.Flat,
                    Enabled   = false
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => OnChairButtonClicked(idx);
                _chairButtons[i] = btn;
                grp.Controls.Add(btn);
            }

            groupBoxParchis.Controls.Add(grp);
        }

        private void OnChairButtonClicked(int colorIndex)
        {
            string color = ChairColorKeys[colorIndex];
            string json  = $"{{\"action\":\"choose_chair\",\"color\":\"{color}\"}}";
            LiveConnectionManager.SendGameAction(
                LiveConnectionManager.CurrentMatchId,
                LiveConnectionManager.LocalUserId,
                json);
            // Disable all buttons immediately — waiting for server confirmation
            for (int i = 0; i < 4; i++)
                if (_chairButtons[i] != null) _chairButtons[i].Enabled = false;
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
            if (!isOurTurn)
            {
                _diceRolled = false;
                _moveablePieces.Clear();
                if (_cmbPiece     != null) _cmbPiece.Enabled     = false;
                if (_btnMovePiece != null) _btnMovePiece.Enabled = false;
            }
        }

        private void UpdateMovePieceButton()
        {
            if (_btnMovePiece == null || _cmbPiece == null) return;
            int pieceId = _cmbPiece.SelectedIndex;
            bool canMove = _diceRolled && _moveablePieces.Contains(pieceId);
            _btnMovePiece.Enabled   = canMove;
            _btnMovePiece.BackColor = canMove ? Color.FromArgb(140, 80, 180) : Color.FromArgb(140, 140, 140);
        }

        private void OnMovePieceClicked(object sender, EventArgs e)
        {
            if (_cmbPiece == null) return;
            int pieceId = _cmbPiece.SelectedIndex;
            if (!_moveablePieces.Contains(pieceId)) return;

            // Disable immediately to prevent double-send
            _btnMovePiece.Enabled = false;
            _cmbPiece.Enabled     = false;
            _diceRolled           = false;
            _moveablePieces.Clear();

            string json = $"{{\"action\":\"move_piece\",\"piece_id\":{pieceId}}}";
            LiveConnectionManager.SendGameAction(
                LiveConnectionManager.CurrentMatchId,
                LiveConnectionManager.LocalUserId,
                json);
        }

        private void OnItemButtonClicked(int itemIndex)
        {
            // Stub — wire to server when game protocol is ready
        }

        private void OnRollDiceClicked(object sender, EventArgs e)
        {
            var rng  = new Random();
            int die1 = rng.Next(1, 7);
            int die2 = rng.Next(1, 7);

            SetOurTurn(false); // disable button until next turn_start
            if (_lblDiceResult != null)
            {
                _lblDiceResult.Text      = $"You rolled {die1}, {die2} (Total: {die1 + die2})";
                _lblDiceResult.ForeColor = Color.DarkGreen;
            }

            string json = $"{{\"action\":\"roll_dice\",\"die1\":{die1},\"die2\":{die2}}}";
            LiveConnectionManager.SendGameAction(
                LiveConnectionManager.CurrentMatchId,
                LiveConnectionManager.LocalUserId,
                json);
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
                statusLbl.Text      = $"IN GAME  (match {LiveConnectionManager.CurrentMatchId}, {LiveConnectionManager.CurrentPlayerCount}p)";
                statusLbl.ForeColor = Color.MediumPurple;
            }
            var readyBtn = _roomReadyButtons[roomId];
            if (readyBtn != null) { readyBtn.Visible = false; readyBtn.Text = "READY"; readyBtn.BackColor = Color.FromArgb(40, 130, 40); readyBtn.Enabled = true; }
            _roomReady[roomId] = false;
            _playerPanelByUserId.Clear();
            _nextRemotePanel = 2;

            // Enable only the chair slots active for this player count.
            // ChairColorKeys: 0=yellow, 1=red, 2=green, 3=blue
            // 2p: yellow+red (0,1)  3p: adds green (2)  4p: adds blue (3)
            int pc = LiveConnectionManager.CurrentPlayerCount;
            bool[] active = { true, true, pc >= 3, pc >= 4 };
            for (int i = 0; i < 4; i++)
            {
                if (_chairButtons[i] == null) continue;
                _chairButtons[i].Text      = $"{ChairColorKeys[i].ToUpper()}: --";
                _chairButtons[i].BackColor = ChairLabelColors[i];
                _chairButtons[i].Enabled   = active[i];
            }
        }

        // ── Game actions ──────────────────────────────────────────────────────────

        private void OnGameActionReceived(string json)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(OnGameActionReceived), json); return; }
            ApplyGameAction(json);
        }

        private void ApplyGameAction(string json)
        {
            string action = JsonStringValue(json, "action");
            if (action == null) return;

            if (action == "chair_taken")
            {
                string color    = JsonStringValue(json, "color");
                string username = JsonStringValue(json, "username") ?? "?";
                string userIdStr = JsonStringValue(json, "user_id") ?? "";
                if (color == null) return;

                if (int.TryParse(userIdStr, out int uid) && uid > 0)
                {
                    _usernameByUserId[uid] = username;
                    if (uid == _userId) _myColor = color;
                    else if (!_playerPanelByUserId.ContainsKey(uid) && _nextRemotePanel <= 4)
                        _playerPanelByUserId[uid] = _nextRemotePanel++;
                }

                int slot = Array.IndexOf(ChairColorKeys, color);
                if (slot < 0 || slot > 3) return;

                if (_chairButtons[slot] != null)
                {
                    bool isMe = username == _username;
                    _chairButtons[slot].Text    = isMe
                        ? $"{color.ToUpper()}: You ({username})"
                        : $"{color.ToUpper()}: {username}";
                    _chairButtons[slot].Enabled = false;
                    var c = ChairLabelColors[slot];
                    _chairButtons[slot].BackColor = isMe
                        ? c
                        : Color.FromArgb(c.R / 2, c.G / 2, c.B / 2);
                }
            }
            else if (action == "turn_start")
            {
                string userIdStr = JsonStringValue(json, "user_id") ?? "";
                if (!int.TryParse(userIdStr, out int uid)) return;

                bool isOurTurn = uid == _userId;
                SetOurTurn(isOurTurn);
                if (!isOurTurn && _lblTurnStatus != null)
                {
                    string name = _usernameByUserId.TryGetValue(uid, out var n) ? n : $"#{uid}";
                    _lblTurnStatus.Text      = $"{name}'s turn";
                    _lblTurnStatus.ForeColor = Color.DimGray;
                    _lblTurnStatus.Font      = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Italic);
                }
            }
            else if (action == "initiative_sequence")
            {
                ParseAndApplyItemGrants(json);
            }
            else if (action == "dice_result")
            {
                string userIdStr = JsonStringValue(json, "user_id") ?? "";
                string die1Str   = JsonStringValue(json, "die1")    ?? "0";
                string die2Str   = JsonStringValue(json, "die2")    ?? "0";
                string totalStr  = JsonStringValue(json, "total")   ?? "0";

                if (!int.TryParse(userIdStr, out int uid)) return;

                bool isOurs = uid == _userId;
                string who  = isOurs ? "You" : (_usernameByUserId.TryGetValue(uid, out var dName) ? dName : $"#{uid}");

                if (_lblDiceResult != null)
                {
                    _lblDiceResult.Text      = $"{who}: {die1Str}, {die2Str} (Total: {totalStr})";
                    _lblDiceResult.ForeColor = isOurs ? Color.DarkGreen : Color.DarkOrange;
                }

                if (isOurs)
                {
                    _moveablePieces.Clear();
                    ParseMoveablePieces(json, _moveablePieces);
                    _diceRolled = _moveablePieces.Count > 0;
                    if (_cmbPiece != null)
                    {
                        _cmbPiece.Enabled = _diceRolled;
                        _cmbPiece.SelectedIndex = 0;
                    }
                    UpdateMovePieceButton();

                    if (!_diceRolled && _lblDiceResult != null)
                        _lblDiceResult.Text += "  [no moves — passing]";
                }
            }
            else if (action == "piece_moved")
            {
                string ownerIdStr  = JsonStringValue(json, "user_id")   ?? "";
                string pieceIdStr  = JsonStringValue(json, "piece_id")  ?? "";
                string fromStr     = JsonStringValue(json, "from")      ?? "";
                string toStr       = JsonStringValue(json, "to")        ?? "";

                if (!int.TryParse(ownerIdStr,  out int ownerId))  return;
                if (!int.TryParse(pieceIdStr,  out int pieceId))  return;
                if (!int.TryParse(fromStr,      out int fromSq))  return;
                if (!int.TryParse(toStr,        out int toSq))    return;

                if (ownerId == _userId && pieceId >= 0 && pieceId < 4)
                    _myPositions[pieceId] = toSq;

                string who = ownerId == _userId ? "You" : (_usernameByUserId.TryGetValue(ownerId, out var mn) ? mn : $"#{ownerId}");
                if (_lblDiceResult != null)
                {
                    _lblDiceResult.Text      = $"{who} moved piece {pieceId}: sq {fromSq} → {toSq}";
                    _lblDiceResult.ForeColor = ownerId == _userId ? Color.DarkGreen : Color.DarkOrange;
                }
            }
            else if (action == "capture")
            {
                string victimIdStr    = JsonStringValue(json, "victim_user_id") ?? "";
                string victimPieceStr = JsonStringValue(json, "piece_id")       ?? "";
                if (int.TryParse(victimIdStr,    out int vid) && vid == _userId &&
                    int.TryParse(victimPieceStr, out int vp)  && vp >= 0 && vp < 4)
                {
                    _myPositions[vp] = 0; // returned to home
                }
            }
        }

        private static void ParseMoveablePieces(string json, List<int> result)
        {
            const string key = "\"moveable_pieces\":[";
            int start = json.IndexOf(key);
            if (start < 0) return;
            start += key.Length;
            int end = json.IndexOf(']', start);
            if (end < 0) return;
            string content = json.Substring(start, end - start);
            foreach (var part in content.Split(','))
            {
                if (int.TryParse(part.Trim(), out int id))
                    result.Add(id);
            }
        }

        private void ParseAndApplyItemGrants(string json)
        {
            const string key = "\"item_grants\":[";
            int start = json.IndexOf(key);
            if (start < 0) return;

            // Walk to the opening '[' and find the matching ']'.
            start += key.Length - 1;
            int depth = 0, end = start;
            for (; end < json.Length; end++)
            {
                char c = json[end];
                if (c == '[' || c == '{') depth++;
                else if (c == ']' || c == '}') { if (--depth == 0) break; }
            }
            if (end >= json.Length) return;

            string arrayContent = json.Substring(start + 1, end - start - 1);

            // Parse each {user_id:N, items:[...]} grant object.
            int i = 0;
            while (i < arrayContent.Length)
            {
                int objStart = arrayContent.IndexOf('{', i);
                if (objStart < 0) break;

                int d = 0, objEnd = objStart;
                for (; objEnd < arrayContent.Length; objEnd++)
                {
                    char c = arrayContent[objEnd];
                    if (c == '{' || c == '[') d++;
                    else if (c == '}' || c == ']') { if (--d == 0) break; }
                }

                string grantJson = arrayContent.Substring(objStart, objEnd - objStart + 1);
                string uidStr    = JsonStringValue(grantJson, "user_id");
                if (int.TryParse(uidStr, out int uid))
                {
                    bool[] items = new bool[5];

                    const string itemsKey = "\"items\":[";
                    int isStart = grantJson.IndexOf(itemsKey);
                    if (isStart >= 0)
                    {
                        isStart += itemsKey.Length;
                        int isEnd = grantJson.IndexOf(']', isStart);
                        if (isEnd >= 0)
                        {
                            string itemsContent = grantJson.Substring(isStart, isEnd - isStart);
                            int j = 0;
                            while (j < itemsContent.Length)
                            {
                                int q1 = itemsContent.IndexOf('"', j);
                                if (q1 < 0) break;
                                int q2 = itemsContent.IndexOf('"', q1 + 1);
                                if (q2 < 0) break;
                                string itemName = itemsContent.Substring(q1 + 1, q2 - q1 - 1);
                                int itemIdx     = ItemServerNameToIndex(itemName);
                                if (itemIdx >= 0) items[itemIdx] = true;
                                j = q2 + 1;
                            }
                        }
                    }

                    if (uid == _userId)
                        SetOurItems(items);
                    else if (_playerPanelByUserId.TryGetValue(uid, out int panelIdx))
                        SetPlayerItems(panelIdx, items);
                }

                i = objEnd + 1;
            }
        }

        private static int ItemServerNameToIndex(string serverName)
        {
            switch (serverName)
            {
                case "handcuffs":        return 0;
                case "fire_axe":         return 1;
                case "magnifying_glass": return 2;
                case "cigarette":        return 3;
                case "gun":              return 4;
                default:                 return -1;
            }
        }

        // Minimal JSON string-value extractor — no external dependency needed.
        private static string JsonStringValue(string json, string key)
        {
            string needle = "\"" + key + "\":\"";
            int idx = json.IndexOf(needle);
            if (idx < 0)
            {
                // Try without quotes (numeric value as string read)
                needle = "\"" + key + "\":";
                idx = json.IndexOf(needle);
                if (idx < 0) return null;
                idx += needle.Length;
                while (idx < json.Length && json[idx] == ' ') idx++;
                int end = idx;
                while (end < json.Length && json[end] != ',' && json[end] != '}') end++;
                return json.Substring(idx, end - idx).Trim();
            }
            idx += needle.Length;
            int close = json.IndexOf('"', idx);
            return close < 0 ? null : json.Substring(idx, close - idx);
        }
    }
}
