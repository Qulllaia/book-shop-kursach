using System.Data;
using System.Text.Json;
using database;
using dtos;
using models;
using StackExchange.Redis;

namespace controller
{
    public class UserController
    {
        private IDbConnection _connection;
        private IDatabase _rdb;

        public UserController(ref IDbConnection connection, ref IDatabase rdb)
        {
            _connection = connection;
            _rdb = rdb;
        }

        public async Task<List<User>> GetUsers()
        {
            var result = new List<User>();
            if (!_rdb.KeyExists("users"))
            {
                result = DatabaseCommands.QueryCommands<User>(
                    _connection,
                    "SELECT * FROM \"user\""
                );
                await _rdb.StringSetAsync("users", JsonSerializer.Serialize(result));
            }
            else
            {
                result = JsonSerializer.Deserialize<List<User>>(
                    await _rdb.StringGetAsync("users")!
                );
            }
            return result;
        }

        public async Task<object> GetUser(int id)
        {
            var result = new User();
            if (!_rdb.KeyExists($"users:{id}"))
            {
                var user = DatabaseCommands.QueryCommands<User>(
                    _connection,
                    "SELECT * FROM \"user\" WHERE userid = @id",
                    new DatabaseParameter
                    {
                        name = "@id",
                        value = id,
                        type = DbType.Int64,
                    }
                );
                if (user.Count > 0)
                {
                    result = user[0];
                    await _rdb.StringSetAsync($"users:{id}", JsonSerializer.Serialize(result));
                    return result;
                }
            }
            else
            {
                result = JsonSerializer.Deserialize<User>(await _rdb.StringGetAsync($"users:{id}"));
                return result;
            }

            return Results.Json(
                new { error = "Пользователь с указанным id не найден" },
                statusCode: StatusCodes.Status404NotFound
            );
        }

        public async Task<string> UpdateUser(CreateUserRequest request)
        {
            int rowsAffected = DatabaseCommands.ExecuteCommands(
                _connection,
                "UPDATE \"user\" SET role = @role WHERE userid = @id",
                new DatabaseParameter
                {
                    name = "@role",
                    value = request.role,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "@id",
                    value = request.userid,
                    type = DbType.Int64,
                }
            );

            await _rdb.KeyDeleteAsync("users");

            await _rdb.KeyDeleteAsync($"users:{request.userid}");
            return $"Обновлено записей: {rowsAffected}";
        }

        public async Task<string> DeleteUser(int userid)
        {
            int rowsAffected = DatabaseCommands.ExecuteCommands(
                _connection,
                "DELETE FROM \"user\" WHERE userid = @id",
                new DatabaseParameter
                {
                    name = "@id",
                    value = userid,
                    type = DbType.Int64,
                }
            );
            await _rdb.KeyDeleteAsync("users");

            await _rdb.KeyDeleteAsync($"users:{userid}");

            await _rdb.StringSetAsync($"usersblacklist:{userid}", 1);
            return $"Удалено записей: {rowsAffected}";
        }
    }
}
