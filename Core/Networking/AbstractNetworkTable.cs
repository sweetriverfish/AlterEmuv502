using System.Collections.Generic;

namespace Core.Networking
{
    public abstract class AbstractNetworkTable<ExternalType, InternalType>
        where ExternalType : IConnection
        where InternalType : IConnection
    {
        private Dictionary<ushort, PacketHandler<ExternalType>> externalPacketList;
        private Dictionary<ushort, PacketHandler<InternalType>> internalPacketList;

        public AbstractNetworkTable()
        {
            externalPacketList = new Dictionary<ushort, PacketHandler<ExternalType>>();
            internalPacketList = new Dictionary<ushort, PacketHandler<InternalType>>();
            OnInitialize();
        }

        protected abstract void OnInitialize();

        protected void AddInternal(Enums.InternalPackets packetType, PacketHandler<InternalType> handler)
        {
            if (!internalPacketList.ContainsKey((ushort)packetType))
            {
                internalPacketList.Add((ushort)packetType, handler);
            }
        }

        public PacketHandler<InternalType> FindInternal(InPacket inPacket)
        {
            if (internalPacketList.ContainsKey(inPacket.Id))
            {
                return internalPacketList[inPacket.Id];
            }
            return null;
        }

        protected void AddExternal(ushort packetId, PacketHandler<ExternalType> handler)
        {
            if (!externalPacketList.ContainsKey(packetId))
            {
                externalPacketList.Add(packetId, handler);
            }
        }

        public PacketHandler<ExternalType> FindExternal(InPacket inPacket)
        {
            if (externalPacketList.ContainsKey(inPacket.Id))
            {
                return externalPacketList[inPacket.Id];
            }
            return null;
        }

    }
}
