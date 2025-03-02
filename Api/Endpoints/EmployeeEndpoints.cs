using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Azure.Data.Tables;
using Bogus;

namespace Api.Endpoints;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("api")
                            .WithOpenApi();

        MapGetAllEmployees(endpoints);
        MapGetAllEmployeesAsync(endpoints);
        MapGetEmployeesByFirstLetter(endpoints);
        MapGetEmployeesGroupByFirstLetterFirstNameAsync(endpoints);
        MapGenerateEmployees(endpoints);
    }

    private static void MapGetAllEmployees(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/GetEmployeesAsync", (TableServiceClient client) => GetAllEmployeeAsync(new AzStrorageTablesService(client)))
            .WithName("GetAllEmployees")
            .WithDescription("Get all employees from the table storage");
    }

    private static void MapGetAllEmployeesAsync(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/GetEmployeeByCountryAsync", (TableServiceClient client) => GetEmployeeByCountryAsync(new AzStrorageTablesService(client)))
            .WithName("GetAllEmployeesAsync")
            .WithDescription("Get all the country with their employees counts, using an Async method")
            .WithTags("Async", "Country");
    }

    private static void MapGetEmployeesByFirstLetter(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employees/{firstLetter}", (TableServiceClient client, string firstLetter) => GetEmployeesByFirstLetter(new AzStrorageTablesService(client), firstLetter))
            .WithName("GetEmployeesByFirstLetter")
            .WithDescription("Get all employees that the lastname starts with the given letter.");
    }

    private static void MapGetEmployeesGroupByFirstLetterFirstNameAsync(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employeesGroupByFirstLetterFirstNameAsync", (TableServiceClient client) => GetEmployeesGroupByFirstLetterFirstNameAsync(new AzStrorageTablesService(client)))
            .WithName("GetEmployeesGroupByFirstLetterFirstNameAsync")
            .WithDescription("Get all employees grouped by the first letter of their first name, using an Async method")
            .WithTags("Async", "Employees");
    }

    private static void MapGenerateEmployees(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/generate/{quantity?}", (TableServiceClient client, int? quantity) => GenerateEmployees(new AzStrorageTablesService(client), quantity))
            .WithName("GenerateEmployees")
            .WithDescription("Generate 5000 employees and save them to the table storage")
            .WithTags("Generate", "Employees");
    }

    private static async Task<IResult> GetAllEmployeeAsync(AzStrorageTablesService service)
    {
        try
        {
            var employees = await service.GetAllEmployeeAsync();
            return Results.Ok(employees);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GetEmployeeByCountryAsync(AzStrorageTablesService service)
    {
        try
        {
            var countryEmployeeCount = await service.GetEmployeeByCountryAsync();
            return Results.Ok(countryEmployeeCount);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static IResult GetEmployeesByFirstLetter(AzStrorageTablesService service, string firstLetter)
    {
        try
        {
            var employees = service.GetEmployeeStartingBy(firstLetter);
            return Results.Ok(employees);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GetEmployeesGroupByFirstLetterFirstNameAsync(AzStrorageTablesService service)
    {
        try
        {
            var employees = await service.GetEmployeesGroupByFirstLetterFirstNameAsync();
            return Results.Ok(employees);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GenerateEmployees(AzStrorageTablesService service, int? quantity)
    {
        try
        {
            var EmployeeFaker = new Faker<EmployeeEntity>()
            .RuleFor(e => e.RowKey, f => f.Random.Guid().ToString())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.PartitionKey, (f, e) => e.LastName?.Substring(0, 1).ToUpper() ?? String.Empty)
            .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.FirstName, e.LastName))
            .RuleFor(e => e.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(e => e.Address, f => f.Address.FullAddress());

            int qty = quantity ?? 500;
            var employees = EmployeeFaker.Generate(qty);

            var result = await service.SaveEmployeesAsync(employees);

            return Results.Ok($"{employees.Count} employees generated");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
