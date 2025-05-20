using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Data.Sqlite;

namespace habitTracker.johnriverz
{
    public class Habit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }

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
                    name TEXT NOT NULL,
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

            while (!closeApp) 
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
                        Environment.Exit(0);
                        break;
                    case "1":
                        // View entries
                        Console.WriteLine("Viewing entries...");
                        ViewEntries();
                        break;
                    case "2":
                        // Add entry
                        Console.WriteLine("Adding entry...");
                        InsertEntry();
                        break;
                    case "3":
                        // Delete entry
                        Console.WriteLine("Deleting entry...");
                        DeleteEntry();
                        break;
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

        private static void ViewEntries()
        {
            Console.Clear();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = 
                    $"SELECT * FROM habits ORDER BY date DESC";

                // list to store rows of our table
                List<Habit> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        try
                        {
                            string dateStr = reader.GetString(2);
                            // First try to parse as dd-MM-yy
                            if (!DateTime.TryParseExact(dateStr, "dd-MM-yy", new CultureInfo("en-US"), 
                                DateTimeStyles.None, out DateTime date))
                            {
                                // If that fails, try dd-MM-yyyy
                                date = DateTime.ParseExact(dateStr, "dd-MM-yyyy", new CultureInfo("en-US"));
                            }

                            tableData.Add(new Habit
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Date = date,
                                Quantity = reader.GetInt32(3)
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading row {reader.GetInt32(0)}: {ex.Message}");
                            continue;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No data found.");
                }

                reader.Close();
                connection.Close();

                if (tableData.Any())
                {
                    Console.WriteLine("| ID | Habit | Date       | Quantity |");
                    Console.WriteLine("|----|------------|------------|----------|");
                    foreach (var row in tableData)
                    {
                        Console.WriteLine($"| {row.Id,-2} | {row.Name,-10} | {row.Date:dd-MM-yyyy} | {row.Quantity,-8} |");
                    }
                }

                Console.WriteLine("\nPress any key to return to main menu...");
                Console.ReadKey();
                GetUserInput();
            }   
        }

        private static void InsertEntry()
        {
            Console.WriteLine("Enter habit name: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Habit name cannot be empty.");
                return;
            }

            string date = GetDateInput();
            int quantity = GetQuantityInput("Enter the quantity: ");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = 
                    @"INSERT INTO habits (name, date, quantity) VALUES (@name, @date, @quantity)";
                
                tableCmd.Parameters.AddWithValue("@name", name);
                tableCmd.Parameters.AddWithValue("@date", date);
                tableCmd.Parameters.AddWithValue("@quantity", quantity);

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        internal static string GetDateInput()
        {
            while (true)
            {
                Console.WriteLine("Enter the date (dd-mm-yyyy) or 0 to exit to main menu: ");

                string? input = Console.ReadLine();

                if (input == "0")
                {
                    GetUserInput();
                }

                if (DateTime.TryParseExact(input, "dd-MM-yyyy", new CultureInfo("en-US"), 
                    DateTimeStyles.None, out DateTime date))
                {
                    return date.ToString("dd-MM-yy");
                }

                Console.WriteLine("Invalid date format. Please use dd-mm-yyyy format.");
            }
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