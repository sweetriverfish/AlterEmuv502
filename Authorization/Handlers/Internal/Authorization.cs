using System;
using Authorization.Entities;
using Core.Enums;
using Core.Networking;

namespace Authorization.Handlers.Internal
{
    class Authorization : PacketHandler<Server>
    {
        public override void Handle(Server sender, InPacket packetReader)
        {
            uint ErrorCode = packetReader.ReadUint();

            if (ErrorCode != 1)
            {
                sender.Disconnect();
                return;
            }

            string globalKey = packetReader.ReadString();
            string serverName = packetReader.ReadString();
            string ipAddress = packetReader.ReadString();
            int port = packetReader.ReadInt();
            byte type = packetReader.ReadByte();

            if (!Enum.IsDefined(typeof(ServerTypes), type))
            {
                sender.Disconnect();
                return;
            }

            ServerTypes enumType = (ServerTypes)type;

            byte serverId = Managers.ServerManager.Instance.Add(sender, serverName, ipAddress, port, enumType);
            if (serverId > 0)
            {
                Log.Instance.WriteLine($"Added Server: {serverName} @ {ipAddress}:{port} with Id: {serverId}");
                sender.Send(new Packets.Internal.Authorize(serverId));
            }
            else
            {
                Log.Instance.WriteError($"Failed to add Server: {serverName} @ {ipAddress}:{port}. Reason: Server Limit reached.");
                sender.Send(new Packets.Internal.Authorize(Core.Enums.Internal.AuthorizationErrorCodes.MaxServersReached));
                sender.Disconnect();
            }
        }
    }
}
