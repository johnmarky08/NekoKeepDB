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

        public static bool VerifyPassword(string password) => Utils.BCryptVerify(password, EncryptedPassword!);

        public static bool VerifyMpin(string mpin) => Utils.BCryptVerify(mpin, EncryptedMpin!);

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
        public static void UpdateLocalPassword(string newEncryptedPassword) => EncryptedPassword = newEncryptedPassword;

        public static void UpdateLocalMpin(string newEncryptedMpin) => EncryptedMpin = newEncryptedMpin;

        public static List<Account> ViewAccounts(bool sortByDate, bool descending)
        {
            List<Account> accounts = Session!.Accounts!;

            // Build the base comparison function
            static int comparisonByDate(Account leftAccount, Account rightAccount) =>
                DateTime.Compare(leftAccount.Data.UpdatedAt!.Value, rightAccount.Data.UpdatedAt!.Value);

            static int comparisonByDisplayName(Account leftAccount, Account rightAccount) =>
                string.Compare(leftAccount.Data.DisplayName, rightAccount.Data.DisplayName, StringComparison.OrdinalIgnoreCase);

            Comparison<Account> chosenComparison = sortByDate ? comparisonByDate : comparisonByDisplayName;

            // If descending is requested, invert the comparison
            Comparison<Account> finalComparison = descending
                ? (leftAccount, rightAccount) => -chosenComparison(leftAccount, rightAccount)
                : chosenComparison;

            // Use the merge sort to return a new sorted list
            return Sort.MergeSort(accounts, finalComparison);
        }

        public static void AddSessionAccount(Account newAccount)
        {
            if (!Utils.IsAuthenticated()) return;
            Session!.Accounts!.Add(newAccount);
        }

        public static void RemoveSessionAccount(int accountId)
        {
            if (!Utils.IsAuthenticated()) return;
            Session!.Accounts = [.. Session!.Accounts!.Where(a => a.Data.Id != accountId)];
        }
    }
}
