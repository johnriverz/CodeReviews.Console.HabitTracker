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

//cmd.CommandText = "SELECT * FROM sqlite_master WHERE type='table'";

//using (var reader = cmd.ExecuteReader())
//{
//    while (reader.Read())
//    {
//        Console.WriteLine(reader.GetString(1));
//    }
//}

cmd.CommandText = "INSERT INTO Habits(habit, count, date) VALUES('Brush teeth', 0, datetime('now'))";
cmd.ExecuteNonQuery();


cmd.CommandText = "INSERT INTO Habits(habit, count, date) VALUES('Skincare', 0, datetime('now'))";
cmd.ExecuteNonQuery();

//cmd.CommandText = "SELECT * FROM Habits";
//using (var reader = cmd.ExecuteReader())
//{
//    while (reader.Read())
//    {
//        Console.WriteLine($"{reader.GetInt32(0)}");
//    }
//}