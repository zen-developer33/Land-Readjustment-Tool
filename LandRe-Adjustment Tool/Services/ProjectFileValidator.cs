using System.Data.SQLite;

public static class ProjectFileValidator
{
    public static bool IsValidProjectFile(string path)
    {
        try
        {
            using var con = new SQLiteConnection($"Data Source={path};");
            con.Open();

            // Check required table exists
            string sql = @"
                SELECT name 
                FROM sqlite_master 
                WHERE type='table' AND name='ProjectInfo';";

            using var cmd = new SQLiteCommand(sql, con);
            var result = cmd.ExecuteScalar();

            return result != null;
        }
        catch
        {
            return false;
        }
    }
}
