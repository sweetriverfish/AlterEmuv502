using System;
using Core.Networking;
using Game.Entities;
using System.Threading;

namespace Game.Handlers {
    class RequestServerTime : PacketHandler<User> {

        private static int CLIENT_VERSION = 3;
        private static int MAC_ADDRESS_LENGTH = 12;

        public override void Handle(User sender, InPacket packetReader)
        {
            int versionId = packetReader.ReadInt(1);
            string MACAdress = packetReader.ReadString(2);

            if (versionId != CLIENT_VERSION)
            {
                sender.Send(new Packets.ServerTime(Packets.ServerTime.ErrorCodes.DiffrentClientVersion));
                Thread.Sleep(10000); // Freeze network thread and disconnect.
                sender.Disconnect();
                return;
            }

            if (MACAdress.Length != MAC_ADDRESS_LENGTH)
            {
                sender.Send(new Packets.Authorization(Packets.Authorization.ErrorCodes.NormalProcedure));
                Thread.Sleep(10000); // Freeze network thread and disconnect.
                sender.Disconnect();
                return;
            }

            sender.Send(new Packets.ServerTime());
        }
    }
}
