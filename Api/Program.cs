
using Api.Repositories;
using Api.Services;
using FluentValidation;
using MongoDB.Driver;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration["MongoDb:ConnectionString"] ??
                               throw new InvalidOperationException("mongodb connection string is null");

        var dbName = builder.Configuration["MongoDb:DatabaseName"] ??
                     throw new InvalidOperationException("mongodb database name is null");

        var mongoClient = new MongoClient(connectionString);

        builder.Services.AddScoped<IMongoClient>(_ => mongoClient);
        builder.Services.AddScoped(_ => mongoClient.GetDatabase(dbName));

        builder.Services.AddScoped<ITokenValidatorService, TokenValidatorService>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
