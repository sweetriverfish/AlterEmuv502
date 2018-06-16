namespace Game.Packets.Internal
{
    class Authorization : Core.Networking.OutPacket
    {
        public Authorization()
            : base((ushort)Core.Enums.InternalPackets.Authorization)
        {
            Append(Core.Constants.Error_OK);
            Append(Config.SERVER_KEY);
            Append(Config.SERVER_NAME);
            Append(Config.SERVER_IP);
            Append((ushort)Core.Enums.Ports.Game);
            Append((byte)Core.Enums.ServerTypes.Normal);
        }
    }
}
