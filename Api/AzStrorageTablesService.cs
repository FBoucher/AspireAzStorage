using System;
using Azure.Data.Tables;

namespace Api;

public class AzStrorageTablesService
{
	private TableServiceClient client;

	public AzStrorageTablesService(TableServiceClient client)
	{
		this.client = client;
	}

	private TableClient GetEmployeeTable()
	{
		client.CreateTableIfNotExists("Employee");
		TableClient table = client.GetTableClient("Employee");
		return table;
	}

	public async Task<List<EmployeeEntity>> GetAllEmployee()
	{
		TableClient tblEmployees = GetEmployeeTable();
		var lstEmployees = new List<EmployeeEntity>();
	
		var queryResult = tblEmployees.Query<EmployeeEntity>(e => e.RowKey != "KEY");

		foreach (var emp in queryResult)
		{
			lstEmployees.Add(emp);
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
