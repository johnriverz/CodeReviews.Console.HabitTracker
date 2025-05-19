using Microsoft.Data.Sqlite;

namespace habitTracker.johnriverz
{
    class Program
    {
        static void Main(string[] args)
        {
            // create and connect to database
            string connectionString = @"Data Source=habit-tracker.db;";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // create table
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS habits (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    date TEXT NOT NULL,
                    quantity INTEGER
                    )";

                tableCmd.ExecuteNonQuery();

                // Verify table exists
                tableCmd.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name='habits';";
                var result = tableCmd.ExecuteScalar();
                Console.WriteLine($"Table exists: {result != null}");

                // close connection
                connection.Close();
            }
        }
    }
}
