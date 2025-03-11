using System.Data.SQLite;

string cs = @"Data Source=habit.db;";

using var con = new SQLiteConnection(cs);
con.Open();

using var cmd = new SQLiteCommand(con);

cmd.CommandText = "DROP TABLE IF EXISTS Habits";
cmd.ExecuteNonQuery();

cmd.CommandText=@"
    CREATE TABLE Habits (
        id INTEGER PRIMARY KEY,
        habit TEXT,
        count INT,
        date TEXT
    )";
cmd.ExecuteNonQuery();

cmd.CommandText = "INSERT INTO Habits(habit, count, date) VALUES(@habit, @count, @date)";
cmd.Parameters.AddWithValue("@habit", "Brushing teeth");
cmd.Parameters.AddWithValue("@count", 0);
cmd.Parameters.AddWithValue("@date", DateTime.Now.ToShortDateString());
cmd.Prepare();
cmd.ExecuteNonQuery();