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

        // Get Account Ids
        public static HashSet<int> GetAccountIdsByTags(List<ITag> tags)
        {
            var accountIds = new HashSet<int>();
            if (tags.Count == 0) return accountIds;

            int[] tagIds = [.. tags.Select(t => t.Id)];
            string parameters = string.Join(",", tagIds.Select((id, i) => $"@tag_id_{i}"));
            string sql = @$"SELECT * FROM Filters WHERE tag_id IN ({parameters});";

            using var cmd = new MySqlCommand(sql, connection);
            for (int i = 0; i < tagIds.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@tag_id_{i}", tagIds[i]);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) accountIds.Add(reader.GetInt32("account_id"));
            reader.Close();

            return accountIds;
        }
    }
}
