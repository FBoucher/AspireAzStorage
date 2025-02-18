using Api.Domain;
using Azure.Data.Tables;

namespace Api.Services;

public class AzStrorageTablesService(TableServiceClient client)
{
	private TableClient GetEmployeeTable()
	{
		client.CreateTableIfNotExists("Employee");
		TableClient table = client.GetTableClient("Employee");
		return table;
	}

	public List<EmployeeEntity> GetAllEmployee()
	{
		TableClient tblEmployees = GetEmployeeTable();
		var lstEmployees = new List<EmployeeEntity>();
	
		var queryResult = tblEmployees.Query<EmployeeEntity>();

		foreach (var emp in queryResult)
		{
			lstEmployees.Add(emp);
		} 

		return lstEmployees;
	}


public async Task<Dictionary<string, int>> GetEmployeeByCountryAsync()
{
	TableClient tblEmployees = GetEmployeeTable();
	var countryCount = new Dictionary<string, int>();
	
	var queryResult = tblEmployees.QueryAsync<EmployeeEntity>();

	await foreach (var emp in queryResult.AsPages())
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
		TableClient tblEmployees = GetEmployeeTable();
		var lstEmployees = new List<EmployeeEntity>();
	
		var queryResult = tblEmployees.Query<EmployeeEntity>(e => e.PartitionKey == firstLetter);

		foreach (var emp in queryResult)
		{
			lstEmployees.Add(emp);
		} 
		return lstEmployees;
	}


	public async Task<Dictionary<char, List<EmployeeEntity>>> GetEmployeesGroupByFirstLetterFirstNameAsync()
	{
		TableClient tblEmployees = GetEmployeeTable();
		var lstEmployees = new List<EmployeeEntity>();

		var queryResult = tblEmployees.QueryAsync<EmployeeEntity>();

		await foreach (var emp in queryResult.AsPages())
		{
			foreach (var item in emp.Values)
			{
				lstEmployees.Add(item);
			}
		}

		var groupedEmployees = lstEmployees
			.GroupBy(e => e.FirstName![0])
			.ToDictionary(g => g.Key, g => g.ToList());

		return groupedEmployees;
	}



	public async Task<bool> SaveEmployees(List<EmployeeEntity> employees)
	{
		TableClient tblEmployees = GetEmployeeTable();
		foreach (var emp in employees)
		{
			await tblEmployees.AddEntityAsync(emp);
		}
		return true;
	}

}
