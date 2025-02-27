using Api.Models;
using Azure.Data.Tables;

namespace Api.Services;

/// <summary>
/// Service for interacting with Azure Table Storage for Employee entities.
/// </summary>
public class AzStrorageTablesService
{
    private readonly TableClient _employeeTableClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzStrorageTablesService"/> class.
    /// </summary>
    /// <param name="client">The TableServiceClient to interact with Azure Table Storage.</param>
    public AzStrorageTablesService(TableServiceClient client)
    {
        client.CreateTableIfNotExists("Employee");
        _employeeTableClient = client.GetTableClient("Employee");
    }

    /// <summary>
    /// Retrieves all employee entities from the table.
    /// </summary>
    /// <returns>A list of <see cref="EmployeeEntity"/>.</returns>
    public async Task<List<EmployeeEntity>> GetAllEmployeeAsync()
    {
        var lstEmployees = new List<EmployeeEntity>();
        var queryResult = _employeeTableClient.QueryAsync<EmployeeEntity>();

        await foreach (var emp in queryResult.AsPages().ConfigureAwait(false))
        {
            lstEmployees.AddRange(emp.Values);
        }

        return lstEmployees;
    }

    /// <summary>
    /// Retrieves a dictionary of employee counts grouped by country.
    /// </summary>
    /// <returns>A dictionary where the key is the country and the value is the count of employees.</returns>
    public async Task<Dictionary<string, int>> GetEmployeeByCountryAsync()
    {
        var countryCount = new Dictionary<string, int>();
        var queryResult = _employeeTableClient.QueryAsync<EmployeeEntity>();

        await foreach (var emp in queryResult.AsPages().ConfigureAwait(false))
        {
            foreach (var item in emp.Values)
            {
                string country = item.Address?.Split(',').LastOrDefault()?.Trim() ?? "Unknown";
                if (countryCount.ContainsKey(country))
                {
                    countryCount[country]++;
                }
                else
                {
                    countryCount[country] = 1;
                }
            }
        }

        return countryCount;
    }

    /// <summary>
    /// Retrieves a list of employees whose partition key starts with the specified letter.
    /// </summary>
    /// <param name="firstLetter">The first letter of the partition key.</param>
    /// <returns>A list of <see cref="EmployeeEntity"/>.</returns>
    public List<EmployeeEntity> GetEmployeeStartingBy(string firstLetter)
    {
        var queryResult = _employeeTableClient.Query<EmployeeEntity>(e => e.PartitionKey == firstLetter);
        return queryResult.ToList();
    }

    /// <summary>
    /// Retrieves employees grouped by the first letter of their first name.
    /// </summary>
    /// <returns>A dictionary where the key is the first letter and the value is a list of <see cref="EmployeeEntity"/>.</returns>
    public async Task<Dictionary<char, List<EmployeeEntity>>> GetEmployeesGroupByFirstLetterFirstNameAsync()
    {
        var lstEmployees = new List<EmployeeEntity>();
        var queryResult = _employeeTableClient.QueryAsync<EmployeeEntity>();

        await foreach (var emp in queryResult.AsPages().ConfigureAwait(false))
        {
            lstEmployees.AddRange(emp.Values);
        }

        var groupedEmployees = lstEmployees
            .GroupBy(e => e.FirstName![0])
            .ToDictionary(g => g.Key, g => g.ToList());

        return groupedEmployees;
    }

    /// <summary>
    /// Saves a list of employee entities to the table.
    /// </summary>
    /// <param name="employees">The list of <see cref="EmployeeEntity"/> to save.</param>
    /// <returns>A boolean indicating whether the operation was successful.</returns>
    public async Task<bool> SaveEmployeesAsync(List<EmployeeEntity> employees)
    {
        foreach (var emp in employees)
        {
            await _employeeTableClient.AddEntityAsync(emp).ConfigureAwait(false);
        }
        return true;
    }
}
