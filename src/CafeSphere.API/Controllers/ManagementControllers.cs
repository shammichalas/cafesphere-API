using CafeSphere.Application.DTOs;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[Authorize]
[Route("api/v1/reservations")]
public class ReservationsController : BaseApiController
{
    /// <summary>
    /// Book a table reservation.
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

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/customers")]
public class CustomersController : BaseApiController
{
    /// <summary>
    /// Retrieve customer directory and loyalty profiles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public IActionResult GetCustomers()
    {
        return Ok(new List<CustomerDto>());
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
        return Ok(new List<EmployeeDto>());
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
        return Ok(new List<ExpenseDto>());
    }
}

[Authorize]
[Route("api/v1/ai")]
public class AIController : BaseApiController
{
    /// <summary>
    /// Query CafeSphere AI Assistant for real-time sales recommendations & stock predictions.
    /// </summary>
    [HttpPost("recommendations")]
    [ProducesResponseType(typeof(AIRecommendationResponse), StatusCodes.Status200OK)]
    public IActionResult GetAIRecommendations([FromBody] AIRecommendationRequest request)
    {
        var response = new AIRecommendationResponse(
            Answer: "Based on current order trends, Cold Brew Espresso demand spikes between 2 PM and 5 PM. Consider stocking 15% more espresso beans.",
            ActionableSuggestions: new List<string>
            {
                "Reorder Whole Bean Arabica (Batch #402)",
                "Create Happy Hour discount coupon for 3 PM - 5 PM"
            },
            DataPayload: new { ConfidenceScore = 0.94 }
        );

        return Ok(response);
    }
}
