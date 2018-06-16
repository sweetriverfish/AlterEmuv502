namespace Authorization.Packets.Internal {
    class Authorize : Core.Networking.OutPacket {
        public Authorize(Core.Enums.Internal.AuthorizationErrorCodes ErrorCode)
            : base((ushort)Core.Enums.InternalPackets.Authorization) {
            Append((ushort)ErrorCode);
        }

        public Authorize(byte serverId)
            : base((ushort)Core.Enums.InternalPackets.Authorization) {
                Append(Core.Constants.Error_OK);
                Append(serverId);
        }
    }
}
