namespace Core.Packets
{
    public class Connection : Networking.OutPacket
    {
        public Connection()
            : base((ushort)Enums.Packets.Connection)
        {
            Append(0);
            Append(77);
        }
    }
}
