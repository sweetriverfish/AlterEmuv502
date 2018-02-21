using Core.Networking;
using Game.Networking;

namespace Game.Handlers.Internal {
    class Connection : PacketHandler<ServerClient> {
        public override void Handle(ServerClient sender, InPacket packetReader)
        {
            sender.Send(new Packets.Internal.Authorization());
        }
    }
}
