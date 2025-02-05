using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var azStorage = builder.AddAzureStorage("azstorage");

if (builder.Environment.IsDevelopment())
{
    azStorage.RunAsEmulator();
}

var strTables = azStorage.AddTables("strTables");

builder.AddProject<Projects.Api>("api")
		.WithExternalHttpEndpoints()
		.WithReference(strTables)
		.WaitFor(strTables);

builder.Build().Run();
