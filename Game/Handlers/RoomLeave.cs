using System;
using System.Linq;
using Core.Networking;
using Game.Entities;

namespace Game.Handlers {
    class RoomLeave : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                if (sender.Room != null)
                {
                    Room room = sender.Room;
                    room.Remove(sender);

                    if (room != null && room.Players.Count > 0)
                    {
                        // SEND THE ROOM UPDATE TO THE LOBBY //
                        byte roomPage = (byte)Math.Floor((decimal)(room.ID / 8));
                        var targetList = Managers.ChannelManager.Instance.Get(room.Channel).Users.Select(n => n.Value).Where(n => n.RoomListPage == roomPage && n.Room == null);
                        if (targetList.Count() > 0)
                        {
                            byte[] outBuffer = new Packets.RoomUpdate(room, false).BuildEncrypted();
                            foreach (User usr in targetList)
                                usr.Send(outBuffer);
                        }
                    }

                    //var result = Managers.ChannelManager.Instance.Get(sender.Channel).Rooms.Select(n => n.Value)
                    //        .Where(n => n.ID >= (uint)(8 * sender.RoomListPage) && n.ID < (uint)(8 * (sender.RoomListPage + 1))).OrderBy(n => n.ID);

                    //sender.Send(new Packets.RoomList(sender.RoomListPage, new ArrayList(result.ToArray()))); 
                }
            }
            else
            {
                sender.Disconnect();
            }
        }
    }
}
