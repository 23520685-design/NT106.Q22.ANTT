using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MiniSocialApp.Offline
{
    internal class LocalPostCacheService
    {
        private readonly LocalDatabase _database;

        public LocalPostCacheService(LocalDatabase database)
        {
            _database = database
                ?? throw new ArgumentNullException(nameof(database));
        }

        public void ReplaceFeed(
            string ownerUserId,
            List<Dictionary<string, object>> posts)
        {
            ValidateOwnerUserId(ownerUserId);

            if (posts == null)
            {
                throw new ArgumentNullException(nameof(posts));
            }

            using (SqliteConnection connection =
                   _database.CreateConnection())
            {
                connection.Open();

                using (SqliteTransaction transaction =
                       connection.BeginTransaction())
                {
                    try
                    {
                        DeleteOldFeed(
                            connection,
                            transaction,
                            ownerUserId);

                        foreach (Dictionary<string, object> post in posts)
                        {
                            SavePost(
                                connection,
                                transaction,
                                ownerUserId,
                                post);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<Dictionary<string, object>> GetCachedFeed(
            string ownerUserId,
            int limit = 50)
        {
            ValidateOwnerUserId(ownerUserId);

            var posts =
                new List<Dictionary<string, object>>();

            using (SqliteConnection connection =
                   _database.CreateConnection())
            {
                connection.Open();

                const string sql = @"
                    SELECT post_json
                    FROM cached_posts
                    WHERE owner_user_id = @ownerUserId
                    ORDER BY created_at DESC
                    LIMIT @limit;
                ";

                using (SqliteCommand command =
                       connection.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Parameters.AddWithValue(
                        "@ownerUserId",
                        ownerUserId);

                    command.Parameters.AddWithValue(
                        "@limit",
                        limit);

                    using (SqliteDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, object> post =
                                DeserializePost(reader);

                            if (post == null)
                            {
                                continue;
                            }

                            post["dataSource"] = "local";
                            post["isOfflineData"] = true;

                            posts.Add(post);
                        }
                    }
                }
            }

            return posts;
        }

        public int GetCachedPostCount(string ownerUserId)
        {
            ValidateOwnerUserId(ownerUserId);

            using (SqliteConnection connection =
                   _database.CreateConnection())
            {
                connection.Open();

                const string sql = @"
                    SELECT COUNT(*)
                    FROM cached_posts
                    WHERE owner_user_id = @ownerUserId;
                ";

                using (SqliteCommand command =
                       connection.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Parameters.AddWithValue(
                        "@ownerUserId",
                        ownerUserId);

                    object result =
                        command.ExecuteScalar();

                    return Convert.ToInt32(result);
                }
            }
        }

        public void ClearUserCache(string ownerUserId)
        {
            ValidateOwnerUserId(ownerUserId);

            using (SqliteConnection connection =
                   _database.CreateConnection())
            {
                connection.Open();

                const string sql = @"
                    DELETE FROM cached_posts
                    WHERE owner_user_id = @ownerUserId;
                ";

                using (SqliteCommand command =
                       connection.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Parameters.AddWithValue(
                        "@ownerUserId",
                        ownerUserId);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteOldFeed(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string ownerUserId)
        {
            const string sql = @"
                DELETE FROM cached_posts
                WHERE owner_user_id = @ownerUserId;
            ";

            using (SqliteCommand command =
                   connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;

                command.Parameters.AddWithValue(
                    "@ownerUserId",
                    ownerUserId);

                command.ExecuteNonQuery();
            }
        }

        private void SavePost(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string ownerUserId,
            Dictionary<string, object> post)
        {
            if (post == null)
            {
                return;
            }

            string postId =
                GetString(post, "postId");

            if (string.IsNullOrWhiteSpace(postId))
            {
                return;
            }

            string postJson =
                JsonConvert.SerializeObject(post);

            string createdAt =
                GetCreatedAt(post).ToString("o");

            const string sql = @"
                INSERT OR REPLACE INTO cached_posts
                (
                    owner_user_id,
                    post_id,
                    post_json,
                    created_at,
                    cached_at
                )
                VALUES
                (
                    @ownerUserId,
                    @postId,
                    @postJson,
                    @createdAt,
                    @cachedAt
                );
            ";

            using (SqliteCommand command =
                   connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;

                command.Parameters.AddWithValue(
                    "@ownerUserId",
                    ownerUserId);

                command.Parameters.AddWithValue(
                    "@postId",
                    postId);

                command.Parameters.AddWithValue(
                    "@postJson",
                    postJson);

                command.Parameters.AddWithValue(
                    "@createdAt",
                    createdAt);

                command.Parameters.AddWithValue(
                    "@cachedAt",
                    DateTime.UtcNow.ToString("o"));

                command.ExecuteNonQuery();
            }
        }

        private Dictionary<string, object> DeserializePost(
            SqliteDataReader reader)
        {
            if (reader["post_json"] == DBNull.Value)
            {
                return null;
            }

            string postJson =
                Convert.ToString(reader["post_json"]);

            if (string.IsNullOrWhiteSpace(postJson))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<
                    Dictionary<string, object>>(postJson);
            }
            catch (JsonException exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Không thể đọc post JSON từ SQLite: " +
                    exception.Message);

                return null;
            }
        }

        private string GetString(
            Dictionary<string, object> data,
            string key)
        {
            if (!data.ContainsKey(key) ||
                data[key] == null)
            {
                return null;
            }

            return Convert.ToString(data[key]);
        }

        private DateTime GetCreatedAt(
            Dictionary<string, object> post)
        {
            if (!post.ContainsKey("createdAt") ||
                post["createdAt"] == null)
            {
                return DateTime.UtcNow;
            }

            object value = post["createdAt"];

            if (value is DateTime dateTime)
            {
                return dateTime.ToUniversalTime();
            }

            DateTime parsedDate;

            bool parsed = DateTime.TryParse(
                Convert.ToString(value),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out parsedDate);

            return parsed
                ? parsedDate.ToUniversalTime()
                : DateTime.UtcNow;
        }

        private void ValidateOwnerUserId(string ownerUserId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                throw new ArgumentException(
                    "Owner user ID is required.",
                    nameof(ownerUserId));
            }
        }
    }
}