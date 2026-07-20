using CafeSphere.Application.DTOs;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[Route("api/v1/categories")]
public class CategoriesController : BaseApiController
{
    /// <summary>
    /// Retrieve all active catalog categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        return Ok(new List<CategoryDto>());
    }
}

[Route("api/v1/products")]
public class ProductsController : BaseApiController
{
    /// <summary>
    /// Retrieve list of products with optional category filter.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public IActionResult GetProducts([FromQuery] string? categoryId = null)
    {
        return Ok(new List<ProductDto>());
    }

    /// <summary>
    /// Create new menu product.
    /// </summary>
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = new ProductDto(
            Id: Guid.NewGuid().ToString("N"),
            Name: request.Name,
            Slug: request.Name.ToLower().Replace(" ", "-"),
            Description: request.Description,
            Price: request.Price,
            CostPrice: request.CostPrice,
            CategoryId: request.CategoryId,
            CategoryName: "Beverages",
            ImageUrl: request.ImageUrl,
            PreparationTimeMinutes: request.PreparationTimeMinutes,
            IsAvailable: true
        );

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/inventory")]
public class InventoryController : BaseApiController
{
    /// <summary>
    /// Get current stock levels of inventory items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<InventoryItemDto>), StatusCodes.Status200OK)]
    public IActionResult GetInventoryItems()
    {
        return Ok(new List<InventoryItemDto>());
    }
}
