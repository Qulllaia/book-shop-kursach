using System.Data;
using System.Text.Json;
using database;
using dtos;
using models;
using StackExchange.Redis;

namespace controller
{
    public class WarehouseController
    {
        private IDbConnection _connection;
        private IDatabase _rdb;

        public WarehouseController(ref IDbConnection connection, ref IDatabase rdb)
        {
            _connection = connection;
            _rdb = rdb;
        }

        public async Task<string> CreateItem(CreateItemRequest request)
        {
            DatabaseCommands.ExecuteCommands(
                _connection,
                "insert into warehouse(bookid, status) values (@bookid, @status)",
                new DatabaseParameter
                {
                    name = "@bookid",
                    value = request.bookid,
                    type = DbType.Int64,
                },
                new DatabaseParameter
                {
                    name = "@status",
                    value = WarehouseStatus.STORED,
                    type = DbType.String,
                }
            );

            await _rdb.KeyDeleteAsync("items");
            return "";
        }

        public async Task<List<Item>> GetItems()
        {
            var result = new List<Item>();
            if (!_rdb.KeyExists("items"))
            {
                result = DatabaseCommands.QueryCommands<Item>(
                    _connection,
                    "SELECT * FROM  warehouse"
                );
                await _rdb.StringSetAsync("items", JsonSerializer.Serialize(result));
            }
            else
            {
                result = JsonSerializer.Deserialize<List<Item>>(
                    await _rdb.StringGetAsync("items")!
                );
            }
            return result;
        }

        public async Task<object> GetItem(int id)
        {
            var result = new Item();
            if (!_rdb.KeyExists($"items:{id}"))
            {
                var item = DatabaseCommands.QueryCommands<Item>(
                    _connection,
                    "SELECT * FROM warehouse WHERE itemid = @id",
                    new DatabaseParameter
                    {
                        name = "@id",
                        value = id,
                        type = DbType.Int64,
                    }
                );
                if (item.Count > 0)
                {
                    result = item[0];
                    await _rdb.StringSetAsync($"items:{id}", JsonSerializer.Serialize(result));
                    return result;
                }
            }
            else
            {
                result = JsonSerializer.Deserialize<Item>(await _rdb.StringGetAsync($"items:{id}"));
                return result;
            }

            return Results.Json(
                new { error = "Предмет на складе с указанным id не найден" },
                statusCode: StatusCodes.Status404NotFound
            );
        }

        public async Task<string> UpdateItem(CreateItemRequest request)
        {
            int rowsAffected = DatabaseCommands.ExecuteCommands(
                _connection,
                "UPDATE warehouse SET status = @status WHERE itemid = @id",
                new DatabaseParameter
                {
                    name = "@status",
                    value = request.status,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "@id",
                    value = request.itemid,
                    type = DbType.Int64,
                }
            );

            await _rdb.KeyDeleteAsync("items");

            await _rdb.KeyDeleteAsync($"items:{request.itemid}");
            return $"Обновлено записей: {rowsAffected}";
        }

        public async Task<string> DeleteItem(int itemid)
        {
            int rowsAffected = DatabaseCommands.ExecuteCommands(
                _connection,
                "DELETE FROM warehouse WHERE itemid = @id",
                new DatabaseParameter
                {
                    name = "@id",
                    value = itemid,
                    type = DbType.Int64,
                }
            );
            await _rdb.KeyDeleteAsync("items");

            await _rdb.KeyDeleteAsync($"item:{itemid}");

            return $"Удалено записей: {rowsAffected}";
        }
    }
}
