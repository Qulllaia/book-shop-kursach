using System.Data;
using StackExchange.Redis;

namespace router
{
    public class Router
    {
        public static void RegisterRouter(
            ref WebApplication app,
            ref IDbConnection connection,
            ref IDatabase rdb
        )
        {
            router.AuthRouter.RegisterAuthRouter(ref app, ref connection, ref rdb);
            router.BookRouter.RegisterBookRouter(ref app, ref connection, ref rdb);
            router.UserRouter.RegisterUserRouter(ref app, ref connection, ref rdb);
            router.WarehouseRouter.RegisterWarehouseRouter(ref app, ref connection, ref rdb);
            router.OrderRouter.RegisterOrderRouter(ref app, ref connection, ref rdb);
        }
    }
}
