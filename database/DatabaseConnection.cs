using System.Data;
using Dapper;
using DotNetEnv;
using Npgsql;

namespace database
{
    public class DatabaseConnection
    {
        public string Host;
        public string Port;
        public string Name;
        public string User;
        public string Password;

        public static DatabaseConnection FromEnvironment()
        {
            return new DatabaseConnection
            {
                Host = GetRequiredEnvVariable("DB_HOST"),
                Port = GetRequiredEnvVariable("DB_PORT"),
                Name = GetRequiredEnvVariable("DB_NAME"),
                User = GetRequiredEnvVariable("DB_USER"),
                Password = GetRequiredEnvVariable("DB_PASSWORD"),
            };
        }

        private static string GetRequiredEnvVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name)
                ?? throw new InvalidOperationException(
                    $"Отсутствует обязательная переменная окружения: {name}"
                );
        }

        public IDbConnection InitDatabase()
        {
            var connection = new NpgsqlConnection(
                $"Host={Host};Port={Port};Database={Name};Username={User};Password={Password}"
            );
            connection.Open();
            return connection;
        }
    }
}
