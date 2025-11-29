using MySql.Data.MySqlClient;

namespace NekoKeepDB.Databases
{
    public class MainDB
    {
        protected static MySqlConnection? connection;

        // Connect to MySql with connection string
        public static void Connect()
        {
            connection = new MySqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
            connection.Open();
        }

        // Dispose and disconnect current MySql connection
        public static void Disconnect()
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        // Delete all tables (For development)
        public static void DropAllTables()
        {
            string sql = @"
                USE neko_keep;
                
                DROP TABLE IF EXISTS Filters;
                DROP TABLE IF EXISTS Accounts;
                DROP TABLE IF EXISTS Tags;
                DROP TABLE IF EXISTS Users;
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
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
                    email VARCHAR(50) NOT NULL UNIQUE,
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
                    type ENUM('OAuth', 'Custom') NOT NULL,
                    display_name VARCHAR(50) NOT NULL,
                    email VARCHAR(50) NOT NULL,
                    provider VARCHAR(50) DEFAULT NULL,
                    encrypted_password BLOB DEFAULT NULL,
                    note TEXT DEFAULT NULL,
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
    }
}
