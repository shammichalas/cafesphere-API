using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Dashboard;
using CafeSphere.Application.Features.Orders;
using CafeSphere.Domain.Enums;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager},{Roles.KitchenStaff}")]
[Route("api/v1/kitchen")]
public class KitchenController : BaseApiController
{
    /// <summary>
    /// Fetch active kitchen queue order tickets needing preparation.
    /// </summary>
    [HttpGet("queue")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKitchenQueue()
    {
        var result = await Mediator.Send(new GetKitchenQueueQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Update status of a kitchen ticket (Preparing, Ready, Completed).
    /// </summary>
    [HttpPatch("orders/{orderId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateKitchenStatus(string orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await Mediator.Send(new UpdateKitchenOrderStatusCommand(orderId, request.NewStatus));
        return HandleResult(result);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/dashboard")]
public class DashboardController : BaseApiController
{
    /// <summary>
    /// Get real-time overview KPIs, sales statistics, and top product metrics from MongoDB Atlas.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics()
    {
        var result = await Mediator.Send(new GetDashboardMetricsQuery());
        return HandleResult(result);
    }
}
