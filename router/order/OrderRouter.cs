using System.Data;
using controller;
using dtos;
using Microsoft.AspNetCore.Mvc;
using middleware;
using StackExchange.Redis;

namespace router
{
    public class OrderRouter
    {
        public static void RegisterOrderRouter(
            ref WebApplication app,
            ref IDbConnection connection,
            ref IDatabase rdb
        )
        {
            var group = app.MapGroup("/api/order")
                .AddEndpointFilter<AuthMiddlewareFilter>()
                .AddEndpointFilter(new BlackListCheckMiddleware(ref rdb))
                .WithTags("Orders");

            var orderController = new OrderController(ref connection, ref rdb);

            group
                .MapPost(
                    "/create",
                    ([FromBody] CreateOrderRequest request) => orderController.CreateOrder(request)
                )
                .WithName("CreateOrder");

            group
                .MapGet("/get/{userid}", (int userid) => orderController.GetOrders(userid))
                .WithName("GetOrders");

            group
                .MapGet(
                    "/get/{id}/userid/{userid}",
                    (int id, int userid) => orderController.GetOrder(id, userid)
                )
                .WithName("GetOrder");

            group
                .MapPatch(
                    "/update",
                    ([FromBody] CreateOrderRequest request) => orderController.UpdateOrder(request)
                )
                .WithName("UpdateOrder");
        }
    }
}
