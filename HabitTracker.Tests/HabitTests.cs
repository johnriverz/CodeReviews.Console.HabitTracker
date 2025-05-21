using System;
using System.IO;
using Xunit;
using Microsoft.Data.Sqlite;
using habitTracker.johnriverz;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace HabitTracker.Tests
{
    public class HabitTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly string _connectionString;

        public HabitTests()
        {
            _testDbPath = $"test-habit-tracker-{Guid.NewGuid()}.db";
            _connectionString = $"Data Source={_testDbPath};";
            SetupTestDatabase();
        }

        private void SetupTestDatabase()
        {
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS habits (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        date TEXT NOT NULL,
                        quantity INTEGER
                    )";
                command.ExecuteNonQuery();
            }
        }

        [Fact]
        public void InsertEntry_ValidData_ShouldInsertSuccessfully()
        {
            string date = "01-01-24";
            int quantity = 5;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO habits (date, quantity) VALUES (@date, @quantity)";
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@quantity", quantity);
                command.ExecuteNonQuery();
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM habits WHERE date = @date AND quantity = @quantity";
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@quantity", quantity);
                var count = Convert.ToInt32(command.ExecuteScalar());
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void DeleteEntry_ExistingId_ShouldDeleteSuccessfully()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO habits (date, quantity) VALUES (@date, @quantity)";
                command.Parameters.AddWithValue("@date", "01-01-24");
                command.Parameters.AddWithValue("@quantity", 5);
                command.ExecuteNonQuery();
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM habits WHERE id = 1";
                command.ExecuteNonQuery();
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM habits WHERE id = 1";
                var count = Convert.ToInt32(command.ExecuteScalar());
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void UpdateEntry_ExistingId_ShouldUpdateSuccessfully()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO habits (date, quantity) VALUES (@date, @quantity)";
                command.Parameters.AddWithValue("@date", "01-01-24");
                command.Parameters.AddWithValue("@quantity", 5);
                command.ExecuteNonQuery();
            }

            string newDate = "02-01-24";
            int newQuantity = 10;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE habits SET date = @date, quantity = @quantity WHERE id = 1";
                command.Parameters.AddWithValue("@date", newDate);
                command.Parameters.AddWithValue("@quantity", newQuantity);
                command.ExecuteNonQuery();
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT date, quantity FROM habits WHERE id = 1";
                using (var reader = command.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.Equal(newDate, reader.GetString(0));
                    Assert.Equal(newQuantity, reader.GetInt32(1));
                }
            }
        }

        [Fact]
        public void ViewEntries_EmptyTable_ShouldReturnNoData()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM habits";
                var count = Convert.ToInt32(command.ExecuteScalar());
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void ViewEntries_WithData_ShouldReturnCorrectCount()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO habits (date, quantity) VALUES 
                    (@date1, @quantity1),
                    (@date2, @quantity2)";
                command.Parameters.AddWithValue("@date1", "01-01-24");
                command.Parameters.AddWithValue("@quantity1", 5);
                command.Parameters.AddWithValue("@date2", "02-01-24");
                command.Parameters.AddWithValue("@quantity2", 10);
                command.ExecuteNonQuery();
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM habits";
                var count = Convert.ToInt32(command.ExecuteScalar());
                Assert.Equal(2, count);
            }
        }

        public void Dispose()
        {
            // Retry delete up to 10 times with a short delay
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (File.Exists(_testDbPath))
                        File.Delete(_testDbPath);
                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
    }
} 