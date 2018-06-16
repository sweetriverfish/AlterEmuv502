using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Core.Networking;
using Authorization.Entities;

namespace Authorization.Handlers
{
    public class ServerList : PacketHandler<User>
    {
        public override void Handle(User sender, InPacket packetReader)
        {
            string crc = packetReader.ReadString();
            byte executionType = packetReader.ReadByte();
            string username = packetReader.ReadString().Trim();
            string password = packetReader.ReadString().Trim();

            if (executionType != 0)
            {
                // Invalid client version.
                SendErrorCodeAndDisconnect(sender, username, Packets.ServerList.ErrorCodes.ClientVerNotMatch);
                return;
            }

            if (string.IsNullOrEmpty(username) || !Core.Constants.IsAlphaNumeric(username))
            {
                // Please enter username error.
                SendErrorCodeAndDisconnect(sender, username, Packets.ServerList.ErrorCodes.EnterID);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                // Please enter password error.
                SendErrorCodeAndDisconnect(sender, username, Packets.ServerList.ErrorCodes.EnterPassword);
                return;
            }

            using (var reader = Databases.Auth.Select(
                       new string[] { "id", "name", "password", "salt", "displayname" },
                        "users",
                       new Dictionary<string, object>() {
                        { "name", username }
                    }))
            {
                if (reader.HasRows && reader.Read())
                {
                    uint id = reader.GetUInt32(0);
                    username = reader.GetString(1);
                    string dbPassword = reader.GetString(2);
                    string dbSalt = reader.GetString(3);
                    string displayname = reader.GetString(4);
                    string hashedPassword = Core.Constants.MD5(password);
                    string doubleHashedPassword = Core.Constants.MD5(hashedPassword);
                    string hashedSalt = Core.Constants.MD5(dbSalt);
                    string finalHash = Core.Constants.MD5(hashedPassword + hashedSalt + doubleHashedPassword);

                    if (finalHash != dbPassword)
                    {
                        SendErrorCodeAndDisconnect(sender, username, Packets.ServerList.ErrorCodes.InvalidPassword);
                        return;
                    }
                    // The connection made a valid Authentication request.
                    var IsOnline = Managers.SessionManager.Instance.Sessions.Select(n => n.Value).Where(n => n.ID == id && n.IsActivated && !n.IsEnded).Count();

                    if (IsOnline != 0)
                    {
                        SendErrorCodeAndDisconnect(sender, username, Packets.ServerList.ErrorCodes.AlreadyLoggedIn);
                        return;
                    }

                    sender.OnAuthorize(id, username, displayname);
                    if (string.IsNullOrWhiteSpace(displayname))
                    {
                        sender.Send(new Packets.ServerList(Packets.ServerList.ErrorCodes.NewNickname));
                    }
                    else
                    {
                        sender.Send(new Packets.ServerList(sender));

                    }

                    Log.Instance.WriteLine($"User {username} successfully logged-in.");

                }
                else
                {
                    SendErrorCodeAndDisconnect(sender, username, Packets.ServerList.ErrorCodes.InvalidUser);
                }
            }
        }

        private void SendErrorCodeAndDisconnect(User sender, string username, Packets.ServerList.ErrorCodes errorCode)
        {
            Log.Instance.WriteError($"User username: {username} failed to authenticate, reason: {errorCode}.");
            sender.Send(new Packets.ServerList(errorCode));
            sender.Disconnect();
        }
    }
}
