// File: Database/DatabaseInitializer.cs
using Microsoft.Data.Sqlite;

namespace MiniOrm.Database
{
    public class DatabaseInitializer(string connectionString = "Data Source=miniorm.db")
    {
        private readonly string _connectionString = connectionString;

        public void Initialize()
        {
            CreateTables();
            SeedData();
            Console.WriteLine("Base SQLite prête !\n");
        }

        private void CreateTables()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT,
                    age INTEGER
                );
            ";
            command.ExecuteNonQuery();
        }

        private void SeedData()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT COUNT(*) FROM users;";
            long count = command.ExecuteScalar() is long c ? c : 0;

            if (count == 0)
            {
                command.CommandText = @"
                    INSERT INTO users (name, age) VALUES ('Alice', 25);
                    INSERT INTO users (name, age) VALUES ('Bob', 17);
                    INSERT INTO users (name, age) VALUES ('Charlie', 30);
                    INSERT INTO users (name, age) VALUES ('David', 22);
                    INSERT INTO users (name, age) VALUES ('Eve', 19);
                ";
                command.ExecuteNonQuery();
                Console.WriteLine("Données de test insérées.");
            }
        }

        public void Reset()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS users;";
            command.ExecuteNonQuery();

            Initialize();
            Console.WriteLine("Base de données réinitialisée !");
        }
    }
}