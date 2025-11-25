namespace NekoKeepDB.Classes
{
    internal class User
    {
        public static bool IsAuthenticated { get; private set; }
        public static int Id { get; private set; }
        public static string DisplayName { get; private set; }
        public static string Email { get; private set; }
        public static int CatPresetId { get; private set; }
        private static string EncryptedPassword { get; set; }
        private static string EncryptedMpin { get; set; }

        public static void Login(int id, string displayName, string email, string encryptedPassword, string encryptedMpin, int catPresetId)
        {
            if (IsAuthenticated) return;

            Id = id;
            DisplayName = displayName;
            Email = email;
            CatPresetId = catPresetId;
            EncryptedPassword = encryptedPassword;
            EncryptedMpin = encryptedMpin;
            IsAuthenticated = true;
        }

        public static void Logout()
        {
            if (!IsAuthenticated) return;

            Id = 0;
            DisplayName = string.Empty;
            Email = string.Empty;
            CatPresetId = 0;
            EncryptedPassword = string.Empty;
            EncryptedMpin = string.Empty;
            IsAuthenticated = false;
        }

        public static void UpdateLocalPassword(string newEncryptedPassword)
        {
            EncryptedPassword = newEncryptedPassword;
        }

        public static void UpdateLocalMpin(string newEncryptedMpin)
        {
            EncryptedMpin = newEncryptedMpin;
        }

        public static bool VerifyPassword(string password)
        {
            return Crypto.Verify(password, EncryptedPassword);
        }

        public static bool VerifyMpin(string mpin)
        {
            return Crypto.Verify(mpin, EncryptedMpin);
        }
    }

}
