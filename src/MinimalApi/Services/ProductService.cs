using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Dtos;
using MinimalApi.Models;

namespace MinimalApi.Services;

public class ProductService(AppDbContext dbContext, ILogger<ProductService> logger) : IProductService
{
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Products.Add(product);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to create product with Name {ProductName}", request.Name);
            throw;
        }

        logger.LogInformation("Product {ProductId} created with Name {ProductName}", product.Id, product.Name);

        return ToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        // AsNoTracking: read-only query, no entities are modified, so change tracking is skipped for lower overhead.
        var products = await dbContext.Products
            .AsNoTracking()
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return products;
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // AsNoTracking: single read-only lookup, result isn't updated or saved back.
        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} not found", id);
            return null;
        }

        return ToResponse(product);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Attempted to update missing product {ProductId}", id);
            return false;
        }

        product.Name = request.Name;
        product.Price = request.Price;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update product {ProductId}", id);
            throw;
        }

        logger.LogInformation("Product {ProductId} updated", id);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Attempted to delete missing product {ProductId}", id);
            return false;
        }

        dbContext.Products.Remove(product);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to delete product {ProductId}", id);
            throw;
        }

        logger.LogInformation("Product {ProductId} deleted", id);

        return true;
    }

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Name, product.Price, product.CreatedAtUtc);
}
