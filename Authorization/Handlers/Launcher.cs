using Authorization.Entities;
using Core.Networking;

namespace Authorization.Handlers
{
    public class Launcher : PacketHandler<User>
    {
        public override void Handle(User sender, InPacket packetReader)
        {
            Log.Instance.WriteDev(string.Format("Patch info request from {0}", sender.RemoteEndIP));
            sender.Send(new Packets.Launcher());
            sender.Disconnect();
        }
    }
}
