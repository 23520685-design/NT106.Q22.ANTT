using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace MiniSocialApp.Offline
{
    internal class LocalDatabase
    {
        private readonly string _databaseDirectory;
        private readonly string _databasePath;
        private readonly string _connectionString;

        public LocalDatabase()
        {
            _databaseDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MiniSocialApp");

            _databasePath = Path.Combine(
                _databaseDirectory,
                "mini_social_local.db");

            _connectionString =
                $"Data Source={_databasePath}";

            InitializeDatabase();
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public string GetDatabasePath()
        {
            return _databasePath;
        }

        private void InitializeDatabase()
        {
            CreateDatabaseDirectory();

            CreateTables();
        }

        private void CreateDatabaseDirectory()
        {
            if (!Directory.Exists(_databaseDirectory))
            {
                Directory.CreateDirectory(_databaseDirectory);
            }
        }

        private void CreateTables()
        {
            using (SqliteConnection connection = CreateConnection())
            {
                connection.Open();

                CreateCachedPostsTable(connection);
                CreateCachedPostsIndex(connection);
            }
        }

        private void CreateCachedPostsTable(
            SqliteConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS cached_posts
                (
                    owner_user_id TEXT NOT NULL,
                    post_id TEXT NOT NULL,
                    post_json TEXT NOT NULL,
                    created_at TEXT NOT NULL,
                    cached_at TEXT NOT NULL,

                    PRIMARY KEY
                    (
                        owner_user_id,
                        post_id
                    )
                );
            ";

            using (SqliteCommand command =
                   connection.CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        private void CreateCachedPostsIndex(
            SqliteConnection connection)
        {
            const string sql = @"
                CREATE INDEX IF NOT EXISTS
                idx_cached_posts_owner_created
                ON cached_posts
                (
                    owner_user_id,
                    created_at DESC
                );
            ";

            using (SqliteCommand command =
                   connection.CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }
    }
}