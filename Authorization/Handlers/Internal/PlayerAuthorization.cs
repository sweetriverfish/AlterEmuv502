using System;
using Authorization.Entities;
using Core.Enums.Internal;
using Core.Networking;

namespace Authorization.Handlers.Internal {
    class PlayerAuthorization : PacketHandler<Server> {
        public override void Handle(Server sender, InPacket packetReader)
        {
            ushort errorCode = packetReader.ReadUshort();

            if (Enum.IsDefined(typeof(PlayerAuthorizationErrorCodes), errorCode))
            {
                PlayerAuthorizationErrorCodes enumErrorCode = (PlayerAuthorizationErrorCodes)errorCode;
                uint targetId = packetReader.ReadUint();

                switch (enumErrorCode)
                {

                    // A new player logs in.
                    case PlayerAuthorizationErrorCodes.Login:
                        {

                            Session session = Managers.SessionManager.Instance.Get(targetId);
                            if (session != null)
                            {
                                if (!session.IsActivated)
                                {
                                    session.Activate((byte)sender.ID);

                                    sender.Send(new Packets.Internal.PlayerAuthorization(session));
                                }
                                else
                                {
                                    sender.Send(new Packets.Internal.PlayerAuthorization(PlayerAuthorizationErrorCodes.SessionAlreadyActivated, targetId));
                                }
                            }
                            else
                            {
                                sender.Send(new Packets.Internal.PlayerAuthorization(PlayerAuthorizationErrorCodes.InvalidSession, targetId));
                            }

                            break;
                        }

                    // Update the information of a player.
                    case PlayerAuthorizationErrorCodes.Update:
                        {
                            Session session = Managers.SessionManager.Instance.Get(targetId);
                            if (session != null)
                            {
                                /* Packet Structure v2 
                                 * 
                                 * Connection Time
                                 * Ping
                                 * 
                                 * [Sub Type]
                                 *  - 1 - Update Channel
                                 *  - 2 - Update Session Information
                                 *  - 3 - Update Action
                                 *  - 4 - Update Lobby Room information
                                 *  - 5 - Update Ingame Room information
                                 *  
                                 * [Data Blocks]
                                 *  - [1] - Update Channel
                                 *      - Current channel Id
                                 *      - Channel Slot
                                 *      
                                 *  - [2] - Update Session Information
                                 *      - Session - Kills
                                 *      - Session - Deaths
                                 *      - Session - Xp Earned
                                 *      - Session - Dinar Earned
                                 *      - Session - Dinar Spend
                                 *      
                                 *  - [3] - Update Action
                                 *      - Update Type:
                                 *      
                                 *          [1]: Join Room
                                 *               - Room ID
                                 *               - Room Slot
                                 *               - Room Is Master
                                 *               
                                 *          [2]: Leave Room
                                 *              - Room ID
                                 *              - Room Old Slot
                                 *              - Room Was Master?
                                 *                  - New master slot
                                 *                  
                                 *          [3]: Room Start
                                 *              - Team
                                 *              
                                 *          [4]: Room Stop
                                 *              - Kills
                                 *              - Deaths
                                 *              - Flags
                                 *              - Points
                                 *              - Xp Earned
                                 *              - Dinar Earned
                                 *              - xp Bonusses (%-Name:%-Name)
                                 *              - dinar bonusses (%-Name:%-Name)
                                 *      
                                 *  - [4] - Update Lobby Room information
                                 *      - Update Type:
                                 *      
                                 *          [1]: Switch Side
                                 *               - Room ID
                                 *               - Room Slot
                                 *               - Room Is Master
                                 *               
                                 *          [2]:                
                                 *
                                 *  - [5] - Update Ingame Room information
                                 *      - Update Type:
                                 *      
                                 *          [1]: Score Update (Player Kill/Player Death)
                                 *               - Room ID
                                 *               - Room Kills
                                 *               - Room Deaths
                                 *               - Room Flags
                                 *               - Room Points 
                                 *               
                                 *          [2]: 
                                 */


                            }
                            else
                            {
                                // Force a closure of the connection.
                                sender.Send(new Packets.Internal.PlayerAuthorization(PlayerAuthorizationErrorCodes.InvalidSession, targetId));
                            }

                            break;
                        }

                    // A player logs out of the server.
                    case PlayerAuthorizationErrorCodes.Logout:
                        {
                            Session session = Managers.SessionManager.Instance.Get(targetId);
                            if (session != null)
                            {
                                if (session.IsActivated)
                                {
                                    session.End();
                                }
                            }
                            break;
                        }

                    default:
                        {
                            // Unused.
                            break;
                        }
                }
            }
            else
            {
                Console.WriteLine(string.Concat("Unknown PlayerAuthorization error: ", errorCode));
            }
        }
    }
}
