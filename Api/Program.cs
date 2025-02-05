using Api;
using Azure.Data.Tables;
using Bogus;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddAzureTableClient("strTables");

var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/employees", async (TableServiceClient client) =>
{
    var service = new AzStrorageTablesService(client);
    var employees = await service.GetAllEmployee();
    return Results.Ok(employees);
})
.WithName("GetAllEmployees");


app.MapGet("/generate", async (TableServiceClient client) =>
{
    var EmployeeFaker = new Faker<EmployeeEntity>()
        .RuleFor(e => e.RowKey, f => f.Random.Guid().ToString())
        .RuleFor(e => e.FirstName, f => f.Person.FirstName)
        .RuleFor(e => e.LastName, f => f.Person.LastName)
        .RuleFor(e => e.PartitionKey, (f,e) => e.LastName.Substring(0,1).ToUpper())
        .RuleFor(e => e.Email, (f,e) => f.Internet.Email(e.FirstName, e.LastName))
        .RuleFor(e => e.PhoneNumber, f => f.Phone.PhoneNumber())
        .RuleFor(e => e.Address, f => f.Address.FullAddress());

    var employees = EmployeeFaker.Generate(5000);
    
    var service = new AzStrorageTablesService(client);
    var result = await service.SaveEmployees(employees);
    return Results.Ok(employees);
})
.WithName("generate");


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}





    //     .RuleFor(e => e.PartitionKey, f => "Employee")
    // .RuleFor(e => e.RowKey, f => f.Random.Guid().ToString())
    // .RuleFor(e => e.FirstName, f => f.Person.FirstName)
    // .RuleFor(e => e.LastName, f => f.Person.LastName)
    // .RuleFor(e => e.Email, f => f.Internet.Email)