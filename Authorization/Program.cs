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

namespace Authorization {
    class Program {
        private static bool isRunning = false;
        public static object sessionLock = new Object();
        public static int totalPlayers = 0;
        public static int onlinePlayers = 0;
        public static int playerPeak = 0;

        static void Main(string[] args) {

            Console.Title = "「Starting」AlterEmu authentication server";
            Console.WindowWidth = Console.LargestWindowWidth - 25;
            Console.WindowHeight = Console.LargestWindowHeight - 25;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" _______        _______ _______  ______ _______ _______ _     _");
            Console.WriteLine(@" |_____| |         |    |______ |_____/ |______ |  |  | |     |");
            Console.WriteLine(@" |     | |_____    |    |______ |    \_ |______ |  |  | |_____|");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(new string('_', Console.WindowWidth));
            Console.WriteLine();

            if (!Config.Read())
            {
                Log.Instance.WriteError("Failed to load the configuration file.");
                Console.ReadKey();
                return;
            }

            if (!Databases.Init()) {
                Log.Instance.WriteError("Failed to initilize all database connections.");
                Console.ReadKey();
                return;
            }

            if (!new Networking.GameServerListener((int)Core.Enums.Ports.Internal).Start()) {
                return;
            }

            isRunning = (new Networking.ServerListener((int)Core.Enums.Ports.Login)).Start();

            while (isRunning) {
                Console.Title = string.Format("「AlterEmu-Authentication」Players: {0} | Peak: {1} | Total: {2}", onlinePlayers, playerPeak, totalPlayers);
                // TODO: Update the console title + basic queries.
                Thread.Sleep(1000);
            }
        }
    }
}
