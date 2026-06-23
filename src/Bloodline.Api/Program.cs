using Bloodline.Api.Features.Identity;
using Bloodline.Application;
using Bloodline.Infrastructure;
using Scalar.AspNetCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", "Bloodline.Api");
});


builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("PostgreSql")!,
        name: "postgresql",
        tags: ["database", "ready"]
    );

var app = builder.Build();

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred. Please try again later."
            });
        });
    });
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});


app.MapIdentityEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    application = "Project Bloodline",
    service = "Bloodline.Api",
    status = "Running",
    utcTimeStamp = DateTimeOffset.UtcNow
}));

try
{
    Log.Information("Starting Bloodline.Api");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}