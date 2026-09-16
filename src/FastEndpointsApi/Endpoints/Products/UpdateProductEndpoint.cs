using FastEndpoints;
using FluentValidation;
using FastEndpointsApi.Dtos;
using FastEndpointsApi.Services;

namespace FastEndpointsApi.Endpoints.Products;

public class UpdateProductEndpointRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateProductEndpointValidator : Validator<UpdateProductEndpointRequest>
{
    public UpdateProductEndpointValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductEndpoint(IProductService productService) : Endpoint<UpdateProductEndpointRequest>
{
    public override void Configure()
    {
        Put("/products/{id}");
    }

    public override async Task HandleAsync(UpdateProductEndpointRequest req, CancellationToken ct)
    {
        var updated = await productService.UpdateAsync(req.Id, new UpdateProductRequest(req.Name, req.Price), ct);

        if (!updated)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
