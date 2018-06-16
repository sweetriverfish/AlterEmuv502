using System;
using System.Net.Sockets;
using Core.Networking;
using Authorization.Networking;
using Core.Entities;

namespace Authorization.Entities
{
    public class User : Entity, IConnection
    {
        public bool Authorized { get; private set; }
        public uint SessionID { get; private set; }

        private Socket socket;
        private byte[] buffer = new byte[1024];
        private byte[] cacheBuffer = new byte[0];
        private bool disconnected;

        public User(Socket socket)
            : base(0, "Unknown", "Unknown")
        {
            this.socket = socket;
            this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
            Send(new Core.Packets.Connection());
        }

        public void OnAuthorize(uint id, string name, string displayname)
        {
            this.ID = id;
            this.Name = name;
            this.Displayname = displayname;
            Managers.SessionManager.Instance.Add(this);
            Authorized = true;
        }

        public void UpdateDisplayname(string displayname)
        {
            this.Displayname = displayname;
            Session s = Managers.SessionManager.Instance.Get(SessionID);
            if (s != null)
            {
                s.UpdateDisplayname(displayname);
            }
        }

        public void SetSession(uint sessionId)
        {
            SessionID = sessionId;
        }

        private void OnDataReceived(IAsyncResult iAr)
        {
            try
            {
                int bytesReceived = socket.EndReceive(iAr);

                if (bytesReceived > 0)
                {
                    byte[] packetBuffer = new byte[bytesReceived];

                    Buffer.BlockCopy(buffer, 0, packetBuffer, 0, packetBuffer.Length);
                    // Decrypt the bytes with the xOrKey.
                    for (int i = 0; i < bytesReceived; i++)
                        packetBuffer[i] ^= Core.Constants.xOrKeyReceive;

                    int oldLength = cacheBuffer.Length;
                    Array.Resize(ref cacheBuffer, oldLength + bytesReceived);
                    Array.Copy(packetBuffer, 0, cacheBuffer, oldLength, packetBuffer.Length);

                    int startIndex = 0; // Determs whre the bytes should split
                    for (int i = 0; i < cacheBuffer.Length; i++)
                    {
                        // loop trough our cached buffer.
                        if (cacheBuffer[i] == 0x0A)
                        {
                            // Found a complete packet
                            byte[] newPacket = new byte[i - startIndex]; // determ the new packet size.
                            for (int j = 0; j < (i - startIndex); j++)
                            {
                                newPacket[j] = cacheBuffer[startIndex + j]; // copy the buffer to the buffer of the new packet.
                            }

                            // Handle the packet instantly.
                            InPacket inPacket = new InPacket(newPacket);
                            if (inPacket.Id > 0)
                            {
                                PacketHandler<User> pHandler = NetworkTable.Instance.FindExternal(inPacket);
                                if (pHandler != null)
                                {
                                    try
                                    {
                                        pHandler.Handle(this, inPacket);
                                    }
                                    catch { /*Disconnect();*/ }
                                }
                                else
                                {
                                    Console.WriteLine("UNKNOWN PACKET :: " + newPacket);
                                }
                            }

                            startIndex = i + 1;
                        }
                    }

                    if (startIndex > 0)
                    {
                        byte[] fullCopy = cacheBuffer;
                        Array.Resize(ref cacheBuffer, (cacheBuffer.Length - startIndex));
                        for (int i = 0; i < (cacheBuffer.Length - startIndex); i++)
                        {
                            cacheBuffer[i] = fullCopy[startIndex + i];
                        }
                        fullCopy = null;
                    }

                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
                }
                else
                {
                    Disconnect();
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public void Send(OutPacket outPacket)
        {
            try
            {
                byte[] sendBuffer = outPacket.BuildEncrypted();
                socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch
            {
                Disconnect();
            }
        }

        private void SendCallback(IAsyncResult iAr)
        {
            try { socket.EndSend(iAr); }
            catch { Disconnect(); }
        }

        public void Disconnect()
        {
            if (disconnected) return;
            disconnected = true;

            try { socket.Close(); } catch { }
        }

        public string RemoteEndIP => socket.RemoteEndPoint.ToString().Split(':')[0];
    }
}
