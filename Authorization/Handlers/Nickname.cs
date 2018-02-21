using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Authorization.Entities;
using Core.Networking;

namespace Authorization.Handlers
{
    public class Nickname : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authorized)
            {
                string nickname = packetReader.ReadString();
                if (nickname.Length >= 3 && Core.Constants.isAlphaNumeric(nickname))
                {
                    if (nickname.Length <= 16)
                    {
                        try
                        {
                            MySqlDataReader reader = Databases.Auth.Select(
                                new string[] { "id" },
                                "users",
                                new Dictionary<string, object>() {
                                    { "displayname", nickname }
                                });

                            if (!reader.HasRows)
                            { // TODO: is the nickname allowed?
                                reader.Close();
                                Databases.Auth.Query(string.Concat("UPDATE users SET `displayname` ='", nickname, "' WHERE id=", sender.ID, ";"));
                                sender.UpdateDisplayname(nickname);
                                sender.Send(new Packets.ServerList(sender));
                                sender.Disconnect();
                            }
                            else
                            {
                                reader.Close();
                                sender.Send(new Packets.ServerList(Packets.ServerList.ErrorCodes.NicknameTaken));
                            }

                        }
                        catch { sender.Send(new Packets.ServerList(Packets.ServerList.ErrorCodes.Exception));  sender.Disconnect(); }
                    }
                    else
                    {
                        sender.Send(new Packets.ServerList(Packets.ServerList.ErrorCodes.NewNickname));
                    }
                }
                else
                {
                    sender.Send(new Packets.ServerList(Packets.ServerList.ErrorCodes.NewNickname));
                }
            }
            else
            {
                sender.Disconnect(); // Not authorized, cheating!
            }
        }
    }
}
