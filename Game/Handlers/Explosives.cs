using Core.Networking;
using Game.Entities;

namespace Game.Handlers {
    class Explosives : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (!sender.Authenticated || sender.Room == null || sender.Room.State != Enums.RoomState.Playing)
            {
                sender.Disconnect(); // Received this packet at a wrong time? Cheating!
                return;
            }

            sender.Room.HandleExplosives(packetReader.Blocks, sender);
        }
    }
}
