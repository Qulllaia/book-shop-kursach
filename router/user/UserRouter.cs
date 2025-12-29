using System.Data;
using controller;
using dtos;
using Microsoft.AspNetCore.Mvc;
using middleware;
using StackExchange.Redis;

namespace router
{
    public class UserRouter
    {
        public static void RegisterUserRouter(
            WebApplication app,
            IDbConnection connection,
            IDatabase rdb
        )
        {
            var group = app.MapGroup("/api/users")
                .AddEndpointFilter<AuthMiddlewareFilter>()
                .AddEndpointFilter(new BlackListCheckMiddleware(ref rdb))
                .AddEndpointFilter<RolesMiddlewareFilter>()
                .WithTags("Users");

            var userController = new UserController(connection, rdb);

            group.MapGet("/get/{id}", (int id) => userController.GetUser(id)).WithName("GetUser");
            group.MapGet("/get", () => userController.GetUsers()).WithName("GetUsers");

            group
                .MapDelete("/delete/{id}", (int id) => userController.DeleteUser(id))
                .WithName("DeleteUser");
            group
                .MapPatch(
                    "/update",
                    ([FromBody] CreateUserRequest request) => userController.UpdateUser(request)
                )
                .WithName("UpdateUser");
        }
    }
}
