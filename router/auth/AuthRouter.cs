using System.Data;
using controller;
using dtos;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace router
{
    public class AuthRouter
    {
        public static void RegisterAuthRouter(
            WebApplication app,
            IDbConnection connection,
            IDatabase rdb
        )
        {
            var group = app.MapGroup("/api/auth").WithTags("Auth");
            var authController = new AuthController(connection, rdb);

            group
                .MapPost(
                    "/login",
                    ([FromBody] LoginRequest request) => authController.LoginUser(request)
                )
                .WithName("LoginUser");

            group
                .MapPost("/reg", ([FromBody] RegRequest request) => authController.RegUser(request))
                .WithName("RegUser");
        }
    }
}
