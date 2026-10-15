using Azure.Monitor.OpenTelemetry.Exporter;
using LoanProcessor.Data;
using LoanProcessor.Function.Persistence;
using LoanProcessor.Function.Transform;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddDbContext<LoanProcessorDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["SqlConnectionString"],
        sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

builder.Services.AddScoped<ITransformedLoanWriter, TransformedLoanWriter>();
builder.Services.AddSingleton<ILoanOwnershipTransformer, LoanOwnershipTransformer>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
