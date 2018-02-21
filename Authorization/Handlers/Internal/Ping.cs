using Authorization.Entities;
using Core.Networking;

namespace Authorization.Handlers.Internal
{
    class Ping : PacketHandler<Server>
    {
        public override void Handle(Server sender, InPacket packetReader)
        {
            // TODO Implement something here?
        }
    }
}
