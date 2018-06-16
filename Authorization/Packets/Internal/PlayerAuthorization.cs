using Core.Enums.Internal;

namespace Authorization.Packets.Internal
{
    class PlayerAuthorization : Core.Networking.OutPacket
    {

        public PlayerAuthorization()
            : base((ushort)Core.Enums.InternalPackets.PlayerAuthorization)
        {
        }

        public PlayerAuthorization(Entities.Session session)
            : base((ushort)Core.Enums.InternalPackets.PlayerAuthorization)
        {
            Append((ushort)PlayerAuthorizationErrorCodes.Login);
            Append(session.SessionID);
            Append(session.ID);
            Append(session.Name);
            Append(session.Displayname);
        }

        public PlayerAuthorization(PlayerAuthorizationErrorCodes errorCode, uint targetId)
            : base((ushort)Core.Enums.InternalPackets.PlayerAuthorization)
        {
            Append((ushort)errorCode);
            Append(targetId);
        }

    }
}
