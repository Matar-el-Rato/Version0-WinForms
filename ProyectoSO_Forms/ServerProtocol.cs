using System;
using System.Net.Sockets;
using System.Text;

namespace ProyectoSO_Forms
{
    public enum ServerResponseCode : byte
    {
        Success = 0,
        UserExists = 1,
        InvalidCredentials = 2,
        Database = 3,
        Unknown = 99
    }

    public class ServerResult
    {
        public bool IsSuccess { get; set; }
        public ServerResponseCode Code { get; set; }
        public string Message { get; set; } = "";
        public int UserId { get; set; } = -1;
    }

    public static class ServerProtocol
    {
        private const int MAX_USERNAME = 12;
        private const int MAX_PASSWORD = 12;
        private const byte REQ_REGISTER = 1;
        private const byte REQ_LOGIN = 2;
        private const byte REQ_CHANGE_SKIN = 3;

        public static ServerResult RegisterUser(string host, int port, string username, string password)
        {
            if (username.Length == 0 || password.Length == 0)
                return new ServerResult { IsSuccess = false, Code = ServerResponseCode.Unknown, Message = "User and password are required." };

            return SendCredentials(host, port, REQ_REGISTER, username, password);
        }

        public static ServerResult LoginUser(string host, int port, string username, string password)
        {
            if (username.Length == 0 || password.Length == 0)
                return new ServerResult { IsSuccess = false, Code = ServerResponseCode.Unknown, Message = "User and password are required." };

            var result = SendCredentials(host, port, REQ_LOGIN, username, password);
            if (!result.IsSuccess) return result;

            // If response is success and message contains ID, parse it
            var msg = result.Message ?? string.Empty;
            var idIdx = msg.IndexOf("ID ", StringComparison.OrdinalIgnoreCase);
            if (idIdx >= 0)
            {
                var idPart = msg.Substring(idIdx + 3).Trim();
                if (int.TryParse(idPart, out var id))
                    result.UserId = id;
            }

            return result;
        }

        public static ServerResult ChangeSkin(string host, int port, int userId, int skinId)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(host, port);
                    using (var stream = client.GetStream())
                    {
                        var packet = new byte[1 + 4 + 4];
                        packet[0] = REQ_CHANGE_SKIN;
                        Array.Copy(BitConverter.GetBytes(userId), 0, packet, 1, 4);
                        Array.Copy(BitConverter.GetBytes(skinId), 0, packet, 5, 4);
                        stream.Write(packet, 0, packet.Length);

                        var response = ReadResponse(stream);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServerResult { IsSuccess = false, Code = ServerResponseCode.Unknown, Message = "Network error: " + ex.Message };
            }
        }

        private static ServerResult SendCredentials(string host, int port, byte requestType, string username, string password)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(host, port);
                    using (var stream = client.GetStream())
                    {
                        var packet = new byte[1 + MAX_USERNAME + MAX_PASSWORD];
                        packet[0] = requestType;

                        var userBytes = Encoding.ASCII.GetBytes(username);
                        var passBytes = Encoding.ASCII.GetBytes(password);

                        Array.Copy(userBytes, 0, packet, 1, Math.Min(userBytes.Length, MAX_USERNAME));
                        Array.Copy(passBytes, 0, packet, 1 + MAX_USERNAME, Math.Min(passBytes.Length, MAX_PASSWORD));

                        stream.Write(packet, 0, packet.Length);

                        var response = ReadResponse(stream);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServerResult { IsSuccess = false, Code = ServerResponseCode.Unknown, Message = "Network error: " + ex.Message };
            }
        }

        private static ServerResult ReadResponse(NetworkStream stream)
        {
            var header = new byte[1];
            int read = stream.Read(header, 0, 1);
            if (read != 1)
                return new ServerResult { IsSuccess = false, Code = ServerResponseCode.Unknown, Message = "Invalid response from server." };

            var code = (ServerResponseCode)header[0];
            var msgBuf = new byte[128];
            int offset = 0;
            while (offset < msgBuf.Length)
            {
                int chunk = stream.Read(msgBuf, offset, msgBuf.Length - offset);
                if (chunk <= 0) break;
                offset += chunk;
            }

            var message = Encoding.ASCII.GetString(msgBuf).TrimEnd('\0', '\r', '\n');
            return new ServerResult
            {
                IsSuccess = code == ServerResponseCode.Success,
                Code = code,
                Message = string.IsNullOrWhiteSpace(message) ? "No response message" : message
            };
        }
    }
}
