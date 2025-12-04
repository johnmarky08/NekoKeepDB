using MySql.Data.MySqlClient;
using NekoKeepDB.Classes;
using NekoKeepDB.Interfaces;
using Org.BouncyCastle.Crypto;
using System.Data;

namespace NekoKeepDB.Databases
{
    // All Accounts Database Queries
    public class AccountsDB : MainDB
    {
        // Helper for creating filter tags for account
        private static void CreateFilterTags(int accountId, List<ITag> tags)
        {
            if (tags.Count == 0) return;

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

            account.Id = (int)cmd.LastInsertedId;
            CreateFilterTags(account.Id, account.Tags);

            account.Tags = TagsDB.RetrieveAccountTags(account.Id);
            account.UpdatedAt = DateTime.Now;
            OAuthAccount oAuthAccount = new(account);
            User.AddSessionAccount(oAuthAccount);
        }

        // Account Creation - Custom Account
        public static void CreateAccount(ICustomAccount account)
        {
            byte[] encryptedPassword;
            if (!Utils.ValidateEmail(account.Email) || (account.Password == null)) return;
            else encryptedPassword = Crypto.Encrypt(account.Password)!;

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

            account.Id = (int)cmd.LastInsertedId;
            CreateFilterTags(account.Id, account.Tags);

            account.Tags = TagsDB.RetrieveAccountTags(account.Id);
            account.UpdatedAt = DateTime.Now;
            CustomAccount customAccount = new(account);
            User.AddSessionAccount(customAccount);
        }

        // Retrieve all Accounts
        public static List<Account> RetrieveAccounts(int userId)
        {
            List<Account> accounts = [];
            var rows = new List<(
                int AccountId,
                string Type,
                int UserId,
                string DisplayName,
                string Email,
                string? Provider,
                string? Note,
                byte[]? EncryptedPassword,
                DateTime UpdatedAt
            )>();

            string sql = @"SELECT * FROM Accounts WHERE user_id = @user_id;";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);

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
                    EncryptedPassword: reader.IsDBNull(ordEncrypted) ? null : reader.GetFieldValue<byte[]>(ordEncrypted),
                    UpdatedAt: reader.GetDateTime("updated_at")
                ));
            }
            reader.Close();

            foreach (var row in rows)
            {
                if (row.Type == "OAuth")
                {
                    IOAuthAccount oAuthAccountData = new OAuthAccountDto()
                    {
                        Id = row.AccountId,
                        UserId = row.UserId,
                        DisplayName = row.DisplayName,
                        Email = row.Email,
                        Provider = row.Provider!,
                        Tags = TagsDB.RetrieveAccountTags(row.AccountId),
                        Note = row.Note,
                        UpdatedAt = row.UpdatedAt
                    };
                    OAuthAccount oAuthAccount = new(oAuthAccountData);
                    accounts.Add(oAuthAccount);
                }
                else if (row.Type == "Custom")
                {
                    ICustomAccount customAccountData = new CustomAccountDto()
                    {
                        Id = row.AccountId,
                        UserId = row.UserId,
                        DisplayName = row.DisplayName,
                        Email = row.Email,
                        Password = Crypto.Decrypt(row.EncryptedPassword!)!,
                        Tags = TagsDB.RetrieveAccountTags(row.AccountId),
                        Note = row.Note,
                        UpdatedAt = row.UpdatedAt
                    };
                    CustomAccount customAccount = new(customAccountData);
                    accounts.Add(customAccount);
                }
            }

            return accounts;
        }

        // Create or Delete Filters for an Account
        public static void UpdateAccountFilters(int accountId, List<ITag> oldTagsList, List<ITag> newTagsList)
        {
            var oldTags = oldTagsList.Select(t => t.Id).ToList();
            var newTags = newTagsList.Select(t => t.Id).ToList();

            var tagsToAdd = newTags.Where(id => !oldTags.Contains(id)).ToList();
            var tagsToRemove = oldTags.Where(id => !newTags.Contains(id)).ToList();

            foreach (var tagId in tagsToAdd)
            {
                var filter = new FilterDto { AccountId = accountId, TagId = tagId };
                FiltersDB.CreateFilter(filter);
            }

            foreach (var tagId in tagsToRemove)
            {
                var filter = new FilterDto { AccountId = accountId, TagId = tagId };
                FiltersDB.DeleteFilter(filter);
            }
        }


        // Update an OAuth Account
        public static void UpdateAccount(IOAuthAccount oAuthAccount)
        {
            string sql = @"
                UPDATE Accounts
                SET display_name = @display_name,
                    email = @email,
                    provider = @provider,
                    note = @note,
                    updated_at = CURRENT_TIMESTAMP
                WHERE account_id = @account_id;
            ";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@display_name", oAuthAccount.DisplayName);
            cmd.Parameters.AddWithValue("@email", oAuthAccount.Email);
            cmd.Parameters.AddWithValue("@provider", oAuthAccount.Provider);
            cmd.Parameters.AddWithValue("@note", oAuthAccount.Note);
            cmd.Parameters.AddWithValue("@account_id", oAuthAccount.Id);
            cmd.ExecuteNonQuery();

            OAuthAccount account = (OAuthAccount)User.Session!.Accounts!.FirstOrDefault(a => a.Data.Id == oAuthAccount.Id)!;
            UpdateAccountFilters(oAuthAccount.Id, account.Data.Tags, oAuthAccount.Tags);
            account.UpdateAccount(oAuthAccount);
        }

        // Update a Custom Account
        public static void UpdateAccount(ICustomAccount customAccount)
        {
            string sql = @"
                UPDATE Accounts
                SET display_name = @display_name,
                    email = @email,
                    encrypted_password = @encrypted_password,
                    note = @note,
                    updated_at = CURRENT_TIMESTAMP
                WHERE account_id = @account_id;
            ";
            byte[] password = Crypto.Encrypt(customAccount.Password!)!;

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@display_name", customAccount.DisplayName);
            cmd.Parameters.AddWithValue("@email", customAccount.Email);
            cmd.Parameters.AddWithValue("@encrypted_password", password);
            cmd.Parameters.AddWithValue("@note", customAccount.Note);
            cmd.Parameters.AddWithValue("@account_id", customAccount.Id);
            cmd.ExecuteNonQuery();

            CustomAccount account = (CustomAccount)User.Session!.Accounts!.FirstOrDefault(a => a.Data.Id == customAccount.Id)!;
            UpdateAccountFilters(customAccount.Id, account.Data.Tags, customAccount.Tags);
            account.UpdateAccount(customAccount);
        }

        // Delete an Account
        public static void DeleteAccount(int accountId)
        {
            string sql = "DELETE FROM Accounts WHERE account_id = @account_id;";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@account_id", accountId);
            cmd.ExecuteNonQuery();

            User.RemoveSessionAccount(accountId);
        }
    }
}
