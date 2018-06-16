using Core.Networking;
using Game.Entities;
using System;
using System.Linq;

namespace Game.Handlers
{
    class RoomCreation : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                if (sender.Room != null) sender.Disconnect();

                bool isRoomValid = true;
                // READING OUT THE ROOM DATA //
                string name = packetReader.ReadString(0);
                bool hasPassword = packetReader.ReadBool(1);
                string password = packetReader.ReadString(2);
                byte playerCount = packetReader.ReadByte(3);
                byte mapId = packetReader.ReadByte(4); // Ignore this from the client, we will use it server side.
                byte unknown1 = packetReader.ReadByte(5); // Unknown?
                byte unknown2 = packetReader.ReadByte(6); // Unknown?
                byte type = 0;
                byte levelLimit = 0;
                bool premiumOnly = false;
                bool enableVoteKick = true;

                // VALIDATE ROOM NAME //
                if (name.Length == 0 || name.Length > 25)
                { // Name Length
                    if (name.Length != 27)
                        isRoomValid = false;
                }

                // VALIDATE ROOM PASSWORD //
                if (hasPassword && (password.Length == 0 || password == "NULL"))
                { // Password Length
                    isRoomValid = false;
                }

                // VALIDATE MAXIMUM PLAYERS //
                byte highestIndex = 0;
                switch (sender.Channel)
                {
                    case Enums.ChannelType.CQC:
                        {
                            highestIndex = 1;
                            break;
                        }
                    case Enums.ChannelType.Urban_Ops:
                        {
                            highestIndex = 3;
                            break;
                        }
                    case Enums.ChannelType.Battle_Group:
                        {
                            highestIndex = 4;
                            break;
                        }
                    default:
                        {
                            highestIndex = 1;
                            break;
                        }
                }

                if (playerCount > highestIndex)
                {
                    isRoomValid = false;
                }

                // TODO: VALIDATE THE LEVEL TYPE.

                // VALIDATE PREMIUM SETTING //
                if (sender.Premium == Enums.Premium.Free2Play && premiumOnly)
                {
                    isRoomValid = false;
                }

                // TODO: VALIDATE SUPERMASTER & ENABLE VOTEKICK //

                if (isRoomValid)
                {
                    // FETCH OPEN ID //
                    Objects.Channel channel = Managers.ChannelManager.Instance.Get(sender.Channel);
                    int openRoomId = channel.GetOpenRoomID();

                    if (openRoomId >= 0)
                    {
                        Entities.Room room = new Entities.Room(sender, (uint)openRoomId, name, hasPassword, password, playerCount, type, levelLimit, premiumOnly, enableVoteKick);
                        if (room != null)
                        {
                            // ROOM CREATED SUCCESSFULLY //
                            Managers.ChannelManager.Instance.Get(room.Channel).Add(room);

                            sender.Send(new Packets.RoomCreation(room));

                            // SEND THE ROOM UPDATE TO THE LOBBY //
                            byte roomPage = (byte)Math.Floor((decimal)(room.ID / 8));
                            var targetList = Managers.ChannelManager.Instance.Get(room.Channel).Users.Select(n => n.Value).Where(n => n.RoomListPage == roomPage && n.Room == null);
                            if (targetList.Count() > 0)
                            {
                                byte[] outBuffer = new Packets.RoomUpdate(room, false).BuildEncrypted();
                                foreach (Entities.User usr in targetList)
                                    usr.Send(outBuffer);
                            }

                        }
                        else
                        {
                            channel.ForceFreeSlot(openRoomId); // Force the room slot open again.
                            sender.Send(new Packets.RoomCreation(Enums.RoomCreationErrors.GenericError));
                        }
                    }
                    else
                    {
                        sender.Send(new Packets.RoomCreation(Enums.RoomCreationErrors.MaxiumRoomsExceeded));
                    }
                }
                else
                {
                    sender.Send(new Packets.RoomCreation(Enums.RoomCreationErrors.GenericError));
                }

            }
            else
            {
                sender.Disconnect();
            }
        }
    }
}
