# Loan Transfer

Loan ownership transfer pipeline. Raw loan records are submitted from a React UI to an ASP.NET Core Web API. The API validates them and publishes one message per loan to an **Azure Service Bus queue (hosted in Azure, not local)**. An Azure Function picks up each message, works out the ownership periods and writes them to SQL Server. The UI then reads the transformed results back through the API.

```
React (Vite) ──POST /api/loan-transfers──▶ Web API ──▶ Azure Service Bus (cloud)
     ▲                                      │                 │ loantransfer-queue
     └──GET /api/transformed-loans──────────┤                 ▼
                                            └──── SQL Server ◀── Azure Function
```

## Repository layout

| Path | Description |
|---|---|
| `frontend/` | React 19 + Vite + Tailwind UI |
| `api/LoanProcessor.Api` | ASP.NET Core 8 Web API (Swagger, validation, Service Bus publisher) |
| `api/LoanProcessor.Function` | Azure Functions v4 (isolated worker) with a Service Bus trigger |
| `api/LoanProcessor.Data` | EF Core DbContext, entities and migrations (shared) |
| `api/LoanProcessor.Messaging` | Message contract shared by the API and the Function |
| `api/*.Tests` | xUnit tests |
| `data/raw-loan-data.json` | Sample input batch |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (`npm i -g azure-functions-core-tools@4`)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) for the Functions host storage (`npm i -g azurite`)
- SQL Server reachable at `localhost` with Windows authentication (SQL Server Developer/Express or LocalDB; update the connection strings if yours is different)
- Access to the **Azure Service Bus namespace in the Azure Portal** (`svbus-loantransfer`)

## Azure Service Bus (cloud)

Service Bus is **not emulated locally**. Both the API and the Function connect to the namespace in Azure.

1. In the Azure Portal, open **Service Bus namespace → `svbus-loantransfer`**.
2. Make sure a queue named **`loantransfer-queue`** exists (under **Entities → Queues**).
3. Copy the connection string from **Settings → Shared access policies → RootManageSharedAccessKey → Primary Connection String**.
   (For anything other than local development, create a policy that only has Send rights for the API and Listen rights for the Function.)
4. Paste it into the config (see below). Don't commit it to git.

## Configuration

**Web API**: set the Service Bus connection string with user secrets (it's intentionally left empty in `appsettings.json`):

```bash
cd api/LoanProcessor.Api
dotnet user-secrets set "ServiceBus:ConnectionString" "<service-bus-connection-string>"
```

| Key | Default |
|---|---|
| `ServiceBus:ConnectionString` | *(required, set via user secrets)* |
| `ServiceBus:QueueName` | `loantransfer-queue` |
| `ConnectionStrings:LoanProcessorDb` | `Server=localhost;Database=LoanProcessorDb;Trusted_Connection=True;TrustServerCertificate=True;` |
| `Cors:AllowedOrigins` | `http://localhost:5173`, `http://localhost:5180` |

**Azure Function**: `api/LoanProcessor.Function/local.settings.json` (git-ignored):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "<service-bus-connection-string>",
    "ServiceBusQueueName": "loantransfer-queue",
    "SqlConnectionString": "Server=localhost;Database=LoanProcessorDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**React app**: `frontend/.env.development`

```
VITE_API_BASE_URL=https://localhost:7224/api
```

## Running locally

Start the services in this order, each in its own terminal.

### 1. Azurite (Functions storage)

```bash
azurite --silent --location ./.azurite
```

### 2. Web API

```bash
cd api/LoanProcessor.Api
dotnet dev-certs https --trust   # first time only
dotnet run --launch-profile https
```

- Runs at `https://localhost:7224` (and `http://localhost:5018`)
- Swagger UI: https://localhost:7224/swagger
- In `Development` it **applies EF Core migrations and seeds the database** when it starts, so run it before the Function.

### 3. Azure Function

```bash
cd api/LoanProcessor.Function
func start --port 7258
```

The `LoanTransferProcessor` function listens on `loantransfer-queue` in Azure. You should see `Processing N ownership record(s) for loan ...` in the console when messages arrive.

### 4. React app

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:5173.

> **Visual Studio:** open `api/LoanProcessor.sln`, right-click the solution → **Configure Startup Projects → Multiple startup projects**, and set both `LoanProcessor.Api` (https profile) and `LoanProcessor.Function` to **Start**.

## Trying it out

1. Submit a batch from the UI, or post the sample file:
   ```bash
   curl -k -X POST https://localhost:7224/api/loan-transfers \
     -H "Content-Type: application/json" \
     --data @data/raw-loan-data.json
   ```
2. Watch the Function console process the messages. You can also check **Service Bus → Queues → loantransfer-queue → Service Bus Explorer** in the Azure Portal.
3. Fetch the results: `GET https://localhost:7224/api/transformed-loans`, or refresh the UI.

## Tests

```bash
cd api && dotnet test          # API + Function tests
cd frontend && npm test        # Vitest
```

## Troubleshooting

| Symptom | Fix |
|---|---|
| API fails at startup with an options validation error | `ServiceBus:ConnectionString` is missing; set it with `dotnet user-secrets` |
| UI shows CORS / network errors | API isn't running on `https://localhost:7224`, or the UI port isn't in `Cors:AllowedOrigins` |
| Function errors with `UseDevelopmentStorage` / storage | Azurite isn't running |
| Function receives nothing | Wrong connection string or queue name, or messages went to the dead-letter queue (check in the Portal) |
| SQL login errors | Adjust the connection strings to match your SQL Server instance |
