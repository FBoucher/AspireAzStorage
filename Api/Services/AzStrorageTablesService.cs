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


	public async Task<List<EmployeeEntity>> GetAllEmployeeAsync()
	{
		TableClient tblEmployees = GetEmployeeTable();
		var lstEmployees = new List<EmployeeEntity>();
	
		var queryResult = tblEmployees.QueryAsync<EmployeeEntity>(); // maxPerPage: 1000 is the default value

		await foreach (var emp in queryResult.AsPages())
		{
			foreach (var item in emp.Values)
			{
				lstEmployees.Add(item);
			}
		}

		return lstEmployees;
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


	public async Task<List<EmployeeEntity>> GetEmployeeStartingByAsync(string firstLetter)
	{
		TableClient tblEmployees = GetEmployeeTable();
		var lstEmployees = new List<EmployeeEntity>();
	
		var queryResult = tblEmployees.QueryAsync<EmployeeEntity>(e => e.PartitionKey == firstLetter);

		await foreach (var emp in queryResult.AsPages())
		{
			foreach (var item in emp.Values)
			{
				lstEmployees.Add(item);
			}
		}
		return lstEmployees;
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
