using System.Data;
using controller;
using dtos;
using Microsoft.AspNetCore.Mvc;
using middleware;
using StackExchange.Redis;

namespace router
{
    public class BookRouter
    {
        public static void RegisterBookRouter(
            WebApplication app,
            IDbConnection connection,
            IDatabase rdb
        )
        {
            var group = app.MapGroup("/api/books")
                .AddEndpointFilter<AuthMiddlewareFilter>()
                .AddEndpointFilter(new BlackListCheckMiddleware(ref rdb))
                .WithTags("Books");

            var bookController = new BookController(connection, rdb);

            group
                .MapPost(
                    "/create",
                    ([FromBody] CreateBookRequest request) => bookController.CreateBook(request)
                )
                .AddEndpointFilter<RolesMiddlewareFilter>()
                .WithName("CreateBook");

            group.MapGet("/get", () => bookController.GetBooks()).WithName("GetBooks");

            group
                .MapDelete("/delete/{id}", (int id) => bookController.DeleteBook(id))
                .AddEndpointFilter<RolesMiddlewareFilter>()
                .WithName("DeleteBooks");
            group
                .MapPatch(
                    "/update",
                    ([FromBody] CreateBookRequest request) => bookController.UpdateBook(request)
                )
                .AddEndpointFilter<RolesMiddlewareFilter>()
                .WithName("UpdateBook");
        }
    }
}
