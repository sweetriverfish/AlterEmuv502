using Core.Networking;
using Game.Entities;
using Game.Networking;

namespace Game.Handlers
{
    class RoomData : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated && sender.Room != null)
            {
                // [0] = ROOM SLOT
                // [1] = ROOM ID
                byte roomSlot = packetReader.ReadByte(0);
                if (roomSlot < sender.Room.MaximumPlayers)
                {
                    ushort roomId = packetReader.ReadUshort(1);
                    if (roomId == sender.Room.ID)
                    {
                        byte unknown = packetReader.ReadByte(2); // Seems to be 2 or 0?
                        ushort subType = packetReader.ReadUshort(3);
                        // HANDLE PACKET IN A SEPERATED CLASS //
                        GameDataHandler handler = NetworkTable.Instance.GetHandler(subType);
                        if (handler != null)
                        {
                            try
                            {
                                handler.Process(sender, packetReader);
                            }
                            catch { /* error? */ }
                        }
                        else
                        {
                            Log.Instance.WriteLine("UNKNOWN SUBPACKET :: " + packetReader);
                        }

                    }
                    else
                    {
                        sender.Disconnect(); // Wrong room targeted - Cheating?
                    }
                }
                else
                {
                    sender.Disconnect(); // Room slot over maximum players - Cheating?
                }
            }
        }
    }
}
