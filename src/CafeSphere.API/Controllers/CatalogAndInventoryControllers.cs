using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Catalog;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[Route("api/v1/categories")]
public class CategoriesController : BaseApiController
{
    /// <summary>
    /// Retrieve all active catalog categories from MongoDB Atlas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var result = await Mediator.Send(new GetCategoriesQuery());
        return HandleResult(result);
    }
}

[Route("api/v1/products")]
public class ProductsController : BaseApiController
{
    /// <summary>
    /// Retrieve list of products with optional category filter from MongoDB Atlas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] string? categoryId = null)
    {
        var result = await Mediator.Send(new GetProductsQuery(categoryId));
        return HandleResult(result);
    }

    /// <summary>
    /// Add a new dish product to the catalog menu (Admin & Kitchen Staff).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager},{Roles.KitchenStaff}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.CostPrice,
            request.CategoryId,
            request.ImageUrl,
            request.PreparationTimeMinutes
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/inventory")]
public class InventoryController : BaseApiController
{
    /// <summary>
    /// Get current stock levels of inventory items from MongoDB Atlas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<InventoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryItems()
    {
        var result = await Mediator.Send(new GetInventoryItemsQuery());
        return HandleResult(result);
    }
}
