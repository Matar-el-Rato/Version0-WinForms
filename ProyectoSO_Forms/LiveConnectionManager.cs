using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ProyectoSO_Forms
{
    /// <summary>
    /// Manages a single persistent TCP connection to the server used
    /// for server-push notifications (MSG_USER_LIST, MSG_CHAT broadcasts)
    /// and for sending chat messages (REQ_SEND_CHAT).
    ///
    /// All networking (connect, handshake, listen) runs on a single
    /// dedicated thread so the UI thread is never blocked.
    ///
    /// MUTUAL EXCLUSION:
    /// _client and _stream are shared between two threads:
    ///   - The UI thread calls Disconnect(), SendChatMessage() and reads _stream.
    ///   - The network thread reads from _stream in ListenerLoop().
    /// _lock protects the connection lifecycle (connect/disconnect).
    /// NetworkStream.Read() and Write() are safe to call simultaneously from
    /// different threads per .NET docs, so reads (listener) and writes (send)
    /// do not need further synchronization — only _stream null-checks do.
    /// </summary>
    public static class LiveConnectionManager
    {
        private const byte ReqConnectLive  = 9;
        private const byte ReqLogout       = 4;
        private const byte ReqSendChat     = 6;
        private const byte ReqJoinRoom     = 5;
        private const byte ReqLeaveRoom    = 8;
        private const byte ReqReady        = 13;
        private const byte ReqUnready      = 16;
        private const byte MsgUserList     = 10;
        private const byte MsgChat         = 11;
        private const byte MsgRoomState    = 12;
        private const byte MsgCountdown    = 14;
        private const byte MsgGameStart    = 15;
        private const int  MaxUsername     = 12;
        private const int  MaxClients      = 64;
        private const int  MaxChatMessage  = 100;
        private const int  MaxRoomPlayers  = 4;
        private const int  NumRooms        = 3;

        // Protects _client/_stream from concurrent access between the UI thread
        // (Disconnect, SendChatMessage) and the network thread (ListenerLoop).
        private static readonly object _lock = new object();

        private static volatile bool   _running = false;
        private static TcpClient       _client;
        private static NetworkStream   _stream;
        private static Thread          _networkThread;
        private static string          _localUsername = "";

        /// <summary>
        /// Fired on the dedicated network thread whenever the server pushes
        /// an updated connected-users list. Subscribers on the UI thread
        /// must use Control.BeginInvoke() to marshal updates safely.
        /// </summary>
        public static event Action<List<string>> OnUserListUpdated;

        /// <summary>Fired on the network thread when the server broadcasts a chat message.</summary>
        public static event Action<string, string> OnChatMessageReceived;

        /// <summary>
        /// Fired on the network thread when a room's player list changes.
        /// (roomId 1-3, players array). Subscribers must use BeginInvoke().
        /// </summary>
        public static event Action<int, string[]> OnRoomStateUpdated;

        /// <summary>Fired on the network thread each countdown tick. (roomId, secondsRemaining)</summary>
        public static event Action<int, int> OnCountdownTick;

        /// <summary>Fired on the network thread when the server broadcasts game start. (roomId)</summary>
        public static event Action<int> OnGameStartReceived;

        /// <summary>Last player list received from the server.</summary>
        public static List<string> LastKnownPlayers { get; private set; } = new List<string>();

        /// <summary>The room the local player is currently in (0 = lobby).</summary>
        public static int CurrentRoomId { get; private set; } = 0;

        /// <summary>Last known player lists per room (index 1-3; index 0 unused).</summary>
        public static string[][] RoomPlayers { get; private set; } = new string[NumRooms + 1][];

        /// <summary>
        /// Opens a persistent connection on a dedicated thread and starts
        /// listening for server-push messages. If already connected,
        /// disconnects cleanly first to prevent two threads running at once.
        /// </summary>
        public static void Connect(string host, int port, string username, int userId)
        {
            _localUsername = username;
            CurrentRoomId  = 0;
            for (int i = 0; i <= NumRooms; i++) RoomPlayers[i] = new string[0];

            Disconnect();

            _running = true;

            // All networking runs on this single dedicated thread.
            _networkThread = new Thread(() => NetworkThreadMain(host, port, username, userId))
            {
                IsBackground = true,
                Name = "MER-LiveConnection"
            };
            _networkThread.Start();
        }

        /// <summary>
        /// Closes the persistent connection and waits for the network
        /// thread to exit. Safe to call when not connected.
        /// </summary>
        public static void Disconnect()
        {
            // Signal the network thread to stop. volatile ensures the
            // thread sees this change on its next loop iteration.
            _running = false;

            // LOCK: Close _client under the lock so we don't race with
            // the network thread which may be reading from the stream.
            // Closing the socket causes the blocked Read() in ListenerLoop
            // to throw an exception, which is the intended shutdown signal.
            lock (_lock)
            {
                if (_client != null)
                {
                    try { _client.Close(); } catch { }
                    _client = null;
                    _stream = null;
                }
            }

            // Wait for the network thread to finish. The 2s timeout is a
            // safety net in case the thread is stuck in a blocking read
            // that Close() didn't unblock (unlikely but defensive).
            if (_networkThread != null && _networkThread.IsAlive)
                _networkThread.Join(2000);
            _networkThread = null;
        }

        /// <summary>
        /// Sends REQ_LOGOUT on the live connection so the server removes this client
        /// gracefully before we close the socket. Safe to call when not connected.
        /// </summary>
        public static void SendLogout()
        {
            NetworkStream stream;
            lock (_lock) { stream = _stream; }
            if (stream == null) return;

            var packet = new byte[] { ReqLogout };
            try { stream.Write(packet, 0, packet.Length); } catch { }
        }

        /// <summary>Sends REQ_JOIN_ROOM on the live connection. roomId must be 1-3.</summary>
        public static void SendJoinRoom(int roomId)
        {
            NetworkStream stream;
            lock (_lock) { stream = _stream; }
            if (stream == null) return;

            var packet = new byte[] { ReqJoinRoom, (byte)roomId };
            try { stream.Write(packet, 0, packet.Length); } catch { }
        }

        /// <summary>Sends REQ_LEAVE_ROOM on the live connection.</summary>
        public static void SendLeaveRoom()
        {
            NetworkStream stream;
            lock (_lock) { stream = _stream; }
            if (stream == null) return;

            var packet = new byte[] { ReqLeaveRoom };
            try { stream.Write(packet, 0, packet.Length); } catch { }
        }

        /// <summary>Sends REQ_READY — signals the player is ready to start the game.</summary>
        public static void SendReady()
        {
            NetworkStream stream;
            lock (_lock) { stream = _stream; }
            if (stream == null) return;

            var packet = new byte[] { ReqReady };
            try { stream.Write(packet, 0, packet.Length); } catch { }
        }

        /// <summary>Sends REQ_UNREADY — cancels a previous ready state.</summary>
        public static void SendUnready()
        {
            NetworkStream stream;
            lock (_lock) { stream = _stream; }
            if (stream == null) return;

            var packet = new byte[] { ReqUnready };
            try { stream.Write(packet, 0, packet.Length); } catch { }
        }

        /// <summary>
        /// Sends a REQ_SEND_CHAT packet on the live connection.
        /// Silently no-ops if not connected. Max 100 chars enforced client-side.
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (message.Length > MaxChatMessage)
                message = message.Substring(0, MaxChatMessage);

            NetworkStream stream;
            lock (_lock) { stream = _stream; }
            if (stream == null) return;

            var packet = new byte[1 + MaxChatMessage];
            packet[0] = ReqSendChat;
            var msgBytes = Encoding.ASCII.GetBytes(message);
            Array.Copy(msgBytes, 0, packet, 1, Math.Min(msgBytes.Length, MaxChatMessage));

            // NetworkStream.Write is safe to call from a different thread than
            // Read() — .NET guarantees independent send/receive buffers.
            try { stream.Write(packet, 0, packet.Length); }
            catch { }
        }

        /// <summary>
        /// Runs entirely on the dedicated network thread: connects,
        /// sends the handshake, then loops reading server-push messages
        /// until disconnected.
        /// </summary>
        private static void NetworkThreadMain(string host, int port, string username, int userId)
        {
            try
            {
                var client = new TcpClient();
                var connectResult = client.BeginConnect(host, port, null, null);
                bool connected = connectResult.AsyncWaitHandle.WaitOne(5000);
                if (!connected || !client.Connected)
                {
                    client.Close();
                    _running = false;
                    return;
                }
                client.EndConnect(connectResult);

                // Build REQ_CONNECT_LIVE packet: [type 1B][username 12B][user_id 4B big-endian]
                var packet = new byte[1 + MaxUsername + 4];
                packet[0] = ReqConnectLive;

                var userBytes = Encoding.ASCII.GetBytes(username);
                Array.Copy(userBytes, 0, packet, 1, Math.Min(userBytes.Length, MaxUsername));

                int beId = IPAddress.HostToNetworkOrder(userId);
                var idBytes = BitConverter.GetBytes(beId);
                Array.Copy(idBytes, 0, packet, 1 + MaxUsername, 4);

                var stream = client.GetStream();
                stream.Write(packet, 0, packet.Length);

                // LOCK: publish the TcpClient and stream references so Disconnect()
                // and SendChatMessage() on the UI thread can find them.
                lock (_lock)
                {
                    _client = client;
                    _stream = stream;
                }

                ListenerLoop(stream);
            }
            catch (Exception)
            {
                // Connection failed — exit cleanly
            }

            _running = false;
        }

        /// <summary>
        /// Blocks on the network stream reading server-push messages.
        /// Handles MSG_USER_LIST and MSG_CHAT. Runs on the dedicated network thread.
        /// </summary>
        private static void ListenerLoop(NetworkStream stream)
        {
            var typeBuf   = new byte[1];
            var usersBuf  = new byte[MaxClients * MaxUsername];
            var chatBuf   = new byte[MaxUsername + MaxChatMessage];

            while (_running)
            {
                // Read 1-byte message type. ReadExact returns false on disconnect.
                if (!ReadExact(stream, typeBuf, 1)) break;
                byte msgType = typeBuf[0];

                if (msgType == MsgUserList)
                {
                    var countBuf = new byte[1];
                    if (!ReadExact(stream, countBuf, 1)) break;
                    int msgCount = countBuf[0];

                    int toRead = msgCount * MaxUsername;
                    if (toRead > 0 && !ReadExact(stream, usersBuf, toRead)) break;

                    var users = new List<string>(msgCount);
                    for (int i = 0; i < msgCount; i++)
                    {
                        int start = i * MaxUsername;
                        int len = 0;
                        while (len < MaxUsername && usersBuf[start + len] != 0) len++;
                        users.Add(Encoding.ASCII.GetString(usersBuf, start, len));
                    }

                    LastKnownPlayers = users;
                    OnUserListUpdated?.Invoke(users);
                }
                else if (msgType == MsgChat)
                {
                    // Payload: username[12B] + message[100B] = 112 bytes
                    if (!ReadExact(stream, chatBuf, MaxUsername + MaxChatMessage)) break;

                    int uLen = 0;
                    while (uLen < MaxUsername && chatBuf[uLen] != 0) uLen++;
                    string sender = Encoding.ASCII.GetString(chatBuf, 0, uLen);

                    int mLen = 0;
                    while (mLen < MaxChatMessage && chatBuf[MaxUsername + mLen] != 0) mLen++;
                    string message = Encoding.ASCII.GetString(chatBuf, MaxUsername, mLen);

                    OnChatMessageReceived?.Invoke(sender, message);
                }
                else if (msgType == MsgRoomState)
                {
                    // Payload: room_id[1B] + count[1B] + players[4][12B] = 50 bytes
                    var roomBuf = new byte[2 + MaxRoomPlayers * MaxUsername];
                    if (!ReadExact(stream, roomBuf, roomBuf.Length)) break;

                    int roomId = roomBuf[0];
                    int count  = roomBuf[1];
                    if (count > MaxRoomPlayers) count = MaxRoomPlayers;

                    var players = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        int start = 2 + i * MaxUsername;
                        int len = 0;
                        while (len < MaxUsername && roomBuf[start + len] != 0) len++;
                        players[i] = Encoding.ASCII.GetString(roomBuf, start, len);
                    }

                    if (roomId >= 1 && roomId <= NumRooms)
                    {
                        RoomPlayers[roomId] = players;

                        bool myNameHere = Array.IndexOf(players, _localUsername) >= 0;
                        if (myNameHere)
                            CurrentRoomId = roomId;
                        else if (CurrentRoomId == roomId)
                            CurrentRoomId = 0;
                    }

                    OnRoomStateUpdated?.Invoke(roomId, players);
                }
                else if (msgType == MsgCountdown)
                {
                    // Payload: room_id(1B) + seconds(1B)
                    var cntBuf = new byte[2];
                    if (!ReadExact(stream, cntBuf, 2)) break;
                    OnCountdownTick?.Invoke(cntBuf[0], cntBuf[1]);
                }
                else if (msgType == MsgGameStart)
                {
                    // Payload: room_id(1B)
                    var gsBuf = new byte[1];
                    if (!ReadExact(stream, gsBuf, 1)) break;
                    OnGameStartReceived?.Invoke(gsBuf[0]);
                }
            }
        }

        /// <summary>
        /// Reads exactly <paramref name="count"/> bytes from the stream,
        /// looping until all bytes arrive or the stream closes.
        /// </summary>
        private static bool ReadExact(NetworkStream stream, byte[] buf, int count)
        {
            int received = 0;
            while (received < count)
            {
                int n = stream.Read(buf, received, count - received);
                if (n <= 0) return false;
                received += n;
            }
            return true;
        }
    }
}
