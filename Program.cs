using database;
using DotNetEnv;
using Microsoft.OpenApi;
using StackExchange.Redis;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();
var dbconn = DatabaseConnection.FromEnvironment();
var connection = dbconn.InitDatabase();

var redis = ConnectionMultiplexer.Connect("localhost:6379");

var rdb = redis.GetDatabase();

router.Router.RegisterRouter(app, connection, rdb);
app.Run();
