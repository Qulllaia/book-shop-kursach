using System.Data;
using controller;
using dtos;
using Microsoft.AspNetCore.Mvc;
using middleware;
using StackExchange.Redis;

namespace router
{
    public class WarehouseRouter
    {
        public static void RegisterWarehouseRouter(
            WebApplication app,
            IDbConnection connection,
            IDatabase rdb
        )
        {
            var group = app.MapGroup("/api/warehouse")
                .AddEndpointFilter<AuthMiddlewareFilter>()
                .AddEndpointFilter(new BlackListCheckMiddleware(ref rdb))
                .WithTags("Items");

            var warehouseController = new WarehouseController(connection, rdb);

            group
                .MapPost(
                    "/create",
                    ([FromBody] CreateItemRequest request) =>
                        warehouseController.CreateItem(request)
                )
                .AddEndpointFilter<RolesMiddlewareFilter>()
                .WithName("CreateItem");

            group.MapGet("/get", () => warehouseController.GetItems()).WithName("GetItems");

            group
                .MapGet("/get/{id}", (int id) => warehouseController.GetItem(id))
                .WithName("GetItem");

            group
                .MapDelete("/delete/{id}", (int id) => warehouseController.DeleteItem(id))
                .AddEndpointFilter<RolesMiddlewareFilter>()
                .WithName("DeleteItem");
            group
                .MapPatch(
                    "/update",
                    ([FromBody] CreateItemRequest request) =>
                        warehouseController.UpdateItem(request)
                )
                .AddEndpointFilter<RolesMiddlewareFilter>()
                .WithName("UpdateItem");
        }
    }
}
