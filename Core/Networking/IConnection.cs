namespace Core.Networking
{
    public interface IConnection
    {
        void Send(OutPacket packet);
        void Disconnect();
    }
}
