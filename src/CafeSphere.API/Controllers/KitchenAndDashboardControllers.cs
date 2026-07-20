using CafeSphere.Application.DTOs;
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
    /// Fetch active kitchen queue order items needing preparation.
    /// </summary>
    [HttpGet("queue")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public IActionResult GetKitchenQueue()
    {
        return Ok(new List<OrderDto>());
    }

    /// <summary>
    /// Update status of a kitchen ticket (Preparing, Ready, Completed).
    /// </summary>
    [HttpPatch("orders/{orderId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult UpdateKitchenStatus(string orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        return Ok(new { OrderId = orderId, Status = request.NewStatus, UpdatedAt = DateTime.UtcNow });
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/dashboard")]
public class DashboardController : BaseApiController
{
    /// <summary>
    /// Get real-time overview KPIs, sales statistics, and top product metrics.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    public IActionResult GetMetrics()
    {
        var metrics = new DashboardMetricsDto(
            TodayRevenue: 1450.50m,
            TodayOrdersCount: 42,
            MonthRevenue: 38400.00m,
            TotalExpenses: 12500.00m,
            NetProfit: 25900.00m,
            ActiveTablesCount: 8,
            PendingKitchenOrdersCount: 3,
            TopProducts: new List<TopProductDto>
            {
                new("prod_1", "Espresso Single Origin", 150, 675.00m),
                new("prod_2", "Iced Vanilla Latte", 120, 780.00m),
                new("prod_3", "Avocado Toast Sourdough", 85, 935.00m)
            }
        );

        return Ok(metrics);
    }
}
