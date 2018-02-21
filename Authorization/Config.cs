using Core.IO;

namespace Authorization
{
    public class Config
    {
        public static byte MAXIMUM_SERVER_COUNT = 4;
        public static string[] GAME_DATABASE;
        public static string[] AUTH_DATABASE;

        public static bool Read()
        {
            INIFile configFile = new INIFile("config");
            bool result = false;
            if (configFile.Exists())
            {
                AUTH_DATABASE = new string[]
		        {
			        configFile.Read("auth-database", "host"),
			        configFile.Read("auth-database", "port"),
			        configFile.Read("auth-database", "username"),
			        configFile.Read("auth-database", "password"),
			        configFile.Read("auth-database", "database")
		        };

                result = true;
            }
            return result;
        }
    }
}
