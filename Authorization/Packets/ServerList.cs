namespace Authorization.Packets
{
    class ServerList : Core.Networking.OutPacket
    {
        public enum ErrorCodes : uint
        {
            Exception = 70101,
            NewNickname = 72000,
            InvalidUser = 72010,
            InvalidPassword = 72020,
            AlreadyLoggedIn = 72030,
            ClientVerNotMatch = 70301,
            Banned = 73050,
            BannedTime = 73020,
            NotActive = 73040,
            EnterID = 74010,
            EnterPassword = 74020,
            ErrorNickname = 74030,
            NicknameTaken = 74070,
            NicknameToLong = 74100, // Longer then 10 characters.
            IlligalNickname = 74110
        }

        public ServerList(ErrorCodes errorCode)
            : base((ushort)Enums.Packets.ServerList)
        {
            Append((uint)errorCode);
        }

        public ServerList(Entities.User u)
            : base((ushort)Enums.Packets.ServerList)
        {
            Append(Core.Constants.Error_OK);
            Append(u.ID);               // UserId
            Append(0);                  // Unknown
            Append(u.Name);             // User login name
            Append("*");                // User password (NULL).
            Append(u.Displayname);      // Displayname or nickname.
            Append(u.SessionID);        // Gender
            Append(21);                 // Age
            Append(0);                  // ?
            Append(0);                  // Permission Level
            // Append("TOKEN");         // Token - Not used in 04082008

            var serverList = Managers.ServerManager.Instance.GetAllAuthorized();
            // Servers list
            Append(serverList.Count);               // Server count
            foreach (Entities.Server server in serverList)
            {
                Append(server.ID);                  // Server ID
                Append(server.Displayname);         // Server Name
                Append(server.IP);                  // Server IP
                Append((int)Core.Enums.Ports.Game); // Server port - Unused
                Append(server.TotalPlayers);        // Server player count. --> 3200 = LIMIT
                Append((byte)server.Type);          // Server type
            }

            // Clan information
            Append(-1); // Clan Id
            Append("NULL"); // Clan Name
            Append(-1); // Clan Level
            Append(-1); // Clan User Level

            // Clan Battle
            Append(0); // Clan Battle Key -> Next block is ignored (but needs to be send) if this is 0.
            Append(0); // Target server Id for clan battle - Note: Server Type needs to be 2!
        }
    }
}
