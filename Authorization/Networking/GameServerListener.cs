using System;
using System.Net;
using System.Net.Sockets;

namespace Authorization.Networking
{
    class GameServerListener
    {
        private readonly int bindPort;
        private Socket socket;

        public GameServerListener(int port)
        {
            bindPort = port;
        }

        public bool Start()
        {
            try
            {
                Log.Instance.WriteLine($"Binding a socket listener to port: {bindPort}.");
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, bindPort));
                socket.Listen(1);
                socket.BeginAccept(OnAcceptConnection, null);
                Log.Instance.WriteLine("The socket is successfully binded to the port!");
                return true;
            }
            catch
            {
                Log.Instance.WriteError("Failed to bind a network socket to the port.");
                Log.Instance.WriteError("Is a server already running on this port?");
            }

            return false;
        }

        private void OnAcceptConnection(IAsyncResult iAr)
        {
            try
            {
                Socket s = socket.EndAccept(iAr);
                Entities.Server gs = new Entities.Server(s);
            }
            catch { }

            if (socket != null)
                socket.BeginAccept(OnAcceptConnection, null);
        }
    }
}
