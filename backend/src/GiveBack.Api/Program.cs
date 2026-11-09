using GiveBack.Api.Extensions;
using GiveBack.Api.Options;
using GiveBack.Application;
using GiveBack.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting GiveBack API.");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.Configure<ConcurrencyLimiterOptions>(builder.Configuration.GetSection("ConcurrencyLimiter"));
    builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("GiveBackFrontend", policy =>
            policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
    {
        throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection before starting the API.");
    }

    builder.Services.AddControllers();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddGiveBackAuthentication(builder.Configuration);
    builder.Services.AddGiveBackApiVersioning();
    builder.Services.AddGiveBackSwagger();

    var app = builder.Build();

    app.UseGlobalExceptionHandling();
    app.UseSerilogRequestLogging();
    app.UseConcurrencyLimiter();
    app.UseGiveBackSwagger();
    app.UseCors("GiveBackFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "GiveBack API terminated unexpectedly.");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
