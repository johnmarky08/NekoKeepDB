using MySql.Data.MySqlClient;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Databases
{
    // All Accounts Database Queries
    public class AccountsDB : MainDB
    {
        // Helper for creating filter tags for account
        private static void CreateFilterTags(int accountId, List<ITag> tags)
        {
            foreach (ITag tag in tags)
            {
                IFilter filter = new FilterDto()
                {
                    AccountId = accountId,
                    TagId = tag.Id
                };
                FiltersDB.CreateFilter(filter);
            }
        }

        // Account Creation - OAuth Account
        public static void CreateAccount(IOAuthAccount account)
        {
            if (!Utils.ValidateEmail(account.Email)) return;

            string sql = @"
                INSERT INTO Accounts (user_id, type, display_name, email, provider, encrypted_password, note)
                VALUES (@user_id, @type, @display_name, @email, @provider, @encrypted_password, @note);
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", account.UserId);
            cmd.Parameters.AddWithValue("@type", "OAuth");
            cmd.Parameters.AddWithValue("@display_name", account.DisplayName);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@provider", account.Provider);
            cmd.Parameters.AddWithValue("@encrypted_password", null);
            cmd.Parameters.AddWithValue("@note", account.Note);
            cmd.ExecuteNonQuery();

            CreateFilterTags((int)cmd.LastInsertedId, account.Tags);
        }

        // Account Creation - Custom Account
        public static void CreateAccount(ICustomAccount account)
        {
            byte[]? encryptedPassword;
            if (!Utils.ValidateEmail(account.Email) || (account.Password == null)) return;
            else encryptedPassword = Crypto.Encrypt(account.Password);

            string sql = @"
                INSERT INTO Accounts (user_id, type, display_name, email, provider, encrypted_password, note)
                VALUES (@user_id, @type, @display_name, @email, @provider, @encrypted_password, @note);
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", account.UserId);
            cmd.Parameters.AddWithValue("@type", "Custom");
            cmd.Parameters.AddWithValue("@display_name", account.DisplayName);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@provider", null);
            cmd.Parameters.AddWithValue("@encrypted_password", encryptedPassword);
            cmd.Parameters.AddWithValue("@note", account.Note);
            cmd.ExecuteNonQuery();

            CreateFilterTags((int)cmd.LastInsertedId, account.Tags);
        }

        // Retrieve an OAuth Account by account ID, returns null if not found
        public static IOAuthAccount? RetrieveOAuthAccount(int accountId)
        {
            string sql = "SELECT * FROM Accounts WHERE account_id = @account_id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@account_id", accountId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                IOAuthAccount account = new OAuthAccountDto()
                {
                    Id = reader.GetInt32("account_id"),
                    UserId = reader.GetInt32("user_id"),
                    DisplayName = reader.GetString("display_name"),
                    Email = reader.GetString("email"),
                    Provider = reader.GetString("provider"),
                    Tags = TagsDB.RetriveTags(accountId),
                    Note = reader.GetString("note"),
                };
                return account;
            }

            return null;
        }

        // Retrieve a Custom Account by account ID, returns null if not found
        public static ICustomAccount? RetrieveCustomAccount(int accountId)
        {
            string sql = "SELECT * FROM Accounts WHERE account_id = @account_id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@account_id", accountId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int ord = reader.GetOrdinal("encrypted_password");
                ICustomAccount account = new CustomAccountDto()
                {
                    Id = reader.GetInt32("account_id"),
                    UserId = reader.GetInt32("user_id"),
                    DisplayName = reader.GetString("display_name"),
                    Email = reader.GetString("email"),
                    Password = Crypto.Decrypt(reader.GetFieldValue<byte[]>(ord))!,
                    Tags = TagsDB.RetriveTags(accountId),
                    Note = reader.GetString("note"),
                };
                return account;
            }

            return null;
        }

        // Retrieve all Accounts
        public static (List<IOAuthAccount>, List<ICustomAccount>) RetrieveAccounts()
        {
            List<IOAuthAccount> oAuthAccounts = [];
            List<ICustomAccount> customAccounts = [];
            var rows = new List<(
                int AccountId,
                string Type,
                int UserId,
                string DisplayName,
                string Email,
                string? Provider,
                string? Note,
                byte[]? EncryptedPassword
            )>();
            string sql = "SELECT * FROM Accounts;";
            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string type = reader.GetString("type");
                int accountId = reader.GetInt32("account_id");
                int ordProvider = reader.GetOrdinal("provider");
                int ordNote = reader.GetOrdinal("note");
                int ordEncrypted = reader.GetOrdinal("encrypted_password");

                rows.Add((
                    AccountId: accountId,
                    Type: type,
                    UserId: reader.GetInt32("user_id"),
                    DisplayName: reader.GetString("display_name"),
                    Email: reader.GetString("email"),
                    Provider: reader.IsDBNull(ordProvider) ? null : reader.GetString(ordProvider),
                    Note: reader.IsDBNull(ordNote) ? null : reader.GetString(ordNote),
                    EncryptedPassword: reader.IsDBNull(ordEncrypted) ? null : reader.GetFieldValue<byte[]>(ordEncrypted)
                ));
            }
            reader.Close();

            foreach (var row in rows)
            {
                if (row.Type == "OAuth")
                {
                    IOAuthAccount oAuthAccount = new OAuthAccountDto()
                    {
                        Id = row.AccountId,
                        UserId = row.UserId,
                        DisplayName = row.DisplayName,
                        Email = row.Email,
                        Provider = row.Provider!,
                        Tags = TagsDB.RetriveTags(row.AccountId),
                        Note = row.Note,
                    };
                    oAuthAccounts.Add(oAuthAccount);
                }
                else if (row.Type == "Custom")
                {
                    ICustomAccount customAccount = new CustomAccountDto()
                    {
                        Id = row.AccountId,
                        UserId = row.UserId,
                        DisplayName = row.DisplayName,
                        Email = row.Email,
                        Password = Crypto.Decrypt(row.EncryptedPassword!)!,
                        Tags = TagsDB.RetriveTags(row.AccountId),
                        Note = row.Note,
                    };
                    customAccounts.Add(customAccount);
                }
            }

            return (oAuthAccounts, customAccounts);
        }
    }
}
