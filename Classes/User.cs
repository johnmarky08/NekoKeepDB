namespace NekoKeepDB.Classes
{
    #pragma warning disable CS8618
    public static class User
    {
        public static int Id { get; private set; }
        public static string DisplayName { get; private set; }
        public static string Email { get; private set; }
        public static string EncryptedMpin { get; private set; }
        public static int CatPresetId { get; private set; }

        private static bool _initialized;

        public static void Initialize(int id, string displayName, string email, string encryptedMpin, int catPresetId)
        {
            if (_initialized) return;
            Id = id;
            DisplayName = displayName;
            Email = email;
            CatPresetId = catPresetId;
            EncryptedMpin = encryptedMpin;
            _initialized = true;
        }
    }

}
