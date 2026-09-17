using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using FastEndpointsApi.Data;
using FastEndpointsApi.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// AppDbContext: Scoped (AddDbContext default) — one context per request, matches EF Core's
// unit-of-work model and avoids sharing a DbContext across concurrent requests.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ProductService: Scoped to match AppDbContext's lifetime. Registering it as Singleton would
// create a captive dependency — a singleton holding a reference to a scoped DbContext that
// outlives its intended request scope, causing errors or stale data across requests.
builder.Services.AddScoped<IProductService, ProductService>();

// Explicitly include this project's own assembly. FastEndpoints' default auto-discovery
// excludes assemblies whose name starts with "FastEndpoints" (to skip its own internal
// assemblies), which also matches this project's name (FastEndpointsApi) and otherwise
// leaves it with zero discovered endpoints.
builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(Program).Assembly]);

var app = builder.Build();

app.UseSerilogRequestLogging();

// UseHttpsRedirection early, before UseFastEndpoints, so an HTTP request is redirected
// before it reaches routing or app logic.
app.UseHttpsRedirection();

app.UseFastEndpoints();

app.Run();
