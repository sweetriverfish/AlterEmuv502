namespace Core.Networking {
    public abstract class PacketHandler<T> where T : IConnection {
        public abstract void Handle(T sender, InPacket packetReader);
    }
}
