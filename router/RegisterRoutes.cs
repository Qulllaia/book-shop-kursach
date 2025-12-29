using System.Data;
using StackExchange.Redis;

namespace router
{
    public class Router
    {
        public static void RegisterRouter(
            WebApplication app,
            IDbConnection connection,
            IDatabase rdb
        )
        {
            router.AuthRouter.RegisterAuthRouter(app, connection, rdb);
            router.BookRouter.RegisterBookRouter(app, connection, rdb);
            router.UserRouter.RegisterUserRouter(app, connection, rdb);
            router.WarehouseRouter.RegisterWarehouseRouter(app, connection, rdb);
            router.OrderRouter.RegisterOrderRouter(app, connection, rdb);
        }
    }
}
