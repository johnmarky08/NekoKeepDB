using MySql.Data.MySqlClient;

namespace NekoKeepDB
{
    internal class Program
    {
        private static readonly string connectionString = "Server=localhost;User ID=root;Pooling=true;";
        private static MySqlConnection? connection;

        static void Main()
        {
            Connect();
            CreateAllTables();

            // Test User Creation


            Disconnect();
        }

        static void Connect()
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        static void Disconnect()
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        static void CreateAllTables()
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
            Console.WriteLine("Success!");
        }

        static void CreateUser(string displayName, string email, string encryptedPassword, string encryptedMpin, int catPresetId)
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
    }
}
