using Api.Domain;
using Api.Services;
using Azure.Data.Tables;
using Bogus;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app){
        var endpoints = app.MapGroup("api")
                            .WithOpenApi();

        endpoints.MapGet("/employees", (TableServiceClient client) => GetAllEmployees(new AzStrorageTablesService(client)))
            .WithName("GetAllEmployees")
            .WithDescription("Get all employees from the table storage");

        endpoints.MapGet("/employeesaync", (TableServiceClient client) => GetAllEmployeesAsync(new AzStrorageTablesService(client)))
            .WithName("GetAllEmployeesAsync")
            .WithDescription("Get all employees from the table storage, using an Async method")
            .WithTags("Async", "Employees");

        endpoints.MapGet("/employees/{firstLetter}", (TableServiceClient client, string firstLetter) => GetEmployeesByFirstLetter(new AzStrorageTablesService(client), firstLetter))
            .WithName("GetEmployeesByFirstLetter")
            .WithDescription("Get all employees that the lastname starts with the given letter.");

        endpoints.MapGet("/employeesAsync/{firstLetter}", (TableServiceClient client, string firstLetter) => GetEmployeesByFirstLetterAsync(new AzStrorageTablesService(client), firstLetter))
            .WithName("GetEmployeesByFirstLetterAsync")
            .WithDescription("Get all employees that the lastname starts with the given letter. Using an Async method")
            .WithTags("Async", "Employees");

        endpoints.MapGet("/generate", (TableServiceClient client) => GenerateEmployees(new AzStrorageTablesService(client)))
            .WithName("generate")
            .WithDescription("Generate 5000 employees and save them to the table storage")
            .WithTags("Generate", "Employees");
    }

    private static IResult GetAllEmployees(AzStrorageTablesService service)
    {			   
        var employees = service.GetAllEmployee();
        return Results.Ok(employees);
    }

    private static async Task<IResult> GetAllEmployeesAsync(AzStrorageTablesService service)
    {
        var employees = await service.GetAllEmployeeAsync();
        return Results.Ok(employees);
    }

    private static IResult GetEmployeesByFirstLetter(AzStrorageTablesService service, string firstLetter)
    {
        var employees = service.GetEmployeeStartingBy(firstLetter);
        return Results.Ok(employees);
    }

    private static async Task<IResult> GetEmployeesByFirstLetterAsync(AzStrorageTablesService service, string firstLetter)
    {
        var employees = await service.GetEmployeeStartingByAsync(firstLetter);
        return Results.Ok(employees);
    }

    private static async Task<IResult> GenerateEmployees(AzStrorageTablesService service)
    {
        var EmployeeFaker = new Faker<EmployeeEntity>()
            .RuleFor(e => e.RowKey, f => f.Random.Guid().ToString())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.PartitionKey, (f,e) => e.LastName?.Substring(0,1).ToUpper() ?? String.Empty)
            .RuleFor(e => e.Email, (f,e) => f.Internet.Email(e.FirstName, e.LastName))
            .RuleFor(e => e.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(e => e.Address, f => f.Address.FullAddress());

        var employees = EmployeeFaker.Generate(5000);
        
        var result = await service.SaveEmployees(employees);

        return Results.Ok($"{employees.Count} employees generated");
    }
}
