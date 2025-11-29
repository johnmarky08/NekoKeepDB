using MySql.Data.MySqlClient;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Databases
{
    // All Filter Database Queries
    public class FiltersDB : MainDB
    {
        // Filter Creation
        public static void CreateFilter(IFilter filter)
        {
            string sql = @"INSERT IGNORE INTO Filters (account_id, tag_id) VALUES (@account_id, @tag_id);";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@account_id", filter.AccountId);
            cmd.Parameters.AddWithValue("@tag_id", filter.TagId);
            cmd.ExecuteNonQuery();
        }

        // Delete filter
        public static void DeleteFilter(IFilter filter)
        {
            string sql = @"DELETE FROM Filters WHERE account_id = @account_id AND tag_id = @tag_id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@account_id", filter.AccountId);
            cmd.Parameters.AddWithValue("@tag_id", filter.TagId);
            cmd.ExecuteNonQuery();
        }
    }
}
