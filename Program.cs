using System.Threading.RateLimiting;
using Microsoft.Extensions.Hosting;
using Asp.Versioning;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TeamAvailability.Extensions;
using TeamAvailability.Middleware;
using TeamAvailability.Application;
using TeamAvailability.Infrastructure;
using TeamAvailability.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

if (!isTesting)
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "TeamAvailability")
            .WriteTo.Console());
}

builder.Services.Configure<HostOptions>(options =>
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

    builder.Services.AddProblemDetails();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Default", policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    if (!isTesting)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("TeamAvailability.Api"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter());
    }

    var healthChecks = builder.Services.AddHealthChecks();
    if (!isTesting)
        healthChecks.AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

    var localDev = builder.Configuration.GetSection(LocalDevOptions.SectionName).Get<LocalDevOptions>() ?? new LocalDevOptions();
    if (!localDev.UseMemoryCache && !isTesting)
        healthChecks.AddRedis(builder.Configuration.GetConnectionString("Redis")!);

    var app = builder.Build();

    if (!isTesting)
    {
        app.UseSerilogRequestLogging(options =>
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]));
    }

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseCors("Default");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    var versionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).ReportApiVersions().Build();
    app.MapApiEndpoints(versionSet);
    app.MapHealthChecks("/health");
    if (!app.Environment.IsEnvironment("Testing"))
        app.MapPrometheusScrapingEndpoint("/metrics");
    app.MapFallbackToFile("index.html");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
