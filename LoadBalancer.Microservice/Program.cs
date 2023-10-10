using Bogus;
using LoadBalancer.Microservice;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICacheService, ConfigCacheService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var faker = new Faker();
var vehicles = Enumerable.Range(1, 100)
                         .Select(num => new { Id = num, Name = faker.Vehicle.Model(), Manufacture = faker.Vehicle.Manufacturer(), Type = faker.Vehicle.Type() })
                         .ToArray();

app.MapGet("/vehicles", (string? filter) => vehicles.Where(p => string.IsNullOrWhiteSpace(filter) || p.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase)));

app.MapGet("/vehicles/{id}", (int id) =>
{
    var vehicle = vehicles.FirstOrDefault(p => p.Id == id);

    return vehicle is not null ? Results.Ok(vehicle) : Results.NotFound();
});

app.Run();
