using System.Data.SQLite;

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

            if (isNew)
            {
                CreateSchemaTables();
            }

            LandOwnerDatabaseSchema.CreateSchema(_connection);
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

        private void CreateSchemaTables()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Connection is not initialized.");
            }

            string createProjectInfoTable = @"
                CREATE TABLE IF NOT EXISTS ProjectInfo (
                    GUID TEXT PRIMARY KEY,
                    ProjectName TEXT,
                    ProjectPath TEXT,
                    CreatedDate TEXT,
                    ApprovalDate TEXT,
                    Province TEXT,
                    District TEXT,
                    Municipality TEXT,
                    WardNo TEXT,
                    ProjectSite TEXT,
                    ImplementingAgency TEXT,
                    ConsultingAgency TEXT
                );";

            using (var cmd = new SQLiteCommand(createProjectInfoTable, _connection))
            {
                _ = cmd.ExecuteNonQuery();
            }
        }
    }
}
