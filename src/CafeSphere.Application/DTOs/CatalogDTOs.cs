namespace CafeSphere.Application.DTOs;

public record CategoryDto(
    string Id,
    string Name,
    string Slug,
    string Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive
);

public record CreateCategoryRequest(
    string Name,
    string Description,
    string? ImageUrl,
    int DisplayOrder
);

public record ProductDto(
    string Id,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    decimal CostPrice,
    string CategoryId,
    string CategoryName,
    string? ImageUrl,
    int PreparationTimeMinutes,
    bool IsAvailable
);

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    decimal CostPrice,
    string CategoryId,
    string? ImageUrl,
    int PreparationTimeMinutes,
    List<ProductIngredientDto>? Ingredients
);

public record ProductIngredientDto(
    string InventoryItemId,
    string ItemName,
    double QuantityRequired,
    string UnitOfMeasure
);
