# EfCorePostgresTest

A .NET 10 Aspire solution with a Blazor Web App frontend (SSR + WebAssembly) and a Web API backend using EF Core + PostgreSQL.

## Solution structure

| Project | Description |
|---------|-------------|
| `EfCorePostgres.AppHost` | .NET Aspire orchestrator |
| `EfCorePostgres.ApiService` | Minimal API with EF Core + PostgreSQL |
| `EfCorePostgres.Web` | Blazor server host (SSR + WASM, BFF proxy) |
| `EfCorePostgres.Web.Client` | Blazor WebAssembly client |
| `EfCorePostgres.ServiceDefaults` | Shared Aspire service defaults |

## Local development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling): `dotnet workload install aspire`
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Aspire provisions a local PostgreSQL container)

### Run

```bash
cd EfCorePostgres
dotnet run --project EfCorePostgres.AppHost
```

The Aspire dashboard opens at `https://localhost:15888`. PostgreSQL migrations run automatically on startup.

Client-side libraries (Bootstrap) are managed by [libman](https://learn.microsoft.com/aspnet/core/client-side/libman/) and restore automatically as part of the build via the `Microsoft.Web.LibraryManager.Build` package — no manual step needed.

## Deployment: Azure Container Apps + Neon PostgreSQL

The app deploys as two Azure Container Apps (Web + ApiService) with a free [Neon](https://neon.tech) PostgreSQL database. Both Container Apps scale to zero when idle.

**Estimated monthly cost: ~$5-8** (Azure Container Registry ~$5 is the main fixed cost; Container Apps themselves are free under light traffic)

### Prerequisites

```bash
# Azure Developer CLI
brew tap azure/azd && brew install azd

# Aspire workload (if not already installed)
dotnet workload install aspire

# Log into Azure
azd auth login
```

### First deploy

1. **Create a free Neon database** at [neon.tech](https://neon.tech) and copy the connection string.

2. **Initialize the azd environment** (prompts for Azure subscription and region):

   ```bash
   azd env new efcorepostgres
   ```

3. **Set the Neon connection string**:

   ```bash
   azd env set ConnectionStrings__efcorepostgresdb "Host=ep-xxx.neon.tech;Database=neondb;Username=xxx;Password=xxx;Ssl Mode=Require"
   ```

4. **Provision and deploy** (~10 min on first run):

   ```bash
   azd up
   ```

   This creates: Resource Group, Container Apps Environment, Container Registry, Log Analytics workspace, and two Container Apps. Database migrations run automatically on first cold start.

### CI/CD with GitHub Actions

After the first `azd up`, run once to set up automatic deployments:

```bash
azd pipeline config
```

This creates `.github/workflows/azure-dev.yml`, an Azure service principal, and sets all required secrets in the GitHub repo. Every push to `main` then runs `azd provision` + `azd deploy` automatically.

Add your Neon connection string as a GitHub secret named `NEON_CONNECTION_STRING` and reference it in the generated workflow if you want CI/CD to manage it.

### Subsequent deploys

```bash
azd deploy
```

### How the Neon connection string works

In local dev, `AppHost.cs` provisions a PostgreSQL Docker container automatically. When `azd` generates the deployment manifest (`IsPublishMode = true`), it switches to `AddConnectionString("efcorepostgresdb")` instead — so no Azure PostgreSQL is provisioned and your Neon connection string is injected directly into the ApiService Container App as an environment variable.
