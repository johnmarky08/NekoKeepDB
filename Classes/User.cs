using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Classes
{
    public class User
    {
        public static IUser? Session { get; private set; } = null;
        private static string EncryptedPassword { get; set; }
        private static string EncryptedMpin { get; set; }

        public static void Login(IUser user, string encryptedPassword, string encryptedMpin)
        {
            if (Session != null) return;

            Session = user;
            EncryptedPassword = encryptedPassword;
            EncryptedMpin = encryptedMpin;
        }

        public static void Logout()
        {
            if (Session == null) return;

            Session = null;
            EncryptedPassword = string.Empty;
            EncryptedMpin = string.Empty;
        }

        public static bool VerifyPassword(string password)
        {
            return Crypto.Verify(password, EncryptedPassword);
        }

        public static bool VerifyMpin(string mpin)
        {
            return Crypto.Verify(mpin, EncryptedMpin);
        }

        public static void UpdateLocalDisplayName(string newDisplayName)
        {
            if (Session == null) return;
            Session.DisplayName = newDisplayName;
        }

        public static void UpdateLocalEmail(string newEmail)
        {
            if (Session == null) return;
            Session.Email = newEmail;
        }

        public static void UpdateLocalCatPresetId(int newCatPresetId)
        {
            if (Session == null) return;
            Session.CatPresetId = newCatPresetId;
        }
        public static void UpdateLocalPassword(string newEncryptedPassword)
        {
            EncryptedPassword = newEncryptedPassword;
        }

        public static void UpdateLocalMpin(string newEncryptedMpin)
        {
            EncryptedMpin = newEncryptedMpin;
        }
    }
}
