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


app.MapGet("/employees", (TableServiceClient client) =>
{
    var service = new AzStrorageTablesService(client);
    var employees = service.GetAllEmployee();
    return Results.Ok(employees);
})
.WithName("GetAllEmployees");


app.MapGet("/employeesaync", async (TableServiceClient client) =>
{
    var service = new AzStrorageTablesService(client);
    var employees = await service.GetAllEmployeeAsync();
    return Results.Ok(employees);
})
.WithName("GetAllEmployeesAsync");



app.MapGet("/employees/{firstLetter}", (TableServiceClient client, string firstLetter) =>
{
    var service = new AzStrorageTablesService(client);
    var employees = service.GetEmployeeStartingBy(firstLetter);
    return Results.Ok(employees);
})
.WithName("GetEmployeesByFirstLetter");


app.MapGet("/employeesAsync/{firstLetter}", async (TableServiceClient client, string firstLetter) =>
{
    var service = new AzStrorageTablesService(client);
    var employees = await service.GetEmployeeStartingByAsync(firstLetter);
    return Results.Ok(employees);
})
.WithName("GetEmployeesByFirstLetterAsync");


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

    return Results.Ok($"{employees.Count} employees generated");
})
.WithName("generate");


app.Run();
