using Authorization.Entities;
using Core.Networking;

namespace Authorization.Networking
{
    class NetworkTable : AbstractNetworkTable<User, Server>
    {
        protected override void OnInitialize()
        {
            // Internal Packets//
            AddInternal(Core.Enums.InternalPackets.Authorization, new Handlers.Internal.Authorization());
            AddInternal(Core.Enums.InternalPackets.Ping, new Handlers.Internal.Ping());
            AddInternal(Core.Enums.InternalPackets.PlayerAuthorization, new Handlers.Internal.PlayerAuthorization());

            // External Packets //
            AddExternal((ushort)Enums.Packets.Launcher, new Handlers.Launcher());
            AddExternal((ushort)Enums.Packets.ServerList, new Handlers.ServerList());
            AddExternal((ushort)Enums.Packets.Nickname, new Handlers.Nickname());
        }

        private static NetworkTable instance = null;
        public static NetworkTable Instance { get { if (instance == null) { instance = new NetworkTable(); } return instance; } }
    }
}
