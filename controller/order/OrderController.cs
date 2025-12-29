using System.Data;
using System.Text.Json;
using database;
using dtos;
using models;
using StackExchange.Redis;

namespace controller
{
    public class OrderController
    {
        private IDbConnection _connection;
        private IDatabase _rdb;

        public OrderController(IDbConnection connection, IDatabase rdb)
        {
            _connection = connection;
            _rdb = rdb;
        }

        public async Task<object> CreateOrder(CreateOrderRequest request)
        {
            var item = DatabaseCommands.QueryCommands<Item>(
                _connection,
                "SELECT * FROM warehouse WHERE itemid = @id",
                new DatabaseParameter
                {
                    name = "@id",
                    value = request.itemid,
                    type = DbType.Int64,
                }
            );

            if (item.Count > 0)
            {
                if (item[0].status.Equals(WarehouseStatus.ORDERED))
                {
                    return Results.Json(
                        new { error = "Указанный id товара уже забронирован" },
                        statusCode: StatusCodes.Status404NotFound
                    );
                }
            }
            else
            {
                return Results.Json(
                    new { error = "Указанный id товара не существует" },
                    statusCode: StatusCodes.Status404NotFound
                );
            }
            DatabaseCommands.ExecuteCommands(
                _connection,
                "insert into \"order\"(userid, status, itemid) values (@userid, @status, @itemid)",
                new DatabaseParameter
                {
                    name = "@userid",
                    value = request.userid,
                    type = DbType.Int64,
                },
                new DatabaseParameter
                {
                    name = "@status",
                    value = OrderStatus.WAPPR,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "@itemid",
                    value = request.itemid,
                    type = DbType.Int64,
                }
            );

            this.changeWareHouseStatus(request.itemid, WarehouseStatus.ORDERED);

            await _rdb.KeyDeleteAsync("orders");
            return "";
        }

        public async Task<List<models.Order>> GetOrders(int userid)
        {
            var result = new List<models.Order>();
            if (!_rdb.KeyExists("orders"))
            {
                result = DatabaseCommands.QueryCommands<models.Order>(
                    _connection,
                    "SELECT * FROM \"order\" where userid = @userid",
                    new DatabaseParameter
                    {
                        name = "@id",
                        value = userid,
                        type = DbType.Int64,
                    }
                );
                await _rdb.StringSetAsync(
                    $"orders:userid:{userid}",
                    JsonSerializer.Serialize(result)
                );
            }
            else
            {
                result = JsonSerializer.Deserialize<List<models.Order>>(
                    await _rdb.StringGetAsync($"orders:userid:{userid}")!
                );
            }
            return result;
        }

        public async Task<object> GetOrder(int id, int userid)
        {
            var result = new models.Order();
            if (!_rdb.KeyExists($"orders:{id}:userid:{userid}"))
            {
                var order = DatabaseCommands.QueryCommands<models.Order>(
                    _connection,
                    "SELECT * FROM \"order\" WHERE orderid = @id and userid = @userid",
                    new DatabaseParameter
                    {
                        name = "@id",
                        value = id,
                        type = DbType.Int64,
                    },
                    new DatabaseParameter
                    {
                        name = "@userid",
                        value = userid,
                        type = DbType.Int64,
                    }
                );
                if (order.Count > 0)
                {
                    result = order[0];
                    await _rdb.StringSetAsync(
                        $"orders:{id}:userid:{userid}",
                        JsonSerializer.Serialize(result)
                    );
                    return result;
                }
            }
            else
            {
                result = JsonSerializer.Deserialize<models.Order>(
                    await _rdb.StringGetAsync($"orders:{id}:userid:{userid}")
                );
                return result;
            }

            return Results.Json(
                new { error = "Заказ с указанным id не найден" },
                statusCode: StatusCodes.Status404NotFound
            );
        }

        public async Task<object?> UpdateOrder(CreateOrderRequest request)
        {
            if (!OrderStatus.isValid(request.status))
            {
                return Results.Json(
                    new { error = "Указанный статус не существует" },
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            var newOrder = DatabaseCommands.QueryCommands<models.Order>(
                _connection,
                "UPDATE  \"order\" SET status = @status WHERE orderid = @id returning *",
                new DatabaseParameter
                {
                    name = "@status",
                    value = request.status,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "@id",
                    value = request.orderid,
                    type = DbType.Int64,
                }
            );

            if (request.status.Equals(OrderStatus.CANCELLED))
            {
                this.changeWareHouseStatus(request.itemid, WarehouseStatus.RETURNED);
            }

            await _rdb.KeyDeleteAsync("orders");

            if (newOrder.Count > 0)
            {
                await _rdb.KeyDeleteAsync($"orders:{request.orderid}:userid:{newOrder[0].userid}");
            }
            await _rdb.KeyDeleteAsync($"orders:{request.orderid}");
            return $"";
        }

        private async void changeWareHouseStatus(int itemid, string status)
        {
            DatabaseCommands.ExecuteCommands(
                _connection,
                "UPDATE warehouse SET status = @status WHERE itemid = @id",
                new DatabaseParameter
                {
                    name = "@status",
                    value = status,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "@id",
                    value = itemid,
                    type = DbType.Int64,
                }
            );

            await _rdb.KeyDeleteAsync("items");

            await _rdb.KeyDeleteAsync($"item:{itemid}");
        }
    }
}
