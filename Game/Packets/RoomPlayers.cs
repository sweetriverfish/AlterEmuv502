using System;
using System.Collections;

namespace Game.Packets {
    class RoomPlayers : Core.Networking.OutPacket {
        public RoomPlayers(ArrayList arrPlayers)
            : base((ushort)Enums.Packets.PlayerInfo) {
                Append(arrPlayers.Count);
                foreach (Entities.Player p in arrPlayers) {
                    Append(p.User.ID);              // Player account id.
                    Append(p.User.SessionID);       // Player session id.
                    Append(p.Id);                   // The id of the room slot.
                    Append(p.Ready);                // Indicates if the player is ready.
                    Append((byte)p.Team);           // The team side of the player.
                    Append(p.Weapon);               // The weapon that the player is currently wearing.
                    Append(0);                      // Unknown?
                    Append((byte)p.Class);          // The current class of the player.
                    Append(p.Health);               // The current health of the player.
                    Append(p.User.Displayname);     // The nickname of the player.
                    // CLAN INFORMATION
                    Fill(3, -1); // 3 BLOCKS
                    // END CLAN
                    Append(1);                      // Unknown?
                    Append(0);                      // Unknown?
                    Append(910);                    // Unknown?
                    Append((byte)p.User.Premium);   // The premium of the current player.
                    Append(-1);                     // Unknown?
                    Append(p.User.Kills);           // The total kills of the player.
                    Append(p.User.Deaths);          // The total deaths of the player.
                    Append(new Random().Next(150)); // Unknown?
                    Append(p.User.XP);              // The current XP of the player | -1 = Special Badge
                    Append(-1);                     // The id of the vehicle that the player is in.
                    Append(-1);                     // The current slot of the vehicle that the player is in.
                    // CONNECTION INFORMATION //
                    Append(p.User.RemoteEndPoint.Address.Address);  // The Remote endpoint ip as a long.
                    Append(p.User.RemotePort);                      // The remote port.
                    Append(p.User.LocalEndPoint.Address.Address);   // The Remote endpoint ip as a long.
                    Append(p.User.LocalPort);                       // The remote port.
                    // END CONNECTION INFO //
                    Append(0);                      // Unknown?
                }
        }
    }
}
