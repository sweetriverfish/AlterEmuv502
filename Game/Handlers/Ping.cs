using Core.Networking;
using Game.Entities;

namespace Game.Handlers
{
    public class Ping : PacketHandler<User>
    {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                sender.PingReceived();
            }
            else
            {
                sender.Disconnect(); // Player not authorized - cheating?
            }
        }
    }
}
