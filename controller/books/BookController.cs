using System.Data;
using System.Text.Json;
using database;
using dtos;
using models;
using StackExchange.Redis;

namespace controller
{
    public class BookController
    {
        private IDbConnection _connection;
        private IDatabase _rdb;

        public BookController(IDbConnection connection, IDatabase rdb)
        {
            _connection = connection;
            _rdb = rdb;
        }

        public async Task<string> CreateBook(CreateBookRequest request)
        {
            DatabaseCommands.ExecuteCommands(
                _connection,
                "insert into book(name) values (@name)",
                new DatabaseParameter
                {
                    name = "@name",
                    value = request.name,
                    type = DbType.String,
                }
            );

            await _rdb.KeyDeleteAsync("books");
            return "";
        }

        public async Task<List<Book>> GetBooks()
        {
            var result = new List<Book>();
            if (!_rdb.KeyExists("books"))
            {
                result = DatabaseCommands.QueryCommands<Book>(_connection, "SELECT * FROM book");
                await _rdb.StringSetAsync("books", JsonSerializer.Serialize(result));
            }
            else
            {
                result = JsonSerializer.Deserialize<List<Book>>(
                    await _rdb.StringGetAsync("books")!
                );
            }
            return result;
        }

        public async Task<string> UpdateBook(CreateBookRequest request)
        {
            var book = new Book { name = request.name, bookid = request.bookid };

            int rowsAffected = DatabaseCommands.ExecuteCommands(
                _connection,
                "UPDATE book SET name = @name WHERE bookid = @id",
                new DatabaseParameter
                {
                    name = "@name",
                    value = request.name,
                    type = DbType.String,
                },
                new DatabaseParameter
                {
                    name = "@id",
                    value = request.bookid,
                    type = DbType.Int32,
                }
            );

            await _rdb.KeyDeleteAsync("books");
            return $"Обновлено записей: {rowsAffected}";
        }

        public async Task<string> DeleteBook(int bookid)
        {
            int rowsAffected = DatabaseCommands.ExecuteCommands(
                _connection,
                "DELETE FROM book WHERE bookid = @id",
                new DatabaseParameter
                {
                    name = "@id",
                    value = bookid,
                    type = DbType.Int32,
                }
            );
            await _rdb.KeyDeleteAsync("books");
            return $"Удалено записей: {rowsAffected}";
        }
    }
}
