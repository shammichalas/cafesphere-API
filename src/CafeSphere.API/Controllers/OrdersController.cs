using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Orders;
using CafeSphere.Domain.Enums;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CafeSphere.Application.Interfaces;

namespace CafeSphere.API.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieve paginated list of cafe orders. Customers are strictly restricted to their own orders server-side.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Shared.Models.PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] OrderStatus? status = null,
        [FromQuery] string? customerId = null)
    {
        // Enforce ownership: If caller is a Customer, force-filter to their own UserId regardless of client input
        var effectiveCustomerId = _currentUserService.UserRole == Roles.Customer 
            ? _currentUserService.UserId 
            : customerId;

        var query = new GetOrdersQuery(pageNumber, pageSize, status, effectiveCustomerId);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieve the current authenticated customer's own order history.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(Shared.Models.PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] OrderStatus? status = null)
    {
        var currentUserId = _currentUserService.UserId;
        var query = new GetOrdersQuery(pageNumber, pageSize, status, currentUserId);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new order (POS / Online / Dine-In).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.CustomerName,
            request.Type,
            request.TableId,
            request.CouponCode,
            request.Notes,
            request.Items
        );

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager},{Roles.Cashier}")]
[Route("api/v1/pos")]
public class PosController : BaseApiController
{
    /// <summary>
    /// Process checkout payment for an open order.
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var command = new CheckoutOrderCommand(
            request.OrderId,
            request.AmountPaid,
            request.Method
        );

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
