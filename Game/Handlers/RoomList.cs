using System.Linq;
using System.Collections;
using Game.Entities;
using Core.Networking;

namespace Game.Handlers
{
    class RoomList : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                if (sender.Room == null)
                {
                    sbyte direction = (sbyte)(packetReader.ReadSbyte(0) - packetReader.ReadSbyte(2));
                    bool waitingOnly = packetReader.ReadBool(1);

                    if (direction == -1)
                    {
                        if (sender.RoomListPage > 0)
                            sender.RoomListPage = (byte)(sender.RoomListPage - 1);
                        else
                            sender.RoomListPage = 0;
                    }
                    else if (direction == 1 && sender.RoomListPage < byte.MaxValue)
                    {
                        sender.RoomListPage += 1;
                    }

                    var result = Managers.ChannelManager.Instance.Get(sender.Channel).Rooms.Select(n => n.Value);

                    if (waitingOnly)
                        result = result.Where(n => n.State == Enums.RoomState.Waiting).OrderByDescending(n => n.ID).Take(8).OrderBy(n => n.ID);
                    else
                        result = result.Where(n => n.ID >= (uint)(8 * sender.RoomListPage) && n.ID < (uint)(8 * (sender.RoomListPage + 1))).OrderBy(n => n.ID);

                    sender.Send(new Packets.RoomList(sender.RoomListPage, new ArrayList(result.ToArray())));
                }
            }
            else
            {
                sender.Disconnect(); // Unauthorized user.
            }
        }
    }
}
