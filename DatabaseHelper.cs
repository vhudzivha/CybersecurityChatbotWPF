using System;
using System.Data.SQLite;
using System.Text;

namespace CybersecurityChatbotWPF
{
    public class DatabaseHelper
    {
        private static string connectionString =
            "Data Source=chatbot.db;Version=3;";

        // Initializes SQLite database and creates required tables
        public static void InitializeDatabase()
        {
            using (SQLiteConnection conn =
                new SQLiteConnection(connectionString))
            {
                conn.Open();

                string activityTable = @"
                CREATE TABLE IF NOT EXISTS ActivityLog(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserMessage TEXT,
                    BotResponse TEXT,
                    TimeStamp TEXT
                )";

                SQLiteCommand cmd1 =
                    new SQLiteCommand(activityTable, conn);

                cmd1.ExecuteNonQuery();


                string taskTable = @"
                CREATE TABLE IF NOT EXISTS Tasks(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TaskName TEXT
                )";

                SQLiteCommand cmd2 =
                    new SQLiteCommand(taskTable, conn);

                cmd2.ExecuteNonQuery();
            }
        }

        public static void SaveActivity(
            string userMessage,
            string botResponse)
        {
            using (SQLiteConnection conn =
                new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = @"
                INSERT INTO ActivityLog
                (UserMessage,BotResponse,TimeStamp)
                VALUES
                (@user,@bot,@time)";

                SQLiteCommand cmd =
                    new SQLiteCommand(query, conn);

                cmd.Parameters.AddWithValue("@user", userMessage);
                cmd.Parameters.AddWithValue("@bot", botResponse);
                cmd.Parameters.AddWithValue("@time",
                    DateTime.Now.ToString());

                cmd.ExecuteNonQuery();
            }
        }

        public void AddTask(string task)
        {
            using (SQLiteConnection conn =
                new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query =
                "INSERT INTO Tasks(TaskName) VALUES(@task)";

                SQLiteCommand cmd =
                    new SQLiteCommand(query, conn);

                cmd.Parameters.AddWithValue("@task", task);

                cmd.ExecuteNonQuery();
            }
        }

        public string ShowTasks()
        {
            StringBuilder sb = new StringBuilder();

            using (SQLiteConnection conn =
                new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query =
                "SELECT * FROM Tasks";

                SQLiteCommand cmd =
                    new SQLiteCommand(query, conn);

                SQLiteDataReader reader =
                    cmd.ExecuteReader();

                int count = 1;

                while (reader.Read())
                {
                    sb.AppendLine(
                        count + ". " +
                        reader["TaskName"].ToString());

                    count++;
                }
            }

            if (sb.Length == 0)
            {
                return "No tasks found.";
            }

            return sb.ToString();
        }
        public void DeleteTask(int id)
        {
            using (SQLiteConnection conn =
                   new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query =
                "DELETE FROM Tasks WHERE Id=@id";

                SQLiteCommand cmd =
                    new SQLiteCommand(query, conn);

                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }
        }



        public string ShowActivityLog()
        {
            StringBuilder sb = new StringBuilder();

            using (SQLiteConnection conn =
                new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query =
                "SELECT UserMessage, BotResponse, TimeStamp FROM ActivityLog ORDER BY Id DESC";

                SQLiteCommand cmd =
                    new SQLiteCommand(query, conn);

                SQLiteDataReader reader =
                    cmd.ExecuteReader();

                int count = 1;

                while (reader.Read())
                {
                    sb.AppendLine(
                        count + ". " +
                        reader["TimeStamp"].ToString());

                    sb.AppendLine(
                        "User: " +
                        reader["UserMessage"].ToString());

                    sb.AppendLine(
                        "Bot: " +
                        reader["BotResponse"].ToString());

                    sb.AppendLine("");

                    count++;
                }
            }

            if (sb.Length == 0)
            {
                return "No activity found.";
            }

            return sb.ToString();
        }
    }
}