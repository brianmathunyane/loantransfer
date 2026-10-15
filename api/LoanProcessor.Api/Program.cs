using Azure.Messaging.ServiceBus;
using LoanProcessor.Api.Development;
using LoanProcessor.Api.ErrorHandling;
using LoanProcessor.Api.Messaging;
using LoanProcessor.Api.Validation;
using LoanProcessor.Data;
using LoanProcessor.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<ServiceBusOptions>()
    .Bind(builder.Configuration.GetSection(ServiceBusOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
    return new ServiceBusClient(options.ConnectionString);
});

builder.Services.AddSingleton<ILoanTransferPublisher, ServiceBusLoanTransferPublisher>();
builder.Services.AddSingleton<IRawLoanBatchValidator, RawLoanBatchValidator>();

builder.Services.AddDbContext<LoanProcessorDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LoanProcessorDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));
builder.Services.AddScoped<ITransformedLoanRepository, TransformedLoanRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddProblemDetails();

const string frontendDevCorsPolicy = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendDevCorsPolicy, policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(frontendDevCorsPolicy);

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LoanProcessorDbContext>();
    await dbContext.Database.MigrateAsync();
    await DevelopmentDataSeeder.SeedAsync(dbContext, CancellationToken.None);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
