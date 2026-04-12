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
    /// exclusively for server-push notifications (MSG_USER_LIST broadcasts).
    ///
    /// All networking (connect, handshake, listen) runs on a single
    /// dedicated thread so the UI thread is never blocked.
    /// </summary>
    public static class LiveConnectionManager
    {
        private const byte ReqConnectLive = 9;
        private const byte MsgUserList = 10;
        private const int MaxUsername = 12;
        private const int MaxClients = 64;

        private static readonly object _lock = new object();
        private static volatile bool _running = false;
        private static TcpClient _client;
        private static Thread _networkThread;

        /// <summary>
        /// Fired on the dedicated network thread whenever the server pushes
        /// an updated connected-users list. Subscribers on the UI thread
        /// must use Control.BeginInvoke() to marshal updates safely.
        /// </summary>
        public static event Action<List<string>> OnUserListUpdated;

        /// <summary>
        /// Last player list received from the server. Read by the UI to
        /// catch up on any broadcast that arrived before subscribing.
        /// </summary>
        public static List<string> LastKnownPlayers { get; private set; } = new List<string>();

        /// <summary>
        /// Opens a persistent connection on a dedicated thread and starts
        /// listening for server-push messages. If already connected,
        /// disconnects cleanly first.
        /// </summary>
        public static void Connect(string host, int port, string username, int userId)
        {
            Disconnect();

            _running = true;

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
            _running = false;

            lock (_lock)
            {
                if (_client != null)
                {
                    try { _client.Close(); } catch { }
                    _client = null;
                }
            }

            if (_networkThread != null && _networkThread.IsAlive)
                _networkThread.Join(2000);
            _networkThread = null;
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

                client.GetStream().Write(packet, 0, packet.Length);

                lock (_lock)
                {
                    _client = client;
                }

                ListenerLoop(client.GetStream());
            }
            catch (Exception)
            {
                // Connection failed — exit cleanly
            }

            _running = false;
        }

        /// <summary>
        /// Blocks on the network stream reading MSG_USER_LIST packets
        /// pushed by the server. Runs on the dedicated network thread.
        /// </summary>
        private static void ListenerLoop(NetworkStream stream)
        {
            var headerBuf = new byte[2];
            var usersBuf = new byte[MaxClients * MaxUsername];

            while (_running)
            {
                if (!ReadExact(stream, headerBuf, 2)) break;

                byte msgType = headerBuf[0];
                int msgCount = headerBuf[1];

                if (msgType == MsgUserList)
                {
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
