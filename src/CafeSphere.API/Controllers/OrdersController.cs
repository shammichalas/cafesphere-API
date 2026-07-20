using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Orders;
using CafeSphere.Domain.Enums;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    /// <summary>
    /// Retrieve paginated list of cafe orders filtered by optional status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Shared.Models.PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] OrderStatus? status = null)
    {
        var query = new GetOrdersQuery(pageNumber, pageSize, status);
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
