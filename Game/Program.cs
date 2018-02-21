/*
 * Alter EMU - Written By CodeDragon
 * 
 * Emulator Verion: 5.0.1
 * Credits:
 *  - CodeDragon
 *  - Basser
 *          
 * Special Thanks:
 *  - DarkRaptor
 *
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Enums;
using Game.Networking;

namespace Game
{
    class Program {
        private static bool isRunning = false;
        private static DateTime startTime;
        public static ServerClient AuthServer;
        private static uint serverLoops = 0;

        static void Main(string[] args) {
            startTime = DateTime.Now;

            Console.Title = "「Starting」AlterEmu Game server";
            Console.WindowWidth = Console.LargestWindowWidth - 25;
            Console.WindowHeight = Console.LargestWindowHeight - 25;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" _______        _______ _______  ______ _______ _______ _     _");
            Console.WriteLine(@" |_____| |         |    |______ |_____/ |______ |  |  | |     |");
            Console.WriteLine(@" |     | |_____    |    |______ |    \_ |______ |  |  | |_____|");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(new string('_', Console.WindowWidth));
            Console.WriteLine();

            if (!Config.Read()) {
                Log.Instance.WriteError("Failed to load the configuration file.");
                Console.ReadKey();
                return;
            }

            if (!Databases.Init()) {
                Log.Instance.WriteError("Failed to initilize all database connections.");
                Console.ReadKey();
                return;
            }

            if (!Managers.ItemManager.Instance.Load()) {
                Log.Instance.WriteError("Failed to initilize the item manager.");
                Console.ReadKey();
                return;
            }

            if (!Managers.MapManager.Instance.Load()) {
                Log.Instance.WriteError("Failed to initilize the map manager.");
                Console.ReadKey();
                return;
            }

            // CONNECT TO THE AUTHORIZATION SERVER //
            AuthServer = new ServerClient(Config.AUTH_SERVER_IP, (int)Ports.Internal);
            if (!AuthServer.Connect()) {
                return;
            }

            if (!new UDPListener((int)Ports.UDP1).Start()) {
                return;
            }

            if (!new UDPListener((int)Ports.UDP2).Start()) {
                return;
            }

            // Start up the listener :)
            isRunning = (new ServerListener((int)Ports.Game)).Start();

            if (isRunning)
            {
                TimeSpan loadTime = DateTime.Now - startTime;
                Log.Instance.WriteLine(string.Format("Emulator loaded in {0} milliseconds!", loadTime.TotalMilliseconds));
            }

            startTime = DateTime.Now;
            while (isRunning) {

                TimeSpan runTime = DateTime.Now - startTime;
                Console.Title = string.Format("「AlterEmu-Game」Uptime {0} | Players: {1} | Peak: {2} | Rooms: {3}", runTime.ToString(@"dd\:hh\:mm\:ss"), Managers.UserManager.Instance.Sessions.Values.Count, Managers.UserManager.Instance.Peak, Managers.ChannelManager.Instance.RoomCount);

                if(serverLoops % 5 == 0) {
                    Parallel.ForEach(Managers.UserManager.Instance.Sessions.Values, user => {
                        if (user.Authenticated)
                            user.SendPing();
                    });
                }
                
                serverLoops++;

                Thread.Sleep(1000);
            }
        }
    }
}
