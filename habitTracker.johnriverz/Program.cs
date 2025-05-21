using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace habitTracker.johnriverz
{
    public class Habit
    {
        public int Id { get; set; }
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
                        // Insert entry
                        Console.WriteLine("Inserting entry...");
                        InsertEntry();
                        break;
                    case "3":
                        // Delete entry
                        Console.WriteLine("Deleting entry...");
                        DeleteEntry();
                        break;
                    case "4":
                        // Update entry
                        Console.WriteLine("Updating entry...");
                        UpdateEntry();
                        break;
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

                while (reader.Read())
                {
                    try
                    {
                        tableData.Add(new Habit
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2)
                        });
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"Error reading row {reader.GetInt32(0)}: {ex.Message}");
                        continue;
                    }
                }

                reader.Close();
                connection.Close();

                if (tableData.Count > 0)
                {
                    Console.WriteLine("| ID | Date       | Quantity |");
                    Console.WriteLine("|----|------------|----------|");
                    foreach (var row in tableData)
                    {
                        Console.WriteLine($"| {row.Id,-2} | {row.Date:dd-MM-yyyy} | {row.Quantity,-8} |");
                    }
                }
                else
                {
                    Console.WriteLine("No data found.");
                }
                Console.WriteLine("\n");
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
                    @"INSERT INTO habits (date, quantity) VALUES (@date, @quantity)";
                
                tableCmd.Parameters.AddWithValue("@date", date);
                tableCmd.Parameters.AddWithValue("@quantity", quantity);

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        private static void DeleteEntry()
        {
            Console.Clear();
            ViewEntries();

            var recordId = GetQuantityInput("Enter the ID of the record you would like to delete (0 to exit): ");

            if (recordId == 0)
            {
                GetUserInput();
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open(); 
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = "DELETE from habits WHERE Id = @id";
                tableCmd.Parameters.AddWithValue("@id", recordId);

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"Habit (ID: {recordId}) does not exist.");
                    DeleteEntry();
                }
            }

            Console.WriteLine($"Habit (ID: {recordId}) was successfully deleted.");
            
            GetUserInput();
        }

        internal static void UpdateEntry()
        {
            ViewEntries();

            var iD = GetQuantityInput("Enter the ID of the record you would like to update (0 to exit): ");

            if (iD == 0)
            {
                GetUserInput();
                return;
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // check if exists
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT EXISTS(SELECT 1 FROM habits WHERE Id = @id)";
                checkCmd.Parameters.AddWithValue("@id", iD);
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine("Record does not exist.");
                    GetUserInput();
                    return;
                }

                string date = GetDateInput();
                int quantity = GetQuantityInput("Enter new quantity: ");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = "UPDATE habits SET date = @date, quantity = @quantity WHERE Id = @id";
                tableCmd.Parameters.AddWithValue("@date", date);
                tableCmd.Parameters.AddWithValue("@quantity", quantity);
                tableCmd.Parameters.AddWithValue("@id", iD);

                tableCmd.ExecuteNonQuery();
                Console.WriteLine($"Record {iD} has been updated successfully.");
            }

            GetUserInput();
        }

        internal static string GetDateInput()
        {
            while (true)
            {
                Console.WriteLine("Enter the date (dd-mm-yyyy): ");

                string? input = Console.ReadLine();

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