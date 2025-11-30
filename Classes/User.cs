using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Classes
{
    public class User
    {
        public static IUser? Session { get; private set; } = null;
        private static string? EncryptedPassword { get; set; }
        private static string? EncryptedMpin { get; set; }

        public static void Login(IUser user, string encryptedPassword, string encryptedMpin)
        {
            if (Session != null)
            {
                Utils.ThrowError("User is already authenticated!");
                return;
            }

            Session = user;
            EncryptedPassword = encryptedPassword;
            EncryptedMpin = encryptedMpin;
        }

        public static void Logout()
        {
            if (!Utils.IsAuthenticated()) return;

            Session = null;
            EncryptedPassword = string.Empty;
            EncryptedMpin = string.Empty;
        }

        public static bool VerifyPassword(string password)
        {
            return Utils.BCryptVerify(password, EncryptedPassword!);
        }

        public static bool VerifyMpin(string mpin)
        {
            return Utils.BCryptVerify(mpin, EncryptedMpin!);
        }

        public static void UpdateLocalDisplayName(string newDisplayName)
        {
            if (!Utils.IsAuthenticated()) return;
            Session!.DisplayName = newDisplayName;
        }

        public static void UpdateLocalEmail(string newEmail)
        {
            if (!Utils.IsAuthenticated()) return;
            Session!.Email = newEmail;
        }

        public static void UpdateLocalCatPresetId(int newCatPresetId)
        {
            if (!Utils.IsAuthenticated()) return;
            Session!.CatPresetId = newCatPresetId;
        }
        public static void UpdateLocalPassword(string newEncryptedPassword)
        {
            EncryptedPassword = newEncryptedPassword;
        }

        public static void UpdateLocalMpin(string newEncryptedMpin)
        {
            EncryptedMpin = newEncryptedMpin;
        }

        public static List<Account> ViewAccounts(bool sortByDate, bool descending)
        {
            List<Account> accounts = Session!.Accounts!;

            if (sortByDate)
            {
                accounts = descending
                    ? [.. accounts.OrderByDescending(a => a.ViewAccount().UpdatedAt)]
                    : [.. accounts.OrderBy(a => a.ViewAccount().UpdatedAt)];
            }
            else
            {
                accounts = descending
                    ? [.. accounts.OrderByDescending(a => a.ViewAccount().DisplayName, StringComparer.OrdinalIgnoreCase)]
                    : [.. accounts.OrderBy(a => a.ViewAccount().DisplayName, StringComparer.OrdinalIgnoreCase)];
            }

            return accounts;
        }
    }
}
