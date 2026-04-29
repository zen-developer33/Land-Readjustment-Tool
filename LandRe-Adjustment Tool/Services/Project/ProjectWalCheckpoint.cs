using Microsoft.Data.Sqlite;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Flushes SQLite WAL data directly through Microsoft.Data.Sqlite.
    /// This avoids routing a simple file checkpoint through EF/provider stacks.
    /// </summary>
    public static class ProjectWalCheckpoint
    {
        public static void Execute(string projectFilePath)
        {
            var connectionString = BuildConnectionString(projectFilePath);

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
            command.ExecuteNonQuery();
        }

        public static async Task ExecuteAsync(
            string projectFilePath,
            CancellationToken ct = default)
        {
            var connectionString = BuildConnectionString(projectFilePath);

            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
            await command.ExecuteNonQueryAsync(ct);
        }

        private static string BuildConnectionString(string projectFilePath)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = projectFilePath,
                Pooling = false
            };

            return builder.ToString();
        }
    }
}
