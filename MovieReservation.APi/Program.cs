using StackExchange.Redis;

using MovieReservation.APi.Extentions;
using System.Threading.Tasks;

namespace MovieReservation.APi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        builder.Services.AddServiceDefaults(builder.Configuration);        // Add services to the container.
        builder.AddRedisClient("redis");

        //builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        //builder.Services.AddOpenApi();

        var app = builder.Build();
        await app.InitializeDatabaseAsync();

        await app.ConfigureMiddlewareAsync();

        // Configure the HTTP request pipeline.




        app.Run();
    }
}
