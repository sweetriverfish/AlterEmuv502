using Core.Networking;
using Game.Entities;

namespace Game.Handlers {
    public class Authorization : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            uint userId = packetReader.ReadUint();
            int nxCode = packetReader.ReadInt(); // Unused.

            string username = packetReader.ReadString();
            string displayname = packetReader.ReadString();

            uint sessionId = packetReader.ReadUint(); // Login session Id -> Doesn't seem to be correct? -> We'll use it like this for now.

            //ushort gender = GetUShort(4);
            //ushort age = GetUShort(5);

            //ushort wrPointScore = GetUShort(6); // WR Point System -> The score of your computer performance.
            //ushort accessCode = GetUShort(7); // Permissions.

            /*int clanId = GetInt(8);
            int clanUserLevel = GetInt(9);
            int clanIcon = GetInt(10);
            string clanName = GetString(11);*/

            if (userId > 0 && username.Length > 2 && displayname.Length > 2 && sessionId > 0)
            {
                if (Managers.UserManager.Instance.Add(sessionId, sender))
                {
                    Program.AuthServer.Send(new Packets.Internal.PlayerAuthorization(sessionId, userId, username));
                }
                else
                {
                    sender.Send(new Packets.Authorization(Packets.Authorization.ErrorCodes.NormalProcedure));
                    sender.Disconnect();
                }
            }
            else
            {
                sender.Send(new Packets.Authorization(Packets.Authorization.ErrorCodes.NormalProcedure));
                sender.Disconnect();
            }
        }
    }
}
