using System.Data;
using System.Text.Json;
using database;
using dtos;
using jwt;
using models;
using StackExchange.Redis;

namespace controller
{
    public class AuthController
    {
        private IDbConnection _connection;
        private IDatabase _rdb;

        public AuthController(IDbConnection connection, IDatabase rdb)
        {
            _connection = connection;
            _rdb = rdb;
        }

        public async Task<AuthResponse> LoginUser(LoginRequest request)
        {
            var commanResult = DatabaseCommands.QueryCommands<JWTClaims>(
                _connection,
                "select * from \"user\" where login = @login and password = @password",
                new DatabaseParameter
                {
                    name = "login",
                    value = request.login,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "password",
                    value = request.password,
                    type = DbType.String,
                }
            );

            var result = commanResult[0];

            return new AuthResponse(JwtService.GenerateSimpleToken(result));
        }

        public async Task<AuthResponse> RegUser(RegRequest request)
        {
            var resultData = DatabaseCommands.QueryCommands<JWTClaims>(
                _connection,
                "insert into \"user\"(login, email, password) values(@login, @email, @password, @role) returning *",
                new DatabaseParameter
                {
                    name = "email",
                    value = request.email,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "login",
                    value = request.login,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "password",
                    value = request.password,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "role",
                    value = "user",
                    type = DbType.String,
                }
            );
            return new AuthResponse(JwtService.GenerateSimpleToken(resultData[0]));
        }
    }
}
