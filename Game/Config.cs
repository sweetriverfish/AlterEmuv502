using Core.IO;
using System;

namespace Game {
    class Config {
        public static string AUTH_SERVER_IP = "127.0.0.1";
        public static byte SERVER_ID = 0;
        public static string SERVER_KEY = "SERVER-KEY";
        public static string SERVER_NAME = "AlterEmu";
        public static string SERVER_IP = "";

        public static double EXP_RATE = 1.0;
        public static double DINAR_RATE = 1.0;

        public static int MAXIMUM_ROOM_COUNT = 5;
        public static int MAXIMUM_TEAM_DIFFRENCE = 1;
        public static long LEVEL_UP_MONEY_REWARD = 25000;

        public static string[] GAME_DATABASE;

        public static bool Read()
        {
            INIFile configFile = new INIFile("config");
            bool result = false;
            if (configFile.Exists())
            {
                // Default server configuration
                AUTH_SERVER_IP = configFile.Read("auth-server", "ip");
                SERVER_KEY = configFile.Read("auth-server", "key");
                SERVER_NAME = configFile.Read("server", "name");
                SERVER_IP = configFile.Read("server", "bindip");

                // Experience & Dinar rates
                string _expRate = configFile.Read("game-settings", "exp_rate");
                string _dinarRate = configFile.Read("game-settings", "dinar_rate");

                try { MAXIMUM_TEAM_DIFFRENCE = int.Parse(configFile.Read("game-settings", "max_team_difference")); }
                catch { MAXIMUM_TEAM_DIFFRENCE = 1; }

                try { EXP_RATE = double.Parse(_expRate); }
                catch { EXP_RATE = 1.0; }

                try { DINAR_RATE = double.Parse(_dinarRate); }
                catch { DINAR_RATE = 1.0; }

                if (EXP_RATE <= 0 || DINAR_RATE <= 0)
                {
                    Log.Instance.WriteError("One of the experience or dinar rates has been configured in a wrong way " + _expRate + "/" + _dinarRate);
                    Log.Instance.WriteError("Setting the wrong configured rate(s) to the default state.");

                    if (EXP_RATE <= 0)
                        EXP_RATE = 1.0;

                    if (DINAR_RATE <= 0)
                        DINAR_RATE = 1.0;
                }

                // Database
                GAME_DATABASE = new string[]
				{
					configFile.Read("game-database", "host"),
					configFile.Read("game-database", "port"),
					configFile.Read("game-database", "username"),
					configFile.Read("game-database", "password"),
					configFile.Read("game-database", "database")
				};
                result = true;
            }
            return result;
        }
    }
}
