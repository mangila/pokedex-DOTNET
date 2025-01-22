using Microsoft.AspNetCore.Mvc;
using pokedex_api.Config;
using pokedex_api.ExceptionHandler;
using pokedex_shared.Config;
using pokedex_shared.Option;
using pokedex_shared.Service;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure API Controllers
builder.Services.AddControllers();
// Configure MongoDB
builder.Services.AddMongoDb(builder.Configuration.GetSection(nameof(MongoDbOption)));
// Configure Redis
builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    var connection = builder.Configuration.GetConnectionString("Redis");
    redisOptions.Configuration = connection;
});
// Add Http Client
builder.Services.AddHttpClient();
// Configure Request timeouts
builder.Services.AddRequestTimeouts(HttpRequestConfig.ConfigureRequestTimeout);
// Configure Rate limiting
builder.Services.AddRateLimiter(HttpRateLimiterConfig.ConfigureRateLimiter);
// Configure Default [ApiController] behaviour
builder.Services.Configure<ApiBehaviorOptions>(ApiBehaviourConfig.ConfigureApiBehaviourOptions);
// Load Serilog from configuration
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
// Add Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// Add Singleton Services
builder.Services.AddSingleton<PokemonService>();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddSingleton<DatasourceService>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseRequestTimeouts();
app.UseRateLimiter();
app.MapControllers();
app.Run();