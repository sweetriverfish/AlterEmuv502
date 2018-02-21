using System;
using Game.Enums;
using System.Linq;
using Core.Networking;
using Game.Entities;

namespace Game.Handlers
{
    class Chat : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                byte type = packetReader.ReadByte();

                if (Enum.IsDefined(typeof(ChatType), type))
                {
                    // TODO: Read everything correctly.
                    ChatType chatType = (ChatType)type;
                    uint nowTimeStamp = (uint)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                    uint targetId = packetReader.ReadUint();
                    string targetName = packetReader.ReadString();
                    string message = packetReader.ReadString();
                    string realMessage = message.Split(new string[] { ">>" }, StringSplitOptions.None)[1].Trim();

                    if (realMessage.Length <= 55)
                    {
                        realMessage = realMessage.Trim();
                        switch (chatType)
                        {
                            case ChatType.LobbyToChannel:
                                {
                                    if (sender.Room == null)
                                    {
                                        Databases.Game.AsyncInsert("chat_public_lobby", new System.Collections.Generic.Dictionary<string, object>() { { "server", Config.SERVER_ID }, { "channel_id", (byte)sender.Channel }, { "sender_id", sender.ID }, { "target_all", 0 }, { "message", realMessage }, { "timestamp", nowTimeStamp } });
                                        OutPacket p = new Packets.Chat(sender, chatType, message, targetId, targetName);
                                        Managers.ChannelManager.Instance.SendLobby(sender.Channel, p.BuildEncrypted());
                                    }
                                    else
                                    {
                                        sender.Disconnect(); // Sending lobby messages when in a room?
                                    }
                                    break;
                                }
                            case ChatType.LobbyToAll:
                                {
                                    if (sender.Room == null)
                                    {
                                        Databases.Game.AsyncInsert("chat_public_lobby", new System.Collections.Generic.Dictionary<string, object>() { { "server", Config.SERVER_ID }, { "channel_id", (byte)sender.Channel }, { "sender_id", sender.ID }, { "target_all", 1 }, { "message", realMessage }, { "timestamp", nowTimeStamp } });
                                        OutPacket p = new Packets.Chat(sender, chatType, message, targetId, targetName);
                                        Managers.ChannelManager.Instance.SendAllLobbies(p.BuildEncrypted());
                                    }
                                    else
                                    {
                                        sender.Disconnect(); // Sending lobby messages when in a room?
                                    }
                                    break;
                                }
                            case ChatType.RoomToAll:
                                {
                                    if (sender.Room != null)
                                    {
                                        if (sender.Room.State == RoomState.Waiting && sender.RoomSlot == sender.Room.Master)
                                        {
                                            if (sender.Room.Supermaster)
                                            {
                                                targetId = 998;
                                            }
                                        }
                                        Databases.Game.AsyncInsert("chat_public_room", new System.Collections.Generic.Dictionary<string, object>() { { "server", Config.SERVER_ID }, { "channel_id", (byte)sender.Channel }, { "sender_id", sender.ID }, { "room_id", sender.Room.ID }, { "team_side", (byte)sender.Room.Players[sender.RoomSlot].Team }, { "target_all", 1 }, { "message", realMessage }, { "timestamp", nowTimeStamp } });
                                        OutPacket p = new Packets.Chat(sender, chatType, message, targetId, targetName);
                                        sender.Room.Send(p.BuildEncrypted());
                                    }
                                    else
                                    {
                                        sender.Disconnect();
                                    }
                                    break;
                                }
                            case ChatType.RoomToTeam:
                                {
                                    if (sender.Room != null)
                                    {
                                        if (sender.Room.Mode != Mode.Free_For_All && sender.Room.State == RoomState.Playing)
                                        {
                                            Databases.Game.AsyncInsert("chat_public_room", new System.Collections.Generic.Dictionary<string, object>() { { "server", Config.SERVER_ID }, { "channel_id", (byte)sender.Channel }, { "sender_id", sender.ID }, { "room_id", sender.Room.ID }, { "team_side", (byte)sender.Room.Players[sender.RoomSlot].Team }, { "target_all", 0 }, { "message", realMessage }, { "timestamp", nowTimeStamp } });
                                            OutPacket p = new Packets.Chat(sender, chatType, message, targetId, targetName);
                                            byte[] buffer = p.BuildEncrypted();
                                            sender.Room.Players.Values.Where(n => n.Team == sender.Room.Players[sender.RoomSlot].Team).ToList().ForEach(n => n.Send(buffer));
                                        }
                                        else
                                        {
                                            sender.Disconnect(); // NO team CHAT IN FFA or in the lobby.
                                        }
                                    }
                                    break;
                                }

                            default:
                                {

                                    break;
                                }
                        }
                    }
                    else
                    {
                        sender.Disconnect(); // Message is to long?
                    }
                }
            }
            else
            {
                sender.Disconnect();
            }
        }
    }
}
