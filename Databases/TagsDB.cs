using MySql.Data.MySqlClient;
using NekoKeepDB.Interfaces;

namespace NekoKeepDB.Databases
{
    // All Tags Database Queries
    public class TagsDB : MainDB
    {
        // Tag Creation
        public static void CreateTag(string tagName)
        {
            string sql = @"INSERT INTO Tags (display_name) VALUES (@display_name);";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@display_name", tagName);
            cmd.ExecuteNonQuery();
        }

        // Retrieve All tags from an account
        public static List<ITag> RetriveTags(int accountId)
        {
            string sql = @"SELECT tag_id FROM Filters WHERE account_id = @account_id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@account_id", accountId);

            List<int> tagsIds = [];
            List<ITag> tags = [];

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) tagsIds.Add(reader.GetInt32("tag_id"));
            reader.Close();

            foreach (int tagId in tagsIds) tags.Add(RetrieveTag(tagId)!);

            return tags;
        }

        // Get a tag's display name, returns null if not found
        public static ITag? RetrieveTag(int tagId)
        {
            string sql = "SELECT display_name FROM Tags WHERE tag_id = @tag_id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@tag_id", tagId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                ITag tag = new TagDto()
                {
                    DisplayName = reader.GetString("display_name"),
                    Id = tagId
                };
                reader.Close();
                return tag;
            }
            reader.Close();
            return null;
        }

        // Get all tags display name
        public static List<ITag> RetrieveTags()
        {
            List<ITag> tags = [];

            string sql = "SELECT * FROM Tags ORDER BY display_name;";

            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ITag tag = new TagDto()
                {
                    Id = reader.GetInt32("tag_id"),
                    DisplayName = reader.GetString("display_name"),
                };
                tags.Add(tag);
            }
            reader.Close();

            return tags;
        }

        // Modify tag
        public static void UpdateTag(ITag tag)
        {
            string sql = @"UPDATE Tags SET display_name = @display_name WHERE tag_id = @tag_id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@display_name", tag.DisplayName);
            cmd.Parameters.AddWithValue("@tag_id", tag.Id);
            cmd.ExecuteNonQuery();
        }

        // Delete tag
        public static void DeleteTag(int tagId)
        {
            string sql = @"DELETE FROM Tags WHERE tag_id = @tag_id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@tag_id", tagId);
            cmd.ExecuteNonQuery();
        }
    }
}
