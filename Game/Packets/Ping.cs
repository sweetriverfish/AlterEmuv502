using Game.Enums;

namespace Game.Packets
{
    class Ping : Core.Networking.OutPacket
    {

        public Ping(Entities.User u)
            : base((ushort)Enums.Packets.Ping)
        {
            Append(5000); // Max players in server?
            Append(u.Ping); // Ping
            Append(0);  // ?
            Append(-1); // Event time
            Append(4); // ?
            Append(1); // EXP Rate
            Append(1); // Dinar Rate
            Append((u.PremiumTimeInSeconds > 0) ? u.PremiumTimeInSeconds : -1); // Premium Time
        }
    }
}
