using System.Data.SQLite;
using System.IO;

namespace Land_Readjustment_Tool.Services
{
    internal class DatabaseHelper
    {
        private readonly string _dbPath;
        private SQLiteConnection? _connection;

        public DatabaseHelper(string dbPath)
        {
            _dbPath = dbPath ?? throw new ArgumentNullException(nameof(dbPath));
        }

        public void InitializeDatabase()
        {
            if (string.IsNullOrWhiteSpace(_dbPath))
            {
                throw new InvalidOperationException("Database path cannot be null or empty.");
            }

            bool isNew = !File.Exists(_dbPath);
            if (isNew)
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            _connection.Open();

            // Enable foreign keys
            using (var cmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", _connection))
            {
                cmd.ExecuteNonQuery();
            }

            // Create all database tables (ProjectInfo, LandOwner, Parcels, etc.)
            DatabaseSchema.CreateSchema(_connection);
        }

        public SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Database not initialized. Call InitializeDatabase() first.");
            }

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }
    }
}
