using FastEndpoints;
using FastEndpointsApi.Dtos;
using FastEndpointsApi.Services;

namespace FastEndpointsApi.Endpoints.Products;

public class GetAllProductsEndpoint(IProductService productService) : EndpointWithoutRequest<IReadOnlyList<ProductResponse>>
{
    public override void Configure()
    {
        Get("/products");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        await Send.OkAsync(products, ct);
    }
}
