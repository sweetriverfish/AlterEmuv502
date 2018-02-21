using Core.Networking;
using Game.Entities;

namespace Game.Handlers
{
    class Scoreboard : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (!sender.Authenticated || sender.Room == null || sender.Room.State != Enums.RoomState.Playing)
            {
                sender.Disconnect();
                return;
            }

            sender.Send(new Packets.Scoreboard(sender.Room)); // Send scoreboard :)
        }
    }
}
