namespace MinimalApi.Dtos;

public record CreateProductRequest(string Name, decimal Price);

public record UpdateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price, DateTime CreatedAtUtc);
