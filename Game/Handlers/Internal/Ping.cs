using Core.Networking;
using Game.Networking;

namespace Game.Handlers.Internal {
    class Ping : PacketHandler<ServerClient>
    {
        public override void Handle(ServerClient sender, InPacket packetReader)
        {

        }
    }
}

