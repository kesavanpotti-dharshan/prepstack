using Prepstack.Api.Health;
using Prepstack.Infrastructure.Mongo;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddMongo(builder.Configuration);
builder.Services
    .AddHealthChecks()
    .AddCheck<MongoHealthCheck>("mongo");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
