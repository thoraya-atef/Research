using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Dtos;
using MinimalApi.Services;
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

var app = builder.Build();

app.UseSerilogRequestLogging();

// UseHttpsRedirection early, before any endpoint mapping, so an HTTP request is redirected
// before it reaches routing or app logic.
app.UseHttpsRedirection();

var products = app.MapGroup("/products");

products.MapPost("/", async (CreateProductRequest request, IProductService service, CancellationToken cancellationToken) =>
{
    var validationError = ValidateProduct(request.Name, request.Price);
    if (validationError is not null)
    {
        return validationError;
    }

    var created = await service.CreateAsync(request, cancellationToken);
    return Results.Created($"/products/{created.Id}", created);
});

products.MapGet("/", async (IProductService service, CancellationToken cancellationToken) =>
{
    var all = await service.GetAllAsync(cancellationToken);
    return Results.Ok(all);
});

products.MapGet("/{id:int}", async (int id, IProductService service, CancellationToken cancellationToken) =>
{
    var product = await service.GetByIdAsync(id, cancellationToken);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

products.MapPut("/{id:int}", async (int id, UpdateProductRequest request, IProductService service, CancellationToken cancellationToken) =>
{
    var validationError = ValidateProduct(request.Name, request.Price);
    if (validationError is not null)
    {
        return validationError;
    }

    var updated = await service.UpdateAsync(id, request, cancellationToken);
    return updated ? Results.NoContent() : Results.NotFound();
});

products.MapDelete("/{id:int}", async (int id, IProductService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();

static IResult? ValidateProduct(string name, decimal price)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(name))
    {
        errors["Name"] = ["Name is required."];
    }
    else if (name.Length > 200)
    {
        errors["Name"] = ["Name must not exceed 200 characters."];
    }

    if (price < 0)
    {
        errors["Price"] = ["Price must not be negative."];
    }

    return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
}
