using Bogus;
using LoadBalancer.Core;
using LoadBalancer.Microservice;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);


//for using IOptions in DI container
//builder.Services.Configure<MessageProcessConfiguration>(builder.Configuration.GetSection("MessageProcessConfiguration"));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICacheService, ConfigCacheService>();

builder.Services.AddHealthChecks()
    .AddCheck("service_readiness_check", () => HealthCheckResult.Healthy("PayrollCommitRoutingService Healthy"), tags: new[] { "service_readiness_check" })
    .AddCheck("service_liveness_check", () => HealthCheckResult.Healthy("PayrollCommitRoutingService Healthy"), tags: new[] { "service_liveness_check" });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



var faker = new Faker();

var vehicles = Enumerable.Range(1, 100)
                         .Select(num => new Vehicle { Id = num, Name = faker.Vehicle.Model(), Manufacture = faker.Vehicle.Manufacturer(), Type = faker.Vehicle.Type() })
                         .ToList();

app.MapGet("/allvehicles", () =>
{
    return vehicles;
});

app.MapGet("/vehicles", (ICacheService cacheService) =>
{
    var cachedVehicles = cacheService.GetValue<List<Vehicle>>("ALL");
    if (cachedVehicles is not null)
    {
        return $"Return from cache: {JsonConvert.SerializeObject(cachedVehicles)}";
    }
    else
        cacheService.SetValue("ALL", vehicles);

    return $"Return from DB: {JsonConvert.SerializeObject(vehicles)}";
});

app.MapGet("/vehicles/{id}", (int id, ICacheService cacheService) =>
{
    //if (id == 2)
    //    throw new TimeoutException("Timeout exception happened");

    Thread.Sleep(id * 10000);

    var cachedVehicles = cacheService.GetValue<Vehicle>(id.ToString());
    if (cachedVehicles is null)
    {
        cachedVehicles = vehicles.FirstOrDefault(p => p.Id == id);
        cacheService.SetValue(id.ToString(), cachedVehicles);

        return $"Return from DB: {JsonConvert.SerializeObject(cachedVehicles)}";

    }
    else
        return $"Return from cache: {JsonConvert.SerializeObject(cachedVehicles)}";

});

app.Run();
