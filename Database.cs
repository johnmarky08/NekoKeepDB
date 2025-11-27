using MySql.Data.MySqlClient;
using NekoKeepDB.Classes;

namespace NekoKeepDB
{
    internal class Database
    {
        private static readonly string connectionString = "Server=localhost;User ID=root;Pooling=true;";
        private static MySqlConnection? connection;

        // Connect to MySql with connection string
        public static void Connect()
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        // Dispose and disconnect cuurent MySql connection
        public static void Disconnect()
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        // Table creation
        public static void CreateAllTables()
        {
            string sql = @"
                CREATE DATABASE IF NOT EXISTS neko_keep;
                USE neko_keep;

                CREATE TABLE IF NOT EXISTS Users (
                    user_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    display_name VARCHAR(50) NOT NULL,
                    email VARCHAR(255) NOT NULL UNIQUE,
                    encrypted_password TEXT NOT NULL,
                    encrypted_mpin TEXT NOT NULL,
                    cat_preset_id INT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Tags (
                    tag_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    display_name VARCHAR(50) NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Accounts (
                    account_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    display_name VARCHAR(50) NOT NULL,
                    email VARCHAR(255) NOT NULL,
                    encrypted_password TEXT NOT NULL,
                    note TEXT,
                    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    CONSTRAINT fk_accounts_user FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Filters (
                    account_id INT NOT NULL,
                    tag_id INT NOT NULL,
                    PRIMARY KEY (account_id, tag_id),
                    CONSTRAINT fk_filters_account FOREIGN KEY (account_id) REFERENCES Accounts(account_id) ON DELETE CASCADE,
                    CONSTRAINT fk_filters_tag FOREIGN KEY (tag_id) REFERENCES Tags(tag_id) ON DELETE CASCADE
                );
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        // User creation
        public static void CreateUser(string displayName, string email, string encryptedPassword, string encryptedMpin, int catPresetId)
        {
            string sql = @"
                INSERT INTO Users (display_name, email, encrypted_password, encrypted_mpin, cat_preset_id)
                VALUES (@display_name, @email, @encrypted_password, @encrypted_mpin, @cat_preset_id);
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@display_name", displayName);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@encrypted_password", encryptedPassword);
            cmd.Parameters.AddWithValue("@encrypted_mpin", encryptedMpin);
            cmd.Parameters.AddWithValue("@cat_preset_id", catPresetId);
            cmd.ExecuteNonQuery();
        }

        // User authentication - returns 1 if success, 0 if wrong password, -1 if user not found
        public static int AuthenticateUser(string email, string password)
        {
            string sql = "SELECT * FROM Users WHERE email = @email;";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string encryptedPassword = reader.GetString("encrypted_password");

                if (Crypto.Verify(password, encryptedPassword))
                {
                    int userId = reader.GetInt32("user_id");
                    string displayName = reader.GetString("display_name");
                    string userEmail = reader.GetString("email");
                    string encryptedMpin = reader.GetString("encrypted_mpin");
                    int catPresetId = reader.GetInt32("cat_preset_id");

                    User.Login(userId, displayName, userEmail, encryptedPassword, encryptedMpin, catPresetId);
                    return 1;
                }
                else return 0;
            }
            else return -1;
        }

        public static void UpdateUserPassword(int userId, string newEncryptedPassword)
        {
            string sql = @"
                UPDATE Users
                SET encrypted_password = @new_encrypted_password
                WHERE user_id = @user_id;
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@new_encrypted_password", newEncryptedPassword);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.ExecuteNonQuery();

            User.UpdateLocalPassword(newEncryptedPassword);
        }

        public static void UpdateUserMpin(int userId, string newEncryptedMpin)
        {
            string sql = @"
                UPDATE Users
                SET encrypted_mpin = @new_encrypted_mpin
                WHERE user_id = @user_id;
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@new_encrypted_mpin", newEncryptedMpin);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.ExecuteNonQuery();

            User.UpdateLocalMpin(newEncryptedMpin);
        }
        
        public static void UpdateUserEmail(int userId, string newEmail)
        {
            string sql = @"
                UPDATE Users
                SET email = @new_email
                WHERE user_id = @user_id;
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@new_email", newEmail);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.ExecuteNonQuery();

            User.UpdateLocalEmail(newEmail);
        }
        
        public static void UpdateUserDisplayName(int userId, string newDisplayName)
        {
            string sql = @"
                UPDATE Users
                SET display_name = @new_display_name
                WHERE user_id = @user_id;
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@new_display_name", newDisplayName);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.ExecuteNonQuery();

            User.UpdateLocalDisplayName(newDisplayName);
        }
        
        public static void UpdateUserCatPresetId(int userId, int newCatPresetId)
        {
            string sql = @"
                UPDATE Users
                SET cat_preset_id = @new_cat_preset_id
                WHERE user_id = @user_id;
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@new_cat_preset_id", newCatPresetId);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.ExecuteNonQuery();

            User.UpdateLocalCatPresetId(newCatPresetId);
        }

        public static void DeleteUser()
        {
            string sql = @"DELETE FROM Users WHERE user_id = @user_id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", User.Id);
            cmd.ExecuteNonQuery();

            User.Logout();
        }
    }
}
