using System;
using Core.Enums.Internal;
using Core.Networking;
using Game.Networking;

namespace Game.Handlers.Internal {
    class PlayerAuthorization : PacketHandler<ServerClient> {
        public override void Handle(ServerClient sender, InPacket packetReader)
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
                            Entities.User u = Managers.UserManager.Instance.Get(targetId);
                            if (u != null)
                            {
                                uint userId = packetReader.ReadUint();
                                string username = packetReader.ReadString();
                                string displayname = packetReader.ReadString();
                                u.OnAuthorize(userId, username, displayname);
                            }
                            break;
                        }

                    // Update the information of a player.
                    case PlayerAuthorizationErrorCodes.Update:
                        {
                            break;
                        }

                    // A player logs out of the server.
                    case PlayerAuthorizationErrorCodes.Logout:
                        {
                            break;
                        }

                    case PlayerAuthorizationErrorCodes.InvalidSession:
                        {
                            Entities.User u = Managers.UserManager.Instance.Get(targetId);
                            if (u != null)
                            {
                                if (!u.Authenticated)
                                    u.Send(new Packets.Authorization(Packets.Authorization.ErrorCodes.NormalProcedure));
                                u.Disconnect();
                            }
                            break;
                        }

                    case PlayerAuthorizationErrorCodes.IvalidMatch:
                        {
                            Entities.User u = Managers.UserManager.Instance.Get(targetId);
                            if (u != null)
                            {
                                u.Send(new Packets.Authorization(Packets.Authorization.ErrorCodes.NormalProcedure));
                                u.Disconnect();
                            }
                            break;
                        }

                    case PlayerAuthorizationErrorCodes.SessionAlreadyActivated:
                        {
                            Entities.User u = Managers.UserManager.Instance.Get(targetId);
                            if (u != null)
                            {
                                u.Send(new Packets.Authorization(Packets.Authorization.ErrorCodes.NormalProcedure));
                                u.Disconnect();
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
                Log.Instance.WriteLine(string.Concat("Unknown PlayerAuthorization error: ", errorCode));
            }
        }
    }
}

