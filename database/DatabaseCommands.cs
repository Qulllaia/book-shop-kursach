using System.Data;
using System.Reflection;
using Npgsql;

namespace database
{
    public struct DatabaseParameter
    {
        public string name;
        public object value;
        public DbType type;
    }

    public static class DatabaseCommands
    {
        public static int ExecuteCommands(
            IDbConnection connection,
            string query,
            params DatabaseParameter[] parameters
        )
        {
            var comm = connection.CreateCommand();
            DatabaseCommands.CommandBase(comm, query, parameters);
            return comm.ExecuteNonQuery();
        }

        public static List<T> QueryCommands<T>(
            IDbConnection connection,
            string query,
            params DatabaseParameter[] parameters
        )
            where T : new()
        {
            var type = typeof(T);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<T>();

            var comm = connection.CreateCommand();
            DatabaseCommands.CommandBase(comm, query, parameters);

            using var reader = comm.ExecuteReader();
            while (reader.Read())
            {
                var item = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var property = properties.FirstOrDefault(p =>
                        p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    );

                    if (property != null && reader[i] != DBNull.Value)
                    {
                        property.SetValue(item, reader[i]);
                    }
                }
                result.Add(item);
            }
            return result;
        }

        private static void CommandBase(
            IDbCommand command,
            string query,
            params DatabaseParameter[] parameters
        )
        {
            command.CommandText += query;
            foreach (DatabaseParameter parameter in parameters)
            {
                DatabaseCommands.AddParameter(
                    command,
                    parameter.name,
                    parameter.value,
                    parameter.type
                );
            }
        }

        private static void AddParameter(IDbCommand command, string name, object value, DbType type)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            param.DbType = type;
            command.Parameters.Add(param);
        }
    }
}
