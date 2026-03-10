var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> apiService;

if (builder.ExecutionContext.IsPublishMode)
{
    // Production (azd): use external PostgreSQL connection string (e.g. Neon).
    // Set via: azd env set ConnectionStrings__efcorepostgresdb "<neon-connection-string>"
    var db = builder.AddConnectionString("efcorepostgresdb");
    apiService = builder.AddProject<Projects.EfCorePostgres_ApiService>("apiservice")
        .WithReference(db)
        .WithHttpHealthCheck("/health");
}
else
{
    // Local dev: provision a PostgreSQL container via Docker.
    var postgres = builder.AddPostgres("postgres").AddDatabase("efcorepostgresdb");
    apiService = builder.AddProject<Projects.EfCorePostgres_ApiService>("apiservice")
        .WithReference(postgres)
        .WaitFor(postgres)
        .WithHttpHealthCheck("/health");
}

builder.AddProject<Projects.EfCorePostgres_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
