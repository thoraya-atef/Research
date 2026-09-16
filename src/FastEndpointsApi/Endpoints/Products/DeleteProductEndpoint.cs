using FastEndpoints;
using FastEndpointsApi.Services;

namespace FastEndpointsApi.Endpoints.Products;

public class DeleteProductRequest
{
    public int Id { get; set; }
}

public class DeleteProductEndpoint(IProductService productService) : Endpoint<DeleteProductRequest>
{
    public override void Configure()
    {
        Delete("/products/{id}");
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        var deleted = await productService.DeleteAsync(req.Id, ct);

        if (!deleted)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
