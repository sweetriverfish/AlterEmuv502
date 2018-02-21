using System;
using Core.Enums.Internal;
using Core.Networking;
using Game.Networking;

namespace Game.Handlers.Internal {
    class Authorization : PacketHandler<ServerClient> {
        public override void Handle(ServerClient sender, InPacket packetReader)
        {
            ushort errorCode = packetReader.ReadUshort();
            if (sender.Authorized) return; // Ignore other packets.
            if (Enum.IsDefined(typeof(AuthorizationErrorCodes), errorCode))
            {
                AuthorizationErrorCodes enumErrorCode = (AuthorizationErrorCodes)errorCode;
                switch (enumErrorCode)
                {

                    case AuthorizationErrorCodes.OK:
                        {
                            sender.OnAuthorize(packetReader.ReadByte());
                            Console.Title = "AlterEmu - Game Server: " + Config.SERVER_NAME;
                            break;
                        }

                    case AuthorizationErrorCodes.InvalidKey:
                        {
                            DisconnectErrorLog(sender, "Error while authorizing: the authorization key didn't match.");
                            break;
                        }

                    case AuthorizationErrorCodes.Duplicate:
                        {
                            DisconnectErrorLog(sender, "Error while authorizing: a server with the same ip address is already online.");
                            break;
                        }

                    case AuthorizationErrorCodes.MaxServersReached:
                        {
                            DisconnectErrorLog(sender, "Error while authorizing: maximum amount of servers reached.");
                            break;
                        }

                    case AuthorizationErrorCodes.NameAlreadyUsed:
                        {
                            DisconnectErrorLog(sender, "Error while authorizing: the server name is already in use.");
                            break;
                        }

                    default:
                        {
                            DisconnectErrorLog(sender, string.Concat("An unknown(", errorCode.ToString("x2"), ") error occured while authorizing the server."));
                            break;
                        }

                }
            }
            else
            {
                // Unknown error
                Log.Instance.WriteLine(string.Concat("An unknown(", errorCode.ToString("x2"), ") error occured while authorizing the server."));
                sender.Disconnect(true);
            }
        }

        private void DisconnectErrorLog(ServerClient sender, string message)
        {
            Log.Instance.WriteLine(message);
            sender.Disconnect(true);
        }

    }
}
