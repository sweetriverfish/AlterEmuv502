using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Core {
    public class Constants {
        public const byte Error_OK = 1;
        public const byte xOrKeySend = 0x96;
        public const byte xOrKeyReceive = 0xC3;

        public const sbyte maxChannelsCount = 3;

        private readonly static Regex objAlphaNumericPattern = new Regex("[^a-zA-Z0-9]");

        public static bool IsAlphaNumeric(string input) {
            return !objAlphaNumericPattern.IsMatch(input);
        }

        public static string MD5(string Input) {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(Input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
