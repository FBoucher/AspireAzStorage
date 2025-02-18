using Api.Domain;
using Azure.Data.Tables;

namespace Api.Services;

public class AzStrorageTablesService
{
	private readonly TableClient _employeeTableClient;

    public AzStrorageTablesService(TableServiceClient client)
    {
        client.CreateTableIfNotExists("Employee");
        _employeeTableClient = client.GetTableClient("Employee");
    }

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

	public List<EmployeeEntity> GetEmployeeStartingBy(string firstLetter)
	{
		var queryResult = _employeeTableClient.Query<EmployeeEntity>(e => e.PartitionKey == firstLetter);
		return queryResult.ToList();
	}


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



	public async Task<bool> SaveEmployeesAsync(List<EmployeeEntity> employees)
	{
		foreach (var emp in employees)
		{
			await _employeeTableClient.AddEntityAsync(emp).ConfigureAwait(false);
		}
		return true;
	}

}
