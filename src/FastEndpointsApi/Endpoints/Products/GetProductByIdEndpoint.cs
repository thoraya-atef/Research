using FastEndpoints;
using FastEndpointsApi.Dtos;
using FastEndpointsApi.Services;

namespace FastEndpointsApi.Endpoints.Products;

public class GetProductByIdRequest
{
    public int Id { get; set; }
}

public class GetProductByIdEndpoint(IProductService productService) : Endpoint<GetProductByIdRequest, ProductResponse>
{
    public override void Configure()
    {
        Get("/products/{id}");
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(req.Id, ct);

        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(product, ct);
    }
}
