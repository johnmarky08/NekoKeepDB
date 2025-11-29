using MySql.Data.MySqlClient;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Databases
{
    // All Accounts Database Queries
    public class AccountsDB : MainDB
    {
        // Account Creation - OAuth Account
        public static void CreateAccount(IOAuthAccount account)
        {
            if (!Utils.ValidateEmail(account.Email)) return;

            string sql = @"
                INSERT INTO Accounts (user_id, type, display_name, email, provider, encrypted_password, note)
                VALUES (@user_id, @type, @display_name, @email, @provider, @encrypted_password, @note);
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", account.Id);
            cmd.Parameters.AddWithValue("@type", "OAuth");
            cmd.Parameters.AddWithValue("@display_name", account.DisplayName);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@provider", account.Provider);
            cmd.Parameters.AddWithValue("@encrypted_password", null);
            cmd.Parameters.AddWithValue("@note", account.Note);
            cmd.ExecuteNonQuery();
        }

        // Account Creation - Custom Account
        public static void CreateAccount(ICustomAccount account)
        {
            string? encryptedPassword;
            if (!Utils.ValidateEmail(account.Email) || (account.Password == null) || !Utils.ValidatePassword(account.Password)) return;
            else encryptedPassword = Utils.Encrypt(account.Password);

            string sql = @"
                INSERT INTO Accounts (user_id, type, display_name, email, provider, encrypted_password, note)
                VALUES (@user_id, @type, @display_name, @email, @provider, @encrypted_password, @note);
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", account.Id);
            cmd.Parameters.AddWithValue("@type", "Custom");
            cmd.Parameters.AddWithValue("@display_name", account.DisplayName);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@provider", null);
            cmd.Parameters.AddWithValue("@encrypted_password", encryptedPassword);
            cmd.Parameters.AddWithValue("@note", account.Note);
            cmd.ExecuteNonQuery();
        }
    }
}
