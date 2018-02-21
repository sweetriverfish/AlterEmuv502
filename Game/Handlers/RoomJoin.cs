using Core.Networking;
using Game.Entities;

namespace Game.Handlers
{
    class RoomJoin : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                if (sender.Room == null)
                {
                    Objects.Channel channel = Managers.ChannelManager.Instance.Get(sender.Channel);

                    ushort roomId = packetReader.ReadUshort();
                    string roomPassword = packetReader.ReadString();

                    if (channel.Rooms.ContainsKey(roomId))
                    {
                        Room r = null;
                        if (channel.Rooms.TryGetValue(roomId, out r))
                        {
                            bool validPassword = true;
                            bool validPremium = true;

                            if (r.HasPassword && r.Password != roomPassword)
                            {
                                validPassword = false;
                            }

                            if (r.PremiumOnly && sender.Premium == Enums.Premium.Free2Play)
                            {
                                validPremium = false;
                            }

                            // TODO: Validate the level limit.

                            if (validPassword)
                            {
                                if (validPremium)
                                {
                                    if (!r.Add(sender))
                                    {
                                        sender.Send(new Packets.RoomJoin(Packets.RoomJoin.ErrorCodes.GenericError));
                                    }
                                }
                                else
                                {
                                    sender.Send(new Packets.RoomJoin(Packets.RoomJoin.ErrorCodes.OnlyPremium));
                                }
                            }
                            else
                            {
                                sender.Send(new Packets.RoomJoin(Packets.RoomJoin.ErrorCodes.InvalidPassword));
                            }
                        }
                        else
                        {
                            sender.Send(new Packets.RoomJoin(Packets.RoomJoin.ErrorCodes.GenericError));
                        }
                    }
                    else
                    {
                        sender.Send(new Packets.RoomJoin(Packets.RoomJoin.ErrorCodes.GenericError));
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
