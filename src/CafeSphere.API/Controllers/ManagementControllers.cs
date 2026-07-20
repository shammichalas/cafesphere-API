using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Catalog;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[Authorize]
[Route("api/v1/reservations")]
public class ReservationsController : BaseApiController
{
    /// <summary>
    /// Book a table reservation. (Scaffolded model)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    public IActionResult CreateReservation([FromBody] CreateReservationRequest request)
    {
        var reservation = new ReservationDto(
            Id: Guid.NewGuid().ToString("N"),
            CustomerId: null,
            CustomerName: request.CustomerName,
            CustomerPhone: request.CustomerPhone,
            CustomerEmail: request.CustomerEmail,
            TableId: request.TableId,
            TableNumber: "T-05",
            PartySize: request.PartySize,
            ReservationTime: request.ReservationTime,
            Status: Domain.Enums.ReservationStatus.Confirmed,
            SpecialNotes: request.SpecialNotes
        );

        return Created($"/api/v1/reservations/{reservation.Id}", reservation);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager},{Roles.Cashier}")]
[Route("api/v1/customers")]
public class CustomersController : BaseApiController
{
    /// <summary>
    /// Retrieve customer directory.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public IActionResult GetCustomers()
    {
        var customers = new List<CustomerDto>
        {
            new("c1", "Jane Smith", "+1234567890", "jane@example.com", 150, 480.50m, "Gold", DateTime.UtcNow.AddDays(-2)),
            new("c2", "Michael Brown", "+1987654321", "michael@example.com", 45, 120.00m, "Bronze", DateTime.UtcNow.AddDays(-5))
        };
        return Ok(customers);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/employees")]
public class EmployeesController : BaseApiController
{
    /// <summary>
    /// List employee records.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
    public IActionResult GetEmployees()
    {
        var employees = new List<EmployeeDto>
        {
            new("e1", "u1", "EMP-001", "Alexandra S.", "Management", "Manager", DateTime.UtcNow.AddYears(-2), 4500.00m, 28.50m, true),
            new("e2", "u2", "EMP-002", "John Doe", "Front Desk", "Cashier", DateTime.UtcNow.AddMonths(-8), 2600.00m, 16.00m, true),
            new("e3", "u3", "EMP-003", "Chef Marco", "Kitchen", "Kitchen Staff", DateTime.UtcNow.AddYears(-1), 3500.00m, 22.00m, true)
        };
        return Ok(employees);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/finance")]
public class FinanceController : BaseApiController
{
    /// <summary>
    /// Retrieve company expenses list.
    /// </summary>
    [HttpGet("expenses")]
    [ProducesResponseType(typeof(List<ExpenseDto>), StatusCodes.Status200OK)]
    public IActionResult GetExpenses()
    {
        var expenses = new List<ExpenseDto>
        {
            new("f1", "Coffee Beans Wholesale Batch", "Inventory", 450.00m, DateTime.UtcNow.AddDays(-3), "Organic Arabica Batch #401", "Alexandra S.", null),
            new("f2", "Organic Milk Delivery", "Inventory", 120.00m, DateTime.UtcNow.AddDays(-1), "Fresh Organic Whole Milk 100L", "Alexandra S.", null)
        };
        return Ok(expenses);
    }
}

[Authorize]
[Route("api/v1/ai")]
public class AIController : BaseApiController
{
    /// <summary>
    /// Query CafeSphere AI Assistant for real-time sales recommendations & stock predictions using MongoDB data context.
    /// </summary>
    [HttpPost("recommendations")]
    [ProducesResponseType(typeof(AIRecommendationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAIRecommendations([FromBody] AIRecommendationRequest request)
    {
        var result = await Mediator.Send(new GetAIRecommendationsQuery());
        return HandleResult(result);
    }
}
