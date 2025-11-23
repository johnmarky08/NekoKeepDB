namespace NekoKeepDB
{
    internal class Crypto
    {
        public static string Encrypt(string text)
        { 
            return BCrypt.Net.BCrypt.HashPassword(text);
        }

        public static bool Verify(string text, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(text, hash);
        }
    }
}
