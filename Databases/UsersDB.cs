using MySql.Data.MySqlClient;
using NekoKeepDB.Classes;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Databases
{
    // All Users Database Queries
    public class UsersDB : MainDB
    {
        // User creation
        public static void CreateUser(IUser user, string encryptedPassword, string encryptedMpin)
        {
            string sql = @"
                INSERT INTO Users (display_name, email, encrypted_password, encrypted_mpin, cat_preset_id)
                VALUES (@display_name, @email, @encrypted_password, @encrypted_mpin, @cat_preset_id);
            ";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@display_name", user.DisplayName);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@encrypted_password", encryptedPassword);
            cmd.Parameters.AddWithValue("@encrypted_mpin", encryptedMpin);
            cmd.Parameters.AddWithValue("@cat_preset_id", user.CatPresetId);
            cmd.ExecuteNonQuery();
        }

        // User authentication - returns 1 if success, 0 if wrong password, -1 if user not found/wrong email
        public static int AuthenticateUser(string email, string password)
        {
            if (!Utils.ValidateEmail(email)) return -1;

            string sql = "SELECT * FROM Users WHERE email = @email;";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string encryptedPassword = reader.GetString("encrypted_password");

                if (Utils.BCryptVerify(password, encryptedPassword))
                {
                    int userId = reader.GetInt32("user_id");
                    string displayName = reader.GetString("display_name");
                    string userEmail = reader.GetString("email");
                    string encryptedMpin = reader.GetString("encrypted_mpin");
                    int catPresetId = reader.GetInt32("cat_preset_id");

                    IUser user = new UserDto()
                    {
                        Id = userId,
                        DisplayName = displayName,
                        Email = email,
                        CatPresetId = catPresetId,
                    };

                    User.Login(user, encryptedPassword, encryptedMpin);
                    return 1;
                }
                else return 0;
            }
            else return -1;
        }

        // Updates user password both locally and in database
        public static void UpdateUserPassword(int userId, string password)
        {
            if (!Utils.ValidatePassword(password)) return;

            string newEncryptedPassword = Utils.BCryptEncrypt(password);
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

        // Updates user mpin both locally and in database
        public static void UpdateUserMpin(int userId, string mpin)
        {
            if (!Utils.ValidateMpin(mpin)) return;

            string newEncryptedMpin = Utils.BCryptEncrypt(mpin);
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

        // Updates user email both locally and in database
        public static void UpdateUserEmail(int userId, string newEmail)
        {
            if (!Utils.ValidateEmail(newEmail)) return;

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

        // Updates user display name both locally and in database
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

        // Updates user cat preset id both locally and in database
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

        // User Deletion
        public static void DeleteUser(int userId)
        {
            string sql = @"DELETE FROM Users WHERE user_id = @user_id";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.ExecuteNonQuery();

            User.Logout();
        }
    }
}
