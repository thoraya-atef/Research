using FastEndpoints;
using FluentValidation;
using FastEndpointsApi.Dtos;
using FastEndpointsApi.Services;

namespace FastEndpointsApi.Endpoints.Products;

public class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductEndpoint(IProductService productService) : Endpoint<CreateProductRequest, ProductResponse>
{
    public override void Configure()
    {
        Post("/products");
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var created = await productService.CreateAsync(req, ct);
        await Send.CreatedAtAsync<GetProductByIdEndpoint>(new { id = created.Id }, created, cancellation: ct);
    }
}
