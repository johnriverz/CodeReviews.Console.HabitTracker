using System.Reflection.Metadata.Ecma335;
using Microsoft.Data.Sqlite;

namespace habitTracker.johnriverz
{
    class Program
    {
        // create database
        private const string connectionString = @"Data Source=habit-tracker.db;";

        static void Main(string[] args)
        {
            
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

            GetUserInput();
        }

        static void GetUserInput()
        {
            Console.Clear();
            bool closeApp = false;

            while (closeApp == false) 
            {
                Console.WriteLine("Welcome to the Habit Tracker!");
                Console.WriteLine("0. Exit.");
                Console.WriteLine("1. View entries.");
                Console.WriteLine("2. Add entry.");
                Console.WriteLine("3. Delete entry.");
                Console.WriteLine("4. Update entry.");
                Console.WriteLine("");

                Console.WriteLine("Enter your choice: ");
                string? choice = Console.ReadLine(); 

            switch (choice)
            {
                case "0":
                    Console.WriteLine("Exiting the application...");
                    closeApp = true;
                    break;
                // case "1":
                //     // View entries
                //     Console.WriteLine("Viewing entries...");
                //     ViewEntries();
                //     break;
                case "2":
                    // Add entry
                    Console.WriteLine("Adding entry...");
                    InsertEntry();
                    break;
                // case "3":
                //     // Delete entry
                //     Console.WriteLine("Deleting entry...");
                //     DeleteEntry();
                //     break;
                // case "4":
                //     // Update entry
                //     Console.WriteLine("Updating entry...");
                //     UpdateEntry();
                //     break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
            }
        }

        private static void InsertEntry()
        {
            string date = GetDateInput();

            int quantity = GetQuantityInput("Enter the quantity: ");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = 
                $@"INSERT INTO habits (date, quantity) VALUES ('{date}', {quantity})";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        internal static string GetDateInput()
        {
            Console.WriteLine("Enter the date (dd-mm-yyyy) or 0 to exit to main menu: ");

            string? input = Console.ReadLine();

            if (input == "0")
            {
                GetUserInput();
            }
            
            return input;
        }

        internal static int GetQuantityInput(string message)
        {
            while (true)
            {
                Console.WriteLine(message);
                string? input = Console.ReadLine();

                if (input == "0")
                {
                    GetUserInput();
                }

                if (int.TryParse(input, out int quantity))
                {
                    return quantity;
                }

                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
    }
}
