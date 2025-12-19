using StackExchange.Redis;

namespace middleware
{
    class BlackListCheckMiddleware : IEndpointFilter
    {
        private IDatabase _rdb;

        public BlackListCheckMiddleware(ref IDatabase rdb)
        {
            _rdb = rdb;
        }

        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next
        )
        {
            var currentId = context.HttpContext.Items["CurrentId"].ToString().Split(' ')[1];
            if (!_rdb.KeyExists($"usersblacklist:{currentId}"))
            {
                return await next(context);
            }

            return Results.Json(
                new { error = "Невалидный токен" },
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
    }
}
