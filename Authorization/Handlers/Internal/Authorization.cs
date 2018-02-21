using System;
using Authorization.Entities;
using Core.Networking;

namespace Authorization.Handlers.Internal {
    class Authorization : PacketHandler<Server> {
        public override void Handle(Server sender, InPacket packetReader)
        {
            uint ErrorCode = packetReader.ReadUint();

            if (ErrorCode == 1)
            {
                string globalKey = packetReader.ReadString();
                string serverName = packetReader.ReadString();
                string ipAddress = packetReader.ReadString();
                int port = packetReader.ReadInt();
                byte type = packetReader.ReadByte();

                Core.Enums.ServerTypes enumType = Core.Enums.ServerTypes.Normal;
                if (Enum.IsDefined(typeof(Core.Enums.ServerTypes), type))
                {
                    enumType = (Core.Enums.ServerTypes)type;
                }
                else
                {
                    sender.Disconnect(); return;
                }

                byte serverId = Managers.ServerManager.Instance.Add(sender, serverName, ipAddress, port, enumType);
                if (serverId > 0)
                {
                    sender.Send(new Packets.Internal.Authorize(serverId));
                }
                else
                {
                    sender.Send(new Packets.Internal.Authorize(Core.Enums.Internal.AuthorizationErrorCodes.MaxServersReached));
                    sender.Disconnect();
                }

            }
            else
            {
                sender.Disconnect();
            }
        }
    }
}
