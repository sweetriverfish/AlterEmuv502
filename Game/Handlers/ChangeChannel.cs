using System;
using System.Collections;
using System.Linq;
using Game.Enums;
using Core.Networking;
using Game.Entities;

namespace Game.Handlers {
    class ChangeChannel : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                sbyte target = packetReader.ReadSbyte(); ;
                if (target >= 0 && target <= Core.Constants.maxChannelsCount)
                {
                    if (Enum.IsDefined(typeof(ChannelType), target))
                    {
                        sender.SetChannel((ChannelType)target);
                        sender.Send(new Packets.ChangeChannel(sender.Channel));

                        var result = Managers.ChannelManager.Instance.Get(sender.Channel).Rooms.Select(n => n.Value)
                            .Where(n => n.ID >= (uint)(8 * sender.RoomListPage) && n.ID < (uint)(8 * (sender.RoomListPage + 1))).OrderBy(n => n.ID);

                        sender.Send(new Packets.RoomList(sender.RoomListPage, new ArrayList(result.ToArray())));

                    }
                    else
                    {
                        sender.Disconnect(); // Channel is not defined?
                    }
                }
                else
                {
                    sender.Disconnect(); // Channel is out of range.
                }
            }
            else
            {
                sender.Disconnect(); // Unauthorized user.
            }
        }
    }
}
