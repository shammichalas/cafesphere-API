using CafeSphere.Domain.Common;

namespace CafeSphere.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; } = 10;
    public bool IsAvailable { get; set; } = true;
    public List<ProductIngredient> Ingredients { get; set; } = new();
}

public class ProductIngredient
{
    public string InventoryItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public double QuantityRequired { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
}
