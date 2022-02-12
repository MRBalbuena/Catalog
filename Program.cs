using System.Net.Mime;
using System.Text.Json;
using Catalog.Respositories;
using Catalog.Settings;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers(option => option.SuppressAsyncSuffixInActionNames = false);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

builder.Services.AddSingleton<IMongoClient>(serviceProvider => {    
    return new MongoClient(mongoDbSettings.ConnectionString);
});

//builder.Services.AddSingleton<IItemsRepository, InMemItemsRepository>();
builder.Services.AddSingleton<IItemsRepository, MongoDbItemsRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongoDbSettings.ConnectionString, 
        name: "mongodb", 
        timeout: TimeSpan.FromSeconds(3),
        tags: new[] {"ready"});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseEndpoints(endpoints => {
    endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = (check) => check.Tags.Contains("ready"), // will only include hc that have been tag as ready        
        ResponseWriter = async(context, report) =>
        {
            var result = JsonSerializer.Serialize(
                new{
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        exception = entry.Value.Exception != null? entry.Value.Exception.Message : "none",
                        duration = entry.Value.Duration.ToString()
                    })
                }
            );

            context.Response.ContentType = MediaTypeNames.Application.Json; // to display in nice json format
            await context.Response.WriteAsync(result);
        }
    });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions{
        Predicate = (_) => false // just to response to a ping
    });
});

app.UseHttpsRedirection(); // Doesn't seem to redirect

app.UseAuthorization();

app.MapControllers();

app.Run();
